import type { RegularOperationDraft } from '@/api/adapters/regular-operations'
import type { Category, CategoryNode } from '@/domain/category'
import { buildCategoryTree } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { compareLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import { OPERATION_TYPES } from '@/domain/operation-type'
import { trimToNull } from '@/domain/text'
import { DATE_UNPARSED, SUM_UNPARSED } from '@/shared/ui/field-messages'
import type { RegularOperationView } from './regular-operation-list'
import type { TimeValueRules } from './time-type'
import { DEFAULT_TIME_TYPE_ID, DEFAULT_TIME_VALUE, regularTimeType, timeValueRules } from './time-type'

export { DATE_UNPARSED, SUM_UNPARSED }

export const FIELD_REQUIRED = 'Заполни меня'
export const DATE_TO_BEFORE_FROM = 'Не раньше даты начала'

export const NAME_MAX_LENGTH = 500
export const PLACE_MAX_LENGTH = 500
export const COMMENT_MAX_LENGTH = 4000

export interface RegularOperationFormValue {
  id: number | null
  name: string
  categoryId: number | null
  sum: Money | null
  comment: string
  place: string
  timeTypeId: number | null
  timeValue: number | null
  dateFrom: LocalDate | null
  dateTo: LocalDate | null
}

export interface RegularOperationFormErrors {
  name?: string
  categoryId?: string
  timeTypeId?: string
  timeValue?: string
  dateFrom?: string
  dateTo?: string
  place?: string
  comment?: string
}

export function tooLongMessage(maxLength: number): string {
  return `Не длиннее ${maxLength} символов`
}

export function outOfRangeMessage(rules: TimeValueRules): string {
  return `Значение от ${rules.min} до ${rules.max}`
}

export function newRegularOperationForm(today: LocalDate): RegularOperationFormValue {
  return {
    id: null,
    name: '',
    categoryId: null,
    sum: Money.zero,
    comment: '',
    place: '',
    timeTypeId: DEFAULT_TIME_TYPE_ID,
    timeValue: DEFAULT_TIME_VALUE,
    dateFrom: today,
    dateTo: null,
  }
}

export function editRegularOperationForm(view: RegularOperationView): RegularOperationFormValue {
  return {
    id: view.id,
    name: view.name,
    categoryId: view.categoryId,
    sum: view.sum,
    comment: view.comment ?? '',
    place: view.place ?? '',
    timeTypeId: view.timeTypeId,
    timeValue: view.timeValue,
    dateFrom: view.dateFrom,
    dateTo: view.dateTo,
  }
}

export function validateRegularOperationForm(value: RegularOperationFormValue): RegularOperationFormErrors {
  const errors: RegularOperationFormErrors = {}
  const name = trimToNull(value.name)

  if (name === null) {
    errors.name = FIELD_REQUIRED
  } else if (name.length > NAME_MAX_LENGTH) {
    errors.name = tooLongMessage(NAME_MAX_LENGTH)
  }

  if (value.categoryId === null) {
    errors.categoryId = FIELD_REQUIRED
  }

  if (regularTimeType(value.timeTypeId) === null) {
    errors.timeTypeId = FIELD_REQUIRED
  }

  const timeValueError = validateTimeValue(value.timeTypeId, value.timeValue)

  if (timeValueError !== null) {
    errors.timeValue = timeValueError
  }

  if (value.dateFrom === null) {
    errors.dateFrom = FIELD_REQUIRED
  } else if (value.dateTo !== null && compareLocalDate(value.dateTo, value.dateFrom) < 0) {
    errors.dateTo = DATE_TO_BEFORE_FROM
  }

  if (value.place.trim().length > PLACE_MAX_LENGTH) {
    errors.place = tooLongMessage(PLACE_MAX_LENGTH)
  }

  if (value.comment.trim().length > COMMENT_MAX_LENGTH) {
    errors.comment = tooLongMessage(COMMENT_MAX_LENGTH)
  }

  return errors
}

export function dateFieldError(shown: LocalDate | null, parsed: LocalDate | null): string {
  if (parsed !== null) {
    return ''
  }

  return shown === null ? FIELD_REQUIRED : DATE_UNPARSED
}

export function toRegularOperationDraft(value: RegularOperationFormValue): RegularOperationDraft {
  const name = trimToNull(value.name)

  if (name === null || value.categoryId === null || value.timeTypeId === null || value.dateFrom === null) {
    throw new Error(FIELD_REQUIRED)
  }

  if (value.sum === null) {
    throw new Error(SUM_UNPARSED)
  }

  return {
    name,
    categoryId: value.categoryId,
    sum: value.sum,
    comment: trimToNull(value.comment),
    place: trimToNull(value.place),
    timeTypeId: value.timeTypeId,
    timeValue: timeValueRules(value.timeTypeId) === null ? null : value.timeValue,
    dateFrom: value.dateFrom,
    dateTo: value.dateTo,
  }
}

export function categoryOptions(
  categories: readonly Category[],
  requiredTypeId: OperationTypeId | null,
  categoryId: number | null,
): Category[] {
  const typeId = requiredTypeId ?? categories.find(category => category.id === categoryId)?.operationTypeId ?? null

  if (typeId === null) {
    return OPERATION_TYPES.flatMap(type => orderedByTree(categories, type.id))
  }

  return orderedByTree(categories, typeId)
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

function validateTimeValue(timeTypeId: number | null, timeValue: number | null): string | null {
  const rules = timeValueRules(timeTypeId)

  if (rules === null) {
    return null
  }

  if (timeValue === null) {
    return FIELD_REQUIRED
  }

  const isInRange = Number.isInteger(timeValue) && timeValue >= rules.min && timeValue <= rules.max

  return isInRange ? null : outOfRangeMessage(rules)
}

function orderedByTree(categories: readonly Category[], typeId: OperationTypeId): Category[] {
  const ordered = flattenNodes(buildCategoryTree(categories, typeId))
  const placed = new Set(ordered.map(category => category.id))
  const detached = categories.filter(category => category.operationTypeId === typeId && !placed.has(category.id))

  return [...ordered, ...detached]
}

function flattenNodes(nodes: readonly CategoryNode[]): Category[] {
  return nodes.flatMap(node => [node.category, ...flattenNodes(node.children)])
}
