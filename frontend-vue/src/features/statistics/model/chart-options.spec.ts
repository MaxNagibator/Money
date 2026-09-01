import { describe, expect, it } from 'vitest'
import { buildBarOptions, buildPieOptions, themePalette } from './chart-options'

const THEME = {
  colors: { 'on-surface': '#1B2A3C' },
  variables: { 'border-color': '#000000', 'border-opacity': 0.12, 'medium-emphasis-opacity': 0.6 },
}

describe('конфигурация диаграмм статистики', () => {
  it('складывает столбцы стопкой, ведёт ось Y от нуля и держит легенду сверху без цветной метки', () => {
    const options = buildBarOptions(null)

    expect(options.responsive).toBe(true)
    expect(plain(options.scales?.x).stacked).toBe(true)
    expect(plain(options.scales?.y).stacked).toBe(true)
    expect(plain(options.scales?.y).beginAtZero).toBe(true)
    expect(options.plugins?.legend?.display).toBe(true)
    expect(options.plugins?.legend?.position).toBe('top')
    expect(options.plugins?.legend?.labels?.boxWidth).toBe(0)
  })

  it('держит легенду круговой диаграммы справа с шириной метки 50', () => {
    const options = buildPieOptions(null)

    expect(options.responsive).toBe(true)
    expect(options.plugins?.legend?.display).toBe(true)
    expect(options.plugins?.legend?.position).toBe('right')
    expect(options.plugins?.legend?.labels?.boxWidth).toBe(50)
  })

  it('подставляет цвета темы готовыми значениями, без ссылок на CSS-переменные', () => {
    const palette = themePalette(THEME)
    const bar = buildBarOptions(palette)
    const pie = buildPieOptions(palette)

    expect(palette).toEqual({
      text: '#1B2A3C',
      mutedText: 'rgba(27, 42, 60, 0.6)',
      grid: 'rgba(0, 0, 0, 0.12)',
    })

    expect(plain(bar.scales?.x).ticks).toEqual({ color: palette.mutedText })
    expect(plain(bar.scales?.y).grid).toEqual({ color: palette.grid })
    expect(bar.plugins?.legend?.labels?.color).toBe(palette.text)
    expect(pie.plugins?.legend?.labels?.color).toBe(palette.text)
    expect(JSON.stringify([bar, pie])).not.toContain('var(--')
  })

  it('обходится без цветов темы, когда настройка выключена', () => {
    const bar = buildBarOptions(null)
    const pie = buildPieOptions(null)

    expect(plain(bar.scales?.x).ticks).toBeUndefined()
    expect(plain(bar.scales?.y).grid).toBeUndefined()
    expect(bar.plugins?.legend?.labels?.color).toBeUndefined()
    expect(pie.plugins?.legend?.labels?.color).toBeUndefined()
  })

  it('разворачивает короткий hex и не трогает нераспознанный цвет', () => {
    const short = themePalette({ colors: { 'on-surface': '#abc' }, variables: {} })
    const named = themePalette({ colors: { 'on-surface': 'currentColor' }, variables: {} })

    expect(short.mutedText).toBe('rgba(170, 187, 204, 0.6)')
    expect(short.grid).toBe('rgba(170, 187, 204, 0.12)')
    expect(named.text).toBe('currentColor')
    expect(named.mutedText).toBe('currentColor')
  })
})

function plain(value: unknown): Record<string, unknown> {
  return (value ?? {}) as Record<string, unknown>
}
