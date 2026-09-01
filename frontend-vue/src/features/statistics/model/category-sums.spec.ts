import { describe, expect, it } from 'vitest'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import type { OperationTypeId } from '@/domain/operation-type'
import type { CategorySum, SumTreeNode } from './category-sums'
import { buildCategorySums, buildPieSlices, buildSumTree, collectSumKeys } from './category-sums'

const CATEGORIES: readonly Category[] = [
  category(1, 'Продукты', null),
  category(2, 'Кафе', 1),
  category(3, 'Транспорт', null),
  category(4, 'Возврат', null),
  category(5, 'Пусто', null),
  category(6, 'Аптека', null),
  category(9, 'Зарплата', null, 2),
]

const OPERATIONS: readonly Operation[] = [
  operation(1, 1, 10_000),
  operation(2, 2, 3_000),
  operation(3, 3, 500),
  operation(4, 4, -2_000),
  operation(5, 6, 500),
  operation(6, 9, 100_000),
]

describe('суммы категорий на экране статистики', () => {
  it('складывает свои операции с подкатегориями по всем категориям типа', () => {
    const sums = buildCategorySums(CATEGORIES, OPERATIONS, 1)

    expect(sums.map(sum => sum.name)).toEqual(['Аптека', 'Возврат', 'Продукты', 'Пусто', 'Транспорт'])
    expect(minorUnits(sums, 'Продукты')).toBe(13_000)
    expect(minorUnits(sums, 'Пусто')).toBe(0)
    expect(minorUnits(sums, 'Возврат')).toBe(-2_000)
    expect(sums.some(sum => sum.name === 'Зарплата')).toBe(false)
  })

  it('добавляет категории с детьми потомка с её собственной суммой', () => {
    const sums = buildCategorySums(CATEGORIES, OPERATIONS, 1)
    const products = find(sums, 'Продукты')

    expect(products.children.map(child => child.key)).toEqual(['2', '1:own'])
    expect(products.children.map(child => child.name)).toEqual(['Кафе', 'Продукты'])
    expect(products.children[1]?.total.toMinorUnits()).toBe(10_000)
    expect(find(sums, 'Транспорт').children).toEqual([])
  })

  it('оставляет в дереве только положительные суммы и считает «Всего» по видимым узлам', () => {
    const tree = buildSumTree(buildCategorySums(CATEGORIES, OPERATIONS, 1))
    const root = tree[0]

    expect(tree).toHaveLength(1)
    expect(root?.name).toBe('Всего')
    expect(root?.total.toMinorUnits()).toBe(14_000)
    expect(root?.children.map(node => node.name)).not.toContain('Возврат')
    expect(root?.children.map(node => node.name)).not.toContain('Пусто')
    expect(collectSumKeys(tree)).toEqual(['total', '1'])
  })

  it('сортирует узлы дерева по убыванию суммы, затем по имени', () => {
    const tree = buildSumTree(buildCategorySums(CATEGORIES, OPERATIONS, 1))
    const root = tree[0]

    expect(root?.children.map(node => node.name)).toEqual(['Продукты', 'Аптека', 'Транспорт'])
    expect(names(root?.children[0]?.children ?? [])).toEqual(['Продукты', 'Кафе'])
  })

  it('берёт в круговую диаграмму корневые категории с ненулевой суммой', () => {
    const slices = buildPieSlices(buildCategorySums(CATEGORIES, OPERATIONS, 1))

    expect(slices.map(slice => slice.label)).toEqual(['Аптека', 'Возврат', 'Продукты', 'Транспорт'])
    expect(slices.map(slice => slice.value)).toEqual([5, -20, 130, 5])
  })
})

function category(id: number, name: string, parentId: number | null, operationTypeId: OperationTypeId = 1): Category {
  return { id, parentId, name, color: null, order: null, operationTypeId, isDeleted: false }
}

function operation(id: number, categoryId: number, minorUnits: number): Operation {
  return {
    id,
    categoryId,
    category: CATEGORIES.find(item => item.id === categoryId) ?? null,
    sum: Money.fromMinorUnits(minorUnits),
    comment: null,
    place: null,
    date: '2026-03-01' as LocalDate,
    createdTaskId: null,
    isDeleted: false,
  }
}

function find(sums: readonly CategorySum[], name: string): CategorySum {
  const found = sums.find(sum => sum.name === name)

  if (found === undefined) {
    throw new Error(`Категория ${name} не найдена`)
  }

  return found
}

function minorUnits(sums: readonly CategorySum[], name: string): number {
  return find(sums, name).total.toMinorUnits()
}

function names(nodes: readonly SumTreeNode[]): string[] {
  return nodes.map(node => node.name)
}
