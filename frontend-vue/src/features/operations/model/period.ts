import type { LocalDate } from '@/domain/local-date'
import { compareLocalDate, toParts } from '@/domain/local-date'
import { formatLocalDate, monthGenitive } from '@/platform/format'

const MISSING_DATE = '-'
const EMPTY_RANGE = 'Период не задан'

export function isWithinPeriod(date: LocalDate, from: LocalDate | null, to: LocalDate | null): boolean {
  return (from === null || compareLocalDate(date, from) >= 0)
    && (to === null || compareLocalDate(date, to) <= 0)
}

export function formatPeriodDate(value: LocalDate | null): string {
  if (value === null) {
    return MISSING_DATE
  }

  const { year, month, day } = toParts(value)

  return `${day} ${monthGenitive(month)} ${year}`
}

export function formatPeriod(from: LocalDate | null, to: LocalDate | null): string {
  return `Период с ${formatPeriodDate(from)} по ${formatPeriodDate(to)}`
}

export function formatPeriodRange(from: LocalDate | null, to: LocalDate | null): string {
  if (from === null && to === null) {
    return EMPTY_RANGE
  }

  return `${shortDate(from)} – ${shortDate(to)}`
}

function shortDate(value: LocalDate | null): string {
  return value === null ? MISSING_DATE : formatLocalDate(value)
}
