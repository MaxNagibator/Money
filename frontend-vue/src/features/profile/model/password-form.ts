import type { PasswordChange } from '@/api/adapters/profile'

export const PASSWORD_REQUIRED = 'Обязательно'

export interface PasswordFormValue {
  currentPassword: string
  newPassword: string
}

export interface PasswordFormErrors {
  currentPassword?: string
  newPassword?: string
}

export function newPasswordForm(): PasswordFormValue {
  return { currentPassword: '', newPassword: '' }
}

export function validatePasswordForm(value: PasswordFormValue): PasswordFormErrors {
  const errors: PasswordFormErrors = {}

  if (value.currentPassword.length === 0) {
    errors.currentPassword = PASSWORD_REQUIRED
  }

  if (value.newPassword.length === 0) {
    errors.newPassword = PASSWORD_REQUIRED
  }

  return errors
}

export function toPasswordChange(value: PasswordFormValue): PasswordChange {
  return {
    currentPassword: value.currentPassword,
    newPassword: value.newPassword,
  }
}
