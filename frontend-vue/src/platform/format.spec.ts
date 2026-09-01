import { describe, expect, it } from 'vitest'
import { Money } from '@/domain/money'
import { formatMoneyInput, formatMoneyPlain } from '@/platform/format'

function money(raw: string): Money {
  const value = Money.parseInput(raw)

  if (value === null) {
    throw new Error(`Не разобрана сумма ${raw}`)
  }

  return value
}

describe('formatMoneyPlain', () => {
  it.each([
    ['100,50', '100,50'],
    ['100,05', '100,05'],
    ['100,5', '100,50'],
    ['100', '100'],
    ['-0,4', '-0,40'],
    ['1234,567', '1234,567'],
  ])('печатает %j как %j', (input, expected) => {
    expect(formatMoneyPlain(money(input))).toBe(expected)
  })
})

describe('formatMoneyInput', () => {
  it.each([
    ['100,50', '100,5'],
    ['5556', '5556'],
    ['0,1', '0,1'],
  ])('готовит %j для числового поля как %j', (input, expected) => {
    expect(formatMoneyInput(money(input))).toBe(expected)
  })
})
