import type { AccountRegistration } from '@/api/adapters/accounts'
import { trimToNull } from '@/domain/text'

export const USER_NAME_REQUIRED = 'Логин обязателен.'
export const EMAIL_INVALID = 'Некорректный email.'
export const PASSWORD_REQUIRED = 'Пароль обязателен.'
export const CONFIRM_MISMATCH = 'Пароли не совпадают.'

export interface RegisterFormValue {
  userName: string
  email: string
  password: string
  confirmPassword: string
  useConfirmPassword: boolean
}

export interface RegisterFormErrors {
  userName?: string
  email?: string
  password?: string
  confirmPassword?: string
}

export function newRegisterForm(): RegisterFormValue {
  return {
    userName: '',
    email: '',
    password: '',
    confirmPassword: '',
    useConfirmPassword: true,
  }
}

export function validateRegisterForm(value: RegisterFormValue): RegisterFormErrors {
  const errors: RegisterFormErrors = {}
  const email = trimToNull(value.email)

  if (trimToNull(value.userName) === null) {
    errors.userName = USER_NAME_REQUIRED
  }

  if (email !== null && !isEmail(email)) {
    errors.email = EMAIL_INVALID
  }

  if (trimToNull(value.password) === null) {
    errors.password = PASSWORD_REQUIRED
  }

  if (value.useConfirmPassword && value.password !== value.confirmPassword) {
    errors.confirmPassword = CONFIRM_MISMATCH
  }

  return errors
}

export function toRegistration(value: RegisterFormValue): AccountRegistration {
  return {
    userName: value.userName.trim(),
    email: trimToNull(value.email),
    password: value.password,
  }
}

function isEmail(value: string): boolean {
  const at = value.indexOf('@')

  return at > 0 && at !== value.length - 1 && at === value.lastIndexOf('@')
}
