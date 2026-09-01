import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { flushPromises, mount } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { defineComponent } from 'vue'
import { ApiError } from '@/api/adapters/problem'
import type { UserProfile } from '@/api/adapters/profile'
import { changePassword, confirmEmail, fetchProfile, resendConfirmCode } from '@/api/adapters/profile'
import { parseLocalDate } from '@/domain/local-date'
import type { Clock } from '@/platform/clock'
import type { ProfileScreenState } from './useProfileScreen'
import {
  CODE_SENT_NOTICE,
  EMAIL_CONFIRMED_NOTICE,
  PASSWORD_CHANGED_NOTICE,
  RESEND_COOLDOWN_SECONDS,
  useProfileScreen,
} from './useProfileScreen'

vi.mock('@/api/adapters/profile', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/profile')>(),
  fetchProfile: vi.fn(),
  confirmEmail: vi.fn(),
  resendConfirmCode: vi.fn(),
  changePassword: vi.fn(),
}))

const fetchProfileMock = vi.mocked(fetchProfile)
const confirmEmailMock = vi.mocked(confirmEmail)
const resendConfirmCodeMock = vi.mocked(resendConfirmCode)
const changePasswordMock = vi.mocked(changePassword)

const clock: Clock = {
  now: () => new Date(),
  today: () => parseLocalDate('2026-03-01')!,
}

let storedProfile: UserProfile = unconfirmed()

beforeEach(() => {
  vi.useFakeTimers()
  vi.clearAllMocks()

  storedProfile = unconfirmed()

  fetchProfileMock.mockImplementation(() => Promise.resolve({ ...storedProfile }))
  confirmEmailMock.mockImplementation(() => {
    storedProfile = { ...storedProfile, isEmailConfirmed: true }

    return Promise.resolve()
  })
  resendConfirmCodeMock.mockResolvedValue(undefined)
  changePasswordMock.mockResolvedValue(undefined)
})

afterEach(() => {
  vi.useRealTimers()
})

describe('загрузка профиля', () => {
  it('до ответа сервера показывается Anonymous без подтверждения', async () => {
    fetchProfileMock.mockReturnValue(defer<UserProfile>().promise)

    const { screen } = setup()

    expect(screen.profile.isLoading.value).toBe(true)
    expect(screen.name.value).toBe('Anonymous')
    expect(screen.email.value).toBeNull()
    expect(screen.isEmailConfirmed.value).toBe(false)

    await flushPromises()
  })

  it('после ответа имя и адрес берутся из профиля', async () => {
    storedProfile = { name: 'ivan', email: 'ivan@money.test', isEmailConfirmed: true }

    const { screen } = await settled()

    expect(screen.name.value).toBe('ivan')
    expect(screen.email.value).toBe('ivan@money.test')
    expect(screen.isEmailConfirmed.value).toBe(true)
  })
})

