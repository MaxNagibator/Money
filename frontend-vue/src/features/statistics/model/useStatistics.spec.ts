import { describe, expect, it } from 'vitest'
import type { ShallowRef } from 'vue'
import { reactive, shallowRef } from 'vue'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation, OperationsDay } from '@/domain/operation'
import type { OperationTypeId } from '@/domain/operation-type'
import type { OperationFilters } from '@/features/operations'
import type { Clock } from '@/platform/clock'
import type { StatisticsState } from './useStatistics'
import { useStatistics } from './useStatistics'

const TODAY = '2026-03-15' as LocalDate

const clock: Clock = {
  now: () => new Date('2026-03-15T12:00:00'),
  today: () => TODAY,
}

const CATEGORIES: readonly Category[] = [
  category(1, 'Продукты', 1),
  category(2, 'Кафе', 1, 1),
  category(9, 'Зарплата', 2),
]

const DAYS: readonly OperationsDay[] = [
  { date: '2026-03-11' as LocalDate, operations: [operation(2, 2, '2026-03-11', 3_000)] },
  { date: '2026-03-10' as LocalDate, operations: [operation(1, 1, '2026-03-10', 10_000)] },
]

describe('состояние экрана статистики', () => {
  it('разводит операции по панелям типов в порядке справочника', () => {
    const { state } = setup()
    const expenses = state.panels.value[0]

    expect(state.panels.value.map(panel => panel.type.name)).toEqual(['Расходы', 'Доходы'])
    expect(expenses?.isEmpty).toBe(false)
    expect(expenses?.bar.series.map(series => series.label)).toEqual(['Кафе', 'Продукты'])
    expect(expenses?.tree[0]?.total.toMinorUnits()).toBe(13_000)
    expect(expenses?.pie.map(slice => slice.label)).toEqual(['Продукты'])
  })

  it('помечает пустым тип без операций и оставляет корень дерева с нулём', () => {
    const { state } = setup()
    const income = state.panels.value[1]

    expect(income?.isEmpty).toBe(true)
    expect(income?.pie).toEqual([])
    expect(income?.tree[0]?.name).toBe('Всего')
    expect(income?.tree[0]?.total.isZero).toBe(true)
    expect(income?.bar.labels).toEqual(['10.03.2026', '11.03.2026'])
  })

  it('берёт границы столбцов из всей выдачи, поэтому оси панелей совпадают', () => {
    const { state, days } = setup()

    days.value = [
      { date: '2026-03-20' as LocalDate, operations: [operation(3, 9, '2026-03-20', 50_000)] },
      { date: '2026-03-11' as LocalDate, operations: [operation(2, 2, '2026-03-11', 3_000)] },
      { date: '2026-03-10' as LocalDate, operations: [operation(1, 1, '2026-03-10', 10_000)] },
    ]

    const expected = ['10.03.2026', '11.03.2026', '12.03.2026', '13.03.2026', '14.03.2026', '15.03.2026', '16.03.2026', '17.03.2026', '18.03.2026', '19.03.2026', '20.03.2026']

    expect(state.panels.value[0]?.bar.labels).toEqual(expected)
    expect(state.panels.value[1]?.bar.labels).toEqual(expected)
  })

  it('не считает локально удалённую операцию', () => {
    const { state, days } = setup()

    days.value = [{
      date: '2026-03-10' as LocalDate,
      operations: [{ ...operation(1, 1, '2026-03-10', 10_000), isDeleted: true }],
    }]

    expect(state.panels.value[0]?.isEmpty).toBe(true)
    expect(state.panels.value[0]?.tree[0]?.total.isZero).toBe(true)
  })

  it('пересчитывает панели при смене периода и состава операций', () => {
    const { state, days, filters } = setup()

    days.value = [{ date: '2026-04-02' as LocalDate, operations: [operation(3, 9, '2026-04-02', 50_000)] }]
    filters.from = '2026-04-01' as LocalDate
    filters.to = '2026-04-30' as LocalDate

    expect(state.panels.value[0]?.isEmpty).toBe(true)
    expect(state.panels.value[1]?.isEmpty).toBe(false)
    expect(state.panels.value[1]?.tree[0]?.total.toMinorUnits()).toBe(50_000)
    expect(state.panels.value[1]?.bar.labels).toEqual(['02.04.2026'])
  })
})

function setup(): {
  state: StatisticsState
  days: ShallowRef<readonly OperationsDay[]>
  filters: OperationFilters
} {
  const days = shallowRef<readonly OperationsDay[]>(DAYS)
  const categories = shallowRef<readonly Category[]>(CATEGORIES)

  const filters = reactive<OperationFilters>({
    from: '2026-03-01' as LocalDate,
    to: '2026-03-31' as LocalDate,
    interval: null,
    comment: '',
    place: '',
    categoryIds: [],
    showEmptyDays: false,
  })

  return { state: useStatistics(days, filters, categories, clock), days, filters }
}

function category(id: number, name: string, operationTypeId: OperationTypeId, parentId: number | null = null): Category {
  return { id, parentId, name, color: null, order: null, operationTypeId, isDeleted: false }
}

function operation(id: number, categoryId: number, date: string, minorUnits: number): Operation {
  return {
    id,
    categoryId,
    category: CATEGORIES.find(item => item.id === categoryId) ?? null,
    sum: Money.fromMinorUnits(minorUnits),
    comment: null,
    place: null,
    date: date as LocalDate,
    createdTaskId: null,
    isDeleted: false,
  }
}
