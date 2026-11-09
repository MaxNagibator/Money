declare const localDateBrand: unique symbol

export type LocalDate = string & { readonly [localDateBrand]: true }

const PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/
const MILLISECONDS_IN_DAY = 86_400_000

export function parseLocalDate(value: string): LocalDate | null {
  const match = PATTERN.exec(value)

  if (match === null) {
    return null
  }

  const [, year, month, day] = match
  return fromParts(Number(year), Number(month), Number(day))
}

export function fromParts(year: number, month: number, day: number): LocalDate | null {
  const utc = Date.UTC(year, month - 1, day)
  const candidate = new Date(utc)

  if (
    candidate.getUTCFullYear() !== year
    || candidate.getUTCMonth() !== month - 1
    || candidate.getUTCDate() !== day
  ) {
    return null
  }

  return format(candidate)
}

export function fromDate(value: Date): LocalDate {
  const utc = Date.UTC(value.getFullYear(), value.getMonth(), value.getDate())
  return format(new Date(utc))
}

export function toParts(value: LocalDate): { year: number, month: number, day: number } {
  const match = PATTERN.exec(value)

  if (match === null) {
    throw new RangeError(`Некорректная дата: ${value}`)
  }

  const [, year, month, day] = match
  return { year: Number(year), month: Number(month), day: Number(day) }
}

export function addDays(value: LocalDate, days: number): LocalDate {
  const { year, month, day } = toParts(value)
  return format(new Date(Date.UTC(year, month - 1, day) + days * MILLISECONDS_IN_DAY))
}

export function compareLocalDate(left: LocalDate, right: LocalDate): number {
  return left < right ? -1 : left > right ? 1 : 0
}

export function addMonths(value: LocalDate, months: number): LocalDate {
  const { year, month, day } = toParts(value)
  const index = year * 12 + (month - 1) + months
  const targetYear = Math.floor(index / 12)
  const targetMonth = index - targetYear * 12 + 1
  const shifted = fromParts(targetYear, targetMonth, Math.min(day, daysInMonth(targetYear, targetMonth)))

  if (shifted === null) {
    throw new RangeError(`Некорректная дата: ${value} со сдвигом на ${months} мес.`)
  }

  return shifted
}

export function weekdayIndex(value: LocalDate): number {
  const { year, month, day } = toParts(value)
  return (new Date(Date.UTC(year, month - 1, day)).getUTCDay() + 6) % 7
}

export function daysInMonth(year: number, month: number): number {
  return new Date(Date.UTC(year, month, 0)).getUTCDate()
}

function format(value: Date): LocalDate {
  const year = String(value.getUTCFullYear()).padStart(4, '0')
  const month = String(value.getUTCMonth() + 1).padStart(2, '0')
  const day = String(value.getUTCDate()).padStart(2, '0')
  return `${year}-${month}-${day}` as LocalDate
}
