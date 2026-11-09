import { describe, expect, it } from 'vitest'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation, OperationsDay } from '@/domain/operation'
import { calculateDayTotal, fillEmptyDays, groupOperationsByDay } from '@/domain/operation'

const food: Category = {
  id: 1,
  parentId: null,
  name: 'Еда',
  color: null,
  order: null,
  operationTypeId: 1,
  isDeleted: false,
}

const salary: Category = { ...food, id: 2, name: 'Зарплата', operationTypeId: 2 }

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function operation(id: number, date: string, minorUnits: number, extra: Partial<Operation> = {}): Operation {
  return {
    id,
    categoryId: food.id,
    category: food,
    sum: Money.fromMinorUnits(minorUnits),
    comment: null,
    place: null,
    date: localDate(date),
    createdTaskId: null,
    isDeleted: false,
    ...extra,
  }
}

function emptyDay(date: string): OperationsDay {
  return { date: localDate(date), operations: [] }
}

function dayOf(date: string, operations: Operation[]): OperationsDay {
  return { date: localDate(date), operations }
}

describe('groupOperationsByDay', () => {
  it('собирает операции одного дня в одну группу и сохраняет порядок ответа', () => {
    const days = groupOperationsByDay([
      operation(1, '2026-08-22', 10_000),
      operation(2, '2026-08-22', 20_000),
      operation(3, '2026-08-21', 30_000),
    ])

    expect(days.map(day => day.date)).toEqual(['2026-08-22', '2026-08-21'])
    expect(days[0]?.operations.map(item => item.id)).toEqual([1, 2])
    expect(days[1]?.operations.map(item => item.id)).toEqual([3])
  })

  it('ставит дни по убыванию даты независимо от порядка ответа', () => {
    const days = groupOperationsByDay([
      operation(1, '2026-08-19', 10_000),
      operation(2, '2026-09-01', 10_000),
      operation(3, '2026-08-22', 10_000),
    ])

    expect(days.map(day => day.date)).toEqual(['2026-09-01', '2026-08-22', '2026-08-19'])
  })

  it('держит в одном дне операции разных типов', () => {
    const days = groupOperationsByDay([
      operation(1, '2026-08-22', 10_000),
      operation(2, '2026-08-22', 20_000, { categoryId: salary.id, category: salary }),
    ])

    expect(days).toHaveLength(1)
    expect(days[0]?.operations.map(item => item.id)).toEqual([1, 2])
  })
})

describe('fillEmptyDays', () => {
  it('достраивает пустые дни между занятыми', () => {
    const days = fillEmptyDays([
      dayOf('2026-08-22', [operation(1, '2026-08-22', 10_000)]),
      dayOf('2026-08-19', [operation(2, '2026-08-19', 10_000)]),
    ])

    expect(days.map(day => day.date)).toEqual(['2026-08-22', '2026-08-21', '2026-08-20', '2026-08-19'])
    expect(days[1]?.operations).toEqual([])
    expect(days[2]?.operations).toEqual([])
  })

  it('не дополняет края периода', () => {
    expect(fillEmptyDays([emptyDay('2026-08-22')]).map(day => day.date)).toEqual(['2026-08-22'])
    expect(fillEmptyDays([])).toEqual([])
  })

  it('ничего не вставляет между соседними днями', () => {
    const days = fillEmptyDays([emptyDay('2026-08-22'), emptyDay('2026-08-21')])

    expect(days.map(day => day.date)).toEqual(['2026-08-22', '2026-08-21'])
  })

  it('достраивает разрыв через границу месяца', () => {
    const days = fillEmptyDays([emptyDay('2026-09-01'), emptyDay('2026-08-30')])

    expect(days.map(day => day.date)).toEqual(['2026-09-01', '2026-08-31', '2026-08-30'])
  })
})

describe('calculateDayTotal', () => {
  it('считает итог по типу операции', () => {
    const day = dayOf('2026-08-22', [
      operation(1, '2026-08-22', 10_000),
      operation(2, '2026-08-22', 5_000),
      operation(3, '2026-08-22', 20_000, { categoryId: salary.id, category: salary }),
    ])

    expect(calculateDayTotal(day, 1).toMinorUnits()).toBe(15_000)
    expect(calculateDayTotal(day, 2).toMinorUnits()).toBe(20_000)
  })

  it('не учитывает удалённые операции', () => {
    const day = dayOf('2026-08-22', [
      operation(1, '2026-08-22', 10_000),
      operation(2, '2026-08-22', 5_000, { isDeleted: true }),
    ])

    expect(calculateDayTotal(day, 1).toMinorUnits()).toBe(10_000)
  })

  it('не учитывает операции с нераспознанной категорией', () => {
    const day = dayOf('2026-08-22', [
      operation(1, '2026-08-22', 10_000),
      operation(2, '2026-08-22', 5_000, { categoryId: 99, category: null }),
    ])

    expect(calculateDayTotal(day, 1).toMinorUnits()).toBe(10_000)
    expect(calculateDayTotal(day, 2).toMinorUnits()).toBe(0)
  })
})
