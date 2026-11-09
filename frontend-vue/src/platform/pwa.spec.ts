import type { RegisterSWOptions } from 'vite-plugin-pwa/types'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  API_CACHE_NAME,
  CONFIG_CACHE_NAME,
  EXPIRATION_RELEASED,
  OFFLINE_COPY_HEADER,
  RELEASE_EXPIRATION_MESSAGE,
} from './offline-caches'

type PwaModule = typeof import('./pwa')

interface Worker {
  options: RegisterSWOptions
  update: ReturnType<typeof vi.fn>
}

let pwa: PwaModule

beforeEach(async () => {
  vi.resetModules()
  pwa = await import('./pwa')
})

afterEach(() => {
  vi.unstubAllGlobals()
  Reflect.deleteProperty(navigator, 'serviceWorker')
})

function setOnline(online: boolean): void {
  vi.spyOn(navigator, 'onLine', 'get').mockReturnValue(online)
}

function installPromptEvent(outcome: 'accepted' | 'dismissed'): Event {
  return Object.assign(new Event('beforeinstallprompt', { cancelable: true }), {
    prompt: vi.fn(() => Promise.resolve()),
    userChoice: Promise.resolve({ outcome }),
  })
}

function registration(overrides: Partial<ServiceWorkerRegistration> = {}): ServiceWorkerRegistration {
  return {
    active: {},
    installing: null,
    waiting: null,
    update: vi.fn(() => Promise.resolve()),
    ...overrides,
  } as unknown as ServiceWorkerRegistration
}

async function startWorker(): Promise<Worker> {
  Object.defineProperty(navigator, 'serviceWorker', { value: {}, configurable: true })

  const update = vi.fn(() => Promise.resolve())
  let captured: RegisterSWOptions = {}

  await pwa.startServiceWorker(() => Promise.resolve((options: RegisterSWOptions) => {
    captured = options

    return update
  }))

  return { options: captured, update }
}

describe('состояние сети', () => {
  it('следит за пропажей и возвращением сети', () => {
    setOnline(false)
    pwa.watchAppEnvironment()

    expect(pwa.pwaState.online).toBe(false)

    window.dispatchEvent(new Event('online'))
    expect(pwa.pwaState.online).toBe(true)

    window.dispatchEvent(new Event('offline'))
    expect(pwa.pwaState.online).toBe(false)
  })

  it('копия из офлайн-кэша при «живой» сети браузера тоже считается отсутствием связи', async () => {
    setOnline(true)
    pwa.watchAppEnvironment()

    const { apiFetch } = await import('./http')

    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(new Response('[]', { headers: { [OFFLINE_COPY_HEADER]: '1' } }))))
    await apiFetch(new Request('https://money.test/api/Categories'))

    expect(pwa.isConnected()).toBe(false)

    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(new Response('[]'))))
    await apiFetch(new Request('https://money.test/api/Categories'))

    expect(pwa.isConnected()).toBe(true)
  })
})

describe('установка', () => {
  it.each([
    ['accepted', 'installed'],
    ['dismissed', 'manual'],
  ] as const)('предложение браузера, ответ %s, даёт состояние %s', async (outcome, expected) => {
    pwa.watchAppEnvironment()

    const event = installPromptEvent(outcome)

    window.dispatchEvent(event)

    expect(event.defaultPrevented).toBe(true)
    expect(pwa.pwaState.install).toBe('available')

    await pwa.promptInstall()

    expect(pwa.pwaState.install).toBe(expected)
  })

  it('открытое как приложение считается установленным и предложение браузера не перебивает', () => {
    vi.spyOn(window, 'matchMedia').mockReturnValue({ matches: true } as MediaQueryList)
    pwa.watchAppEnvironment()

    window.dispatchEvent(installPromptEvent('accepted'))

    expect(pwa.pwaState.install).toBe('installed')
  })
})

describe('работа без сети', () => {
  it('без поддержки воркеров честно говорит, что офлайна нет', async () => {
    const load = vi.fn()

    await pwa.startServiceWorker(load)

    expect(pwa.pwaState.offlineSupport).toBe('unsupported')
    expect(load).not.toHaveBeenCalled()
  })

  it('несостоявшаяся загрузка регистрации не оставляет состояние «готовим»', async () => {
    Object.defineProperty(navigator, 'serviceWorker', { value: {}, configurable: true })

    await pwa.startServiceWorker(() => Promise.reject(new Error('чанк не загрузился')))

    expect(pwa.pwaState.offlineSupport).toBe('failed')
  })

  it('работающий воркер делает приложение готовым к офлайну и открывает проверку обновлений', async () => {
    const worker = await startWorker()

    expect(pwa.pwaState.offlineSupport).toBe('preparing')
    expect(pwa.pwaState.canCheckUpdates).toBe(false)

    worker.options.onRegisteredSW?.('/service-worker.js', registration())

    expect(pwa.pwaState.offlineSupport).toBe('ready')
    expect(pwa.pwaState.canCheckUpdates).toBe(true)
  })

  it('отказ регистрации виден как отказ', async () => {
    const worker = await startWorker()

    worker.options.onRegisterError?.(new Error('нет'))

    expect(pwa.pwaState.offlineSupport).toBe('failed')
  })
})

