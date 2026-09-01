export const EVERY_DAY_TIME_TYPE_ID = 1
export const EVERY_WEEK_TIME_TYPE_ID = 2
export const EVERY_MONTH_TIME_TYPE_ID = 3

export const DEFAULT_TIME_TYPE_ID = EVERY_MONTH_TIME_TYPE_ID
export const DEFAULT_TIME_VALUE = 1

export interface TimeValueRules {
  label: string
  min: number
  max: number
}

export interface RegularTimeType {
  id: number
  name: string
  valueRules: TimeValueRules | null
}

export const REGULAR_TIME_TYPES: readonly RegularTimeType[] = [
  { id: EVERY_DAY_TIME_TYPE_ID, name: 'Каждый день', valueRules: null },
  { id: EVERY_WEEK_TIME_TYPE_ID, name: 'Каждую неделю', valueRules: { label: 'День недели', min: 1, max: 7 } },
  { id: EVERY_MONTH_TIME_TYPE_ID, name: 'Каждый месяц', valueRules: { label: 'День месяца', min: 1, max: 31 } },
]

const WEEKDAY_NAMES = ['Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота', 'Воскресенье']
const SHORTEST_MONTH_LENGTH = 28
const SHORT_MONTH_HINT = 'В коротком месяце запуск сдвигается на последний день'

export function regularTimeType(id: number | null): RegularTimeType | null {
  return REGULAR_TIME_TYPES.find(type => type.id === id) ?? null
}

export function timeValueRules(id: number | null): TimeValueRules | null {
  return regularTimeType(id)?.valueRules ?? null
}

export function timeValueHint(id: number | null, value: number | null): string | null {
  if (value === null || timeValueRules(id) === null) {
    return null
  }

  if (id === EVERY_WEEK_TIME_TYPE_ID) {
    return WEEKDAY_NAMES[value - 1] ?? null
  }

  return value > SHORTEST_MONTH_LENGTH ? SHORT_MONTH_HINT : null
}
