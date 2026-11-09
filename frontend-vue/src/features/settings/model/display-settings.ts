import { reactive, watch } from 'vue'
import { readSetting, writeSetting } from '@/platform/storage'

export type AlternateRowStyle = 'none' | 'background' | 'border'

export const ALTERNATE_ROW_STYLES: readonly AlternateRowStyle[] = ['none', 'background', 'border']

export const MONEY_COLUMN_WIDTH_MIN = 4
export const MONEY_COLUMN_WIDTH_MAX = 14

export interface DisplaySettings {
  showDividers: boolean
  alternateRowStyle: AlternateRowStyle
  alignMoneyDecimals: boolean
  moneyColumnWidth: number
  useChartThemeColors: boolean
}

export interface DisplaySettingsState {
  settings: DisplaySettings
  update: (patch: Partial<DisplaySettings>) => void
}

const STORAGE_KEY = 'settings:display'

export function displaySettingsDefaults(): DisplaySettings {
  return {
    showDividers: false,
    alternateRowStyle: 'none',
    alignMoneyDecimals: true,
    moneyColumnWidth: 9,
    useChartThemeColors: true,
  }
}

export function clampMoneyColumnWidth(value: number): number {
  if (!Number.isFinite(value)) {
    return displaySettingsDefaults().moneyColumnWidth
  }

  return Math.min(Math.max(Math.round(value), MONEY_COLUMN_WIDTH_MIN), MONEY_COLUMN_WIDTH_MAX)
}

export function readDisplaySettings(): DisplaySettings {
  const defaults = displaySettingsDefaults()
  const stored = readSetting<unknown>(STORAGE_KEY, null)

  if (stored === null || typeof stored !== 'object') {
    return defaults
  }

  const raw = stored as Record<string, unknown>

  return {
    showDividers: readBoolean(raw.showDividers, defaults.showDividers),
    alternateRowStyle: ALTERNATE_ROW_STYLES.find(style => style === raw.alternateRowStyle) ?? defaults.alternateRowStyle,
    alignMoneyDecimals: readBoolean(raw.alignMoneyDecimals, defaults.alignMoneyDecimals),
    moneyColumnWidth: typeof raw.moneyColumnWidth === 'number'
      ? clampMoneyColumnWidth(raw.moneyColumnWidth)
      : defaults.moneyColumnWidth,
    useChartThemeColors: readBoolean(raw.useChartThemeColors, defaults.useChartThemeColors),
  }
}

export function shouldShowDivider(settings: DisplaySettings, index: number, count: number): boolean {
  return settings.showDividers && index !== count - 1
}

export function alternateRowClass(style: AlternateRowStyle, index: number): string | null {
  if (style === 'none' || index % 2 !== 1) {
    return null
  }

  return 'operation-row-alt'
}

export function operationsListClasses(settings: DisplaySettings): string[] {
  const classes: string[] = []

  if (settings.alternateRowStyle === 'background') {
    classes.push('operations-alt-bg')
  }

  if (settings.alternateRowStyle === 'border') {
    classes.push('operations-alt-border')
  }

  if (settings.alignMoneyDecimals) {
    classes.push('operations-money-align')
  }

  return classes
}

export function moneyColumnStyle(settings: DisplaySettings): Record<string, string> | null {
  if (!settings.alignMoneyDecimals) {
    return null
  }

  return { '--money-col-width': `${settings.moneyColumnWidth}ch` }
}

export function createDisplaySettings(): DisplaySettingsState {
  const settings = reactive<DisplaySettings>(readDisplaySettings())

  watch(
    settings,
    () => {
      writeSetting(STORAGE_KEY, { ...settings })
    },
    { deep: true, flush: 'sync' },
  )

  function update(patch: Partial<DisplaySettings>): void {
    if (patch.showDividers !== undefined) {
      settings.showDividers = patch.showDividers
    }

    if (patch.alternateRowStyle !== undefined) {
      settings.alternateRowStyle = patch.alternateRowStyle
    }

    if (patch.alignMoneyDecimals !== undefined) {
      settings.alignMoneyDecimals = patch.alignMoneyDecimals
    }

    if (patch.moneyColumnWidth !== undefined) {
      settings.moneyColumnWidth = clampMoneyColumnWidth(patch.moneyColumnWidth)
    }

    if (patch.useChartThemeColors !== undefined) {
      settings.useChartThemeColors = patch.useChartThemeColors
    }
  }

  return { settings, update }
}

let shared: DisplaySettingsState | null = null

export function useDisplaySettings(): DisplaySettingsState {
  shared ??= createDisplaySettings()

  return shared
}

function readBoolean(value: unknown, fallback: boolean): boolean {
  return typeof value === 'boolean' ? value : fallback
}
