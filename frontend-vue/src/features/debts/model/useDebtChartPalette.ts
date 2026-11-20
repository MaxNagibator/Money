import type { ComputedRef } from 'vue'
import { computed } from 'vue'
import { useDisplaySettings, useThemeSettings } from '@/features/settings'
import { resolveChartChrome, withAlpha } from '@/shared/ui/chart-theme'
import type { AppTheme } from '@/shared/ui/theme'
import { darkTheme, lightTheme } from '@/shared/ui/theme'

export interface DebtChartPalette {
  text: string
  mutedText: string
  grid: string
  receivable: string
  receivableFill: string
  payable: string
  payableFill: string
  net: string
  issued: string
  repaid: string
  cycleReceivableActive: string
  cycleReceivableClosed: string
  cyclePayableActive: string
  cyclePayableClosed: string
  zeroLine: string
}

const DEFAULT_TEXT = '#666666'
const DEFAULT_MUTED_TEXT = '#666666'
const DEFAULT_GRID = 'rgba(0, 0, 0, 0.1)'
const DEFAULT_ZERO_LINE = 'rgba(0, 0, 0, 0.3)'
const FALLBACK_ON_SURFACE = '#424242'
const SERIES_FILL_OPACITY = 0.16
const CLOSED_CYCLE_OPACITY = 0.35
const ZERO_LINE_DARK_OPACITY = 0.35
const ZERO_LINE_LIGHT_OPACITY = 0.3
const REPAID_LIGHT = '#00897B'
const REPAID_DARK = '#4DB6AC'

export function resolveDebtChartPalette(theme: AppTheme, useThemeChrome = true): DebtChartPalette {
  const chrome = resolveChartChrome(theme, FALLBACK_ON_SURFACE)
  const receivable = theme.colors.success ?? '#00C853'
  const payable = theme.colors.warning ?? '#FF9800'
  const net = theme.colors.primary ?? '#594AE2'

  return {
    text: useThemeChrome ? chrome.text : DEFAULT_TEXT,
    mutedText: useThemeChrome ? chrome.mutedText : DEFAULT_MUTED_TEXT,
    grid: useThemeChrome ? chrome.grid : DEFAULT_GRID,
    receivable,
    receivableFill: withAlpha(receivable, SERIES_FILL_OPACITY),
    payable,
    payableFill: withAlpha(payable, SERIES_FILL_OPACITY),
    net,
    issued: net,
    repaid: theme.dark ? REPAID_DARK : REPAID_LIGHT,
    cycleReceivableActive: receivable,
    cycleReceivableClosed: withAlpha(receivable, CLOSED_CYCLE_OPACITY),
    cyclePayableActive: payable,
    cyclePayableClosed: withAlpha(payable, CLOSED_CYCLE_OPACITY),
    zeroLine: useThemeChrome
      ? withAlpha(chrome.text, theme.dark ? ZERO_LINE_DARK_OPACITY : ZERO_LINE_LIGHT_OPACITY)
      : DEFAULT_ZERO_LINE,
  }
}

export function useDebtChartPalette(): ComputedRef<DebtChartPalette> {
  const { settings } = useDisplaySettings()
  const theme = useThemeSettings()

  return computed(() => resolveDebtChartPalette(
    theme.isDark.value ? darkTheme : lightTheme,
    settings.useChartThemeColors,
  ))
}
