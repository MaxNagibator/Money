import { describe, expect, it } from 'vitest'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { formatPeriod } from './period'

function date(value: string): LocalDate {
  const parsed = parseLocalDate(value)

  if (parsed === null) {
    throw new Error(`Некорректная дата: ${value}`)
  }

  return parsed
}

describe('formatPeriod', () => {
  it.each([
    ['2026-02-01', '2026-02-28', 'Период с 1 февраля 2026 по 28 февраля 2026'],
    ['2026-01-09', '2026-12-31', 'Период с 9 января 2026 по 31 декабря 2026'],
  ])('склеивает границы диапазона: %s .. %s', (from, to, expected) => {
    expect(formatPeriod(date(from), date(to))).toBe(expected)
  })

  it('ставит прочерк вместо незаданной границы', () => {
    expect(formatPeriod(null, null)).toBe('Период с - по -')
  })
})
