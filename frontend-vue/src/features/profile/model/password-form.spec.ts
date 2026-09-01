import { describe, expect, it } from 'vitest'
import { newPasswordForm, PASSWORD_REQUIRED, toPasswordChange, validatePasswordForm } from './password-form'

describe('validatePasswordForm', () => {
  it('оба поля обязательны', () => {
    expect(validatePasswordForm(newPasswordForm())).toEqual({
      currentPassword: PASSWORD_REQUIRED,
      newPassword: PASSWORD_REQUIRED,
    })

    expect(validatePasswordForm({ currentPassword: 'old', newPassword: '' }))
      .toEqual({ newPassword: PASSWORD_REQUIRED })
  })

  it('сложность нового пароля клиент не проверяет', () => {
    expect(validatePasswordForm({ currentPassword: 'old', newPassword: '1' })).toEqual({})
  })
})

describe('toPasswordChange', () => {
  it('пробелы в пароле сохраняются', () => {
    expect(toPasswordChange({ currentPassword: ' old ', newPassword: ' new ' }))
      .toEqual({ currentPassword: ' old ', newPassword: ' new ' })
  })
})
