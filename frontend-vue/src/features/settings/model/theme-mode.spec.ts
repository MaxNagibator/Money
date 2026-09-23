import { describe, expect, it } from 'vitest'
import { computed, ref } from 'vue'
import type { ThemeMode } from '@/shared/ui/useAppTheme'
import type { EffectiveTheme } from './theme-mode'
import { brightnessToggleMode, isManualMode, resolveTheme, themeStatusLabel } from './theme-mode'
import type { ThemeSchedule } from './theme-schedule'
import { themeScheduleDefaults } from './theme-schedule'

const NOW = new Date(2026, 7, 23, 23, 0)

function schedule(patch: Partial<ThemeSchedule> = {}): ThemeSchedule {
  return { ...themeScheduleDefaults(), ...patch }
}

describe('переключение яркости', () => {
  it('из авто одним кликом уводит в ручной режим с обратной темой', () => {
    const applied = resolveTheme({ mode: 'system', isSystemDark: false, schedule: schedule(), now: NOW })
    const next = brightnessToggleMode(applied === 'dark')

    expect(next).toBe('dark')
    expect(isManualMode(next)).toBe(true)
  })

  it('ручной режим игнорирует расписание', () => {
    const value = schedule({ isEnabled: true, start: '18:00', end: '06:00' })

    expect(resolveTheme({ mode: 'light', isSystemDark: true, schedule: value, now: NOW })).toBe('light')
    expect(resolveTheme({ mode: 'dark', isSystemDark: false, schedule: value, now: NOW })).toBe('dark')
  })
})

describe('автоматический режим', () => {
  it.each<[boolean, string]>([
    [false, 'light'],
    [true, 'dark'],
  ])('без расписания повторяет системную тему, isSystemDark=%s', (isSystemDark, expected) => {
    expect(resolveTheme({ mode: 'system', isSystemDark, schedule: schedule(), now: NOW })).toBe(expected)
  })

  it('включённое расписание вытесняет системную тему', () => {
    const value = schedule({ isEnabled: true, start: '18:00', end: '06:00' })

    expect(resolveTheme({ mode: 'system', isSystemDark: false, schedule: value, now: NOW })).toBe('dark')
    expect(resolveTheme({ mode: 'system', isSystemDark: true, schedule: value, now: new Date(2026, 7, 23, 12, 0) }))
      .toBe('light')
  })

  it('смена системной темы применяется сразу', () => {
    const isSystemDark = ref(false)
    const applied = computed(() => resolveTheme({
      mode: 'system',
      isSystemDark: isSystemDark.value,
      schedule: schedule(),
      now: NOW,
    }))

    expect(applied.value).toBe('light')

    isSystemDark.value = true

    expect(applied.value).toBe('dark')
  })
})

describe('строка состояния темы', () => {
  it.each<[ThemeMode, EffectiveTheme, boolean, string]>([
    ['light', 'light', false, 'Сейчас светлая тема, выбрана вручную'],
    ['dark', 'dark', true, 'Сейчас тёмная тема, выбрана вручную'],
    ['system', 'dark', true, 'Сейчас тёмная тема, по расписанию'],
    ['system', 'light', true, 'Сейчас светлая тема, по расписанию'],
    ['system', 'dark', false, 'Сейчас тёмная тема, как в системе'],
    ['system', 'light', false, 'Сейчас светлая тема, как в системе'],
  ])('режим %s, тема %s, расписание %s', (mode, effective, isScheduleEnabled, expected) => {
    expect(themeStatusLabel(mode, effective, isScheduleEnabled)).toBe(expected)
  })
})