describe('обновление', () => {
  it('новая версия ждёт согласия и не перезагружает страницу сама', async () => {
    const reload = vi.spyOn(window.location, 'reload').mockImplementation(() => undefined)

    const worker = await startWorker()

    worker.options.onNeedRefresh?.()

    expect(pwa.pwaState.needRefresh).toBe(true)
    expect(worker.update).not.toHaveBeenCalled()

    worker.options.onNeedReload?.()

    expect(reload).not.toHaveBeenCalled()
    expect(pwa.pwaState.updatedElsewhere).toBe(true)
  })

  it('обновление по кнопке активирует воркер и перезагружает страницу', async () => {
    const reload = vi.spyOn(window.location, 'reload').mockImplementation(() => undefined)

    const worker = await startWorker()

    worker.options.onNeedRefresh?.()
    await pwa.applyUpdate()

    expect(worker.update).toHaveBeenCalledWith(true)

    worker.options.onNeedReload?.()

    expect(reload).toHaveBeenCalledTimes(1)
  })

  it.each([
    ['без новой версии', registration(), 'latest'],
    ['с найденной версией', registration({ installing: {} as ServiceWorker }), 'downloading'],
    ['с отказом проверки', registration({ update: vi.fn(() => Promise.reject(new Error('502'))) }), 'failed'],
  ])('проверка %s', async (_case, registered, expected) => {
    setOnline(true)

    const worker = await startWorker()

    worker.options.onRegisteredSW?.('/service-worker.js', registered)
    await pwa.checkForUpdates()

    expect(pwa.pwaState.updateCheck).toBe(expected)
  })

  it('без сети проверку не начинает', async () => {
    const update = vi.fn(() => Promise.resolve({} as ServiceWorkerRegistration))
    const worker = await startWorker()

    worker.options.onRegisteredSW?.('/service-worker.js', registration({ update }))
    setOnline(false)
    await pwa.checkForUpdates()

    expect(pwa.pwaState.updateCheck).toBe('offline')
    expect(update).not.toHaveBeenCalled()
  })
})

type DeleteOutcome = 'success' | 'blocked' | 'error' | 'silent' | 'throws'

function stubIndexedDb(outcome: DeleteOutcome): ReturnType<typeof vi.fn> {
  const deleteDatabase = vi.fn(() => {
    if (outcome === 'throws') {
      throw new DOMException('нет доступа', 'SecurityError')
    }

    const request: Partial<Record<'onsuccess' | 'onblocked' | 'onerror', () => void>> = {}

    if (outcome !== 'silent') {
      queueMicrotask(() => request[`on${outcome}`]?.())
    }

    return request
  })

  vi.stubGlobal('indexedDB', { deleteDatabase })

  return deleteDatabase
}

describe('офлайн-кэш', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('выход удаляет ответы API и сроки их хранения, очистка из настроек – и адрес API', async () => {
    const remove = vi.fn(() => Promise.resolve(true))
    const deleteDatabase = stubIndexedDb('success')

    vi.stubGlobal('caches', { delete: remove })

    expect(await pwa.clearApiCache()).toBe(true)
    expect(remove.mock.calls).toEqual([[API_CACHE_NAME]])
    expect(deleteDatabase.mock.calls).toEqual([['workbox-expiration']])

    remove.mockClear()
    deleteDatabase.mockClear()

    expect(await pwa.clearOfflineData()).toBe(true)
    expect(remove.mock.calls).toEqual([[API_CACHE_NAME], [CONFIG_CACHE_NAME]])
    expect(deleteDatabase.mock.calls).toEqual([['workbox-expiration']])
  })

  it('отказ хранилища возвращается как неудача, а не исключение', async () => {
    stubIndexedDb('success')
    vi.stubGlobal('caches', { delete: vi.fn(() => Promise.reject(new Error('квота'))) })

    expect(await pwa.clearApiCache()).toBe(false)
  })

  it.each<[DeleteOutcome, boolean]>([
    ['blocked', true],
    ['error', false],
    ['throws', false],
  ])('удаление базы с исходом %s не держит выход и даёт %s', async (outcome, expected) => {
    stubIndexedDb(outcome)
    vi.stubGlobal('caches', { delete: vi.fn(() => Promise.resolve(true)) })

    expect(await pwa.clearApiCache()).toBe(expected)
  })

  it('зависшее удаление базы отпускает выход по таймауту', async () => {
    vi.useFakeTimers()
    stubIndexedDb('silent')
    vi.stubGlobal('caches', { delete: vi.fn(() => Promise.resolve(true)) })

    const cleared = pwa.clearApiCache()

    await vi.advanceTimersByTimeAsync(3000)

    expect(await cleared).toBe(false)
  })
})

type WorkerReply = 'released' | 'silent'

interface WorkerWithDatabase {
  postMessage: ReturnType<typeof vi.fn>
  deleteDatabase: ReturnType<typeof vi.fn>
  events: string[]
}

