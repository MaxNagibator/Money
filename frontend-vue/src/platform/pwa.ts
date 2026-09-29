import type { RegisterSWOptions } from 'vite-plugin-pwa/types'
import { reactive, readonly } from 'vue'
import { isOffline, setReachabilityHandler } from './http'
import {
  API_CACHE_NAME,
  CONFIG_CACHE_NAME,
  EXPIRATION_DATABASE,
  EXPIRATION_RELEASED,
  RELEASE_EXPIRATION_MESSAGE,
} from './offline-caches'

export type OfflineSupport = 'disabled' | 'withdrawn' | 'unsupported' | 'preparing' | 'ready' | 'failed'
export type InstallState = 'installed' | 'available' | 'manual'
export type UpdateCheck = 'idle' | 'checking' | 'latest' | 'downloading' | 'offline' | 'failed'
export type RegisterServiceWorker = (options: RegisterSWOptions) => (reloadPage?: boolean) => Promise<void>

export interface StorageUsage {
  usage: number
  quota: number
}

interface PwaState {
  online: boolean
  serverReachable: boolean
  offlineSupport: OfflineSupport
  canCheckUpdates: boolean
  needRefresh: boolean
  updatedElsewhere: boolean
  updateCheck: UpdateCheck
  install: InstallState
}

interface InstallPromptEvent extends Event {
  prompt: () => Promise<void>
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>
}

const STANDALONE_QUERY = '(display-mode: standalone)'
const DATABASE_DELETE_TIMEOUT_MS = 3000
const WORKER_RELEASE_TIMEOUT_MS = 1000

const state = reactive<PwaState>({
  online: true,
  serverReachable: true,
  offlineSupport: 'disabled',
  canCheckUpdates: false,
  needRefresh: false,
  updatedElsewhere: false,
  updateCheck: 'idle',
  install: 'manual',
})

export const pwaState = readonly(state)

export function isConnected(): boolean {
  return state.online && state.serverReachable
}

let installPrompt: InstallPromptEvent | null = null
let registration: ServiceWorkerRegistration | null = null
let activateUpdate: (() => Promise<void>) | null = null
let isApplyingUpdate = false
let isWatching = false

export function watchAppEnvironment(): void {
  if (isWatching) {
    return
  }

  isWatching = true
  state.online = !isOffline()
  state.install = isStandalone() ? 'installed' : 'manual'

  setReachabilityHandler(reachable => {
    state.serverReachable = reachable
  })

  window.addEventListener('online', () => {
    state.online = true
  })
  window.addEventListener('offline', () => {
    state.online = false
  })
  window.addEventListener('beforeinstallprompt', event => {
    event.preventDefault()
    installPrompt = event as InstallPromptEvent

    if (state.install !== 'installed') {
      state.install = 'available'
    }
  })
  window.addEventListener('appinstalled', () => {
    installPrompt = null
    state.install = 'installed'
  })
}

export async function startServiceWorker(load: () => Promise<RegisterServiceWorker>): Promise<void> {
  if (!('serviceWorker' in navigator)) {
    state.offlineSupport = 'unsupported'

    return
  }

  state.offlineSupport = 'preparing'

  let update: (reloadPage?: boolean) => Promise<void>

  try {
    const register = await load()

    update = register({
      onNeedRefresh() {
        state.needRefresh = true
      },
      onNeedReload() {
        if (isApplyingUpdate) {
          window.location.reload()
        } else {
          state.updatedElsewhere = true
        }
      },
      onOfflineReady() {
        state.offlineSupport = 'ready'
      },
      onRegisteredSW(_url, registered) {
        registration = registered ?? null
        state.canCheckUpdates = registration !== null

        if (registered?.active) {
          state.offlineSupport = 'ready'
        }
      },
      onRegisterError() {
        state.offlineSupport = 'failed'
      },
    })
  } catch {
    state.offlineSupport = 'failed'

    return
  }

  activateUpdate = () => update(true)
}

export async function removeServiceWorker(): Promise<boolean> {
  state.offlineSupport = 'withdrawn'

  const unregistered = await unregisterWorkers()
  const [cleared, deleted] = await Promise.all([deleteAllCaches(), deleteExpirationDatabase()])

  return unregistered && cleared && deleted
}

async function unregisterWorkers(): Promise<boolean> {
  if (!('serviceWorker' in navigator)) {
    return true
  }

  try {
    const registrations = await navigator.serviceWorker.getRegistrations()
    const results = await Promise.all(registrations.map(registered => registered.unregister()))

    return results.every(Boolean)
  } catch {
    return false
  }
}

