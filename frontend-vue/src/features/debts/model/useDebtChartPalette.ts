import type { ComputedRef } from 'vue'
import { computed } from 'vue'
import { useDisplaySettings, useThemeSettings } from '@/features/settings'
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
const DEFAULT_MUTED_OPACITY = 0.6
const DEFAULT_GRID_OPACITY = 0.12
const SERIES_FILL_OPACITY = 0.16
const CLOSED_CYCLE_OPACITY = 0.35
const ZERO_LINE_DARK_OPACITY = 0.35
const ZERO_LINE_LIGHT_OPACITY = 0.3
const REPAID_LIGHT = '#00897B'
const REPAID_DARK = '#4DB6AC'
const HEX_PATTERN = /^#(?<digits>[\da-f]{3}|[\da-f]{6})$/i
const SHORT_HEX_LENGTH = 3
const HEX_RADIX = 16

export function resolveDebtChartPalette(theme: AppTheme, useThemeChrome = true): DebtChartPalette {
  const onSurface = theme.colors['on-surface'] ?? '#424242'
  const border = typeof theme.variables['border-color'] === 'string'
    ? theme.variables['border-color']
    : onSurface
  const mutedOpacity = typeof theme.variables['medium-emphasis-opacity'] === 'number'
    ? theme.variables['medium-emphasis-opacity']
    : DEFAULT_MUTED_OPACITY
  const gridOpacity = typeof theme.variables['border-opacity'] === 'number'
    ? theme.variables['border-opacity']
    : DEFAULT_GRID_OPACITY

  const receivable = theme.colors.success ?? '#00C853'
  const payable = theme.colors.warning ?? '#FF9800'
  const net = theme.colors.primary ?? '#594AE2'

  return {
    text: useThemeChrome ? onSurface : DEFAULT_TEXT,
    mutedText: useThemeChrome ? withAlpha(onSurface, mutedOpacity) : DEFAULT_MUTED_TEXT,
    grid: useThemeChrome ? withAlpha(border, gridOpacity) : DEFAULT_GRID,
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
      ? withAlpha(onSurface, theme.dark ? ZERO_LINE_DARK_OPACITY : ZERO_LINE_LIGHT_OPACITY)
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

function withAlpha(color: string, alpha: number): string {
  const digits = HEX_PATTERN.exec(color)?.groups?.digits

  if (digits === undefined) {
    return color
  }

  const full = digits.length === SHORT_HEX_LENGTH
    ? [...digits].map(digit => digit + digit).join('')
    : digits

  const red = Number.parseInt(full.slice(0, 2), HEX_RADIX)
  const green = Number.parseInt(full.slice(2, 4), HEX_RADIX)
  const blue = Number.parseInt(full.slice(4, 6), HEX_RADIX)

  return `rgba(${red}, ${green}, ${blue}, ${alpha})`
}
