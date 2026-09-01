import type { components } from '@/api/generated/schema'
import { apiClient } from '@/api/transport/client'
import { Money, MoneyDecimal } from '@/domain/money'
import { ApiError, fail } from './problem'

type FastOperationDto = components['schemas']['Money.Api.Dto.FastOperations.FastOperationDto']
type FastOperationSaveRequest = components['schemas']['Money.Api.Dto.FastOperations.SaveRequest']

export interface FastOperation {
  id: number
  name: string
  order: number | null
  categoryId: number
  sum: Money
  comment: string | null
  place: string | null
  isDeleted: boolean
}

export interface FastOperationDraft {
  name: string
  order: number | null
  categoryId: number
  sum: Money
  comment: string | null
  place: string | null
}

export function toFastOperation(dto: FastOperationDto): FastOperation[] {
  if (dto.id === undefined) {
    return []
  }

  return [{
    id: dto.id,
    name: dto.name ?? '',
    order: dto.order ?? null,
    categoryId: dto.categoryId,
    sum: Money.fromDecimal(new MoneyDecimal(dto.sum ?? 0)),
    comment: dto.comment ?? null,
    place: dto.place ?? null,
    isDeleted: false,
  }]
}

export function toSaveRequest(draft: FastOperationDraft): FastOperationSaveRequest {
  return {
    categoryId: draft.categoryId,
    sum: Money.fromMinorUnits(draft.sum.toMinorUnits()).toDecimal().toNumber(),
    name: draft.name,
    comment: draft.comment,
    place: draft.place,
    order: draft.order,
  }
}

export async function fetchFastOperations(): Promise<FastOperation[]> {
  const { data, error } = await apiClient.GET('/FastOperations')

  fail(error, 'Не удалось загрузить быстрые операции')

  return (data ?? []).flatMap(toFastOperation)
}

export async function fetchFastOperation(id: number): Promise<FastOperation | null> {
  const { data, error } = await apiClient.GET('/FastOperations/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось загрузить быструю операцию')

  return data === undefined ? null : (toFastOperation(data)[0] ?? null)
}

export async function createFastOperation(draft: FastOperationDraft): Promise<number> {
  const { data, error } = await apiClient.POST('/FastOperations', { body: toSaveRequest(draft) })

  fail(error, 'Не удалось создать быструю операцию')

  if (data === undefined) {
    throw new ApiError('Сервер не вернул идентификатор быстрой операции')
  }

  return data
}

export async function updateFastOperation(id: number, draft: FastOperationDraft): Promise<void> {
  const { error } = await apiClient.PUT('/FastOperations/{id}', {
    params: { path: { id } },
    body: toSaveRequest(draft),
  })

  fail(error, 'Не удалось сохранить быструю операцию')
}

export async function deleteFastOperation(id: number): Promise<void> {
  const { error } = await apiClient.DELETE('/FastOperations/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось удалить быструю операцию')
}

export async function restoreFastOperation(id: number): Promise<void> {
  const { error } = await apiClient.POST('/FastOperations/{id}/Restore', { params: { path: { id } } })

  fail(error, 'Не удалось восстановить быструю операцию')
}
