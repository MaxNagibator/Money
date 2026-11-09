import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { NetworkError } from '@/platform/http'
import { clearApiCache } from '@/platform/pwa'
import type { AuthTokens } from '../api/auth-api'
import { AuthError, requestPasswordGrant, requestRefreshGrant, revokeRefreshToken } from '../api/auth-api'
import { setSessionChangeHandler, useSessionStore } from './session'

const { store } = vi.hoisted(() => ({ store: new Map<string, unknown>() }))

vi.mock('@/platform/storage', () => ({
  readSetting: <T>(key: string, fallback: T): T => (store.has(key) ? store.get(key) as T : fallback),
  writeSetting: (key: string, value: unknown): void => {
    store.set(key, value)
  },
  removeSetting: (key: string): void => {
    store.delete(key)
  },
}))

vi.mock('@/platform/pwa', () => ({
  clearApiCache: vi.fn(() => Promise.resolve(true)),
}))

vi.mock('../api/auth-api', async importOriginal => ({
  ...await importOriginal<typeof import('../api/auth-api')>(),
  requestPasswordGrant: vi.fn(),
  requestRefreshGrant: vi.fn(),
  revokeRefreshToken: vi.fn(),
}))

const TOKENS_KEY = 'auth-tokens'

const requestPasswordGrantMock = vi.mocked(requestPasswordGrant)
const requestRefreshGrantMock = vi.mocked(requestRefreshGrant)
const revokeRefreshTokenMock = vi.mocked(revokeRefreshToken)

let onSessionChange = vi.fn()

beforeEach(() => {
  setActivePinia(createPinia())
  store.clear()
  vi.clearAllMocks()

  onSessionChange = vi.fn()
  setSessionChangeHandler(onSessionChange)
  revokeRefreshTokenMock.mockResolvedValue(undefined)
})

describe('конец сессии', () => {
  it('выход уносит данные прежнего пользователя вместе с токенами', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))

    const session = useSessionStore()

    await session.signOut()

    expect(session.isAuthenticated).toBe(false)
    expect(store.has(TOKENS_KEY)).toBe(false)
    expect(onSessionChange.mock.calls).toEqual([['signedOut']])
  })

  it('несостоявшееся обновление токена завершает сессию как протухшую', async () => {
    store.set(TOKENS_KEY, tokens(-1_000))
    requestRefreshGrantMock.mockRejectedValue(new AuthError('срок истёк'))

    const session = useSessionStore()

    await expect(session.getAccessToken()).resolves.toBeNull()
    expect(session.isAuthenticated).toBe(false)
    expect(onSessionChange.mock.calls).toEqual([['expired']])
  })

  it('живой токен сессию не роняет', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))

    const session = useSessionStore()

    await expect(session.getAccessToken()).resolves.toBe('access')
    expect(onSessionChange).not.toHaveBeenCalled()
  })

  it('удачное обновление токена сменой личности не считается', async () => {
    store.set(TOKENS_KEY, tokens(-1_000))
    requestRefreshGrantMock.mockResolvedValue({ ...tokens(3_600_000), accessToken: 'свежий' })

    const session = useSessionStore()

    await expect(session.getAccessToken()).resolves.toBe('свежий')
    expect(onSessionChange).not.toHaveBeenCalled()
  })
})