describe('подтверждение почты', () => {
  it('пустой код не уходит на сервер', async () => {
    const { screen } = await settled()

    await screen.submitConfirm()

    expect(confirmEmailMock).not.toHaveBeenCalled()
    expect(screen.confirmErrors.value.code).toBe('Пожалуйста, введите код из письма')
  })

  it('код не из шести символов не уходит на сервер', async () => {
    const { screen } = await settled()

    screen.confirmForm.value.code = '12345'
    await screen.submitConfirm()

    expect(confirmEmailMock).not.toHaveBeenCalled()
    expect(screen.confirmErrors.value.code).toBe('Код должен содержать 6 символов')
  })

  it('успех поднимает флаг подтверждения и перечитывает профиль', async () => {
    const { screen } = await settled()

    screen.confirmForm.value.code = '123456'
    await screen.submitConfirm()
    await flushPromises()

    expect(confirmEmailMock).toHaveBeenCalledWith('123456')
    expect(screen.isEmailConfirmed.value).toBe(true)
    expect(screen.notice.value).toEqual({ text: EMAIL_CONFIRMED_NOTICE, type: 'success' })
    expect(fetchProfileMock.mock.calls.length).toBeGreaterThan(1)
  })

  it('успех останавливает отсчёт до повторной отправки', async () => {
    const { screen } = await settled()

    await screen.resendCode()

    expect(screen.resendSecondsLeft.value).toBe(RESEND_COOLDOWN_SECONDS)

    screen.confirmForm.value.code = '123456'
    await screen.submitConfirm()

    expect(screen.resendSecondsLeft.value).toBe(0)
  })

  it('ошибка сервера показывается тостом с текстом ответа', async () => {
    confirmEmailMock.mockRejectedValue(new ApiError('Извините, код подтверждения недействительный.'))

    const { screen } = await settled()

    screen.confirmForm.value.code = '000000'
    await screen.submitConfirm()

    expect(screen.notice.value).toEqual({
      text: 'Извините, код подтверждения недействительный.',
      type: 'warning',
    })
    expect(screen.isEmailConfirmed.value).toBe(false)
  })
})

describe('повторная отправка кода', () => {
  it('успех запускает отсчёт и меняет подпись кнопки', async () => {
    const { screen } = await settled()

    await screen.resendCode()

    expect(screen.notice.value).toEqual({ text: CODE_SENT_NOTICE, type: 'success' })
    expect(screen.resendLabel.value).toBe('Отправить код (30 сек)')
    expect(screen.canResend.value).toBe(false)

    vi.advanceTimersByTime(1000)

    expect(screen.resendLabel.value).toBe('Отправить код (29 сек)')
    expect(screen.canResend.value).toBe(false)
  })

  it('кнопка заблокирована, пока идёт запрос', async () => {
    const request = defer<void>()

    resendConfirmCodeMock.mockReturnValue(request.promise)

    const { screen } = await settled()
    const pending = screen.resendCode()

    expect(screen.isResending.value).toBe(true)
    expect(screen.canResend.value).toBe(false)
    expect(screen.resendSecondsLeft.value).toBe(0)

    request.resolve()
    await pending

    expect(screen.isResending.value).toBe(false)
    expect(screen.resendSecondsLeft.value).toBe(RESEND_COOLDOWN_SECONDS)
  })

  it('на тридцатой секунде кнопка снова доступна', async () => {
    const { screen } = await settled()

    await screen.resendCode()

    vi.advanceTimersByTime(RESEND_COOLDOWN_SECONDS * 1000)

    expect(screen.resendSecondsLeft.value).toBe(0)
    expect(screen.canResend.value).toBe(true)
    expect(screen.resendLabel.value).toBe('Отправить код')
  })

  it('ошибка отправки не запирает кнопку на полминуты', async () => {
    resendConfirmCodeMock.mockRejectedValue(new ApiError('Простите, но у вас не заполнен email.'))

    const { screen } = await settled()

    await screen.resendCode()

    expect(screen.notice.value).toEqual({ text: 'Простите, но у вас не заполнен email.', type: 'warning' })
    expect(screen.resendSecondsLeft.value).toBe(0)
    expect(screen.canResend.value).toBe(true)
  })

  it('повтор уходит без параметров', async () => {
    const { screen } = await settled()

    await screen.resendCode()

    expect(resendConfirmCodeMock).toHaveBeenCalledWith()
  })
})

