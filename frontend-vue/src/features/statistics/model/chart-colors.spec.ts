import { describe, expect, it } from 'vitest'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import { buildBarChartData } from './bar-series'
import { buildCategorySums, buildPieSlices } from './category-sums'
import { categoryColor, CHART_PALETTE } from './chart-colors'

const TODAY = '2026-03-15' as LocalDate
const RANGE = { from: '2026-03-01' as LocalDate, to: '2026-03-02' as LocalDate }

const FUEL = category(3, 'Бензин', null)
const CAFE = category(4, 'Кафе', null)
const HOME = category(5, 'Дом', '#ABCDEF')
const IDLE = category(6, 'Простой', null)

describe('цвета категорий на диаграммах', () => {
  it('берёт цвет категории, а без него палитру из восемнадцати цветов с оборотом', () => {
    expect(CHART_PALETTE).toHaveLength(18)
    expect(CHART_PALETTE.slice(0, 4)).toEqual(['#2196F3', '#F44336', '#4CAF50', '#FFC107'])
    expect(categoryColor({ color: '#123456' }, 7)).toBe('#123456')
    expect(categoryColor({ color: null }, 0)).toBe(CHART_PALETTE[0])
    expect(categoryColor({ color: null }, 18)).toBe(CHART_PALETTE[0])
    expect(categoryColor({ color: null }, 19)).toBe(CHART_PALETTE[1])
  })

  it('запасной цвет берётся по месту категории в наборе', () => {
    const categories = [FUEL, CAFE, HOME, IDLE]

    const operations = [
      operation(1, FUEL, '2026-03-01', 10_000),
      operation(2, CAFE, '2026-03-01', 20_000),
      operation(3, HOME, '2026-03-01', 30_000),
    ]

    const full = buildBarChartData(operations, RANGE, TODAY)
    const alone = buildBarChartData([operation(2, CAFE, '2026-03-01', 20_000)], RANGE, TODAY)
    const slices = buildPieSlices(buildCategorySums(categories, operations, 1))

    expect(colorOf(full.series, 'Бензин')).toBe(CHART_PALETTE[0])
    expect(colorOf(full.series, 'Кафе')).toBe(CHART_PALETTE[1])
    expect(colorOf(full.series, 'Дом')).toBe('#ABCDEF')
    expect(colorOf(alone.series, 'Кафе')).toBe(CHART_PALETTE[0])

    expect(colorOf(slices, 'Бензин')).toBe(CHART_PALETTE[0])
    expect(colorOf(slices, 'Дом')).toBe('#ABCDEF')
    expect(colorOf(slices, 'Кафе')).toBe(CHART_PALETTE[1])
  })
})

function colorOf(items: readonly { label: string, color: string }[], label: string): string {
  const found = items.find(item => item.label === label)

  if (found === undefined) {
    throw new Error(`Категория ${label} не попала в диаграмму`)
  }

  return found.color
}

function category(id: number, name: string, color: string | null): Category {
  return { id, parentId: null, name, color, order: null, operationTypeId: 1, isDeleted: false }
}

function operation(id: number, owner: Category, date: string, minorUnits: number): Operation {
  return {
    id,
    categoryId: owner.id,
    category: owner,
    sum: Money.fromMinorUnits(minorUnits),
    comment: null,
    place: null,
    date: date as LocalDate,
    createdTaskId: null,
    isDeleted: false,
  }
}
