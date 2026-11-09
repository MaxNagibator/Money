import type { ComputedRef } from 'vue'
import { computed } from 'vue'
import { useDisplaySettings, useThemeSettings } from '@/features/settings'
import { darkTheme, lightTheme } from '@/shared/ui/theme'
import type { ChartPalette } from './chart-options'
import { themePalette } from './chart-options'

export function useChartPalette(): ComputedRef<ChartPalette | null> {
  const { settings } = useDisplaySettings()
  const theme = useThemeSettings()

  return computed(() => settings.useChartThemeColors
    ? themePalette(theme.isDark.value ? darkTheme : lightTheme)
    : null)
}
