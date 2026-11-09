import type { OperationTypeId } from '@/domain/operation-type'
import { compareNames } from '@/platform/format'

export interface Category {
  id: number
  parentId: number | null
  name: string
  color: string | null
  order: number | null
  operationTypeId: OperationTypeId
  isDeleted: boolean
}

export interface CategoryNode {
  category: Category
  children: CategoryNode[]
}

export function buildCategoryTree(categories: readonly Category[], operationTypeId: OperationTypeId): CategoryNode[] {
  const scoped = categories.filter(category => category.operationTypeId === operationTypeId)

  return collectChildren(scoped, null)
}

export function findCategoryNode(nodes: readonly CategoryNode[], id: number): CategoryNode | null {
  for (const node of nodes) {
    if (node.category.id === id) {
      return node
    }

    const found = findCategoryNode(node.children, id)

    if (found !== null) {
      return found
    }
  }

  return null
}

function collectChildren(categories: readonly Category[], parentId: number | null): CategoryNode[] {
  return categories
    .filter(category => category.parentId === parentId)
    .map(category => ({ category, children: collectChildren(categories, category.id) }))
    .sort(compareNodes)
}

function compareNodes(left: CategoryNode, right: CategoryNode): number {
  const leftOrder = left.category.order
  const rightOrder = right.category.order

  if (leftOrder !== rightOrder) {
    if (leftOrder === null) {
      return 1
    }

    if (rightOrder === null) {
      return -1
    }

    return leftOrder - rightOrder
  }

  return compareNames(left.category.name, right.category.name)
}
