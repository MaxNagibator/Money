import type { ThemeMode } from '@/shared/ui/useAppTheme'
import type { ThemeSchedule } from './theme-schedule'
import { isDarkBySchedule } from './theme-schedule'

export type EffectiveTheme = 'light' | 'dark'

export interface ThemeInput {
  mode: ThemeMode
  isSystemDark: boolean
  schedule: ThemeSchedule
  now: Date
}

export function resolveTheme(input: ThemeInput): EffectiveTheme {
  if (input.mode !== 'system') {
    return input.mode
  }

  if (input.schedule.isEnabled) {
    return isDarkBySchedule(input.schedule, input.now) ? 'dark' : 'light'
  }

  return input.isSystemDark ? 'dark' : 'light'
}

export function brightnessToggleMode(isDark: boolean): ThemeMode {
  return isDark ? 'light' : 'dark'
}

export function isManualMode(mode: ThemeMode): boolean {
  return mode !== 'system'
}
