import { describe, expect, it } from 'vitest'
import type { CarEvent } from './car'
import { maxMileage, sortCarEvents } from './car'
import type { LocalDate } from './local-date'
import { parseLocalDate } from './local-date'

function event(id: number, date: string, mileage: number | null = null): CarEvent {
  return {
    id,
    carId: 1,
    typeId: 3,
    title: null,
    comment: null,
    mileage,
    date: localDate(date),
    isDeleted: false,
  }
}

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

describe('sortCarEvents', () => {
  it('ставит новые события выше старых', () => {
    const sorted = sortCarEvents([event(1, '2026-01-10'), event(2, '2026-03-01'), event(3, '2025-12-31')])

    expect(sorted.map(item => item.id)).toEqual([2, 1, 3])
  })

  it('внутри одного дня показывает позже созданное первым', () => {
    const sorted = sortCarEvents([event(7, '2026-03-01'), event(12, '2026-03-01'), event(9, '2026-03-01')])

    expect(sorted.map(item => item.id)).toEqual([12, 9, 7])
  })

  it('не меняет исходный список', () => {
    const source = [event(1, '2026-01-10'), event(2, '2026-03-01')]

    sortCarEvents(source)

    expect(source.map(item => item.id)).toEqual([1, 2])
  })
})

describe('maxMileage', () => {
  it('берёт наибольший пробег, а не последний по дате', () => {
    const top = maxMileage([event(1, '2026-01-10', 120_000), event(2, '2026-03-01', 5_000)])

    expect(top).toBe(120_000)
  })

  it('пропускает события без пробега', () => {
    const top = maxMileage([event(1, '2026-01-10', null), event(2, '2026-03-01', 5_000)])

    expect(top).toBe(5_000)
  })

  it('без событий возвращает ноль', () => {
    expect(maxMileage([])).toBe(0)
  })
})