describe('отозванный токен', () => {
  it('параллельные отказы 401 обновляют токен один раз', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))
    requestRefreshGrantMock.mockResolvedValue({ ...tokens(3_600_000), accessToken: 'свежий' })

    const session = useSessionStore()

    await expect(Promise.all([session.refreshAccessToken('access'), session.refreshAccessToken('access')]))
      .resolves.toEqual(['свежий', 'свежий'])
    expect(requestRefreshGrantMock).toHaveBeenCalledTimes(1)
    expect(onSessionChange).not.toHaveBeenCalled()
  })

  it('опоздавший отказ получает уже обновлённый токен без нового обновления', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))
    requestRefreshGrantMock.mockResolvedValue({ ...tokens(3_600_000), accessToken: 'свежий' })

    const session = useSessionStore()

    await session.refreshAccessToken('access')

    await expect(session.refreshAccessToken('access')).resolves.toBe('свежий')
    expect(requestRefreshGrantMock).toHaveBeenCalledTimes(1)
  })

  it('несостоявшееся обновление по отказу 401 гасит сессию', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))
    requestRefreshGrantMock.mockRejectedValue(new AuthError('токен отозван'))

    const session = useSessionStore()

    await expect(session.refreshAccessToken('access')).resolves.toBeNull()
    expect(session.isAuthenticated).toBe(false)
    expect(onSessionChange.mock.calls).toEqual([['expired']])
  })

  it('отказ по только что обновлённому токену гасит сессию без второго обновления', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))
    requestRefreshGrantMock.mockResolvedValue({ ...tokens(3_600_000), accessToken: 'свежий' })

    const session = useSessionStore()

    await session.refreshAccessToken('access')

    await expect(session.refreshAccessToken('свежий')).resolves.toBeNull()
    expect(requestRefreshGrantMock).toHaveBeenCalledTimes(1)
    expect(session.isAuthenticated).toBe(false)
    expect(onSessionChange.mock.calls).toEqual([['expired']])
  })

  it('отказ формы входа остаётся ошибкой формы, а не концом сессии', async () => {
    requestPasswordGrantMock.mockRejectedValue(new AuthError('Неверный логин или пароль'))

    const session = useSessionStore()

    await expect(session.signIn('пётр', 'пароль')).rejects.toThrow('Неверный логин или пароль')
    expect(onSessionChange).not.toHaveBeenCalled()
    expect(requestRefreshGrantMock).not.toHaveBeenCalled()
  })

  it('без сессии обновлять нечего', async () => {
    const session = useSessionStore()

    await expect(session.refreshAccessToken('access')).resolves.toBeNull()
    expect(requestRefreshGrantMock).not.toHaveBeenCalled()
    expect(onSessionChange).not.toHaveBeenCalled()
  })
})

describe('смена личности', () => {
  it('вход поверх живой сессии объявляется сменой личности', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))
    requestPasswordGrantMock.mockResolvedValue({ ...tokens(3_600_000), accessToken: 'другой' })

    const session = useSessionStore()

    await session.signIn('пётр', 'пароль')

    expect(onSessionChange.mock.calls).toEqual([['signedIn']])
    await expect(session.getAccessToken()).resolves.toBe('другой')
  })

  it('отказ входа личность не меняет', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))
    requestPasswordGrantMock.mockRejectedValue(new AuthError('Неверный логин или пароль'))

    const session = useSessionStore()

    await expect(session.signIn('пётр', 'пароль')).rejects.toThrow('Неверный логин или пароль')
    expect(onSessionChange).not.toHaveBeenCalled()
    await expect(session.getAccessToken()).resolves.toBe('access')
  })

  it('обновление, начатое до входа, не затирает выданные входом токены', async () => {
    store.set(TOKENS_KEY, tokens(-1_000))

    let completeRefresh: (value: AuthTokens) => void = () => undefined

    requestRefreshGrantMock.mockReturnValue(new Promise<AuthTokens>(resolve => {
      completeRefresh = resolve
    }))

    requestPasswordGrantMock.mockResolvedValue({ ...tokens(3_600_000), accessToken: 'после входа' })

    const session = useSessionStore()
    const pending = session.refreshAccessToken('access')

    await session.signIn('пётр', 'пароль')

    completeRefresh({ ...tokens(3_600_000), accessToken: 'из прежней сессии' })

    await expect(pending).resolves.toBe('после входа')
    await expect(session.getAccessToken()).resolves.toBe('после входа')
    expect(onSessionChange.mock.calls).toEqual([['signedIn']])
  })

  it('вход, ответивший после выхода, сессию не воскрешает и отзывает выданный токен', async () => {
    store.set(TOKENS_KEY, tokens(3_600_000))

    let completeSignIn: (value: AuthTokens) => void = () => undefined

    requestPasswordGrantMock.mockReturnValue(new Promise<AuthTokens>(resolve => {
      completeSignIn = resolve
    }))

    const session = useSessionStore()
    const pending = session.signIn('пётр', 'пароль')

    await session.signOut()

    completeSignIn({ ...tokens(3_600_000), accessToken: 'опоздавший' })
    await pending

    expect(session.isAuthenticated).toBe(false)
    expect(store.has(TOKENS_KEY)).toBe(false)
    expect(onSessionChange.mock.calls).toEqual([['signedOut']])
    expect(revokeRefreshTokenMock).toHaveBeenCalledTimes(2)
  })

  it('обновление, отказавшее после входа, свежую сессию не гасит', async () => {
    store.set(TOKENS_KEY, tokens(-1_000))

    let failRefresh: (reason: Error) => void = () => undefined

    requestRefreshGrantMock.mockReturnValue(new Promise<AuthTokens>((_, reject) => {
      failRefresh = reject
    }))

    requestPasswordGrantMock.mockResolvedValue({ ...tokens(3_600_000), accessToken: 'после входа' })

    const session = useSessionStore()
    const pending = session.refreshAccessToken('access')

    await session.signIn('пётр', 'пароль')

    failRefresh(new AuthError('срок истёк'))

    await expect(pending).resolves.toBe('после входа')
    expect(session.isAuthenticated).toBe(true)
    expect(onSessionChange.mock.calls).toEqual([['signedIn']])
  })
})

