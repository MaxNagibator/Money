import { describe, expect, it } from 'vitest'
import type { CarEvent } from '@/domain/car'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import type { CarEventFormValue } from './car-event-form'
import {
  DATE_REQUIRED,
  editCarEventForm,
  newCarEventForm,
  toCarEventDraft,
  TYPE_REQUIRED,
  validateCarEventForm,
} from './car-event-form'

const stored: CarEvent = {
  id: 42,
  carId: 7,
  typeId: 2,
  title: 'ОСАГО',
  comment: 'через приложение',
  mileage: 120_000,
  date: localDate('2026-02-14'),
  isDeleted: false,
}

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function filled(extra: Partial<CarEventFormValue> = {}): CarEventFormValue {
  return {
    id: null,
    typeId: 3,
    title: 'Замена масла',
    comment: 'сервис у дома',
    mileage: 130_000,
    date: localDate('2026-03-01'),
    ...extra,
  }
}

describe('newCarEventForm', () => {
  it('открывается без типа и с сегодняшней датой', () => {
    const form = newCarEventForm(localDate('2026-03-01'))

    expect(form).toEqual({
      id: null,
      typeId: null,
      title: '',
      comment: '',
      mileage: null,
      date: localDate('2026-03-01'),
    })
  })
})

describe('editCarEventForm', () => {
  it('подставляет значения события', () => {
    expect(editCarEventForm(stored)).toEqual({
      id: 42,
      typeId: 2,
      title: 'ОСАГО',
      comment: 'через приложение',
      mileage: 120_000,
      date: localDate('2026-02-14'),
    })
  })

  it('пустые поля события показывает пустыми строками', () => {
    const form = editCarEventForm({ ...stored, title: null, comment: null, mileage: null })

    expect(form.title).toBe('')
    expect(form.comment).toBe('')
    expect(form.mileage).toBeNull()
  })
})

describe('validateCarEventForm', () => {
  it('требует тип события', () => {
    expect(validateCarEventForm(filled({ typeId: null }))).toEqual({ typeId: TYPE_REQUIRED })
  })

  it('требует дату', () => {
    expect(validateCarEventForm(filled({ date: null }))).toEqual({ date: DATE_REQUIRED })
  })

  it('не требует описание, комментарий и пробег', () => {
    expect(validateCarEventForm(filled({ title: '', comment: '', mileage: null }))).toEqual({})
  })
})

describe('toCarEventDraft', () => {
  it('переносит значения формы вместе с авто', () => {
    expect(toCarEventDraft(7, filled())).toEqual({
      carId: 7,
      typeId: 3,
      title: 'Замена масла',
      comment: 'сервис у дома',
      mileage: 130_000,
      date: localDate('2026-03-01'),
    })
  })

  it('пустые описание и комментарий уходят как отсутствующие', () => {
    const draft = toCarEventDraft(7, filled({ title: '   ', comment: '' }))

    expect(draft.title).toBeNull()
    expect(draft.comment).toBeNull()
  })

  it('обрезает пробелы вокруг описания', () => {
    expect(toCarEventDraft(7, filled({ title: '  ТО  ' })).title).toBe('ТО')
  })

  it('без типа и без даты черновик не собирается', () => {
    expect(() => toCarEventDraft(7, filled({ typeId: null }))).toThrow(TYPE_REQUIRED)
    expect(() => toCarEventDraft(7, filled({ date: null }))).toThrow(DATE_REQUIRED)
  })
})
