import { formatTimeOfDay } from '@/domain/time-input'
import type { ThemeSchedule } from './theme-schedule'
import { CHECK_INTERVAL_MAX, CHECK_INTERVAL_MIN, parseTimeOfDay } from './theme-schedule'

export interface ScheduleFormValue {
  start: string
  end: string
  checkInterval: string
}

export type ScheduleFormResult =
  | { ok: true, patch: Partial<ThemeSchedule> }
  | { ok: false, error: string }

export function scheduleFormFrom(schedule: ThemeSchedule): ScheduleFormValue {
  return {
    start: schedule.start,
    end: schedule.end,
    checkInterval: formatTimeOfDay(schedule.checkIntervalMinutes),
  }
}

export function validateScheduleForm(value: ScheduleFormValue): ScheduleFormResult {
  const start = parseTimeOfDay(value.start)
  const end = parseTimeOfDay(value.end)

  if (start === null || end === null) {
    return { ok: false, error: 'Укажите время начала и окончания в формате ЧЧ:ММ' }
  }

  if (start === end) {
    return { ok: false, error: 'Время начала и окончания не должны совпадать' }
  }

  const patch: Partial<ThemeSchedule> = { start: value.start.trim(), end: value.end.trim() }
  const interval = value.checkInterval.trim()

  if (interval.length === 0) {
    return { ok: true, patch }
  }

  const minutes = parseTimeOfDay(interval)

  if (minutes === null || minutes < CHECK_INTERVAL_MIN || minutes > CHECK_INTERVAL_MAX) {
    const min = formatTimeOfDay(CHECK_INTERVAL_MIN)
    const max = formatTimeOfDay(CHECK_INTERVAL_MAX)

    return { ok: false, error: `Интервал проверки задаётся в формате ЧЧ:ММ, от ${min} до ${max}` }
  }

  return { ok: true, patch: { ...patch, checkIntervalMinutes: minutes } }
}
