import { apiClient } from '@/api/transport/client'
import { trimToNull } from '@/domain/text'
import { fail } from './problem'

export const ANONYMOUS_NAME = 'Anonymous'

export const PROFILE_QUERY_KEY = ['profile']

export const PROFILE_LOAD_ERROR = 'Не удалось загрузить профиль'
export const CONFIRM_EMAIL_ERROR = 'Не удалось подтвердить почту'
export const RESEND_CODE_ERROR = 'Не удалось отправить код'
export const CHANGE_PASSWORD_ERROR = 'Не удалось поменять пароль'

export interface UserProfile {
  name: string
  email: string | null
  isEmailConfirmed: boolean
}

export interface PasswordChange {
  currentPassword: string
  newPassword: string
}

export async function fetchProfile(): Promise<UserProfile> {
  const { data, error } = await apiClient.GET('/connect/userinfo')

  fail(error, PROFILE_LOAD_ERROR)

  return toProfile(data ?? {})
}

export async function confirmEmail(confirmCode: string): Promise<void> {
  const { error } = await apiClient.POST('/Accounts/ConfirmEmail', { body: { confirmCode } })

  fail(error, CONFIRM_EMAIL_ERROR)
}

export async function resendConfirmCode(): Promise<void> {
  const { error } = await apiClient.POST('/Accounts/ResendConfirmCode')

  fail(error, RESEND_CODE_ERROR)
}

export async function changePassword(change: PasswordChange): Promise<void> {
  const { error } = await apiClient.POST('/Accounts/ChangePassword', { body: change })

  fail(error, CHANGE_PASSWORD_ERROR)
}

export function toProfile(claims: Record<string, string>): UserProfile {
  return {
    name: trimToNull(claims.name ?? null) ?? ANONYMOUS_NAME,
    email: trimToNull(claims.email ?? null),
    isEmailConfirmed: claims.email_verified === 'true',
  }
}
