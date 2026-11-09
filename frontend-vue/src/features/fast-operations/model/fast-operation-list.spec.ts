import { describe, expect, it } from 'vitest'
import type { FastOperation } from '@/api/adapters/fast-operations'
import type { Category } from '@/domain/category'
import { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import type { FastOperationView } from './fast-operation-list'
import {
  categoryNameOf,
  fastOperationKind,
  sortFastOperations,
  UNKNOWN_CATEGORY_NAME,
  withCategories,
} from './fast-operation-list'

function category(id: number, name: string, operationTypeId: OperationTypeId = 1): Category {
  return { id, parentId: null, name, color: null, order: null, operationTypeId, isDeleted: false }
}

function operation(id: number, extra: Partial<FastOperation> = {}): FastOperation {
  return {
    id,
    name: `Шаблон ${id}`,
    order: null,
    categoryId: 1,
    sum: Money.zero,
    comment: null,
    place: null,
    isDeleted: false,
    ...extra,
  }
}

function view(id: number, extra: Partial<FastOperationView> = {}): FastOperationView {
  return { ...operation(id), category: category(1, 'Еда'), ...extra }
}

describe('порядок карточек', () => {
  it('сначала с приоритетом по возрастанию, затем без него', () => {
    const sorted = sortFastOperations([
      view(1, { order: null }),
      view(2, { order: 5 }),
      view(3, { order: 1 }),
    ])

    expect(sorted.map(item => item.id)).toEqual([3, 2, 1])
  })

  it('при равном приоритете сравнивает имя категории, затем имя операции', () => {
    const sorted = sortFastOperations([
      view(1, { name: 'Ямал', category: category(1, 'Еда') }),
      view(2, { name: 'Аптека', category: category(2, 'Здоровье') }),
      view(3, { name: 'Батон', category: category(1, 'Еда') }),
    ])

    expect(sorted.map(item => item.id)).toEqual([3, 1, 2])
  })

  it('сортировка не меняет исходный список', () => {
    const source = [view(1, { order: 3 }), view(2, { order: 1 })]

    sortFastOperations(source)

    expect(source.map(item => item.id)).toEqual([1, 2])
  })
})

describe('неизвестная категория', () => {
  it('операция с чужим идентификатором категории остаётся в списке без категории', () => {
    const views = withCategories([operation(1, { categoryId: 99 })], [category(1, 'Еда')])

    expect(views[0]!.category).toBeNull()
    expect(categoryNameOf(views[0]!)).toBe(UNKNOWN_CATEGORY_NAME)
  })

  it('подставленное имя участвует в сортировке по категории', () => {
    const sorted = sortFastOperations([
      view(1, { category: null }),
      view(2, { category: category(2, 'Яхта') }),
      view(3, { category: category(1, 'Аренда') }),
    ])

    expect(sorted.map(item => item.id)).toEqual([3, 1, 2])
  })
})

describe('вид карточки', () => {
  it('категория расходов красит карточку расходом, любая другая доходом', () => {
    expect(fastOperationKind(view(1, { category: category(1, 'Еда', 1) }))).toBe('expense')
    expect(fastOperationKind(view(2, { category: category(2, 'Работа', 2) }))).toBe('income')
  })

  it('неизвестная категория даёт нейтральный вид, а не доход', () => {
    expect(fastOperationKind(view(1, { category: null }))).toBe('unknown')
  })

  it('удалённая карточка перекрывает оба вида', () => {
    expect(fastOperationKind(view(1, { isDeleted: true, category: category(1, 'Еда', 1) }))).toBe('deleted')
    expect(fastOperationKind(view(2, { isDeleted: true, category: null }))).toBe('deleted')
  })
})
