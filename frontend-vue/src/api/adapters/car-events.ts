import type { components } from '@/api/generated/schema'
import { apiClient } from '@/api/transport/client'
import type { CarEvent } from '@/domain/car'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { fail } from './problem'

type CarEventDto = components['schemas']['Money.Api.Dto.CarEvents.CarEventDto']
type CarEventSaveRequest = components['schemas']['Money.Api.Dto.CarEvents.SaveRequest']

export interface CarEventDraft {
  carId: number
  typeId: number
  title: string | null
  comment: string | null
  mileage: number | null
  date: LocalDate
}

export const CAR_EVENT_SAVE_ERROR = 'Ошибка. Пожалуйста, попробуйте еще раз.'

const DAY_START_TIME = 'T00:00:00'
const DATE_LENGTH = 10

export async function fetchCarEvents(carId: number): Promise<CarEvent[]> {
  const { data, error } = await apiClient.GET('/CarEvents/Car/{id}', { params: { path: { id: carId } } })

  fail(error, 'Не удалось загрузить события авто')

  return (data ?? []).flatMap(dto => toCarEvent(dto, carId))
}

export async function createCarEvent(draft: CarEventDraft): Promise<number> {
  const { data, error } = await apiClient.POST('/CarEvents', { body: toSaveRequest(draft) })

  fail(error, CAR_EVENT_SAVE_ERROR)

  return data ?? 0
}

export async function updateCarEvent(id: number, draft: CarEventDraft): Promise<void> {
  const { error } = await apiClient.PUT('/CarEvents/{id}', {
    params: { path: { id } },
    body: toSaveRequest(draft),
  })

  fail(error, CAR_EVENT_SAVE_ERROR)
}

export async function deleteCarEvent(id: number): Promise<void> {
  const { error } = await apiClient.DELETE('/CarEvents/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось удалить событие')
}

export async function restoreCarEvent(id: number): Promise<void> {
  const { error } = await apiClient.POST('/CarEvents/{id}/Restore', { params: { path: { id } } })

  fail(error, 'Не удалось восстановить событие')
}

export function toCarEvent(dto: CarEventDto, carId: number): CarEvent[] {
  const date = dto.date === undefined ? null : parseLocalDate(dto.date.slice(0, DATE_LENGTH))

  if (dto.id === undefined || dto.typeId === undefined || date === null) {
    return []
  }

  return [{
    id: dto.id,
    carId,
    typeId: dto.typeId,
    title: dto.title ?? null,
    comment: dto.comment ?? null,
    mileage: dto.mileage ?? null,
    date,
    isDeleted: false,
  }]
}

export function toSaveRequest(draft: CarEventDraft): CarEventSaveRequest {
  return {
    carId: draft.carId,
    typeId: draft.typeId,
    title: draft.title,
    comment: draft.comment,
    mileage: draft.mileage,
    date: `${draft.date}${DAY_START_TIME}`,
  }
}
