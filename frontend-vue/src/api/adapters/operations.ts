import type { components, paths } from '@/api/generated/schema'
import { apiClient } from '@/api/transport/client'
import { trimToNull } from '@/domain/text'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { addDays, parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import type { OperationTypeId } from '@/domain/operation-type'
import { toWireSum } from './money-wire'
import { ApiError, fail } from './problem'

type OperationDto = components['schemas']['Money.Api.Dto.Operations.OperationDto']
type OperationSaveRequest = components['schemas']['Money.Api.Dto.Operations.SaveRequest']

export type OperationsQuery = NonNullable<paths['/Operations']['get']['parameters']['query']>

export interface OperationFilter {
  dateFrom: LocalDate | null
  dateTo: LocalDate | null
  categoryIds: readonly number[]
  comment: string | null
  place: string | null
}

export interface OperationDraft {
  categoryId: number
  sum: Money
  comment: string | null
  place: string | null
  date: LocalDate
}

const DAY_START_TIME = 'T00:00:00'
const DATE_LENGTH = 10
const INFER_CATEGORY_COUNT = 5

export function buildOperationsQuery(filter: OperationFilter): OperationsQuery {
  const query: OperationsQuery = {}

  if (filter.dateFrom !== null) {
    query.DateFrom = toApiDate(filter.dateFrom)
  }

  if (filter.dateTo !== null) {
    query.DateTo = toApiDate(addDays(filter.dateTo, 1))
  }

  if (filter.categoryIds.length > 0) {
    query.CategoryIds = filter.categoryIds.join(',')
  }

  const comment = trimToNull(filter.comment)

  if (comment !== null) {
    query.Comment = comment
  }

  const place = trimToNull(filter.place)

  if (place !== null) {
    query.Place = place
  }

  return query
}

export function toOperation(dto: OperationDto, categories: ReadonlyMap<number, Category>): Operation[] {
  const date = dto.date === undefined ? null : parseLocalDate(dto.date.slice(0, DATE_LENGTH))

  if (dto.id === undefined || date === null || dto.sum === undefined) {
    return []
  }

  return [{
    id: dto.id,
    categoryId: dto.categoryId,
    category: categories.get(dto.categoryId) ?? null,
    sum: Money.fromWire(dto.sum),
    comment: dto.comment ?? null,
    place: dto.place ?? null,
    date,
    createdTaskId: dto.createdTaskId ?? null,
    isDeleted: false,
  }]
}

export function toSaveRequest(draft: OperationDraft): OperationSaveRequest {
  return {
    categoryId: draft.categoryId,
    sum: toWireSum(draft.sum),
    comment: draft.comment,
    place: draft.place,
    date: toApiDate(draft.date),
  }
}

export async function fetchOperations(filter: OperationFilter, categories: readonly Category[]): Promise<Operation[]> {
  const { data, error } = await apiClient.GET('/Operations', {
    params: { query: buildOperationsQuery(filter) },
  })

  fail(error, 'Не удалось загрузить операции')

  const known = new Map(categories.map(category => [category.id, category]))

  return (data ?? []).flatMap(dto => toOperation(dto, known))
}

export async function createOperation(draft: OperationDraft): Promise<number> {
  const { data, error } = await apiClient.POST('/Operations', { body: toSaveRequest(draft) })

  fail(error, 'Не удалось создать операцию')

  if (data === undefined) {
    throw new ApiError('Сервер не вернул идентификатор операции')
  }

  return data
}

export async function updateOperation(id: number, draft: OperationDraft): Promise<void> {
  const { error } = await apiClient.PUT('/Operations/{id}', {
    params: { path: { id } },
    body: toSaveRequest(draft),
  })

  fail(error, 'Не удалось сохранить операцию')
}

export async function transferOperations(operationIds: readonly number[], categoryId: number): Promise<void> {
  const { error } = await apiClient.POST('/Operations/UpdateBatch', {
    body: { operationIds: [...operationIds], categoryId },
  })

  fail(error, 'Не удалось перенести операции')
}

export async function deleteOperation(id: number): Promise<void> {
  const { error } = await apiClient.DELETE('/Operations/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось удалить операцию')
}

export async function restoreOperation(id: number): Promise<void> {
  const { error } = await apiClient.POST('/Operations/{id}/Restore', { params: { path: { id } } })

  fail(error, 'Не удалось восстановить операцию')
}

export async function inferCategory(
  place: string,
  operationTypeId: OperationTypeId,
  count: number = INFER_CATEGORY_COUNT,
): Promise<number | null> {
  const { data, error } = await apiClient.GET('/Operations/InferCategory/{place}', {
    params: {
      path: { place },
      query: { operationType: operationTypeId, count },
    },
  })

  fail(error, 'Не удалось подобрать категорию по месту')

  return data?.categoryId ?? null
}

export async function fetchPlaces(offset: number, count: number, name: string | null = null): Promise<string[]> {
  const search = trimToNull(name)

  if (search === null) {
    const { data, error } = await apiClient.GET('/Operations/GetPlaces/{offset}/{count}', {
      params: { path: { offset, count } },
    })

    fail(error, 'Не удалось загрузить места')

    return data ?? []
  }

  const { data, error } = await apiClient.GET('/Operations/GetPlaces/{offset}/{count}/{name}', {
    params: { path: { offset, count, name: search } },
  })

  fail(error, 'Не удалось загрузить места')

  return data ?? []
}

function toApiDate(value: LocalDate): string {
  return `${value}${DAY_START_TIME}`
}
