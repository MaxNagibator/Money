import { describe, expect, it } from 'vitest'
import { addDays, addMonths, compareLocalDate, fromDate, parseLocalDate, weekdayIndex } from '@/domain/local-date'

describe('parseLocalDate', () => {
  it.each([
    ['2026-08-22'],
    ['2024-02-29'],
    ['2000-01-01'],
  ])('принимает %s', (input) => {
    expect(parseLocalDate(input)).toBe(input)
  })

  it.each([
    ['2026-02-29'],
    ['2026-13-01'],
    ['2026-00-10'],
    ['2026-08-32'],
    ['22.08.2026'],
    ['2026-8-22'],
    [''],
  ])('отвергает %j', (input) => {
    expect(parseLocalDate(input)).toBeNull()
  })
})

describe('addDays', () => {
  it.each([
    ['2026-08-22', 1, '2026-08-23'],
    ['2026-08-31', 1, '2026-09-01'],
    ['2026-12-31', 1, '2027-01-01'],
    ['2026-03-01', -1, '2026-02-28'],
    ['2024-03-01', -1, '2024-02-29'],
  ])('%s + %i дней даёт %s', (input, days, expected) => {
    expect(addDays(parseLocalDate(input)!, days)).toBe(expected)
  })
})

describe('addMonths', () => {
  it.each([
    ['2026-01-31', 1, '2026-02-28'],
    ['2024-01-31', 1, '2024-02-29'],
    ['2026-03-31', -1, '2026-02-28'],
    ['2026-01-15', -1, '2025-12-15'],
    ['2026-02-11', -14, '2024-12-11'],
    ['2026-08-19', 12, '2027-08-19'],
  ])('%s со сдвигом на %i мес. даёт %s', (input, months, expected) => {
    expect(addMonths(parseLocalDate(input)!, months)).toBe(expected)
  })
})

describe('weekdayIndex, неделя с понедельника', () => {
  it.each([
    ['2026-08-17', 0],
    ['2026-08-19', 2],
    ['2026-08-23', 6],
  ])('%s даёт %i', (input, expected) => {
    expect(weekdayIndex(parseLocalDate(input)!)).toBe(expected)
  })
})

describe('прочее', () => {
  it('строит дату из локального Date', () => {
    expect(fromDate(new Date(2026, 7, 22, 23, 59))).toBe('2026-08-22')
  })

  it('сравнивает лексикографически', () => {
    const early = parseLocalDate('2026-01-05')!
    const late = parseLocalDate('2026-11-05')!

    expect(compareLocalDate(early, late)).toBe(-1)
    expect(compareLocalDate(late, early)).toBe(1)
    expect(compareLocalDate(early, early)).toBe(0)
  })
})
