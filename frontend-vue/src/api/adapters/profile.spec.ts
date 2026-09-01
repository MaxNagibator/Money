import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '@/api/adapters/problem'
import {
  changePassword,
  confirmEmail,
  fetchProfile,
  resendConfirmCode,
  toProfile,
} from '@/api/adapters/profile'
import { apiFetch } from '@/platform/http'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

function respondJson(body: unknown, status = 200): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })))
}

function respondEmpty(status = 204): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(null, { status })))
}

function lastRequest(): Request {
  return vi.mocked(apiFetch).mock.calls.at(-1)![0]
}

describe('toProfile', () => {
  it('имя берётся из claim name', () => {
    expect(toProfile({ name: 'Иван', sub: '1' }).name).toBe('Иван')
  })

  it('без claim name и при пустом значении имя равно Anonymous', () => {
    expect(toProfile({ sub: '1' }).name).toBe('Anonymous')
    expect(toProfile({ name: '  ' }).name).toBe('Anonymous')
  })

  it('подтверждением считается только строка true', () => {
    expect(toProfile({ email_verified: 'true' }).isEmailConfirmed).toBe(true)
    expect(toProfile({ email_verified: 'True' }).isEmailConfirmed).toBe(false)
    expect(toProfile({ email_verified: 'false' }).isEmailConfirmed).toBe(false)
    expect(toProfile({}).isEmailConfirmed).toBe(false)
  })

  it('адрес берётся из claim email, а не из имени пользователя', () => {
    expect(toProfile({ name: 'ivan', email: 'ivan@money.test' }).email).toBe('ivan@money.test')
  })

  it('без claim email адреса нет', () => {
    expect(toProfile({ name: 'ivan', email_verified: 'true' }).email).toBeNull()
  })
})

describe('fetchProfile', () => {
  it('профиль читается из userinfo', async () => {
    respondJson({ sub: '1', name: 'ivan', email: 'ivan@money.test', email_verified: 'true' })

    await expect(fetchProfile()).resolves.toEqual({
      name: 'ivan',
      email: 'ivan@money.test',
      isEmailConfirmed: true,
    })

    expect(new URL(lastRequest().url).pathname).toBe('/api/connect/userinfo')
  })
})

describe('confirmEmail', () => {
  it('код уходит на /Accounts/ConfirmEmail', async () => {
    respondEmpty()

    await confirmEmail('123456')

    const request = lastRequest()

    expect(request.method).toBe('POST')
    expect(new URL(request.url).pathname).toBe('/api/Accounts/ConfirmEmail')
    expect(JSON.parse(await request.text())).toEqual({ confirmCode: '123456' })
  })

  it('сообщение сервера доходит до экрана', async () => {
    respondJson({ detail: 'Извините, код подтверждения недействительный.', title: 'Ошибка' }, 400)

    await expect(confirmEmail('000000'))
      .rejects
      .toThrow(new ApiError('Извините, код подтверждения недействительный.'))
  })

  it('без detail показывается title', async () => {
    respondJson({ title: 'Извините, но у вас уже подтвержденный email.' }, 400)

    await expect(confirmEmail('000000'))
      .rejects
      .toThrow(new ApiError('Извините, но у вас уже подтвержденный email.'))
  })
})

describe('resendConfirmCode', () => {
  it('повтор уходит на /Accounts/ResendConfirmCode без тела', async () => {
    respondEmpty()

    await resendConfirmCode()

    const request = lastRequest()

    expect(request.method).toBe('POST')
    expect(new URL(request.url).pathname).toBe('/api/Accounts/ResendConfirmCode')
    expect(await request.text()).toBe('')
  })
})

describe('changePassword', () => {
  it('пароли уходят на /Accounts/ChangePassword', async () => {
    respondEmpty()

    await changePassword({ currentPassword: 'old', newPassword: 'new' })

    const request = lastRequest()

    expect(request.method).toBe('POST')
    expect(new URL(request.url).pathname).toBe('/api/Accounts/ChangePassword')
    expect(JSON.parse(await request.text())).toEqual({ currentPassword: 'old', newPassword: 'new' })
  })

  it('перечень требований Identity доходит до экрана', async () => {
    respondJson({ detail: 'Ошибки: Passwords must have at least one digit.' }, 400)

    await expect(changePassword({ currentPassword: 'old', newPassword: 'new' }))
      .rejects
      .toThrow(new ApiError('Ошибки: Passwords must have at least one digit.'))
  })
})