async function deleteAllCaches(): Promise<boolean> {
  if (typeof caches === 'undefined') {
    return true
  }

  try {
    return await deleteCaches(await caches.keys())
  } catch {
    return false
  }
}

export async function checkForUpdates(): Promise<void> {
  if (registration === null || state.needRefresh || state.updateCheck === 'checking') {
    return
  }

  if (isOffline()) {
    state.updateCheck = 'offline'

    return
  }

  state.updateCheck = 'checking'

  try {
    await registration.update()

    const found = registration.installing !== null || registration.waiting !== null

    state.updateCheck = found && !state.needRefresh ? 'downloading' : 'latest'
  } catch {
    state.updateCheck = isOffline() ? 'offline' : 'failed'
  }
}

export async function applyUpdate(): Promise<void> {
  if (activateUpdate === null) {
    return
  }

  isApplyingUpdate = true

  try {
    await activateUpdate()
  } catch {
    isApplyingUpdate = false
  }
}

export function reloadApp(): void {
  window.location.reload()
}

export async function promptInstall(): Promise<void> {
  const prompt = installPrompt

  if (prompt === null) {
    return
  }

  installPrompt = null

  try {
    await prompt.prompt()
    const choice = await prompt.userChoice

    state.install = choice.outcome === 'accepted' ? 'installed' : 'manual'
  } catch {
    state.install = 'manual'
  }
}

export async function estimateStorage(): Promise<StorageUsage | null> {
  if (!('storage' in navigator) || typeof navigator.storage.estimate !== 'function') {
    return null
  }

  try {
    const { usage, quota } = await navigator.storage.estimate()

    return usage === undefined || quota === undefined ? null : { usage, quota }
  } catch {
    return null
  }
}

export function clearApiCache(): Promise<boolean> {
  return clearOffline([API_CACHE_NAME])
}

export function clearOfflineData(): Promise<boolean> {
  return clearOffline([API_CACHE_NAME, CONFIG_CACHE_NAME])
}

async function clearOffline(names: readonly string[]): Promise<boolean> {
  const results = await Promise.all([deleteCaches(names), deleteExpirationDatabase()])

  return results.every(Boolean)
}

function deleteExpirationDatabase(): Promise<boolean> {
  if (typeof indexedDB === 'undefined') {
    return Promise.resolve(true)
  }

  return new Promise(resolve => {
    let settled = false

    const settle = (deleted: boolean): void => {
      if (!settled) {
        settled = true
        clearTimeout(timer)
        resolve(deleted)
      }
    }

    const timer = setTimeout(() => {
      settle(false)
    }, DATABASE_DELETE_TIMEOUT_MS)

    void releaseExpiration().then(released => {
      if (settled) {
        return
      }

      try {
        const request = indexedDB.deleteDatabase(EXPIRATION_DATABASE)

        request.onsuccess = () => {
          settle(true)
        }
        request.onblocked = () => {
          if (!released) {
            settle(true)
          }
        }
        request.onerror = () => {
          settle(false)
        }
      } catch {
        settle(false)
      }
    })
  })
}

function releaseExpiration(): Promise<boolean> {
  if (!('serviceWorker' in navigator) || typeof MessageChannel === 'undefined') {
    return Promise.resolve(false)
  }

  return new Promise(resolve => {
    const channel = new MessageChannel()

    const finish = (released: boolean): void => {
      clearTimeout(timer)
      channel.port1.close()
      resolve(released)
    }

    const timer = setTimeout(() => {
      finish(false)
    }, WORKER_RELEASE_TIMEOUT_MS)

    channel.port1.onmessage = event => {
      finish(event.data === EXPIRATION_RELEASED)
    }

    void activeWorker().then(worker => {
      if (worker === null) {
        finish(false)

        return
      }

      try {
        worker.postMessage({ type: RELEASE_EXPIRATION_MESSAGE }, [channel.port2])
      } catch {
        finish(false)
      }
    })
  })
}

async function activeWorker(): Promise<ServiceWorker | null> {
  try {
    const registered = await navigator.serviceWorker.getRegistration()

    return registered?.active ?? null
  } catch {
    return null
  }
}

async function deleteCaches(names: readonly string[]): Promise<boolean> {
  if (typeof caches === 'undefined') {
    return true
  }

  try {
    await Promise.all(names.map(name => caches.delete(name)))

    return true
  } catch {
    return false
  }
}

function isStandalone(): boolean {
  const legacyStandalone = (navigator as Navigator & { standalone?: boolean }).standalone === true

  return legacyStandalone || window.matchMedia(STANDALONE_QUERY).matches
}
