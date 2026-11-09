import type { FastOperationDraft } from '@/api/adapters/fast-operations'
import type { Category, CategoryNode } from '@/domain/category'
import { buildCategoryTree } from '@/domain/category'
import type { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import { OPERATION_TYPES } from '@/domain/operation-type'
import { trimToNull } from '@/domain/text'
import { SUM_UNPARSED } from '@/shared/ui/field-messages'
import type { FastOperationView } from './fast-operation-list'

export { SUM_UNPARSED }

export const NAME_REQUIRED = 'Наименование обязательно'
export const CATEGORY_REQUIRED = 'Категория обязательна'

export interface FastOperationFormValue {
  id: number | null
  operationTypeId: OperationTypeId | null
  categoryId: number | null
  name: string
  sum: Money | null
  order: number | null
  comment: string
  place: string
}

export interface FastOperationFormErrors {
  name?: string
  categoryId?: string
}

export function newFastOperationForm(operationTypeId: OperationTypeId | null): FastOperationFormValue {
  return {
    id: null,
    operationTypeId,
    categoryId: null,
    name: '',
    sum: null,
    order: null,
    comment: '',
    place: '',
  }
}

export function editFastOperationForm(view: FastOperationView): FastOperationFormValue {
  return {
    id: view.id,
    operationTypeId: view.category?.operationTypeId ?? null,
    categoryId: view.categoryId,
    name: view.name,
    sum: view.sum.isZero ? null : view.sum,
    order: view.order,
    comment: view.comment ?? '',
    place: view.place ?? '',
  }
}

export function validateFastOperationForm(value: FastOperationFormValue): FastOperationFormErrors {
  const errors: FastOperationFormErrors = {}

  if (trimToNull(value.name) === null) {
    errors.name = NAME_REQUIRED
  }

  if (value.categoryId === null) {
    errors.categoryId = CATEGORY_REQUIRED
  }

  return errors
}

export function toFastOperationDraft(value: FastOperationFormValue): FastOperationDraft {
  const name = trimToNull(value.name)

  if (name === null) {
    throw new Error(NAME_REQUIRED)
  }

  if (value.categoryId === null) {
    throw new Error(CATEGORY_REQUIRED)
  }

  if (value.sum === null) {
    throw new Error(SUM_UNPARSED)
  }

  return {
    name,
    order: value.order,
    categoryId: value.categoryId,
    sum: value.sum,
    comment: trimToNull(value.comment),
    place: trimToNull(value.place),
  }
}

export function categoryOptions(
  categories: readonly Category[],
  operationTypeId: OperationTypeId | null,
): Category[] {
  if (operationTypeId === null) {
    return OPERATION_TYPES.flatMap(type => orderedForType(categories, type.id))
  }

  return orderedForType(categories, operationTypeId)
}

export function searchCategories(options: readonly Category[], query: string): Category[] {
  const search = query.trim().toLowerCase()

  if (search.length === 0) {
    return [...options]
  }

  return options.filter(category => category.name.toLowerCase().includes(search))
}

export function shouldResetPlaces(previous: string | null, next: string): boolean {
  return next.trim().length > 0 && next !== (previous ?? '')
}

function orderedForType(categories: readonly Category[], operationTypeId: OperationTypeId): Category[] {
  const ordered = flattenNodes(buildCategoryTree(categories, operationTypeId))
  const placed = new Set(ordered.map(category => category.id))
  const detached = categories.filter(category => category.operationTypeId === operationTypeId && !placed.has(category.id))

  return [...ordered, ...detached]
}

function flattenNodes(nodes: readonly CategoryNode[]): Category[] {
  return nodes.flatMap(node => [node.category, ...flattenNodes(node.children)])
}
