import type { components } from '@/api/generated/schema'
import { apiClient } from '@/api/transport/client'
import type { Debt, DebtOwner } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { toWireSum } from './money-wire'
import { ApiError, fail } from './problem'

type DebtDto = components['schemas']['Money.Api.Dto.Debts.DebtDto']
type DebtOwnerDto = components['schemas']['Money.Api.Dto.Debts.DebtOwnerDto']
type DebtSaveRequest = components['schemas']['Money.Api.Dto.Debts.SaveRequest']
type DebtPayRequest = components['schemas']['Money.Api.Dto.Debts.PayRequest']
type DebtForgiveRequest = components['schemas']['Money.Api.Dto.Debts.ForgiveRequest']

export interface DebtDraft {
  typeId: number
  sum: Money
  comment: string | null
  ownerName: string
  date: LocalDate
}

export interface DebtPaymentDraft {
  sum: Money
  comment: string | null
  date: LocalDate
}

export interface DebtForgiveDraft {
  debtIds: readonly number[]
  categoryId: number
  comment: string | null
}

export const DEBT_SAVE_ERROR = 'Ошибка. Пожалуйста, попробуйте еще раз.'
export const UNNAMED_DEBT_OWNER = 'Неназванный держатель'

const DAY_START_TIME = 'T00:00:00'
const DATE_LENGTH = 10

export async function fetchDebts(withPaid: boolean): Promise<Debt[]> {
  const { data, error } = await apiClient.GET('/Debts', { params: { query: { withPaid } } })

  fail(error, 'Не удалось загрузить долги')

  return (data ?? []).flatMap(toDebt)
}

export async function fetchDebt(id: number): Promise<Debt | null> {
  const { data, error } = await apiClient.GET('/Debts/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось загрузить долг')

  return data === undefined ? null : (toDebt(data)[0] ?? null)
}

export async function createDebt(draft: DebtDraft): Promise<number> {
  const { data, error } = await apiClient.POST('/Debts', { body: toSaveRequest(draft) })

  fail(error, DEBT_SAVE_ERROR)

  if (data === undefined) {
    throw new ApiError('Сервер не вернул идентификатор долга')
  }

  return data
}

export async function updateDebt(id: number, draft: DebtDraft): Promise<void> {
  const { error } = await apiClient.PUT('/Debts/{id}', {
    params: { path: { id } },
    body: toSaveRequest(draft),
  })

  fail(error, DEBT_SAVE_ERROR)
}

export async function deleteDebt(id: number): Promise<void> {
  const { error } = await apiClient.DELETE('/Debts/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось удалить долг')
}

export async function restoreDebt(id: number): Promise<void> {
  const { error } = await apiClient.POST('/Debts/{id}/Restore', { params: { path: { id } } })

  fail(error, 'Не удалось восстановить долг')
}

export async function payDebt(id: number, draft: DebtPaymentDraft): Promise<void> {
  const { error } = await apiClient.POST('/Debts/{id}/Pay', {
    params: { path: { id } },
    body: toPayRequest(draft),
  })

  fail(error, 'Не удалось оплатить долг')
}

export async function forgiveDebts(draft: DebtForgiveDraft): Promise<void> {
  const body: DebtForgiveRequest = {
    debtIds: [...draft.debtIds],
    operationCategoryId: draft.categoryId,
    operationComment: draft.comment,
  }

  const { error } = await apiClient.POST('/Debts/Forgive', { body })

  fail(error, 'Не удалось простить долги')
}

export async function mergeDebtOwners(fromUserId: number, toUserId: number): Promise<void> {
  const { error } = await apiClient.POST('/Debts/MergeOwners/{fromUserId}/with/{toUserId}', {
    params: { path: { fromUserId, toUserId } },
  })

  fail(error, 'Не удалось объединить держателей')
}

export async function fetchDebtOwners(): Promise<DebtOwner[]> {
  const { data, error } = await apiClient.GET('/Debts/Owners')

  fail(error, 'Не удалось загрузить держателей')

  return (data ?? []).flatMap(toDebtOwner)
}

export async function fetchDebtOwnerNames(offset: number, count: number, name: string | null = null): Promise<string[]> {
  const search = name === null ? null : name.trim()

  if (search === null || search.length === 0) {
    const { data, error } = await apiClient.GET('/Debts/Owners/{offset}/{count}', {
      params: { path: { offset, count } },
    })

    fail(error, 'Не удалось загрузить держателей')

    return data ?? []
  }

  const { data, error } = await apiClient.GET('/Debts/Owners/{offset}/{count}/{name}', {
    params: { path: { offset, count, name: search } },
  })

  fail(error, 'Не удалось загрузить держателей')

  return data ?? []
}

export function toDebt(dto: DebtDto): Debt[] {
  const date = dto.date === undefined ? null : parseLocalDate(dto.date.slice(0, DATE_LENGTH))

  if (dto.id === undefined || dto.typeId === undefined || date === null || dto.sum === undefined || dto.paySum === undefined) {
    return []
  }

  return [{
    id: dto.id,
    typeId: dto.typeId,
    sum: Money.fromWire(dto.sum),
    paySum: Money.fromWire(dto.paySum),
    comment: dto.comment ?? null,
    ownerName: dto.ownerName ?? '',
    date,
    payComment: dto.payComment ?? null,
    isDeleted: dto.isDeleted ?? false,
  }]
}

export function toDebtOwner(dto: DebtOwnerDto): DebtOwner[] {
  if (dto.id === undefined) {
    return []
  }

  const name = dto.name ?? ''

  return [{ id: dto.id, name: name.length === 0 ? UNNAMED_DEBT_OWNER : name }]
}

export function toSaveRequest(draft: DebtDraft): DebtSaveRequest {
  return {
    typeId: draft.typeId,
    sum: toWireSum(draft.sum),
    comment: draft.comment,
    ownerName: draft.ownerName,
    date: toApiDate(draft.date),
  }
}

export function toPayRequest(draft: DebtPaymentDraft): DebtPayRequest {
  return {
    sum: toWireSum(draft.sum),
    comment: draft.comment,
    date: toApiDate(draft.date),
  }
}

function toApiDate(value: LocalDate): string {
  return `${value}${DAY_START_TIME}`
}
