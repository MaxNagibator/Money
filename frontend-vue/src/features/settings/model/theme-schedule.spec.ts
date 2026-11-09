import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { ThemeSchedule } from './theme-schedule'
import { createThemeSchedule, isDarkBySchedule, themeScheduleDefaults } from './theme-schedule'

const { store, writes } = vi.hoisted(() => ({
  store: new Map<string, unknown>(),
  writes: { count: 0 },
}))

vi.mock('@/platform/storage', () => ({
  readSetting: <T>(key: string, fallback: T): T => (store.has(key) ? store.get(key) as T : fallback),
  writeSetting: (key: string, value: unknown): void => {
    writes.count += 1
    store.set(key, value)
  },
  removeSetting: (key: string): void => {
    store.delete(key)
  },
}))

const KEY = 'settings:theme-schedule'

function schedule(patch: Partial<ThemeSchedule> = {}): ThemeSchedule {
  return { ...themeScheduleDefaults(), isEnabled: true, ...patch }
}

function at(hours: number, minutes: number): Date {
  return new Date(2026, 7, 23, hours, minutes)
}

beforeEach(() => {
  store.clear()
  writes.count = 0
})

describe('расписание тёмной темы', () => {
  it.each<[number, number, boolean]>([
    [8, 59, false],
    [9, 0, true],
    [17, 59, true],
    [18, 0, false],
    [23, 30, false],
  ])('обычный интервал 09:00-18:00 в %i:%i', (hours, minutes, expected) => {
    const value = schedule({ start: '09:00', end: '18:00' })

    expect(isDarkBySchedule(value, at(hours, minutes))).toBe(expected)
  })

  it.each<[number, number, boolean]>([
    [17, 59, false],
    [18, 0, true],
    [23, 59, true],
    [0, 30, true],
    [5, 59, true],
    [6, 0, false],
    [12, 0, false],
  ])('интервал через полночь 18:00-06:00 в %i:%i', (hours, minutes, expected) => {
    const value = schedule({ start: '18:00', end: '06:00' })

    expect(isDarkBySchedule(value, at(hours, minutes))).toBe(expected)
  })

  it('выключенное расписание не даёт тёмную тему даже внутри интервала', () => {
    const value = schedule({ isEnabled: false, start: '18:00', end: '06:00' })

    expect(isDarkBySchedule(value, at(23, 0))).toBe(false)
  })

  it.each<[string, string]>([
    ['18:00', '18:00'],
    ['', '06:00'],
    ['18:00', '25:00'],
    ['18-00', '06:00'],
  ])('границы %s и %s не включают тёмную тему', (start, end) => {
    expect(isDarkBySchedule(schedule({ start, end }), at(23, 0))).toBe(false)
  })
})

describe('состояние расписания', () => {
  it('без записи в хранилище берёт значения по умолчанию', () => {
    expect(createThemeSchedule().schedule).toEqual({
      isEnabled: false,
      isLocationChangedEnabled: true,
      start: '18:00',
      end: '06:00',
      isRecheckEnabled: true,
      checkIntervalMinutes: 5,
    })
  })

  it('испорченная запись не ломает чтение', () => {
    store.set(KEY, { start: 'вечером', end: 42, checkIntervalMinutes: 'часто' })

    expect(createThemeSchedule().schedule).toEqual(themeScheduleDefaults())
  })

  it('тумблер пересчёта сохраняется сразу и переживает перезагрузку', () => {
    createThemeSchedule().update({ isRecheckEnabled: false })

    expect(store.get(KEY)).toMatchObject({ isRecheckEnabled: false })
    expect(createThemeSchedule().schedule.isRecheckEnabled).toBe(false)
  })

  it('отслеживание перехода по страницам включено по умолчанию и сохраняется сразу', () => {
    createThemeSchedule().update({ isLocationChangedEnabled: false })

    expect(store.get(KEY)).toMatchObject({ isLocationChangedEnabled: false })
    expect(createThemeSchedule().schedule.isLocationChangedEnabled).toBe(false)
  })

  it('интервал проверки зажимается в границах', () => {
    const state = createThemeSchedule()

    state.update({ checkIntervalMinutes: 0 })
    expect(state.schedule.checkIntervalMinutes).toBe(1)

    state.update({ checkIntervalMinutes: 5000 })
    expect(state.schedule.checkIntervalMinutes).toBe(1439)
  })
})
