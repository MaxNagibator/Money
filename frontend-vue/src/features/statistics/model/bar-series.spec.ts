import { describe, expect, it } from 'vitest'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import { buildBarChartData } from './bar-series'

const TODAY = local('2026-03-15')

const FOOD = category(2, 'Продукты')
const CAR = category(1, 'Авто')

describe('столбчатая диаграмма статистики', () => {
  it('даёт по датасету на категорию из пришедших операций в порядке их появления', () => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-01', 10_000), operation(2, CAR, '2026-03-02', 20_000)],
      { from: local('2026-03-01'), to: local('2026-03-03') },
      TODAY,
    )

    expect(data.series.map(series => series.label)).toEqual(['Продукты', 'Авто'])
    expect(data.labels).toEqual(['01.03.2026', '02.03.2026', '03.03.2026'])
    expect(data.series[0]?.values).toEqual([100, null, null])
    expect(data.series[1]?.values).toEqual([null, 200, null])
  })

  it.each([
    { step: 'день', last: '2026-04-01', buckets: 32, first: '01.03.2026' },
    { step: 'неделя', last: '2026-04-02', buckets: 6, first: '23 февраля' },
  ])('выбирает шаг «$step» по длине периода', ({ last, buckets, first }) => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-01', 10_000), operation(2, FOOD, last, 20_000)],
      { from: local('2026-01-01'), to: local('2026-12-31') },
      TODAY,
    )

    expect(data.labels).toHaveLength(buckets)
    expect(data.labels[0]).toBe(first)
  })

  it.each([
    { step: 'неделя', last: '2026-05-21', buckets: 21, first: '29 декабря' },
    { step: 'месяц', last: '2026-05-22', buckets: 5, first: 'январь 2026' },
  ])('переключается на шаг «$step» на границе в 140 дней', ({ last, buckets, first }) => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-01-01', 10_000), operation(2, FOOD, last, 20_000)],
      { from: local('2026-01-01'), to: local('2026-12-31') },
      TODAY,
    )

    expect(data.labels).toHaveLength(buckets)
    expect(data.labels[0]).toBe(first)
  })

  it('подписывает корзины днём, неделей и месяцем по-русски', () => {
    const day = buildBarChartData([], { from: local('2026-03-01'), to: local('2026-03-01') }, TODAY)

    const week = buildBarChartData(
      [operation(1, FOOD, '2026-03-01', 10_000), operation(2, FOOD, '2026-04-02', 10_000)],
      { from: local('2026-01-01'), to: local('2026-12-31') },
      TODAY,
    )

    const month = buildBarChartData(
      [operation(1, FOOD, '2026-01-01', 10_000), operation(2, FOOD, '2026-12-31', 10_000)],
      { from: local('2026-01-01'), to: local('2026-12-31') },
      TODAY,
    )

    expect(day.labels).toEqual(['01.03.2026'])
    expect(week.labels[1]).toBe('02 марта')
    expect(month.labels[2]).toBe('март 2026')
    expect(month.labels).toHaveLength(12)
  })

  it('берёт границы из дат операций, когда период длиннее десяти дней', () => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-10', 10_000), operation(2, FOOD, '2026-03-12', 20_000)],
      { from: local('2026-03-01'), to: local('2026-03-31') },
      TODAY,
    )

    expect(data.labels).toEqual(['10.03.2026', '11.03.2026', '12.03.2026'])
  })

  it('границы берутся из отдельного списка, а рисуются только свои операции', () => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-10', 10_000)],
      { from: local('2026-03-01'), to: local('2026-03-31') },
      TODAY,
      [operation(1, FOOD, '2026-03-10', 10_000), operation(2, FOOD, '2026-03-12', 20_000)],
    )

    expect(data.labels).toEqual(['10.03.2026', '11.03.2026', '12.03.2026'])
    expect(data.series[0]?.values).toEqual([100, null, null])
  })

  it('оставляет границы фильтра на периоде до десяти дней', () => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-05', 10_000)],
      { from: local('2026-03-01'), to: local('2026-03-11') },
      TODAY,
    )

    expect(data.labels).toHaveLength(11)
    expect(data.labels.at(-1)).toBe('11.03.2026')
    expect(data.series[0]?.values).toEqual([null, null, null, null, 100, null, null, null, null, null, null])
  })

  it.each([
    { period: 'не задан', range: { from: null, to: null } },
    { period: 'задан только началом', range: { from: local('2026-03-10'), to: null } },
    { period: 'задан только концом', range: { from: null, to: local('2026-03-12') } },
  ])('берёт границы из дат операций, когда период $period', ({ range }) => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-10', 10_000), operation(2, FOOD, '2026-03-12', 20_000)],
      range,
      TODAY,
    )

    expect(data.labels).toEqual(['10.03.2026', '11.03.2026', '12.03.2026'])
    expect(data.series[0]?.values).toEqual([100, null, 200])
  })

  it('на пустом списке операций даёт одну корзину за сегодня', () => {
    const data = buildBarChartData([], { from: local('2026-01-01'), to: local('2026-12-31') }, TODAY)

    expect(data.labels).toEqual(['15.03.2026'])
    expect(data.series).toEqual([])
  })

  it('пишет нулевую сумму корзины как null, оставляя категорию в легенде', () => {
    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-01', 10_000), operation(2, FOOD, '2026-03-01', -10_000)],
      { from: local('2026-03-01'), to: local('2026-03-02') },
      TODAY,
    )

    expect(data.series.map(series => series.label)).toEqual(['Продукты'])
    expect(data.series[0]?.values).toEqual([null, null])
  })

  it('пропускает операции вне корзин и без известной категории', () => {
    const homeless: Operation = { ...operation(3, CAR, '2026-03-01', 50_000), categoryId: 99, category: null }
    const outside = operation(4, CAR, '2026-03-05', 70_000)

    const data = buildBarChartData(
      [operation(1, FOOD, '2026-03-01', 10_000), homeless, outside],
      { from: local('2026-03-01'), to: local('2026-03-02') },
      TODAY,
    )

    expect(data.series.map(series => series.label)).toEqual(['Продукты', 'Авто'])
    expect(data.series[0]?.values).toEqual([100, null])
    expect(data.series[1]?.values).toEqual([null, null])
  })
})

function category(id: number, name: string): Category {
  return { id, parentId: null, name, color: null, order: null, operationTypeId: 1, isDeleted: false }
}

function operation(id: number, owner: Category, date: string, minorUnits: number): Operation {
  return {
    id,
    categoryId: owner.id,
    category: owner,
    sum: Money.fromMinorUnits(minorUnits),
    comment: null,
    place: null,
    date: local(date),
    createdTaskId: null,
    isDeleted: false,
  }
}

function local(value: string): LocalDate {
  return value as LocalDate
}
