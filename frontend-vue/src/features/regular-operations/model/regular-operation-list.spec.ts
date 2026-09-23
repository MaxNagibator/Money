import { describe, expect, it } from 'vitest'
import type { RegularOperation } from '@/api/adapters/regular-operations'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import type { RegularOperationKind, RegularOperationView } from './regular-operation-list'
import {
  categoryNameOf,
  formatRunTime,
  regularOperationKind,
  scheduleText,
  sortRegularOperations,
  UNKNOWN_CATEGORY_NAME,
  withCategories,
} from './regular-operation-list'

const food = category(1, 'Еда', 1)
const salary = category(2, 'Зарплата', 2)

describe('порядок списка', () => {
  it('ближайший запуск первым, без запуска в конце', () => {
    const sorted = sortRegularOperations([
      view(1, { runTime: null }),
      view(2, { runTime: localDate('2026-09-10') }),
      view(3, { runTime: localDate('2026-09-01') }),
    ])

    expect(sorted.map(item => item.id)).toEqual([3, 2, 1])
  })

  it('равные даты запуска сохраняют порядок ответа', () => {
    const sorted = sortRegularOperations([
      view(5, { runTime: localDate('2026-09-01') }),
      view(4, { runTime: localDate('2026-09-01') }),
      view(6, { runTime: null }),
      view(7, { runTime: null }),
    ])

    expect(sorted.map(item => item.id)).toEqual([5, 4, 6, 7])
  })

  it('сортировка не меняет исходный список', () => {
    const source = [view(1, { runTime: null }), view(2, { runTime: localDate('2026-09-01') })]

    sortRegularOperations(source)

    expect(source.map(item => item.id)).toEqual([1, 2])
  })
})

describe('расписание в строке телефона', () => {
  it.each([
    [3, '2026-09-26', 'следующая 26.09 · каждый месяц'],
    [2, '2026-10-05', 'следующая 05.10 · каждую неделю'],
    [1, null, 'запусков больше нет · каждый день'],
    [99, '2026-09-26', 'следующая 26.09'],
    [99, null, 'запусков больше нет'],
  ])('шаг %j и запуск %j дают %j', (timeTypeId, runTime, expected) => {
    const schedule = scheduleText(view(1, { timeTypeId, runTime: runTime === null ? null : localDate(runTime) }))

    expect(schedule).toBe(expected)
  })
})

describe('категория операции', () => {
  it('подставляется по идентификатору из справочника', () => {
    const views = withCategories([operation(1, { categoryId: salary.id })], [food, salary])

    expect(views[0]?.category).toEqual(salary)
    expect(categoryNameOf(views[0]!)).toBe('Зарплата')
  })

  it('операция с чужой категорией остаётся в списке под именем несуществующей', () => {
    const views = withCategories([operation(1, { categoryId: 99 })], [food, salary])

    expect(views).toHaveLength(1)
    expect(views[0]?.category).toBeNull()
    expect(categoryNameOf(views[0]!)).toBe(UNKNOWN_CATEGORY_NAME)
  })
})

describe('вид карточки', () => {
  it.each<[string, Category | null, RegularOperationKind]>([
    ['категория расходов красит расходом', food, 'expense'],
    ['категория доходов красит доходом', salary, 'income'],
    ['неизвестная категория остаётся доходом', null, 'income'],
  ])('%s', (_, value, expected) => {
    expect(regularOperationKind(view(1, { category: value }))).toBe(expected)
  })

  it('удалённая карточка перекрывает оба вида', () => {
    expect(regularOperationKind(view(1, { category: food, isDeleted: true }))).toBe('deleted')
  })
})

describe('чип ближайшего запуска', () => {
  it('показывает дату дня без времени', () => {
    expect(formatRunTime(localDate('2026-09-01'))).toBe('01.09.2026')
  })

  it('без ближайшего запуска показывает знак бесконечности', () => {
    expect(formatRunTime(null)).toBe('∞')
  })
})

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function category(id: number, name: string, operationTypeId: OperationTypeId): Category {
  return { id, parentId: null, name, color: null, order: null, operationTypeId, isDeleted: false }
}

function operation(id: number, extra: Partial<RegularOperation> = {}): RegularOperation {
  return {
    id,
    name: `Операция ${id}`,
    categoryId: food.id,
    sum: Money.parseInput('100')!,
    comment: null,
    place: null,
    timeTypeId: 3,
    timeValue: 1,
    dateFrom: localDate('2026-08-23'),
    dateTo: null,
    runTime: localDate('2026-09-01'),
    isDeleted: false,
    ...extra,
  }
}

function view(id: number, extra: Partial<RegularOperationView> = {}): RegularOperationView {
  return { ...operation(id), category: food, ...extra }
}