describe('смена пароля', () => {
  it('пустые поля не уходят на сервер', async () => {
    const { screen } = await settled()

    await screen.submitPassword()

    expect(changePasswordMock).not.toHaveBeenCalled()
    expect(screen.passwordErrors.value).toEqual({
      currentPassword: 'Обязательно',
      newPassword: 'Обязательно',
    })
  })

  it('успех очищает оба поля', async () => {
    const { screen } = await settled()

    screen.passwordForm.value = { currentPassword: 'old', newPassword: 'new' }
    await screen.submitPassword()

    expect(changePasswordMock).toHaveBeenCalledWith({ currentPassword: 'old', newPassword: 'new' })
    expect(screen.passwordForm.value).toEqual({ currentPassword: '', newPassword: '' })
    expect(screen.notice.value).toEqual({ text: PASSWORD_CHANGED_NOTICE, type: 'success' })
  })

  it('ошибка сохраняет введённые пароли', async () => {
    changePasswordMock.mockRejectedValue(new ApiError('Неверное имя пользователя или пароль.'))

    const { screen } = await settled()

    screen.passwordForm.value = { currentPassword: 'old', newPassword: 'new' }
    await screen.submitPassword()

    expect(screen.passwordForm.value).toEqual({ currentPassword: 'old', newPassword: 'new' })
    expect(screen.notice.value).toEqual({ text: 'Неверное имя пользователя или пароль.', type: 'warning' })
  })
})

describe('состояние отправки', () => {
  it('отправка кода не блокирует смену пароля', async () => {
    const request = defer<void>()

    resendConfirmCodeMock.mockReturnValue(request.promise)

    const { screen } = await settled()
    const pending = screen.resendCode()

    screen.passwordForm.value = { currentPassword: 'old', newPassword: 'new' }
    await screen.submitPassword()

    expect(screen.isResending.value).toBe(true)
    expect(screen.isChangingPassword.value).toBe(false)
    expect(changePasswordMock).toHaveBeenCalledOnce()

    request.resolve()
    await pending
  })

  it('подтверждение кода не блокирует смену пароля', async () => {
    const request = defer<void>()

    confirmEmailMock.mockReturnValue(request.promise)

    const { screen } = await settled()

    screen.confirmForm.value.code = '123456'

    const pending = screen.submitConfirm()

    expect(screen.isConfirming.value).toBe(true)
    expect(screen.isChangingPassword.value).toBe(false)

    screen.passwordForm.value = { currentPassword: 'old', newPassword: 'new' }
    await screen.submitPassword()

    expect(changePasswordMock).toHaveBeenCalledOnce()
    expect(screen.isConfirming.value).toBe(true)

    request.resolve()
    await pending
  })
})

describe('уход со страницы', () => {
  it('размонтирование останавливает отсчёт', async () => {
    const startTimer = vi.spyOn(globalThis, 'setInterval')
    const stopTimer = vi.spyOn(globalThis, 'clearInterval')

    const { screen, unmount } = await settled()

    await screen.resendCode()

    const ticker = startTimer.mock.results.at(-1)!.value as number

    expect(startTimer).toHaveBeenCalledOnce()

    unmount()

    expect(stopTimer).toHaveBeenCalledWith(ticker)
    expect(screen.resendSecondsLeft.value).toBe(0)
  })
})

function setup(): { screen: ProfileScreenState, unmount: () => void } {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  let screen!: ProfileScreenState

  const wrapper = mount(
    defineComponent({
      setup() {
        screen = useProfileScreen(clock)

        return () => null
      },
    }),
    { global: { plugins: [[VueQueryPlugin, { queryClient }]] } },
  )

  let isMounted = true

  const unmount = (): void => {
    if (isMounted) {
      wrapper.unmount()
      isMounted = false
    }
  }

  onTestFinished(() => {
    unmount()
    queryClient.clear()
  })

  return { screen, unmount }
}

async function settled(): Promise<{ screen: ProfileScreenState, unmount: () => void }> {
  const mounted = setup()

  await flushPromises()

  return mounted
}

function unconfirmed(): UserProfile {
  return { name: 'ivan', email: 'ivan@money.test', isEmailConfirmed: false }
}

function defer<T>(): { promise: Promise<T>, resolve: (value: T) => void } {
  let resolve!: (value: T) => void

  const promise = new Promise<T>(settleWith => {
    resolve = settleWith
  })

  return { promise, resolve }
}
