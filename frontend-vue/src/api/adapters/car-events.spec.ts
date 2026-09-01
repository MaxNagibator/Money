import { describe, expect, it, vi } from 'vitest'
import type { CarEventDraft } from '@/api/adapters/car-events'
import {
  createCarEvent,
  deleteCarEvent,
  fetchCarEvents,
  restoreCarEvent,
  toCarEvent,
  toSaveRequest,
  updateCarEvent,
} from '@/api/adapters/car-events'
import { ApiError } from '@/api/adapters/problem'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { apiFetch } from '@/platform/http'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function draft(extra: Partial<CarEventDraft> = {}): CarEventDraft {
  return {
    carId: 7,
    typeId: 3,
    title: 'ТО',
    comment: null,
    mileage: 120_000,
    date: localDate('2026-03-01'),
    ...extra,
  }
}

function respondJson(body: unknown, status = 200): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(JSON.stringify(body), { status })))
}

function respondEmpty(status: number): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(null, { status })))
}

function lastRequest(): Request {
  return vi.mocked(apiFetch).mock.calls.at(-1)![0]
}

function lastUrl(): URL {
  return new URL(lastRequest().url)
}

describe('fetchCarEvents', () => {
  it('запрашивает события выбранного авто и сохраняет порядок ответа', async () => {
    respondJson([
      { id: 1, typeId: 3, title: 'ТО', comment: null, mileage: 100, date: '2026-03-01T00:00:00' },
      { id: 2, typeId: 99, title: null, comment: 'без деталей', mileage: null, date: '2026-02-01T00:00:00' },
    ])

    const events = await fetchCarEvents(7)

    expect(lastUrl().pathname).toBe('/api/CarEvents/Car/7')
    expect(events.map(item => item.id)).toEqual([1, 2])
    expect(events[0]?.carId).toBe(7)
  })

  it('читает дату ответа как день без времени', () => {
    const events = toCarEvent({ id: 1, typeId: 3, date: '2026-03-01T00:00:00' }, 7)

    expect(events[0]?.date).toBe('2026-03-01')
  })

  it('приходящие события не помечены удалёнными', () => {
    const events = toCarEvent({ id: 1, typeId: 3, date: '2026-03-01T00:00:00' }, 7)

    expect(events[0]?.isDeleted).toBe(false)
  })

  it('событие без даты не роняет список', () => {
    expect(toCarEvent({ id: 1, typeId: 3 }, 7)).toEqual([])
  })
})

describe('toSaveRequest', () => {
  it('отправляет дату полуночью без смещения', () => {
    expect(toSaveRequest(draft()).date).toBe('2026-03-01T00:00:00')
  })

  it('переносит поля черновика как есть', () => {
    expect(toSaveRequest(draft({ comment: 'у дома' }))).toEqual({
      carId: 7,
      typeId: 3,
      title: 'ТО',
      comment: 'у дома',
      mileage: 120_000,
      date: '2026-03-01T00:00:00',
    })
  })
})

describe('сохранение события', () => {
  it('создание уходит на /CarEvents и возвращает идентификатор', async () => {
    respondJson(42, 201)

    await expect(createCarEvent(draft())).resolves.toBe(42)

    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/CarEvents')
  })

  it('изменение уходит на маршрут события', async () => {
    respondEmpty(204)

    await updateCarEvent(42, draft())

    expect(lastRequest().method).toBe('PUT')
    expect(lastUrl().pathname).toBe('/api/CarEvents/42')
  })

  it('сообщение сервера доходит до диалога', async () => {
    respondJson({ title: 'Bad Request', detail: 'Извините, пробег должен быть положительным' }, 400)

    await expect(createCarEvent(draft({ mileage: -1 }))).rejects.toThrow(ApiError)
    await expect(createCarEvent(draft({ mileage: -1 }))).rejects.toThrow('Извините, пробег должен быть положительным')
  })
})

describe('удаление события', () => {
  it('удаление уходит на маршрут события', async () => {
    respondEmpty(204)

    await deleteCarEvent(42)

    expect(lastRequest().method).toBe('DELETE')
    expect(lastUrl().pathname).toBe('/api/CarEvents/42')
  })

  it('восстановление уходит на Restore', async () => {
    respondEmpty(204)

    await restoreCarEvent(42)

    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/CarEvents/42/Restore')
  })

  it('отказ восстановления доходит сообщением сервера', async () => {
    respondJson({ detail: 'Извините, но невозможно восстановить неудаленную сущность' }, 400)

    await expect(restoreCarEvent(42)).rejects.toThrow('Извините, но невозможно восстановить неудаленную сущность')
  })
})
