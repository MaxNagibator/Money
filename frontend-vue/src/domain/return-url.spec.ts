import { describe, expect, it } from 'vitest'
import {
  DEFAULT_RETURN_URL,
  readSafeReturnUrl,
  readSignedInReturnUrl,
  SIGNED_IN_RETURN_URL,
} from './return-url'

describe('readSafeReturnUrl', () => {
  it.each([
    '/debts',
    '/operations?category=1',
    '/cars#events',
  ])('свой путь %s сохраняется', value => {
    expect(readSafeReturnUrl(value)).toBe(value)
  })

  it.each([
    '//evil.test/steal',
    '/\\evil.test/steal',
    'https://evil.test/steal',
    'javascript:alert(1)',
    'debts',
    '',
  ])('чужой адрес %s заменяется главной', value => {
    expect(readSafeReturnUrl(value)).toBe(DEFAULT_RETURN_URL)
  })

  it.each([undefined, null, ['/debts', '/cars']])('нестроковое значение %s заменяется главной', value => {
    expect(readSafeReturnUrl(value)).toBe(DEFAULT_RETURN_URL)
  })

  it('запасной адрес задаётся вызывающим', () => {
    expect(readSafeReturnUrl('//evil.test', '/operations')).toBe('/operations')
  })
})

describe('readSignedInReturnUrl', () => {
  it('вход без returnUrl открывает операции', () => {
    expect(SIGNED_IN_RETURN_URL).toBe('/operations')
    expect(readSignedInReturnUrl(undefined)).toBe(SIGNED_IN_RETURN_URL)
  })

  it('свой путь из returnUrl сохраняется', () => {
    expect(readSignedInReturnUrl('/debts')).toBe('/debts')
  })

  it.each(['//evil.test/steal', null, ''])('чужой и пустой returnUrl %s заменяются главной', value => {
    expect(readSignedInReturnUrl(value)).toBe(DEFAULT_RETURN_URL)
  })
})
