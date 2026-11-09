import type { CarEventDraft } from '@/api/adapters/car-events'
import type { CarEvent } from '@/domain/car'
import type { LocalDate } from '@/domain/local-date'
import { trimToNull } from '@/domain/text'

export const TYPE_REQUIRED = 'Выбери меня'
export const DATE_REQUIRED = 'Заполни меня'

export interface CarEventFormValue {
  id: number | null
  typeId: number | null
  title: string
  comment: string
  mileage: number | null
  date: LocalDate | null
}

export interface CarEventFormErrors {
  typeId?: string
  date?: string
}

export function newCarEventForm(today: LocalDate): CarEventFormValue {
  return {
    id: null,
    typeId: null,
    title: '',
    comment: '',
    mileage: null,
    date: today,
  }
}

export function editCarEventForm(event: CarEvent): CarEventFormValue {
  return {
    id: event.id,
    typeId: event.typeId,
    title: event.title ?? '',
    comment: event.comment ?? '',
    mileage: event.mileage,
    date: event.date,
  }
}

export function validateCarEventForm(value: CarEventFormValue): CarEventFormErrors {
  const errors: CarEventFormErrors = {}

  if (value.typeId === null) {
    errors.typeId = TYPE_REQUIRED
  }

  if (value.date === null) {
    errors.date = DATE_REQUIRED
  }

  return errors
}

export function toCarEventDraft(carId: number, value: CarEventFormValue): CarEventDraft {
  if (value.typeId === null) {
    throw new Error(TYPE_REQUIRED)
  }

  if (value.date === null) {
    throw new Error(DATE_REQUIRED)
  }

  return {
    carId,
    typeId: value.typeId,
    title: trimToNull(value.title),
    comment: trimToNull(value.comment),
    mileage: value.mileage,
    date: value.date,
  }
}
