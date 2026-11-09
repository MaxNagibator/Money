import { beforeEach, describe, expect, it, vi } from 'vitest'
import { scheduleFormFrom, validateScheduleForm } from './schedule-form'
import { createThemeSchedule } from './theme-schedule'

const { store } = vi.hoisted(() => ({ store: new Map<string, unknown>() }))

vi.mock('@/platform/storage', () => ({
  readSetting: <T>(key: string, fallback: T): T => (store.has(key) ? store.get(key) as T : fallback),
  writeSetting: (key: string, value: unknown): void => {
    store.set(key, value)
  },
  removeSetting: (key: string): void => {
    store.delete(key)
  },
}))

beforeEach(() => {
  store.clear()
})

describe('форма расписания', () => {
  it('принимает корректное расписание', () => {
    const result = validateScheduleForm({ start: '21:30', end: '07:15', checkInterval: '00:10' })

    expect(result).toEqual({ ok: true, patch: { start: '21:30', end: '07:15', checkIntervalMinutes: 10 } })
  })

  it.each<[string, string, string]>([
    ['', '06:00', 'Укажите время начала и окончания в формате ЧЧ:ММ'],
    ['18:00', '', 'Укажите время начала и окончания в формате ЧЧ:ММ'],
    ['24:00', '06:00', 'Укажите время начала и окончания в формате ЧЧ:ММ'],
    ['18:00', '18:00', 'Время начала и окончания не должны совпадать'],
  ])('границы %s и %s отвергаются', (start, end, error) => {
    expect(validateScheduleForm({ start, end, checkInterval: '00:05' })).toEqual({ ok: false, error })
  })

  it('пустой интервал проверки оставляет ранее сохранённый', () => {
    const state = createThemeSchedule()
    state.update({ checkIntervalMinutes: 30 })

    const result = validateScheduleForm({ ...scheduleFormFrom(state.schedule), checkInterval: ' ' })

    expect(result.ok).toBe(true)

    if (result.ok) {
      state.update(result.patch)
    }

    expect(state.schedule.checkIntervalMinutes).toBe(30)
  })

  it('предельный сохранённый интервал возвращается формой без ошибки', () => {
    const state = createThemeSchedule()
    state.update({ checkIntervalMinutes: 5000 })

    expect(validateScheduleForm(scheduleFormFrom(state.schedule))).toEqual({
      ok: true,
      patch: { start: '18:00', end: '06:00', checkIntervalMinutes: 1439 },
    })
  })

  it.each<string>(['00:00', '24:00', '1441', 'скоро'])('интервал %s отвергается', checkInterval => {
    expect(validateScheduleForm({ start: '18:00', end: '06:00', checkInterval })).toEqual({
      ok: false,
      error: 'Интервал проверки задаётся в формате ЧЧ:ММ, от 00:01 до 23:59',
    })
  })
})
