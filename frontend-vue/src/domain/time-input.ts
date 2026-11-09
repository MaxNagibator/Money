const MINUTES_IN_HOUR = 60
const MINUTES_IN_DAY = 24 * MINUTES_IN_HOUR
const SEPARATED_PATTERN = /^(\d{1,2})[:.,\-\s](\d{1,2})$/
const COMPACT_PATTERN = /^(\d{1,4})$/

export function parseTimeInput(raw: string): number | null {
  const normalized = raw.trim()

  if (normalized.length === 0) {
    return null
  }

  const separated = SEPARATED_PATTERN.exec(normalized)

  if (separated !== null) {
    return toMinutes(Number(separated[1]), Number(separated[2]))
  }

  const compact = COMPACT_PATTERN.exec(normalized)

  if (compact === null) {
    return null
  }

  const digits = compact[1] ?? ''

  if (digits.length <= 2) {
    return toMinutes(Number(digits), 0)
  }

  return toMinutes(Number(digits.slice(0, digits.length - 2)), Number(digits.slice(-2)))
}

export function formatTimeOfDay(minutes: number): string {
  const total = normalizeMinutes(minutes)
  const hours = Math.floor(total / MINUTES_IN_HOUR)

  return `${pad(hours)}:${pad(total % MINUTES_IN_HOUR)}`
}

export function normalizeTimeInput(raw: string): string | null {
  const minutes = parseTimeInput(raw)

  return minutes === null ? null : formatTimeOfDay(minutes)
}

function toMinutes(hours: number, minutes: number): number | null {
  if (hours > 23 || minutes > 59) {
    return null
  }

  return hours * MINUTES_IN_HOUR + minutes
}

function normalizeMinutes(minutes: number): number {
  if (!Number.isFinite(minutes)) {
    return 0
  }

  return ((Math.round(minutes) % MINUTES_IN_DAY) + MINUTES_IN_DAY) % MINUTES_IN_DAY
}

function pad(value: number): string {
  return String(value).padStart(2, '0')
}
