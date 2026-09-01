import type { LocalDate } from '@/domain/local-date'
import { toParts } from '@/domain/local-date'
import { monthGenitive } from '@/platform/format'

const MISSING_DATE = '-'

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
