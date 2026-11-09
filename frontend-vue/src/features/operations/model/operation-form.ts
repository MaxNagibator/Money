import type { FastOperation } from '@/api/adapters/fast-operations'
import type { OperationDraft } from '@/api/adapters/operations'
import { trimToNull } from '@/domain/text'
import type { Category, CategoryNode } from '@/domain/category'
import { buildCategoryTree } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import type { OperationTypeId } from '@/domain/operation-type'
import { DATE_UNPARSED, SUM_UNPARSED } from '@/shared/ui/field-messages'

export { DATE_UNPARSED, SUM_UNPARSED }

export const CATEGORY_REQUIRED = 'Категория обязательна'
export const DATE_REQUIRED = 'Укажите дату'

const DEFAULT_OPERATION_TYPE_ID: OperationTypeId = 1

export interface OperationFormValue {
  id: number | null
  operationTypeId: OperationTypeId
  categoryId: number | null
  sum: Money | null
  date: LocalDate | null
  comment: string
  place: string
}

export interface OperationFormErrors {
  categoryId?: string
  date?: string
}

export function newOperationForm(operationTypeId: OperationTypeId, date: LocalDate): OperationFormValue {
  return {
    id: null,
    operationTypeId,
    categoryId: null,
    sum: null,
    date,
    comment: '',
    place: '',
  }
}

// TODO: тип операции берётся из её категории, при отсутствии категории – расходы; убрать подстановку, когда операция начнёт нести operationTypeId из API
export function editOperationForm(operation: Operation): OperationFormValue {
  return {
    id: operation.id,
    operationTypeId: operation.category?.operationTypeId ?? DEFAULT_OPERATION_TYPE_ID,
    categoryId: operation.categoryId,
    sum: operation.sum.isZero ? null : operation.sum,
    date: operation.date,
    comment: operation.comment ?? '',
    place: operation.place ?? '',
  }
}

export function fastOperationForm(
  source: FastOperation,
  category: Category | null,
  date: LocalDate,
): OperationFormValue {
  return {
    id: null,
    operationTypeId: category?.operationTypeId ?? DEFAULT_OPERATION_TYPE_ID,
    categoryId: category?.id ?? null,
    sum: source.sum.isZero ? null : source.sum,
    date,
    comment: source.comment ?? '',
    place: source.place ?? '',
  }
}

export function validateOperationForm(value: OperationFormValue): OperationFormErrors {
  const errors: OperationFormErrors = {}

  if (value.categoryId === null) {
    errors.categoryId = CATEGORY_REQUIRED
  }

  if (value.date === null) {
    errors.date = DATE_REQUIRED
  }

  return errors
}

export function toOperationDraft(value: OperationFormValue): OperationDraft {
  if (value.categoryId === null) {
    throw new Error(CATEGORY_REQUIRED)
  }

  if (value.date === null) {
    throw new Error(DATE_REQUIRED)
  }

  if (value.sum === null) {
    throw new Error(SUM_UNPARSED)
  }

  return {
    categoryId: value.categoryId,
    sum: value.sum,
    comment: trimToNull(value.comment),
    place: trimToNull(value.place),
    date: value.date,
  }
}

export function shouldResetPlaces(previous: string | null, next: string): boolean {
  return next.trim().length > 0 && next !== (previous ?? '')
}

export function categoriesForType(categories: readonly Category[], operationTypeId: OperationTypeId): Category[] {
  const ordered = flattenNodes(buildCategoryTree(categories, operationTypeId))
  const placed = new Set(ordered.map(category => category.id))
  const detached = categories.filter(category => category.operationTypeId === operationTypeId && !placed.has(category.id))

  return [...ordered, ...detached]
}

function flattenNodes(nodes: readonly CategoryNode[]): Category[] {
  return nodes.flatMap(node => [node.category, ...flattenNodes(node.children)])
}
