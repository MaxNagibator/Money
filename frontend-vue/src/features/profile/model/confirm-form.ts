export const CONFIRM_CODE_LENGTH = 6
export const CONFIRM_CODE_REQUIRED = 'Пожалуйста, введите код из письма'
export const CONFIRM_CODE_LENGTH_MESSAGE = 'Код должен содержать 6 символов'

export interface ConfirmFormValue {
  code: string
}

export interface ConfirmFormErrors {
  code?: string
}

export function newConfirmForm(): ConfirmFormValue {
  return { code: '' }
}

export function validateConfirmForm(value: ConfirmFormValue): ConfirmFormErrors {
  const code = value.code.trim()

  if (code.length === 0) {
    return { code: CONFIRM_CODE_REQUIRED }
  }

  if (code.length !== CONFIRM_CODE_LENGTH) {
    return { code: CONFIRM_CODE_LENGTH_MESSAGE }
  }

  return {}
}

export function toConfirmCode(value: ConfirmFormValue): string {
  return value.code.trim()
}
