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
const PIE_LEGEND_BOX_WIDTH = 50
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

export function buildBarOptions(palette: ChartPalette | null): ChartOptions<'bar'> {
  if (palette === null) {
    return {
      responsive: true,
      scales: {
        x: { stacked: true },
        y: { stacked: true, beginAtZero: true },
      },
      plugins: {
        legend: { display: true, position: 'top', labels: { boxWidth: BAR_LEGEND_BOX_WIDTH } },
      },
    }
  }

  return {
    responsive: true,
    scales: {
      x: {
        stacked: true,
        ticks: { color: palette.mutedText },
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
      legend: { display: true, position: 'top', labels: { boxWidth: BAR_LEGEND_BOX_WIDTH, color: palette.text } },
    },
  }
}

export function buildPieOptions(palette: ChartPalette | null): ChartOptions<'pie'> {
  if (palette === null) {
    return {
      responsive: true,
      plugins: {
        legend: { display: true, position: 'right', labels: { boxWidth: PIE_LEGEND_BOX_WIDTH } },
      },
    }
  }

  return {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'right',
        labels: { boxWidth: PIE_LEGEND_BOX_WIDTH, color: palette.text },
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
