import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { components } from '@/api/generated/schema'
import { ApiError } from '@/api/adapters/problem'
import type { RegularOperationDraft } from '@/api/adapters/regular-operations'
import {
  createRegularOperation,
  deleteRegularOperation,
  fetchRegularOperation,
  fetchRegularOperations,
  restoreRegularOperation,
  toRegularOperation,
  toSaveRequest,
  updateRegularOperation,
} from '@/api/adapters/regular-operations'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { apiFetch } from '@/platform/http'

type RegularOperationDto = components['schemas']['Money.Api.Dto.RegularOperations.RegularOperationDto']

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

beforeEach(() => {
  vi.clearAllMocks()
})

describe('чтение ответа', () => {
  it('читает даты как дни без времени', () => {
    const operations = toRegularOperation(dto({
      dateFrom: '2026-08-23T00:00:00',
      dateTo: '2026-12-31T00:00:00',
      runTime: '2026-09-01T13:45:00',
    }))

    expect(operations[0]?.dateFrom).toBe('2026-08-23')
    expect(operations[0]?.dateTo).toBe('2026-12-31')
    expect(operations[0]?.runTime).toBe('2026-09-01')
  })

  it.each([
    [1234.56, '1234.56'],
    [0.145, '0.145'],
    [12_345_678.91, '12345678.91'],
  ])('читает сумму %f как %s без промежуточного округления', (sum, expected) => {
    expect(toRegularOperation(dto({ sum }))[0]?.sum.toDecimal().toString()).toBe(expected)
  })

  it.each([
    ['', null],
    ['   ', null],
    ['Пятёрочка', 'Пятёрочка'],
  ])('место и комментарий %j читаются как %j', (value, expected) => {
    const operation = toRegularOperation(dto({ place: value, comment: value }))[0]

    expect(operation?.place).toBe(expected)
    expect(operation?.comment).toBe(expected)
  })

  it('операция без даты окончания и без ближайшего запуска остаётся в списке', () => {
    const operations = toRegularOperation(dto({ dateTo: null, runTime: null }))

    expect(operations[0]?.dateTo).toBeNull()
    expect(operations[0]?.runTime).toBeNull()
  })

  it('незнакомый временной шаг не выбрасывает операцию из списка', () => {
    expect(toRegularOperation(dto({ timeTypeId: 99 }))[0]?.timeTypeId).toBe(99)
  })

  it('битая операция не роняет список', async () => {
    respondJson([
      { name: 'Без идентификатора', categoryId: 3, dateFrom: '2026-08-23T00:00:00' },
      dto({ id: 2 }),
      dto({ id: 3, dateFrom: 'не дата' }),
    ])

    expect((await fetchRegularOperations()).map(operation => operation.id)).toEqual([2])
  })

  it('список приходит без признака удаления, все операции читаются активными', async () => {
    respondJson([dto({ id: 1 }), dto({ id: 2 })])

    const operations = await fetchRegularOperations()

    expect(operations.map(operation => operation.isDeleted)).toEqual([false, false])
  })

  it('отказ загрузки списка приходит ошибкой, а не пустым списком', async () => {
    respondProblem('Извините, произошла ошибка')

    await expect(fetchRegularOperations()).rejects.toThrow(ApiError)
  })
})

describe('отправка запроса', () => {
  it('даты уходят локальной полуночью без смещения', () => {
    const request = toSaveRequest(draft())

    expect(request.dateFrom).toBe('2026-08-23T00:00:00')
    expect(request.dateTo).toBe('2026-12-31T00:00:00')
  })

  it('сумма уходит числом и доли копейки не срезаются', async () => {
    respondJson(7, 201)

    await createRegularOperation(draft({ sum: Money.parseInput('1234,564')! }))

    expect(await vi.mocked(apiFetch).mock.calls.at(-1)![0].text()).toContain('"sum":1234.564')
  })

  it('пустая дата окончания и отсутствующий шаг уходят как null', () => {
    const request = toSaveRequest(draft({ dateTo: null, timeValue: null }))

    expect(request.dateTo).toBeNull()
    expect(request.timeValue).toBeNull()
  })

  it('создание отдаёт идентификатор, изменение уходит по нему', async () => {
    respondJson(7, 201)

    expect(await createRegularOperation(draft())).toBe(7)

    respondEmpty(204)
    await updateRegularOperation(7, draft())

    expect(requestLog()).toEqual([
      'POST /api/RegularOperations',
      'PUT /api/RegularOperations/7',
    ])
  })

  it('операция перечитывается по идентификатору ради ближайшего запуска', async () => {
    respondJson(dto({ id: 7, runTime: '2026-09-01T00:00:00' }))

    const fresh = await fetchRegularOperation(7)

    expect(fresh?.runTime).toBe('2026-09-01')
    expect(requestLog()).toEqual(['GET /api/RegularOperations/7'])
  })

  it('удаление и восстановление идут по идентификатору', async () => {
    respondEmpty(204)

    await deleteRegularOperation(7)
    await restoreRegularOperation(7)

    expect(requestLog()).toEqual([
      'DELETE /api/RegularOperations/7',
      'POST /api/RegularOperations/7/Restore',
    ])
  })

  it('отказ сервера приходит его текстом', async () => {
    respondProblem('Извините, но значение интервала должно быть в диапазоне от 1 до 31 при Каждый месяц')

    await expect(createRegularOperation(draft())).rejects.toThrow(
      'Извините, но значение интервала должно быть в диапазоне от 1 до 31 при Каждый месяц',
    )
  })
})

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function dto(extra: Partial<RegularOperationDto> = {}): RegularOperationDto {
  return {
    id: 1,
    name: 'Аренда',
    categoryId: 3,
    sum: 100,
    comment: 'за месяц',
    place: 'Дом',
    timeTypeId: 3,
    timeValue: 1,
    dateFrom: '2026-08-23T00:00:00',
    dateTo: null,
    runTime: '2026-09-01T00:00:00',
    ...extra,
  }
}

function draft(extra: Partial<RegularOperationDraft> = {}): RegularOperationDraft {
  return {
    name: 'Аренда',
    categoryId: 3,
    sum: Money.parseInput('1234,56')!,
    comment: 'за месяц',
    place: 'Дом',
    timeTypeId: 3,
    timeValue: 1,
    dateFrom: localDate('2026-08-23'),
    dateTo: localDate('2026-12-31'),
    ...extra,
  }
}

function respondJson(body: unknown, status = 200): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(JSON.stringify(body), {
    status,
    headers: { 'content-type': 'application/json' },
  })))
}

function respondEmpty(status: number): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(null, { status })))
}

function respondProblem(detail: string, status = 400): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(
    JSON.stringify({ detail }),
    { status, headers: { 'content-type': 'application/json' } },
  )))
}

function requestLog(): string[] {
  return vi.mocked(apiFetch).mock.calls.map(([request]) => `${request.method} ${new URL(request.url).pathname}`)
}
