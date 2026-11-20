export interface ChartChrome {
  text: string
  mutedText: string
  grid: string
}

export interface ThemeSource {
  colors: Readonly<Record<string, unknown>>
  variables: Readonly<Record<string, unknown>>
}

const DEFAULT_TEXT_COLOR = '#000000'
const MUTED_OPACITY = 0.6
const GRID_OPACITY = 0.12
const HEX_PATTERN = /^#(?<digits>[\da-f]{3}|[\da-f]{6})$/i
const SHORT_HEX_LENGTH = 3
const HEX_RADIX = 16

export function resolveChartChrome(theme: ThemeSource, defaultText = DEFAULT_TEXT_COLOR): ChartChrome {
  const text = readColor(theme.colors, 'on-surface', defaultText)
  const border = readColor(theme.variables, 'border-color', text)

  return {
    text,
    mutedText: withAlpha(text, readOpacity(theme.variables, 'medium-emphasis-opacity', MUTED_OPACITY)),
    grid: withAlpha(border, readOpacity(theme.variables, 'border-opacity', GRID_OPACITY)),
  }
}

export function withAlpha(color: string, alpha: number): string {
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

function readColor(source: Readonly<Record<string, unknown>>, key: string, fallback: string): string {
  const value = source[key]

  return typeof value === 'string' ? value : fallback
}

function readOpacity(source: Readonly<Record<string, unknown>>, key: string, fallback: number): number {
  const value = source[key]

  return typeof value === 'number' ? value : fallback
}
