import { reactive, watch } from 'vue'
import { readSetting, writeSetting } from '@/platform/storage'

export interface ThemeSchedule {
  isEnabled: boolean
  isLocationChangedEnabled: boolean
  start: string
  end: string
  isRecheckEnabled: boolean
  checkIntervalMinutes: number
}

export interface ThemeScheduleState {
  schedule: ThemeSchedule
  update: (patch: Partial<ThemeSchedule>) => void
}

export const CHECK_INTERVAL_MIN = 1
export const CHECK_INTERVAL_MAX = 1439

const STORAGE_KEY = 'settings:theme-schedule'
const TIME_PATTERN = /^(\d{1,2}):(\d{2})$/

export function themeScheduleDefaults(): ThemeSchedule {
  return {
    isEnabled: false,
    isLocationChangedEnabled: true,
    start: '18:00',
    end: '06:00',
    isRecheckEnabled: true,
    checkIntervalMinutes: 5,
  }
}

export function parseTimeOfDay(value: string): number | null {
  const match = TIME_PATTERN.exec(value.trim())

  if (match === null) {
    return null
  }

  const hours = Number(match[1])
  const minutes = Number(match[2])

  if (hours > 23 || minutes > 59) {
    return null
  }

  return hours * 60 + minutes
}

export function clampCheckInterval(minutes: number): number {
  if (!Number.isFinite(minutes)) {
    return themeScheduleDefaults().checkIntervalMinutes
  }

  return Math.min(Math.max(Math.round(minutes), CHECK_INTERVAL_MIN), CHECK_INTERVAL_MAX)
}

export function isDarkBySchedule(schedule: ThemeSchedule, now: Date): boolean {
  if (!schedule.isEnabled) {
    return false
  }

  const start = parseTimeOfDay(schedule.start)
  const end = parseTimeOfDay(schedule.end)

  if (start === null || end === null || start === end) {
    return false
  }

  const current = now.getHours() * 60 + now.getMinutes()

  return start < end
    ? current >= start && current < end
    : current >= start || current < end
}

export function readThemeSchedule(): ThemeSchedule {
  const defaults = themeScheduleDefaults()
  const stored = readSetting<unknown>(STORAGE_KEY, null)

  if (stored === null || typeof stored !== 'object') {
    return defaults
  }

  const raw = stored as Record<string, unknown>

  return {
    isEnabled: readBoolean(raw.isEnabled, defaults.isEnabled),
    isLocationChangedEnabled: readBoolean(raw.isLocationChangedEnabled, defaults.isLocationChangedEnabled),
    start: readTime(raw.start, defaults.start),
    end: readTime(raw.end, defaults.end),
    isRecheckEnabled: readBoolean(raw.isRecheckEnabled, defaults.isRecheckEnabled),
    checkIntervalMinutes: typeof raw.checkIntervalMinutes === 'number'
      ? clampCheckInterval(raw.checkIntervalMinutes)
      : defaults.checkIntervalMinutes,
  }
}

export function createThemeSchedule(): ThemeScheduleState {
  const schedule = reactive<ThemeSchedule>(readThemeSchedule())

  watch(
    schedule,
    () => {
      writeSetting(STORAGE_KEY, { ...schedule })
    },
    { deep: true, flush: 'sync' },
  )

  function update(patch: Partial<ThemeSchedule>): void {
    if (patch.isEnabled !== undefined) {
      schedule.isEnabled = patch.isEnabled
    }

    if (patch.isLocationChangedEnabled !== undefined) {
      schedule.isLocationChangedEnabled = patch.isLocationChangedEnabled
    }

    if (patch.start !== undefined && parseTimeOfDay(patch.start) !== null) {
      schedule.start = patch.start
    }

    if (patch.end !== undefined && parseTimeOfDay(patch.end) !== null) {
      schedule.end = patch.end
    }

    if (patch.isRecheckEnabled !== undefined) {
      schedule.isRecheckEnabled = patch.isRecheckEnabled
    }

    if (patch.checkIntervalMinutes !== undefined) {
      schedule.checkIntervalMinutes = clampCheckInterval(patch.checkIntervalMinutes)
    }
  }

  return { schedule, update }
}

let shared: ThemeScheduleState | null = null

export function useThemeSchedule(): ThemeScheduleState {
  shared ??= createThemeSchedule()

  return shared
}

function readBoolean(value: unknown, fallback: boolean): boolean {
  return typeof value === 'boolean' ? value : fallback
}

function readTime(value: unknown, fallback: string): string {
  return typeof value === 'string' && parseTimeOfDay(value) !== null ? value : fallback
}
