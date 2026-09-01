import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import type { Clock } from '@/platform/clock'
import type { OperationInterval } from './filters'
import { createOperationFilters, useOperationFiltersStore } from './filters'

const { store } = vi.hoisted(() => ({ store: new Map<string, unknown>() }))

vi.mock('@/platform/storage', () => ({
  readSetting: <T>(key: string, fallback: T): T => (store.has(key) ? store.get(key) as T : fallback),
  writeSetting: (key: string, value: unknown): void => {
    store.set(key, value)
  },
  removeSetting: (key: string): void => {
    store.delete(key)
  },
}))

const TODAY = '2026-08-19' as LocalDate

const clock: Clock = {
  now: () => new Date('2026-08-19T12:00:00Z'),
  today: () => TODAY,
}

function setup() {
  return createOperationFilters(clock)
}

function date(value: string): LocalDate {
  return value as LocalDate
}

beforeEach(() => {
  store.clear()
  setActivePinia(createPinia())
})

describe('состояние фильтров операций', () => {
  it('сохраняет даты и интервал, но не текстовые фильтры', () => {
    const first = setup()

    first.filters.from = date('2026-03-02')
    first.filters.to = date('2026-03-08')
    first.filters.interval = 'week'
    first.filters.comment = 'кофе'
    first.filters.place = 'вокзал'
    first.filters.categoryIds = [4, 7]

    const restored = setup().filters

    expect(restored.from).toBe('2026-03-02')
    expect(restored.to).toBe('2026-03-08')
    expect(restored.interval).toBe('week')
    expect(restored.comment).toBe('')
    expect(restored.place).toBe('')
    expect(restored.categoryIds).toEqual([])
  })

  it('игнорирует испорченные значения в хранилище', () => {
    store.set('operations:from', '02.03.2026')
    store.set('operations:to', 17)
    store.set('operations:interval', 'декада')

    const { filters } = setup()

    expect(filters.from).toBeNull()
    expect(filters.to).toBeNull()
    expect(filters.interval).toBeNull()
  })

  it.each<[OperationInterval, LocalDate, LocalDate]>([
    ['day', date('2026-08-19'), date('2026-08-19')],
    ['week', date('2026-08-17'), date('2026-08-23')],
    ['month', date('2026-08-01'), date('2026-08-31')],
    ['year', date('2026-01-01'), date('2026-12-31')],
  ])('интервал %s считает границы от начала диапазона', (interval, from, to) => {
    const state = setup()

    state.filters.from = date('2026-08-19')
    state.filters.to = date('2026-12-31')
    state.applyInterval(interval)

    expect(state.filters.from).toBe(from)
    expect(state.filters.to).toBe(to)
  })

  it('без начала диапазона считает границы от конца', () => {
    const state = setup()

    state.filters.to = date('2026-02-11')
    state.applyInterval('month')

    expect(state.filters.from).toBe('2026-02-01')
    expect(state.filters.to).toBe('2026-02-28')
  })

  it('без дат считает границы от сегодняшнего дня', () => {
    const state = setup()

    state.applyInterval('week')

    expect(state.filters.from).toBe('2026-08-17')
    expect(state.filters.to).toBe('2026-08-23')
  })

  it('сохранённый интервал без конца диапазона считается от уцелевшего начала', () => {
    store.set('operations:from', '2026-02-11')
    store.set('operations:to', null)
    store.set('operations:interval', 'month')

    const { filters } = setup()

    expect(filters.from).toBe('2026-02-01')
    expect(filters.to).toBe('2026-02-28')
    expect(store.get('operations:from')).toBe('2026-02-01')
    expect(store.get('operations:to')).toBe('2026-02-28')
  })

  it('сохранённый интервал без начала диапазона считается от уцелевшего конца', () => {
    store.set('operations:from', null)
    store.set('operations:to', '2026-02-11')
    store.set('operations:interval', 'month')

    const { filters } = setup()

    expect(filters.from).toBe('2026-02-01')
    expect(filters.to).toBe('2026-02-28')
  })

  it('сохранённый интервал без обеих дат считается от сегодняшнего дня', () => {
    store.set('operations:interval', 'month')

    const { filters } = setup()

    expect(filters.from).toBe('2026-08-01')
    expect(filters.to).toBe('2026-08-31')
    expect(store.get('operations:from')).toBe('2026-08-01')
    expect(store.get('operations:to')).toBe('2026-08-31')
  })

  it('снятый интервал переживает перезагрузку', () => {
    const first = setup()

    first.applyInterval('month')
    first.applyInterval(null)

    expect(store.get('operations:interval')).toBeNull()

    const restored = setup().filters

    expect(restored.interval).toBeNull()
    expect(restored.from).toBe('2026-08-01')
    expect(restored.to).toBe('2026-08-31')
  })

  it.each<[OperationInterval, -1 | 1, LocalDate, LocalDate]>([
    ['day', 1, date('2026-08-20'), date('2026-08-20')],
    ['day', -1, date('2026-08-18'), date('2026-08-18')],
    ['week', 1, date('2026-08-24'), date('2026-08-30')],
    ['week', -1, date('2026-08-10'), date('2026-08-16')],
    ['month', 1, date('2026-09-01'), date('2026-09-30')],
    ['month', -1, date('2026-07-01'), date('2026-07-31')],
    ['year', 1, date('2027-01-01'), date('2027-12-31')],
    ['year', -1, date('2025-01-01'), date('2025-12-31')],
  ])('стрелка сдвигает интервал %s на %i', (interval, direction, from, to) => {
    const state = setup()

    state.applyInterval(interval)
    state.shift(direction)

    expect(state.filters.from).toBe(from)
    expect(state.filters.to).toBe(to)
  })

  it('без интервала стрелки не двигают диапазон', () => {
    const state = setup()

    state.filters.from = date('2026-08-19')
    state.filters.to = date('2026-08-25')
    state.shift(1)

    expect(state.filters.from).toBe('2026-08-19')
    expect(state.filters.to).toBe('2026-08-25')
  })

  it('сдвиг месяца от конца месяца не перескакивает через месяц', () => {
    const state = setup()

    state.filters.interval = 'month'
    state.filters.from = date('2026-01-31')
    state.shift(1)

    expect(state.filters.from).toBe('2026-02-28')
    expect(state.filters.to).toBe('2026-02-28')
  })

  it('сброс чистит комментарий, место и категории', () => {
    const state = setup()

    state.applyInterval('month')
    state.filters.comment = 'кофе'
    state.filters.place = 'вокзал'
    state.filters.categoryIds = [4, 7]
    state.filters.showEmptyDays = true

    state.reset()

    expect(state.filters.comment).toBe('')
    expect(state.filters.place).toBe('')
    expect(state.filters.categoryIds).toEqual([])
    expect(state.filters.from).toBe('2026-08-01')
    expect(state.filters.to).toBe('2026-08-31')
    expect(state.filters.interval).toBe('month')
    expect(state.filters.showEmptyDays).toBe(true)
  })
})

