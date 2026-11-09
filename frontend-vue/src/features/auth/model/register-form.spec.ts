import { describe, expect, it } from 'vitest'
import type { RegisterFormValue } from './register-form'
import {
  CONFIRM_MISMATCH,
  EMAIL_INVALID,
  newRegisterForm,
  PASSWORD_REQUIRED,
  toRegistration,
  USER_NAME_REQUIRED,
  validateRegisterForm,
} from './register-form'

function form(patch: Partial<RegisterFormValue> = {}): RegisterFormValue {
  return { ...newRegisterForm(), ...patch }
}

function filled(patch: Partial<RegisterFormValue> = {}): RegisterFormValue {
  return form({ userName: 'ivan', password: 'тайна', confirmPassword: 'тайна', ...patch })
}

describe('validateRegisterForm', () => {
  it('пустая форма требует логин и пароль, почту не требует', () => {
    expect(validateRegisterForm(form())).toEqual({
      userName: USER_NAME_REQUIRED,
      password: PASSWORD_REQUIRED,
    })
  })

  it('пробелы логином и паролем не считаются', () => {
    expect(validateRegisterForm(form({ userName: '   ', password: '   ', confirmPassword: '   ' }))).toEqual({
      userName: USER_NAME_REQUIRED,
      password: PASSWORD_REQUIRED,
    })
  })

  it.each<[string, boolean]>([
    ['ivan@money.test', true],
    ['ivan@money@test', false],
    ['ivan.money.test', false],
    ['@money.test', false],
    ['ivan@', false],
  ])('почта %s проверяется', (email, isValid) => {
    const errors = validateRegisterForm(filled({ email }))

    expect(errors.email).toBe(isValid ? undefined : EMAIL_INVALID)
  })

  it('несовпадение подтверждения ловится до отправки', () => {
    expect(validateRegisterForm(filled({ confirmPassword: 'другая' })))
      .toEqual({ confirmPassword: CONFIRM_MISMATCH })
  })

  it('показанный пароль отменяет подтверждение', () => {
    expect(validateRegisterForm(filled({ confirmPassword: '', useConfirmPassword: false }))).toEqual({})
  })
})

describe('toRegistration', () => {
  it('логин обрезается, пустая почта уходит как null, пароль сохраняется дословно', () => {
    expect(toRegistration(filled({ userName: '  ivan  ', email: '  ', password: ' тайна ' }))).toEqual({
      userName: 'ivan',
      email: null,
      password: ' тайна ',
    })
  })
})
