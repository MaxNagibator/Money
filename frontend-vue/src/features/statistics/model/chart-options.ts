import type { ChartOptions } from 'chart.js'

export interface ChartPalette {
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
const BAR_LEGEND_BOX_WIDTH = 0
const NARROW_BAR_LEGEND_BOX_WIDTH = 12
const PIE_LEGEND_BOX_WIDTH = 50
const NARROW_PIE_LEGEND_BOX_WIDTH = 20
const NARROW_PIE_ASPECT_RATIO = 1.6
const NARROW_BAR_ASPECT_RATIO = 1
const NARROW_BAR_TICKS ={ autoSkip: true, maxTicksLimit: 3, maxRotation: 45 }
const HEX_PATTERN = /^#(?<digits>[\da-f]{3}|[\da-f]{6})$/i
const SHORT_HEX_LENGTH = 3
const HEX_RADIX = 16

export function themePalette(theme: ThemeSource): ChartPalette {
  const text = readColor(theme.colors, 'on-surface', DEFAULT_TEXT_COLOR)
  const border = readColor(theme.variables, 'border-color', text)

  return {
    text,
    mutedText: withAlpha(text, readOpacity(theme.variables, 'medium-emphasis-opacity', MUTED_OPACITY)),
    grid: withAlpha(border, readOpacity(theme.variables, 'border-opacity', GRID_OPACITY)),
  }
}

export function buildBarOptions(palette: ChartPalette | null, isNarrow = false): ChartOptions<'bar'> {
  const narrowTicks = isNarrow ? NARROW_BAR_TICKS : {}
  const legendPosition = isNarrow ? 'bottom' as const : 'top' as const
  const aspect = isNarrow ? { aspectRatio: NARROW_BAR_ASPECT_RATIO } : {}
  const boxWidth = isNarrow ? NARROW_BAR_LEGEND_BOX_WIDTH : BAR_LEGEND_BOX_WIDTH

  if (palette === null) {
    return {
      responsive: true,
      ...aspect,
      scales: {
        x: { stacked: true, ...(isNarrow ? { ticks: narrowTicks } : {}) },
        y: { stacked: true, beginAtZero: true },
      },
      plugins: {
        legend: { display: true, position: legendPosition, labels: { boxWidth } },
      },
    }
  }

  return {
    responsive: true,
    ...aspect,
    scales: {
      x: {
        stacked: true,
        ticks: { ...narrowTicks, color: palette.mutedText },
        grid: { color: palette.grid },
      },
      y: {
        stacked: true,
        beginAtZero: true,
        ticks: { color: palette.mutedText },
        grid: { color: palette.grid },
      },
    },
    plugins: {
      legend: { display: true, position: legendPosition, labels: { boxWidth, color: palette.text } },
    },
  }
}

export function buildPieOptions(palette: ChartPalette | null, isNarrow = false): ChartOptions<'pie'> {
  const legendPosition = isNarrow ? 'bottom' as const : 'right' as const
  const boxWidth = isNarrow ? NARROW_PIE_LEGEND_BOX_WIDTH : PIE_LEGEND_BOX_WIDTH
  const aspect = isNarrow ? { aspectRatio: NARROW_PIE_ASPECT_RATIO } : {}

  if (palette === null) {
    return {
      responsive: true,
      ...aspect,
      plugins: {
        legend: { display: true, position: legendPosition, labels: { boxWidth } },
      },
    }
  }

  return {
    responsive: true,
    ...aspect,
    plugins: {
      legend: {
        display: true,
        position: legendPosition,
        labels: { boxWidth, color: palette.text },
      },
    },
  }
}

function readColor(source: Readonly<Record<string, unknown>>, key: string, fallback: string): string {
  const value = source[key]

  return typeof value === 'string' ? value : fallback
}

function readOpacity(source: Readonly<Record<string, unknown>>, key: string, fallback: number): number {
  const value = source[key]

  return typeof value === 'number' ? value : fallback
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
