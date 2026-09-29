import type { LocalDate } from '@/domain/local-date'
import { toParts, weekdayIndex } from '@/domain/local-date'
import type { Money } from '@/domain/money'

const LOCALE = 'ru-RU'
const TIME_ZONE = 'Europe/Moscow'
const MINOR_FRACTION_DIGITS = 2

const moneyFormatter = new Intl.NumberFormat(LOCALE, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const wholeFormatter = new Intl.NumberFormat(LOCALE, {
  maximumFractionDigits: 0,
})

const sizeFormatter = new Intl.NumberFormat(LOCALE, {
  maximumFractionDigits: 1,
})

const SIZE_UNITS = ['Б', 'КБ', 'МБ', 'ГБ', 'ТБ']
const SIZE_STEP = 1024

const nameCollator = new Intl.Collator(LOCALE)

const MONTHS_GENITIVE = [
  'января',
  'февраля',
  'марта',
  'апреля',
  'мая',
  'июня',
  'июля',
  'августа',
  'сентября',
  'октября',
  'ноября',
  'декабря',
]

const MONTHS_NOMINATIVE = [
  'январь',
  'февраль',
  'март',
  'апрель',
  'май',
  'июнь',
  'июль',
  'август',
  'сентябрь',
  'октябрь',
  'ноябрь',
  'декабрь',
]

const WEEKDAYS_SHORT = [
  'пн',
  'вт',
  'ср',
  'чт',
  'пт',
  'сб',
  'вс',
]

const instantFormatter = new Intl.DateTimeFormat(LOCALE, {
  timeZone: TIME_ZONE,
  dateStyle: 'short',
  timeStyle: 'short',
})

export function formatMoney(value: Money): string {
  return moneyFormatter.format(value.toDecimal().toNumber())
}

export function formatMoneyPlain(value: Money): string {
  const amount = value.toDecimal()
  const scale = value.wireScale

  if (scale !== null) {
    return amount.toFixed(scale).replace('.', ',')
  }

  const text = amount.isInteger()
    ? amount.toString()
    : amount.toFixed(Math.max(amount.decimalPlaces(), MINOR_FRACTION_DIGITS))

  return text.replace('.', ',')
}

export function formatMoneyInput(value: Money): string {
  const amount = value.toDecimal()
  const scale = value.wireScale

  return (scale === null ? amount.toString() : amount.toFixed(scale)).replace('.', ',')
}

export function formatMoneyWhole(value: Money): string {
  return wholeFormatter.format(value.toDecimal().toNumber())
}

export function formatMileage(value: number): string {
  return wholeFormatter.format(value)
}

export function formatByteSize(bytes: number): string {
  let value = Math.max(0, bytes)
  let unit = 0

  while (value >= SIZE_STEP && unit < SIZE_UNITS.length - 1) {
    value /= SIZE_STEP
    unit += 1
  }

  return `${sizeFormatter.format(value)} ${SIZE_UNITS[unit] ?? ''}`
}

export function formatLocalDate(value: LocalDate): string {
  const { year, month, day } = toParts(value)
  return `${pad(day)}.${pad(month)}.${String(year).padStart(4, '0')}`
}

export function formatLocalDateDayMonth(value: LocalDate): string {
  const { month, day } = toParts(value)
  return `${pad(day)}.${pad(month)}`
}

export function formatLocalDateWithWeekday(value: LocalDate): string {
  const { month, day } = toParts(value)
  return `${day} ${monthGenitive(month)}, ${WEEKDAYS_SHORT[weekdayIndex(value)]}`
}

export function monthGenitive(month: number): string {
  return MONTHS_GENITIVE[month - 1] ?? ''
}

export function monthNominative(month: number): string {
  return MONTHS_NOMINATIVE[month - 1] ?? ''
}

export function formatInstant(value: Date): string {
  return instantFormatter.format(value)
}

function pad(value: number): string {
  return String(value).padStart(2, '0')
}

export function compareNames(left: string, right: string): number {
  return nameCollator.compare(left, right)
}
