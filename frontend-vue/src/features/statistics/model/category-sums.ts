import type { Category, CategoryNode } from '@/domain/category'
import { buildCategoryTree } from '@/domain/category'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import type { OperationTypeId } from '@/domain/operation-type'
import { compareNames } from '@/platform/format'
import { fallbackColor } from './chart-colors'

export interface CategorySum {
  key: string
  name: string
  color: string | null
  total: Money
  children: CategorySum[]
}

export interface PieSlice {
  key: string
  label: string
  color: string
  value: number
}

export interface SumTreeNode {
  key: string
  name: string
  total: Money
  children: SumTreeNode[]
}

export const TOTAL_NODE_KEY = 'total'

export function buildCategorySums(
  categories: readonly Category[],
  operations: readonly Operation[],
  operationTypeId: OperationTypeId,
): CategorySum[] {
  const ownSums = sumByCategory(operations)

  return buildCategoryTree(categories, operationTypeId).map(node => toCategorySum(node, ownSums))
}

export function buildPieSlices(sums: readonly CategorySum[]): PieSlice[] {
  let position = 0

  return sums
    .filter(sum => !sum.total.isZero)
    .map(sum => ({
      key: sum.key,
      label: sum.name,
      color: sum.color ?? fallbackColor(position++),
      value: sum.total.toDecimal().toNumber(),
    }))
}

export function buildSumTree(sums: readonly CategorySum[]): SumTreeNode[] {
  const children = visibleNodes(sums)

  return [{
    key: TOTAL_NODE_KEY,
    name: 'Всего',
    total: children.reduce((total, child) => total.add(child.total), Money.zero),
    children,
  }]
}

export function collectSumKeys(nodes: readonly SumTreeNode[]): string[] {
  return nodes.flatMap(node => node.children.length === 0 ? [] : [node.key, ...collectSumKeys(node.children)])
}

function toCategorySum(node: CategoryNode, ownSums: ReadonlyMap<number, Money>): CategorySum {
  const children = node.children.map(child => toCategorySum(child, ownSums))
  const ownSum = ownSums.get(node.category.id) ?? Money.zero
  const total = children.reduce((sum, child) => sum.add(child.total), ownSum)
  const color = node.category.color

  if (children.length > 0) {
    children.push({
      key: `${node.category.id}:own`,
      name: node.category.name,
      color,
      total: ownSum,
      children: [],
    })
  }

  return {
    key: String(node.category.id),
    name: node.category.name,
    color,
    total,
    children,
  }
}

function sumByCategory(operations: readonly Operation[]): Map<number, Money> {
  const sums = new Map<number, Money>()

  for (const operation of operations) {
    sums.set(operation.categoryId, (sums.get(operation.categoryId) ?? Money.zero).add(operation.sum))
  }

  return sums
}

function visibleNodes(sums: readonly CategorySum[]): SumTreeNode[] {
  return sums
    .filter(sum => sum.total.compare(Money.zero) > 0)
    .map(sum => ({
      key: sum.key,
      name: sum.name,
      total: sum.total,
      children: visibleNodes(sum.children),
    }))
    .sort(byTotalThenName)
}

function byTotalThenName(left: SumTreeNode, right: SumTreeNode): number {
  const byTotal = right.total.compare(left.total)

  return byTotal === 0 ? compareNames(left.name, right.name) : byTotal
}
