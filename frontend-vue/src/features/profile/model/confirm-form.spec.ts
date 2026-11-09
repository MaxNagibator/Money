import { describe, expect, it } from 'vitest'
import {
  CONFIRM_CODE_LENGTH_MESSAGE,
  CONFIRM_CODE_REQUIRED,
  newConfirmForm,
  toConfirmCode,
  validateConfirmForm,
} from './confirm-form'

describe('validateConfirmForm', () => {
  it('пустой код обязателен', () => {
    expect(validateConfirmForm(newConfirmForm())).toEqual({ code: CONFIRM_CODE_REQUIRED })
    expect(validateConfirmForm({ code: '   ' })).toEqual({ code: CONFIRM_CODE_REQUIRED })
  })

  it('код короче или длиннее шести символов отвергается', () => {
    expect(validateConfirmForm({ code: '12345' })).toEqual({ code: CONFIRM_CODE_LENGTH_MESSAGE })
    expect(validateConfirmForm({ code: '1234567' })).toEqual({ code: CONFIRM_CODE_LENGTH_MESSAGE })
  })

  it('код из шести символов проходит', () => {
    expect(validateConfirmForm({ code: '123456' })).toEqual({})
    expect(validateConfirmForm({ code: ' 123456 ' })).toEqual({})
  })
})

describe('toConfirmCode', () => {
  it('код уходит без окружающих пробелов', () => {
    expect(toConfirmCode({ code: ' 123456 ' })).toBe('123456')
  })
})