describe('общий фильтр списка и статистики', () => {
  it('обращения из разных мест дают одно состояние', () => {
    const first = useOperationFiltersStore()
    const second = useOperationFiltersStore()

    first.filters.comment = 'кофе'
    first.filters.categoryIds = [4, 7]
    first.apply()

    expect(second.filters.comment).toBe('кофе')
    expect(second.filters.categoryIds).toEqual([4, 7])
    expect(second.applied.comment).toBe('кофе')
    expect(second.applied.categoryIds).toEqual([4, 7])
  })

  it('поиск переносит в запрос весь фильтр, а не только даты', () => {
    const state = setup()

    state.filters.from = date('2026-03-02')
    state.filters.comment = 'кофе'
    state.filters.place = 'вокзал'
    state.filters.categoryIds = [4, 7]

    expect(state.applied.comment).toBe('')
    expect(state.applied.categoryIds).toEqual([])

    state.apply()

    expect(state.applied.from).toBe('2026-03-02')
    expect(state.applied.comment).toBe('кофе')
    expect(state.applied.place).toBe('вокзал')
    expect(state.applied.categoryIds).toEqual([4, 7])
  })

  it('правка после поиска не меняет снимок до следующего поиска', () => {
    const state = setup()

    state.filters.categoryIds = [4]
    state.apply()

    state.filters.categoryIds.push(7)
    state.filters.comment = 'кофе'

    expect(state.applied.categoryIds).toEqual([4])
    expect(state.applied.comment).toBe('')
  })

  it('показ пустых дней попадает в снимок без поиска', async () => {
    const state = setup()

    state.filters.showEmptyDays = true
    await nextTick()

    expect(state.applied.showEmptyDays).toBe(true)
  })
})