describe('сохранённые ответы API', () => {
  it.each([
    ['выход', 'signedOut', (session: ReturnType<typeof useSessionStore>) => session.signOut()],
    ['протухшая сессия', 'expired', async (session: ReturnType<typeof useSessionStore>) => {
      requestRefreshGrantMock.mockRejectedValue(new AuthError('срок истёк'))
      await session.refreshAccessToken('access')
    }],
  ])('%s стирает их, чтобы следующий пользователь устройства не увидел чужого', async (_case, change, end) => {
    store.set(TOKENS_KEY, tokens(3_600_000))

    const session = useSessionStore()

    await end(session)

    expect(vi.mocked(clearApiCache)).toHaveBeenCalledTimes(1)
    expect(onSessionChange.mock.calls).toEqual([[change]])
  })

  it('вход стирает их до того, как новая сессия начнёт запросы', async () => {
    requestPasswordGrantMock.mockResolvedValue(tokens(3_600_000))

    const session = useSessionStore()

    await session.signIn('пётр', 'пароль')

    const cleared = vi.mocked(clearApiCache).mock.invocationCallOrder[0] ?? Infinity
    const announced = onSessionChange.mock.invocationCallOrder[0] ?? -Infinity

    expect(cleared).toBeLessThan(announced)
  })

  it.each([
    ['обрыв сети', new NetworkError('нет сети')],
    ['отказ прокси при лежащем API', new AuthError('Bad Gateway', 502)],
  ])('%s при обновлении токена сессию не гасит и сохранённое не стирает', async (_case, failure) => {
    store.set(TOKENS_KEY, tokens(-1_000))
    requestRefreshGrantMock.mockRejectedValue(failure)

    const session = useSessionStore()

    await expect(session.getAccessToken()).resolves.toBe('access')
    expect(session.isAuthenticated).toBe(true)
    expect(onSessionChange).not.toHaveBeenCalled()
    expect(vi.mocked(clearApiCache)).not.toHaveBeenCalled()
  })
})

function tokens(expiresIn: number): AuthTokens {
  return {
    accessToken: 'access',
    refreshToken: 'refresh',
    expiresAt: Date.now() + expiresIn,
  }
}
