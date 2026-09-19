import { defineStore } from 'pinia'
import { reactive, watch } from 'vue'
import type { OperationFilter } from '@/api/adapters/operations'
import type { LocalDate } from '@/domain/local-date'
import { addDays, addMonths, daysInMonth, fromParts, parseLocalDate, toParts, weekdayIndex } from '@/domain/local-date'
import { trimToNull } from '@/domain/text'
import type { Clock } from '@/platform/clock'
import { systemClock } from '@/platform/clock'
import { readSetting, writeSetting } from '@/platform/storage'

export type OperationInterval = 'day' | 'week' | 'month' | 'year'

export const OPERATION_INTERVALS: readonly OperationInterval[] = ['day', 'week', 'month', 'year']

export interface OperationFilters {
  from: LocalDate | null
  to: LocalDate | null
  interval: OperationInterval | null
  comment: string
  place: string
  categoryIds: number[]
  showEmptyDays: boolean
}

export interface OperationFiltersState {
  filters: OperationFilters
  applied: OperationFilters
  applyInterval: (interval: OperationInterval | null) => void
  shift: (direction: -1 | 1) => void
  reset: () => void
  apply: () => void
}

const FROM_KEY = 'operations:from'
const TO_KEY = 'operations:to'
const INTERVAL_KEY = 'operations:interval'

export function toOperationFilter(filters: OperationFilters): OperationFilter {
  return {
    dateFrom: filters.from,
    dateTo: filters.to,
    categoryIds: [...filters.categoryIds].sort((left, right) => left - right),
    comment: trimToNull(filters.comment),
    place: trimToNull(filters.place),
  }
}

export function intervalStart(interval: OperationInterval, anchor: LocalDate): LocalDate {
  const { year, month } = toParts(anchor)

  switch (interval) {
    case 'day':
      return anchor
    case 'week':
      return addDays(anchor, -weekdayIndex(anchor))
    case 'month':
      return dateAt(year, month, 1)
    case 'year':
      return dateAt(year, 1, 1)
  }
}

export function intervalEnd(interval: OperationInterval, start: LocalDate): LocalDate {
  const { year, month } = toParts(start)

  switch (interval) {
    case 'day':
      return start
    case 'week':
      return addDays(start, 6)
    case 'month':
      return dateAt(year, month, daysInMonth(year, month))
    case 'year':
      return dateAt(year, 12, 31)
  }
}

export function intervalRange(
  interval: OperationInterval,
  anchor: LocalDate,
): { from: LocalDate, to: LocalDate } {
  const from = intervalStart(interval, anchor)
  return { from, to: intervalEnd(interval, from) }
}

export function createOperationFilters(clock: Clock = systemClock): OperationFiltersState {
  const filters = reactive<OperationFilters>({
    from: readDate(FROM_KEY),
    to: readDate(TO_KEY),
    interval: readInterval(),
    comment: '',
    place: '',
    categoryIds: [],
    showEmptyDays: false,
  })

  watch(
    [() => filters.from, () => filters.to, () => filters.interval],
    () => {
      writeSetting(FROM_KEY, filters.from)
      writeSetting(TO_KEY, filters.to)
      writeSetting(INTERVAL_KEY, filters.interval)
    },
    { flush: 'sync' },
  )

  function applyInterval(interval: OperationInterval | null): void {
    filters.interval = interval

    if (interval === null) {
      return
    }

    setRange(intervalRange(interval, filters.from ?? filters.to ?? clock.today()))
  }

  function shift(direction: -1 | 1): void {
    const interval = filters.interval

    if (interval === null || filters.from === null) {
      return
    }

    const start = shiftStart(interval, filters.from, direction)

    setRange({ from: start, to: intervalEnd(interval, start) })
  }

  function setRange(range: { from: LocalDate, to: LocalDate }): void {
    filters.from = range.from
    filters.to = range.to
  }

  function reset(): void {
    filters.comment = ''
    filters.place = ''
    filters.categoryIds = []
  }

  if (filters.interval !== null && (filters.from === null || filters.to === null)) {
    applyInterval(filters.interval)
  }

  const applied = reactive<OperationFilters>(snapshot(filters))

  function apply(): void {
    Object.assign(applied, snapshot(filters))
  }

  watch(() => filters.showEmptyDays, value => {
    applied.showEmptyDays = value
  })

  return { filters, applied, applyInterval, shift, reset, apply }
}

export const useOperationFiltersStore = defineStore('operation-filters', () => createOperationFilters())

function snapshot(filters: OperationFilters): OperationFilters {
  return { ...filters, categoryIds: [...filters.categoryIds] }
}

function shiftStart(interval: OperationInterval, start: LocalDate, direction: -1 | 1): LocalDate {
  switch (interval) {
    case 'day':
      return addDays(start, direction)
    case 'week':
      return addDays(start, 7 * direction)
    case 'month':
      return addMonths(start, direction)
    case 'year':
      return addMonths(start, 12 * direction)
  }
}

function dateAt(year: number, month: number, day: number): LocalDate {
  const value = fromParts(year, month, day)

  if (value === null) {
    throw new RangeError(`Некорректная дата: ${year}-${month}-${day}`)
  }

  return value
}

function readDate(key: string): LocalDate | null {
  const raw = readSetting<unknown>(key, null)
  return typeof raw === 'string' ? parseLocalDate(raw) : null
}

function readInterval(): OperationInterval | null {
  const raw = readSetting<unknown>(INTERVAL_KEY, null)

  return OPERATION_INTERVALS.find(interval => interval === raw) ?? null
}
