import type { RegularOperation } from '@/api/adapters/regular-operations'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { compareLocalDate } from '@/domain/local-date'
import { formatLocalDate } from '@/platform/format'

export const UNKNOWN_CATEGORY_NAME = 'Несуществующая'
export const NO_RUN_TIME = '∞'

export type RegularOperationKind = 'expense' | 'income' | 'deleted'

export interface RegularOperationView extends RegularOperation {
  category: Category | null
}

export function categoryNameOf(view: RegularOperationView): string {
  return view.category?.name ?? UNKNOWN_CATEGORY_NAME
}

export function regularOperationKind(view: RegularOperationView): RegularOperationKind {
  if (view.isDeleted) {
    return 'deleted'
  }

  return view.category?.operationTypeId === 1 ? 'expense' : 'income'
}

export function formatRunTime(runTime: LocalDate | null): string {
  return runTime === null ? NO_RUN_TIME : formatLocalDate(runTime)
}

export function withCategories(
  operations: readonly RegularOperation[],
  categories: readonly Category[],
): RegularOperationView[] {
  const known = new Map(categories.map(category => [category.id, category]))

  return operations.map(operation => ({
    ...operation,
    category: known.get(operation.categoryId) ?? null,
  }))
}

export function sortRegularOperations(views: readonly RegularOperationView[]): RegularOperationView[] {
  return [...views].sort(compareViews)
}

function compareViews(left: RegularOperationView, right: RegularOperationView): number {
  if (left.runTime === null || right.runTime === null) {
    if (left.runTime === right.runTime) {
      return 0
    }

    return left.runTime === null ? 1 : -1
  }

  return compareLocalDate(left.runTime, right.runTime)
}
