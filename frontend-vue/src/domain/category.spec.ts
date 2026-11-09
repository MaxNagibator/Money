import { describe, expect, it } from 'vitest'
import type { Category } from '@/domain/category'
import { buildCategoryTree, findCategoryNode } from '@/domain/category'

function category(id: number, name: string, extra: Partial<Category> = {}): Category {
  return {
    id,
    parentId: null,
    name,
    color: null,
    order: null,
    operationTypeId: 1,
    isDeleted: false,
    ...extra,
  }
}

describe('buildCategoryTree', () => {
  it('оставляет только категории запрошенного типа', () => {
    const tree = buildCategoryTree([
      category(1, 'Еда'),
      category(2, 'Зарплата', { operationTypeId: 2 }),
    ], 1)

    expect(tree.map(node => node.category.name)).toEqual(['Еда'])
  })

  it('вкладывает потомков в родителей', () => {
    const tree = buildCategoryTree([
      category(1, 'Еда'),
      category(2, 'Кафе', { parentId: 1 }),
      category(3, 'Кофе', { parentId: 2 }),
    ], 1)

    expect(tree).toHaveLength(1)
    expect(tree[0]?.children[0]?.category.name).toBe('Кафе')
    expect(tree[0]?.children[0]?.children[0]?.category.name).toBe('Кофе')
  })

  it('ставит категории с порядком выше остальных', () => {
    const tree = buildCategoryTree([
      category(1, 'Аренда'),
      category(2, 'Транспорт', { order: 2 }),
      category(3, 'Еда', { order: 1 }),
    ], 1)

    expect(tree.map(node => node.category.name)).toEqual(['Еда', 'Транспорт', 'Аренда'])
  })

  it('равный порядок разбирает по имени в русской культуре', () => {
    const tree = buildCategoryTree([
      category(1, 'Ёлка'),
      category(2, 'Ель'),
      category(3, 'Дом'),
    ], 1)

    expect(tree.map(node => node.category.name)).toEqual(['Дом', 'Ёлка', 'Ель'])
  })

  it('сортирует и вложенные уровни', () => {
    const tree = buildCategoryTree([
      category(1, 'Еда'),
      category(2, 'Ужин', { parentId: 1 }),
      category(3, 'Завтрак', { parentId: 1, order: 1 }),
    ], 1)

    expect(tree[0]?.children.map(node => node.category.name)).toEqual(['Завтрак', 'Ужин'])
  })

  it('отбрасывает потомков потерянного родителя', () => {
    const tree = buildCategoryTree([category(2, 'Кафе', { parentId: 99 })], 1)

    expect(tree).toEqual([])
  })
})

describe('findCategoryNode', () => {
  it('находит узел на любой глубине', () => {
    const tree = buildCategoryTree([
      category(1, 'Еда'),
      category(2, 'Кафе', { parentId: 1 }),
      category(3, 'Кофе', { parentId: 2 }),
    ], 1)

    expect(findCategoryNode(tree, 3)?.category.name).toBe('Кофе')
    expect(findCategoryNode(tree, 99)).toBeNull()
  })
})
