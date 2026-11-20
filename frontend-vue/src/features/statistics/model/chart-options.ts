import type { ChartOptions } from 'chart.js'
import type { ChartChrome, ThemeSource } from '@/shared/ui/chart-theme'
import { resolveChartChrome } from '@/shared/ui/chart-theme'

export type ChartPalette = ChartChrome
export type { ThemeSource }

const BAR_LEGEND_BOX_WIDTH = 0
const NARROW_BAR_LEGEND_BOX_WIDTH = 12
const PIE_LEGEND_BOX_WIDTH = 50
const NARROW_PIE_LEGEND_BOX_WIDTH = 20
const NARROW_PIE_ASPECT_RATIO = 1.6
const NARROW_BAR_ASPECT_RATIO = 1
const NARROW_BAR_TICKS = { autoSkip: true, maxTicksLimit: 3, maxRotation: 45 }

export function themePalette(theme: ThemeSource): ChartPalette {
  return resolveChartChrome(theme)
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
