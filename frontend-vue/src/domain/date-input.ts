import type { LocalDate } from '@/domain/local-date'
import { addDays, fromParts } from '@/domain/local-date'

const TODAY_PATTERN = /^(?:с|сегодня)$/
const TOMORROW_PATTERN = /^(после)*завтра$/
const TOMORROW_SHORT_PATTERN = /^(п)*з$/
const YESTERDAY_PATTERN = /^(поза)*вчера$/
const YESTERDAY_SHORT_PATTERN = /^(п)*в$/
const EXPLICIT_PATTERN = /^(\d{1,2})\.(\d{1,2})\.(\d{4})$/

export function parseDateInput(raw: string, today: LocalDate): LocalDate | null {
  const normalized = raw.trim().toLowerCase()

  if (normalized.length === 0) {
    return null
  }

  if (TODAY_PATTERN.test(normalized)) {
    return today
  }

  const offset = matchOffset(normalized)

  if (offset !== null) {
    return addDays(today, offset)
  }

  const explicit = EXPLICIT_PATTERN.exec(normalized)

  if (explicit === null) {
    return null
  }

  const [, day, month, year] = explicit
  return fromParts(Number(year), Number(month), Number(day))
}

function matchOffset(normalized: string): number | null {
  if (TOMORROW_PATTERN.test(normalized)) {
    return countPrefix(normalized, 'после') + 1
  }

  if (TOMORROW_SHORT_PATTERN.test(normalized)) {
    return normalized.length
  }

  if (YESTERDAY_PATTERN.test(normalized)) {
    return -(countPrefix(normalized, 'поза') + 1)
  }

  if (YESTERDAY_SHORT_PATTERN.test(normalized)) {
    return -normalized.length
  }

  return null
}

function countPrefix(value: string, prefix: string): number {
  let count = 0

  while (value.startsWith(prefix, count * prefix.length)) {
    count += 1
  }

  return count
}
