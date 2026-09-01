import { useDocumentVisibility, useIntervalFn, usePreferredDark } from '@vueuse/core'
import type { ComputedRef } from 'vue'
import { computed, effectScope, shallowRef, watch, watchEffect } from 'vue'
import { systemClock } from '@/platform/clock'
import type { ThemeMode } from '@/shared/ui/useAppTheme'
import { setThemeMode, themeMode } from '@/shared/ui/useAppTheme'
import type { EffectiveTheme } from './theme-mode'
import { brightnessToggleMode, isManualMode, resolveTheme } from './theme-mode'
import type { ThemeSchedule } from './theme-schedule'
import { useThemeSchedule } from './theme-schedule'

export interface ThemeSettingsState {
  mode: ComputedRef<ThemeMode>
  isManual: ComputedRef<boolean>
  isDark: ComputedRef<boolean>
  effective: ComputedRef<EffectiveTheme>
  schedule: ThemeSchedule
  setMode: (mode: ThemeMode) => void
  toggleBrightness: () => void
  toggleLocationChanged: () => void
  recheckOnNavigation: () => void
  updateSchedule: (patch: Partial<ThemeSchedule>) => void
}

const tick = shallowRef(systemClock.now())

let shared: ThemeSettingsState | null = null

export function useThemeSettings(): ThemeSettingsState {
  shared ??= effectScope(true).run(createThemeSettings) as ThemeSettingsState

  return shared
}

function createThemeSettings(): ThemeSettingsState {
  const isSystemDark = usePreferredDark()
  const visibility = useDocumentVisibility()
  const scheduleState = useThemeSchedule()

  const effective = computed<EffectiveTheme>(() => resolveTheme({
    mode: themeMode.value,
    isSystemDark: isSystemDark.value,
    schedule: scheduleState.schedule,
    now: tick.value,
  }))

  const isScheduleActive = computed(() => !isManualMode(themeMode.value) && scheduleState.schedule.isEnabled)

  const timer = useIntervalFn(
    () => {
      tick.value = systemClock.now()
    },
    () => scheduleState.schedule.checkIntervalMinutes * 60_000,
    { immediate: false },
  )

  watch(visibility, value => {
    if (value === 'visible' && isScheduleActive.value) {
      tick.value = systemClock.now()
    }
  })

  watchEffect(() => {
    if (isScheduleActive.value && scheduleState.schedule.isRecheckEnabled) {
      timer.resume()
      return
    }

    timer.pause()
  })

  function setMode(mode: ThemeMode): void {
    setThemeMode(mode)
    tick.value = systemClock.now()
  }

  function recheckOnNavigation(): void {
    if (isManualMode(themeMode.value) || !scheduleState.schedule.isLocationChangedEnabled) {
      return
    }

    tick.value = systemClock.now()
  }

  function toggleBrightness(): void {
    setMode(brightnessToggleMode(effective.value === 'dark'))
  }

  function toggleLocationChanged(): void {
    scheduleState.update({ isLocationChangedEnabled: !scheduleState.schedule.isLocationChangedEnabled })
  }

  function updateSchedule(patch: Partial<ThemeSchedule>): void {
    scheduleState.update(patch)
    tick.value = systemClock.now()
  }

  return {
    mode: computed(() => themeMode.value),
    isManual: computed(() => isManualMode(themeMode.value)),
    isDark: computed(() => effective.value === 'dark'),
    effective,
    schedule: scheduleState.schedule,
    setMode,
    toggleBrightness,
    recheckOnNavigation,
    toggleLocationChanged,
    updateSchedule,
  }
}
