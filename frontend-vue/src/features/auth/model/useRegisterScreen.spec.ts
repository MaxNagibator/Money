import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { registerAccount } from '@/api/adapters/accounts'
import { ApiError } from '@/api/adapters/problem'
import { AuthError, requestPasswordGrant } from '../api/auth-api'
import type { RegisterScreenState } from './useRegisterScreen'
import { REGISTER_FAILURE, SIGN_IN_AFTER_REGISTER_FAILURE, useRegisterScreen } from './useRegisterScreen'

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

vi.mock('@/api/adapters/accounts', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/accounts')>(),
  registerAccount: vi.fn(),
}))

vi.mock('../api/auth-api', async importOriginal => ({
  ...await importOriginal<typeof import('../api/auth-api')>(),
  requestPasswordGrant: vi.fn(),
  revokeRefreshToken: vi.fn(),
}))

const registerAccountMock = vi.mocked(registerAccount)
const requestPasswordGrantMock = vi.mocked(requestPasswordGrant)

beforeEach(() => {
  setActivePinia(createPinia())
  store.clear()
  vi.clearAllMocks()

  registerAccountMock.mockResolvedValue(undefined)
  requestPasswordGrantMock.mockResolvedValue({
    accessToken: 'access',
    refreshToken: 'refresh',
    expiresAt: Date.now() + 3_600_000,
  })
})

function fill(screen: RegisterScreenState): void {
  screen.form.value.userName = 'ivan'
  screen.form.value.email = 'ivan@money.test'
  screen.form.value.password = 'тайна'
  screen.form.value.confirmPassword = 'тайна'
}

describe('регистрация', () => {
  it('создаёт аккаунт и сразу входит', async () => {
    const screen = useRegisterScreen()

    fill(screen)

    await expect(screen.submit()).resolves.toBe(true)

    expect(registerAccountMock).toHaveBeenCalledWith({ userName: 'ivan', email: 'ivan@money.test', password: 'тайна' })
    expect(requestPasswordGrantMock).toHaveBeenCalledWith('ivan', 'тайна')
    expect(screen.notice.value).toBeNull()
  })

  it('негодная форма не уходит на сервер', async () => {
    const screen = useRegisterScreen()

    screen.form.value.userName = 'ivan'

    await expect(screen.submit()).resolves.toBe(false)

    expect(registerAccountMock).not.toHaveBeenCalled()
    expect(screen.errors.value.password).toBeDefined()
  })

  it('ошибка сервера остаётся на форме', async () => {
    registerAccountMock.mockRejectedValue(new ApiError('Логин занят'))

    const screen = useRegisterScreen()

    fill(screen)

    await expect(screen.submit()).resolves.toBe(false)

    expect(screen.notice.value).toEqual({ text: `${REGISTER_FAILURE} Логин занят`, type: 'error' })
    expect(requestPasswordGrantMock).not.toHaveBeenCalled()
  })

  it('после неудачного входа повторная отправка не регистрирует второй раз', async () => {
    requestPasswordGrantMock.mockRejectedValueOnce(new AuthError('Сервис недоступен'))

    const screen = useRegisterScreen()

    fill(screen)

    await expect(screen.submit()).resolves.toBe(false)

    expect(screen.notice.value)
      .toEqual({ text: `${SIGN_IN_AFTER_REGISTER_FAILURE} Сервис недоступен`, type: 'warning' })

    await expect(screen.submit()).resolves.toBe(true)

    expect(registerAccountMock).toHaveBeenCalledTimes(1)
    expect(requestPasswordGrantMock).toHaveBeenCalledTimes(2)
  })

  it('смена логина после неудачного входа регистрирует заново', async () => {
    requestPasswordGrantMock.mockRejectedValueOnce(new AuthError('Сервис недоступен'))

    const screen = useRegisterScreen()

    fill(screen)

    await screen.submit()

    screen.form.value.userName = 'petr'

    await expect(screen.submit()).resolves.toBe(true)

    expect(registerAccountMock).toHaveBeenCalledTimes(2)
    expect(registerAccountMock).toHaveBeenLastCalledWith({
      userName: 'petr',
      email: 'ivan@money.test',
      password: 'тайна',
    })
  })
})
