import type { FastOperation } from '@/api/adapters/fast-operations'
import type { Category } from '@/domain/category'
import { compareNames } from '@/platform/format'

export const UNKNOWN_CATEGORY_NAME = 'Несуществующая'

export type FastOperationKind = 'expense' | 'income' | 'unknown' | 'deleted'

export interface FastOperationView extends FastOperation {
  category: Category | null
}

export function categoryNameOf(view: FastOperationView): string {
  return view.category?.name ?? UNKNOWN_CATEGORY_NAME
}

export function fastOperationKind(view: FastOperationView): FastOperationKind {
  if (view.isDeleted) {
    return 'deleted'
  }

  if (view.category === null) {
    return 'unknown'
  }

  return view.category.operationTypeId === 1 ? 'expense' : 'income'
}

export function withCategories(
  operations: readonly FastOperation[],
  categories: readonly Category[],
): FastOperationView[] {
  const known = new Map(categories.map(category => [category.id, category]))

  return operations.map(operation => ({
    ...operation,
    category: known.get(operation.categoryId) ?? null,
  }))
}

export function sortFastOperations(views: readonly FastOperationView[]): FastOperationView[] {
  return [...views].sort(compareViews)
}

function compareViews(left: FastOperationView, right: FastOperationView): number {
  if (left.order !== right.order) {
    if (left.order === null) {
      return 1
    }

    if (right.order === null) {
      return -1
    }

    return left.order - right.order
  }

  const byCategory = compareNames(categoryNameOf(left), categoryNameOf(right))

  return byCategory === 0 ? compareNames(left.name, right.name) : byCategory
}
