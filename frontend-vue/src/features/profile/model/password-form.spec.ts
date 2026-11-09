import { describe, expect, it } from 'vitest'
import { FIELD_REQUIRED, newPasswordForm, toPasswordChange, validatePasswordForm } from './password-form'

describe('validatePasswordForm', () => {
  it('оба поля обязательны', () => {
    expect(validatePasswordForm(newPasswordForm())).toEqual({
      currentPassword: FIELD_REQUIRED,
      newPassword: FIELD_REQUIRED,
    })

    expect(validatePasswordForm({ currentPassword: 'old', newPassword: '' }))
      .toEqual({ newPassword: FIELD_REQUIRED })
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
