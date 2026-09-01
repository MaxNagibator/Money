import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { addDays, addMonths, compareLocalDate, fromParts, toParts, weekdayIndex } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import { formatLocalDate, monthGenitive, monthNominative } from '@/platform/format'
import { categoryColor } from './chart-colors'

export type BucketStep = 'day' | 'week' | 'month'

export interface BarSeries {
  key: string
  label: string
  color: string
  values: (number | null)[]
}

export interface BarChartData {
  labels: string[]
  series: BarSeries[]
}

export interface BarChartRange {
  from: LocalDate | null
  to: LocalDate | null
}

const DAY_STEP_MAX_DAYS = 31
const WEEK_STEP_MAX_DAYS = 140

const FILTER_RANGE_MAX_DAYS = 10

export function buildBarChartData(
  operations: readonly Operation[],
  range: BarChartRange,
  today: LocalDate,
  scale: readonly Operation[] = operations,
): BarChartData {
  const bounds = resolveBounds(scale, range, today)
  const step = resolveStep(bounds)
  const buckets = buildBuckets(alignToStep(bounds.from, step), alignToStep(bounds.to, step), step)
  const categories = collectCategories(operations)
  const sums = new Map(categories.map(category => [category.id, buckets.map(() => Money.zero)]))

  for (const operation of operations) {
    const row = operation.category === null ? undefined : sums.get(operation.category.id)

    if (row === undefined) {
      continue
    }

    const index = bucketIndexOf(buckets, step, operation.date)
    const bucket = index < 0 ? undefined : row[index]

    if (bucket === undefined) {
      continue
    }

    row[index] = bucket.add(operation.sum)
  }

  return {
    labels: buckets.map(bucket => bucketLabel(bucket, step)),
    series: categories.map((category, position) => ({
      key: String(category.id),
      label: category.name,
      color: categoryColor(category, position),
      values: (sums.get(category.id) ?? []).map(toChartValue),
    })),
  }
}

function collectCategories(operations: readonly Operation[]): Category[] {
  const byId = new Map<number, Category>()

  for (const operation of operations) {
    if (operation.category !== null && !byId.has(operation.category.id)) {
      byId.set(operation.category.id, operation.category)
    }
  }

  return [...byId.values()]
}

function resolveBounds(
  operations: readonly Operation[],
  range: BarChartRange,
  today: LocalDate,
): { from: LocalDate, to: LocalDate } {
  const from = range.from ?? today
  const to = range.to ?? today

  if (compareLocalDate(addDays(from, FILTER_RANGE_MAX_DAYS), to) >= 0) {
    return { from, to }
  }

  const dates = operations.map(operation => operation.date)

  if (dates.length === 0) {
    return { from: today, to: today }
  }

  return {
    from: dates.reduce((left, right) => compareLocalDate(left, right) <= 0 ? left : right),
    to: dates.reduce((left, right) => compareLocalDate(left, right) >= 0 ? left : right),
  }
}

function resolveStep(bounds: { from: LocalDate, to: LocalDate }): BucketStep {
  if (compareLocalDate(addDays(bounds.from, DAY_STEP_MAX_DAYS), bounds.to) >= 0) {
    return 'day'
  }

  if (compareLocalDate(addDays(bounds.from, WEEK_STEP_MAX_DAYS), bounds.to) >= 0) {
    return 'week'
  }

  return 'month'
}

function alignToStep(value: LocalDate, step: BucketStep): LocalDate {
  switch (step) {
    case 'day':
      return value
    case 'week':
      return addDays(value, -weekdayIndex(value))
    case 'month':
      return startOfMonth(value)
  }
}

function startOfMonth(value: LocalDate): LocalDate {
  const { year, month } = toParts(value)
  const first = fromParts(year, month, 1)

  if (first === null) {
    throw new RangeError(`Некорректная дата: ${value}`)
  }

  return first
}

function buildBuckets(from: LocalDate, to: LocalDate, step: BucketStep): LocalDate[] {
  const buckets: LocalDate[] = []
  let cursor = from

  do {
    buckets.push(cursor)
    cursor = nextBucket(cursor, step)
  } while (compareLocalDate(cursor, to) <= 0)

  return buckets
}

function nextBucket(value: LocalDate, step: BucketStep): LocalDate {
  switch (step) {
    case 'day':
      return addDays(value, 1)
    case 'week':
      return addDays(value, 7)
    case 'month':
      return addMonths(value, 1)
  }
}

function bucketIndexOf(buckets: readonly LocalDate[], step: BucketStep, date: LocalDate): number {
  const first = buckets[0]
  const last = buckets.at(-1)

  if (first === undefined || last === undefined) {
    return -1
  }

  if (compareLocalDate(date, first) < 0 || compareLocalDate(date, nextBucket(last, step)) >= 0) {
    return -1
  }

  let low = 0
  let high = buckets.length - 1

  while (low < high) {
    const middle = Math.ceil((low + high) / 2)
    const candidate = buckets[middle]

    if (candidate !== undefined && compareLocalDate(candidate, date) <= 0) {
      low = middle
    }
    else {
      high = middle - 1
    }
  }

  return low
}

function bucketLabel(value: LocalDate, step: BucketStep): string {
  const { year, month, day } = toParts(value)

  switch (step) {
    case 'day':
      return formatLocalDate(value)
    case 'week':
      return `${String(day).padStart(2, '0')} ${monthGenitive(month)}`
    case 'month':
      return `${monthNominative(month)} ${year}`
  }
}

function toChartValue(sum: Money): number | null {
  return sum.isZero ? null : sum.toDecimal().toNumber()
}
