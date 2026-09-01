import type { ComputedRef, Ref } from 'vue'
import { computed, ref } from 'vue'
import { readSetting, writeSetting } from '@/platform/storage'
import { mdComputer, mdDarkMode, mdLightMode } from '@/shared/icons'

const SETTING_KEY = 'theme'

export const EFFECTIVE_THEME_KEY = 'theme-effective'

const MODES = [
  { name: 'system', icon: mdComputer, label: 'Тема как в системе' },
  { name: 'light', icon: mdLightMode, label: 'Светлая тема' },
  { name: 'dark', icon: mdDarkMode, label: 'Тёмная тема' },
] as const

export type ThemeMode = typeof MODES[number]['name']

export interface AppThemeState {
  icon: ComputedRef<string>
  label: ComputedRef<string>
  switchToNext: () => void
}

export function readThemeMode(): ThemeMode {
  const stored = readSetting<string>(SETTING_KEY, 'system')

  return MODES.some(mode => mode.name === stored) ? stored as ThemeMode : 'system'
}

export const themeMode: Ref<ThemeMode> = ref(readThemeMode())

export function setThemeMode(next: ThemeMode): void {
  themeMode.value = next
  writeSetting(SETTING_KEY, next)
}

export function useAppTheme(): AppThemeState {
  const current = computed(() => MODES.find(mode => mode.name === themeMode.value) ?? MODES[0])

  function switchToNext(): void {
    const index = (MODES.indexOf(current.value) + 1) % MODES.length
    const next = MODES.at(index) ?? MODES[0]

    setThemeMode(next.name)
  }

  return {
    icon: computed(() => current.value.icon),
    label: computed(() => current.value.label),
    switchToNext,
  }
}