function stubWorkerHoldingDatabase(reply: WorkerReply, transactionMs = 0): WorkerWithDatabase {
  const events: string[] = []
  let connectionOpen = true

  const postMessage = vi.fn((_message: unknown, [port]: MessagePort[]) => {
    if (reply === 'silent') {
      return
    }

    const close = (): void => {
      connectionOpen = false
      events.push('released')
    }

    if (transactionMs === 0) {
      close()
    } else {
      setTimeout(close, transactionMs)
    }

    port?.postMessage(EXPIRATION_RELEASED)
  })

  const deleteDatabase = vi.fn(() => {
    const request: Partial<Record<'onsuccess' | 'onblocked', () => void>> = {}
    const finish = (): void => {
      if (connectionOpen) {
        setTimeout(finish, 1)

        return
      }

      events.push('success')
      request.onsuccess?.()
    }

    queueMicrotask(() => {
      if (connectionOpen) {
        events.push('blocked')
        request.onblocked?.()
      }

      finish()
    })

    return request
  })

  Object.defineProperty(navigator, 'serviceWorker', {
    value: { getRegistration: () => Promise.resolve(registration({ active: { postMessage } as unknown as ServiceWorker })) },
    configurable: true,
  })
  vi.stubGlobal('indexedDB', { deleteDatabase })
  vi.stubGlobal('caches', { delete: vi.fn(() => Promise.resolve(true)) })

  return { postMessage, deleteDatabase, events }
}

describe('база отметок при живом воркере', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('выход сначала просит воркер закрыть соединение, и база удаляется с исходом success, а не blocked', async () => {
    const worker = stubWorkerHoldingDatabase('released')

    expect(await pwa.clearApiCache()).toBe(true)
    expect(worker.postMessage.mock.calls[0]?.[0]).toEqual({ type: RELEASE_EXPIRATION_MESSAGE })
    expect(worker.postMessage.mock.invocationCallOrder[0]).toBeLessThan(worker.deleteDatabase.mock.invocationCallOrder[0] ?? 0)
    expect(worker.events).toEqual(['released', 'success'])
  })

  it('отпустивший воркер дожимает транзакцию, и выход ждёт самого удаления, а не отметки blocked', async () => {
    const worker = stubWorkerHoldingDatabase('released', 50)

    const cleared = pwa.clearApiCache().then(result => {
      worker.events.push('cleared')

      return result
    })

    expect(await cleared).toBe(true)
    expect(worker.events).toEqual(['blocked', 'released', 'success', 'cleared'])
  })

  it('воркер прежней версии не отвечает: удаление ждёт секунду и откатывается к blocked без ожидания', async () => {
    vi.useFakeTimers()
    const worker = stubWorkerHoldingDatabase('silent')

    const cleared = pwa.clearApiCache()

    await vi.advanceTimersByTimeAsync(999)
    expect(worker.deleteDatabase).not.toHaveBeenCalled()

    await vi.advanceTimersByTimeAsync(1)

    expect(await cleared).toBe(true)
    expect(worker.events).toEqual(['blocked'])
  })
})

describe('аварийное снятие воркера', () => {
  const PRECACHE_NAME = 'workbox-precache-v2-https://localhost:8443/'

  it('снимает все регистрации, стирает все кэши с базой отметок и не обещает работу без сети', async () => {
    const unregister = vi.fn(() => Promise.resolve(true))
    const remove = vi.fn(() => Promise.resolve(true))
    const deleteDatabase = stubIndexedDb('success')

    Object.defineProperty(navigator, 'serviceWorker', {
      value: { getRegistrations: () => Promise.resolve([{ unregister }, { unregister }]) },
      configurable: true,
    })
    vi.stubGlobal('caches', {
      keys: () => Promise.resolve([API_CACHE_NAME, CONFIG_CACHE_NAME, PRECACHE_NAME]),
      delete: remove,
    })

    expect(await pwa.removeServiceWorker()).toBe(true)
    expect(unregister).toHaveBeenCalledTimes(2)
    expect(remove.mock.calls).toEqual([[API_CACHE_NAME], [CONFIG_CACHE_NAME], [PRECACHE_NAME]])
    expect(deleteDatabase.mock.calls).toEqual([['workbox-expiration']])
    expect(pwa.pwaState.offlineSupport).toBe('withdrawn')
    expect(pwa.pwaState.canCheckUpdates).toBe(false)
  })

  it('отказ снятия регистрации не мешает стереть кэши и возвращается как неудача', async () => {
    const remove = vi.fn(() => Promise.resolve(true))

    Object.defineProperty(navigator, 'serviceWorker', {
      value: { getRegistrations: () => Promise.reject(new Error('нет доступа')) },
      configurable: true,
    })
    vi.stubGlobal('caches', { keys: () => Promise.resolve([API_CACHE_NAME]), delete: remove })

    expect(await pwa.removeServiceWorker()).toBe(false)
    expect(remove.mock.calls).toEqual([[API_CACHE_NAME]])
  })
})
