import { shallowRef } from 'vue'
import type { OperationDraft, OperationFilter } from '@/api/adapters/operations'
import type { LocalDate } from '@/domain/local-date'
import type { Operation } from '@/domain/operation'
import { trimToNull } from '@/domain/text'
import { isWithinPeriod } from './period'

export interface FilteredOperation {
  categoryId: number | null
  comment: string | null
  place: string | null
  date: LocalDate | null
}

const pinned = shallowRef(new Map<number, Operation>())

let epoch = 0

export function matchesOperationFilter(operation: FilteredOperation, filter: OperationFilter): boolean {
  return matchesPeriod(operation.date, filter)
    && matchesCategory(operation.categoryId, filter.categoryIds)
    && matchesText(operation.comment, filter.comment)
    && matchesText(operation.place, filter.place)
}

export function pinnedOperationsMatching(filter: OperationFilter): ReadonlyMap<number, Operation> {
  const selected = new Map<number, Operation>()

  for (const [id, operation] of pinned.value) {
    if (matchesOperationFilter(operation, filter)) {
      selected.set(id, operation)
    }
  }

  return selected
}

export function pinsEpoch(): number {
  return epoch
}

export function pinOperation(id: number, draft: OperationDraft, createdTaskId: number | null, since: number): void {
  if (since !== epoch) {
    return
  }

  pinned.value = new Map(pinned.value).set(id, {
    id,
    categoryId: draft.categoryId,
    category: null,
    sum: draft.sum,
    comment: draft.comment,
    place: draft.place,
    date: draft.date,
    createdTaskId,
    isDeleted: false,
  })
}

export function unpinOperation(id: number): void {
  if (!pinned.value.has(id)) {
    return
  }

  const rest = new Map(pinned.value)

  rest.delete(id)
  pinned.value = rest
}

export function clearPinnedOperations(): void {
  epoch += 1
  pinned.value = new Map()
}

function matchesPeriod(date: LocalDate | null, filter: OperationFilter): boolean {
  return date === null || isWithinPeriod(date, filter.dateFrom, filter.dateTo)
}

function matchesCategory(categoryId: number | null, categoryIds: readonly number[]): boolean {
  return categoryIds.length === 0 || (categoryId !== null && categoryIds.includes(categoryId))
}

function matchesText(value: string | null, expected: string | null): boolean {
  const needle = trimToNull(expected)

  if (needle === null) {
    return true
  }

  return value !== null && value.toLowerCase().includes(needle.toLowerCase())
}
