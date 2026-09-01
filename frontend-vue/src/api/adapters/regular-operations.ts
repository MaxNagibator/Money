import type { components } from '@/api/generated/schema'
import { apiClient } from '@/api/transport/client'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money, MoneyDecimal } from '@/domain/money'
import { trimToNull } from '@/domain/text'
import { ApiError, fail } from './problem'

type RegularOperationDto = components['schemas']['Money.Api.Dto.RegularOperations.RegularOperationDto']
type RegularOperationSaveRequest = components['schemas']['Money.Api.Dto.RegularOperations.SaveRequest']

export interface RegularOperation {
  id: number
  name: string
  categoryId: number
  sum: Money
  comment: string | null
  place: string | null
  timeTypeId: number
  timeValue: number | null
  dateFrom: LocalDate
  dateTo: LocalDate | null
  runTime: LocalDate | null
  isDeleted: boolean
}

export interface RegularOperationDraft {
  name: string
  categoryId: number
  sum: Money
  comment: string | null
  place: string | null
  timeTypeId: number
  timeValue: number | null
  dateFrom: LocalDate
  dateTo: LocalDate | null
}

export const REGULAR_OPERATION_SAVE_ERROR = 'Ошибка. Пожалуйста, попробуйте еще раз.'

const DAY_START_TIME = 'T00:00:00'
const DATE_LENGTH = 10

export function toRegularOperation(dto: RegularOperationDto): RegularOperation[] {
  const dateFrom = toLocalDate(dto.dateFrom)

  if (dto.id === undefined || dateFrom === null) {
    return []
  }

  return [{
    id: dto.id,
    name: dto.name ?? '',
    categoryId: dto.categoryId,
    sum: Money.fromDecimal(new MoneyDecimal(dto.sum ?? 0)),
    comment: trimToNull(dto.comment ?? null),
    place: trimToNull(dto.place ?? null),
    timeTypeId: dto.timeTypeId ?? 0,
    timeValue: dto.timeValue ?? null,
    dateFrom,
    dateTo: toLocalDate(dto.dateTo),
    runTime: toLocalDate(dto.runTime),
    isDeleted: false,
  }]
}

export function toSaveRequest(draft: RegularOperationDraft): RegularOperationSaveRequest {
  return {
    categoryId: draft.categoryId,
    sum: Money.fromMinorUnits(draft.sum.toMinorUnits()).toDecimal().toNumber(),
    name: draft.name,
    comment: draft.comment,
    place: draft.place,
    timeTypeId: draft.timeTypeId,
    timeValue: draft.timeValue,
    dateFrom: toApiDate(draft.dateFrom),
    dateTo: draft.dateTo === null ? null : toApiDate(draft.dateTo),
  }
}

export async function fetchRegularOperations(): Promise<RegularOperation[]> {
  const { data, error } = await apiClient.GET('/RegularOperations')

  fail(error, 'Не удалось загрузить регулярные операции')

  return (data ?? []).flatMap(toRegularOperation)
}

export async function fetchRegularOperation(id: number): Promise<RegularOperation | null> {
  const { data, error } = await apiClient.GET('/RegularOperations/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось загрузить регулярную операцию')

  return data === undefined ? null : (toRegularOperation(data)[0] ?? null)
}

export async function createRegularOperation(draft: RegularOperationDraft): Promise<number> {
  const { data, error } = await apiClient.POST('/RegularOperations', { body: toSaveRequest(draft) })

  fail(error, REGULAR_OPERATION_SAVE_ERROR)

  if (data === undefined) {
    throw new ApiError('Сервер не вернул идентификатор регулярной операции')
  }

  return data
}

export async function updateRegularOperation(id: number, draft: RegularOperationDraft): Promise<void> {
  const { error } = await apiClient.PUT('/RegularOperations/{id}', {
    params: { path: { id } },
    body: toSaveRequest(draft),
  })

  fail(error, REGULAR_OPERATION_SAVE_ERROR)
}

export async function deleteRegularOperation(id: number): Promise<void> {
  const { error } = await apiClient.DELETE('/RegularOperations/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось удалить регулярную операцию')
}

export async function restoreRegularOperation(id: number): Promise<void> {
  const { error } = await apiClient.POST('/RegularOperations/{id}/Restore', { params: { path: { id } } })

  fail(error, 'Не удалось восстановить регулярную операцию')
}

function toLocalDate(value: string | null | undefined): LocalDate | null {
  return value === null || value === undefined ? null : parseLocalDate(value.slice(0, DATE_LENGTH))
}

function toApiDate(value: LocalDate): string {
  return `${value}${DAY_START_TIME}`
}
