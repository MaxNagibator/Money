import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { SessionChange } from '@/domain/session'
import { systemClock } from '@/platform/clock'
import { readSetting, removeSetting, writeSetting } from '@/platform/storage'
import type { AuthTokens } from '../api/auth-api'
import { requestExternalGrant, requestPasswordGrant, requestRefreshGrant, revokeRefreshToken } from '../api/auth-api'

// TODO: временная схема, токены лежат в localStorage и доступны любому XSS.
// Убрать вместе с переходом на cookie-сессию same-origin.
const TOKENS_KEY = 'auth-tokens'
const EXPIRY_GUARD_MS = 30_000

let sessionChangeHandler: (change: SessionChange) => void = () => undefined

export function setSessionChangeHandler(handler: (change: SessionChange) => void): void {
  sessionChangeHandler = handler
}

export const useSessionStore = defineStore('session', () => {
  const tokens = ref(readSetting<AuthTokens | null>(TOKENS_KEY, null))
  const isAuthenticated = computed(() => tokens.value !== null)

  let refreshing: Promise<string | null> | null = null
  let refreshedToken: string | null = null
  let identity = 0

  function beginIdentity(): void {
    identity += 1
    refreshing = null
    refreshedToken = null
  }

  function remember(value: AuthTokens): void {
    tokens.value = value
    writeSetting(TOKENS_KEY, value)
  }

  function forget(change: SessionChange): void {
    tokens.value = null
    removeSetting(TOKENS_KEY)
    sessionChangeHandler(change)
  }

  async function signIn(userName: string, password: string): Promise<void> {
    const started = identity

    await accept(started, await requestPasswordGrant(userName, password))
  }

  async function signInExternal(): Promise<void> {
    const started = identity

    await accept(started, await requestExternalGrant())
  }

  async function accept(started: number, granted: AuthTokens): Promise<void> {
    if (started !== identity) {
      await revokeRefreshToken(granted).catch(() => undefined)

      return
    }

    beginIdentity()
    remember(granted)
    sessionChangeHandler('signedIn')
  }

  async function signOut(): Promise<void> {
    const current = tokens.value

    beginIdentity()
    forget('signedOut')

    if (current !== null) {
      await revokeRefreshToken(current).catch(() => undefined)
    }
  }

  async function refresh(current: AuthTokens): Promise<string | null> {
    const started = identity

    try {
      const refreshed = await requestRefreshGrant(current)

      if (started !== identity) {
        return tokens.value?.accessToken ?? null
      }

      remember(refreshed)
      refreshedToken = refreshed.accessToken

      return refreshed.accessToken
    } catch {
      if (started !== identity) {
        return tokens.value?.accessToken ?? null
      }

      forget('expired')
      return null
    } finally {
      if (started === identity) {
        refreshing = null
      }
    }
  }

  function getAccessToken(): Promise<string | null> {
    const current = tokens.value

    if (current === null) {
      return Promise.resolve(null)
    }

    if (current.expiresAt - EXPIRY_GUARD_MS > systemClock.now().getTime()) {
      return Promise.resolve(current.accessToken)
    }

    refreshing ??= refresh(current)

    return refreshing
  }

  function refreshAccessToken(staleToken: string): Promise<string | null> {
    const current = tokens.value

    if (current === null) {
      return Promise.resolve(null)
    }

    if (current.accessToken !== staleToken) {
      return Promise.resolve(current.accessToken)
    }

    if (refreshedToken === staleToken) {
      forget('expired')

      return Promise.resolve(null)
    }

    refreshing ??= refresh(current)

    return refreshing
  }

  return {
    isAuthenticated,
    signIn,
    signInExternal,
    signOut,
    getAccessToken,
    refreshAccessToken,
  }
})
