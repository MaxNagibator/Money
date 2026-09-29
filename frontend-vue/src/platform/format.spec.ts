import { describe, expect, it } from 'vitest'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { formatByteSize, formatLocalDateDayMonth,formatLocalDateWithWeekday, formatMoneyInput, formatMoneyPlain } from '@/platform/format'

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
    ['100,5', '100,5'],
    ['100', '100'],
    ['-0,4', '-0,4'],
    ['1234,567', '1234,567'],
  ])('печатает введённое %j как %j', (input, expected) => {
    expect(formatMoneyPlain(money(input))).toBe(expected)
  })

  it.each([
    ['100.00', '100,00'],
    ['100.5', '100,5'],
    ['100', '100'],
    ['-0.4', '-0,4'],
    ['1234.567', '1234,567'],
  ])('печатает сумму ответа %j как %j', (literal, expected) => {
    expect(formatMoneyPlain(Money.fromWire(literal))).toBe(expected)
  })

  it('сумму без масштаба печатает не короче копеек', () => {
    expect(formatMoneyPlain(money('0,1').add(money('0,2')))).toBe('0,30')
    expect(formatMoneyPlain(money('0,5').add(money('0,5')))).toBe('1')
  })
})

describe('formatMoneyInput', () => {
  it.each([
    ['100,50', '100,50'],
    ['5556', '5556'],
    ['0,1', '0,1'],
  ])('готовит %j для числового поля как %j', (input, expected) => {
    expect(formatMoneyInput(money(input))).toBe(expected)
  })

  it('сумму без масштаба готовит по её записи', () => {
    expect(formatMoneyInput(Money.fromMinorUnits(150))).toBe('1,5')
  })
})

describe('formatLocalDateDayMonth', () => {
  it.each([
    ['2026-09-26', '26.09'],
    ['2027-01-04', '04.01'],
  ])('печатает %j как %j', (value, expected) => {
    expect(formatLocalDateDayMonth(value as LocalDate)).toBe(expected)
  })
})

describe('formatLocalDateWithWeekday', () => {
  it.each([
    ['2026-09-17', '17 сентября, чт'],
    ['2026-09-14', '14 сентября, пн'],
    ['2026-01-04', '4 января, вс'],
    ['2024-02-29', '29 февраля, чт'],
  ])('печатает %j как %j', (value, expected) => {
    expect(formatLocalDateWithWeekday(value as LocalDate)).toBe(expected)
  })
})

describe('formatByteSize', () => {
  it.each([
    [0, '0 Б'],
    [1023, '1 023 Б'],
    [1536, '1,5 КБ'],
    [5_242_880, '5 МБ'],
    [2 ** 50, '1 024 ТБ'],
  ])('печатает %j как %j', (value, expected) => {
    expect(formatByteSize(value).replace(/\s/gu, ' ')).toBe(expected)
  })
})
