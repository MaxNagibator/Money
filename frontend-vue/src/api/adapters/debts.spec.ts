import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { DebtDraft, DebtPaymentDraft } from '@/api/adapters/debts'
import {
  createDebt,
  deleteDebt,
  fetchDebt,
  fetchDebtOwnerNames,
  fetchDebtOwners,
  fetchDebts,
  forgiveDebts,
  mergeDebtOwners,
  payDebt,
  restoreDebt,
  toDebt,
  toPayRequest,
  toSaveRequest,
  UNNAMED_DEBT_OWNER,
  updateDebt,
} from '@/api/adapters/debts'
import { ApiError } from '@/api/adapters/problem'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { apiFetch } from '@/platform/http'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

beforeEach(() => {
  vi.clearAllMocks()
})

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function draft(extra: Partial<DebtDraft> = {}): DebtDraft {
  return {
    typeId: 1,
    sum: Money.parseInput('1234,56')!,
    comment: 'за обед',
    ownerName: 'Иван',
    date: localDate('2026-08-22'),
    ...extra,
  }
}

function payment(extra: Partial<DebtPaymentDraft> = {}): DebtPaymentDraft {
  return {
    sum: Money.parseInput('100')!,
    comment: 'первый взнос',
    date: localDate('2026-08-22'),
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

function lastRequest(): Request {
  return vi.mocked(apiFetch).mock.calls.at(-1)![0]
}

function lastUrl(): URL {
  return new URL(lastRequest().url)
}

function requestLog(): string[] {
  return vi.mocked(apiFetch).mock.calls.map(([request]) => `${request.method} ${new URL(request.url).pathname}`)
}

async function lastBody(): Promise<Record<string, unknown>> {
  return await lastRequest().clone().json() as Record<string, unknown>
}

describe('fetchDebts', () => {
  it.each([
    [false, 'false'],
    [true, 'true'],
  ])('запрашивает список одним GET с withPaid=%s', async (withPaid, expected) => {
    respondJson([])

    await fetchDebts(withPaid)

    const url = lastUrl()

    expect(lastRequest().method).toBe('GET')
    expect(url.pathname).toBe('/api/Debts')
    expect(url.searchParams.get('withPaid')).toBe(expected)
    expect([...url.searchParams.keys()]).toEqual(['withPaid'])
    expect(vi.mocked(apiFetch)).toHaveBeenCalledTimes(1)
  })

  it('сохраняет порядок ответа и не роняет список на битой строке', async () => {
    respondJson([
      { id: 1, typeId: 1, sum: 100, ownerName: 'Иван', date: '2026-08-22T00:00:00', paySum: 0 },
      { id: 2, typeId: 2, sum: 200, ownerName: null, date: 'не дата', paySum: 0 },
      { id: 3, typeId: 2, sum: 300, ownerName: 'Пётр', date: '2026-08-21T00:00:00', paySum: 0 },
    ])

    const debts = await fetchDebts(false)

    expect(debts.map(debt => debt.id)).toEqual([1, 3])
  })

  it('поднимает ошибку загрузки как ApiError', async () => {
    respondProblem('Извините, но долг не найден', 404)

    await expect(fetchDebts(false)).rejects.toThrow(ApiError)
  })
})

describe('toDebt', () => {
  it('читает дату ответа как день без времени', () => {
    const debts = toDebt({ id: 1, typeId: 1, sum: 100, ownerName: 'Иван', date: '2026-08-22T13:45:00', paySum: 0 })

    expect(debts[0]?.date).toBe('2026-08-22')
  })

  it.each([
    [1234.56, 123_456],
    [0.145, 15],
    [12_345_678.91, 1_234_567_891],
  ])('читает сумму %f как %i копеек без промежуточного округления', (sum, expected) => {
    const debts = toDebt({ id: 1, typeId: 1, sum, ownerName: 'Иван', date: '2026-08-22T00:00:00', paySum: sum })

    expect(debts[0]?.sum.toMinorUnits()).toBe(expected)
    expect(debts[0]?.paySum.toMinorUnits()).toBe(expected)
  })

  it('держатель без имени приходит пустой строкой, а не null', () => {
    const debts = toDebt({ id: 1, typeId: 1, sum: 100, ownerName: null, date: '2026-08-22T00:00:00', paySum: 0 })

    expect(debts[0]?.ownerName).toBe('')
  })

  it('долг из списка не помечен удалённым', () => {
    const debts = toDebt({ id: 1, typeId: 1, sum: 100, ownerName: 'Иван', date: '2026-08-22T00:00:00', paySum: 0 })

    expect(debts[0]?.isDeleted).toBe(false)
  })

  it('незнакомый тип не выбрасывает долг из списка', () => {
    const debts = toDebt({ id: 1, typeId: 99, sum: 100, ownerName: 'Иван', date: '2026-08-22T00:00:00', paySum: 0 })

    expect(debts[0]?.typeId).toBe(99)
  })

  it.each([
    ['без суммы', { id: 1, typeId: 1, ownerName: 'Иван', date: '2026-08-22T00:00:00', paySum: 100 }],
    ['без оплаченной части', { id: 1, typeId: 1, sum: 100, ownerName: 'Иван', date: '2026-08-22T00:00:00' }],
  ])('долг %s отбрасывается, а не читается нулём', (_, dto) => {
    expect(toDebt(dto)).toEqual([])
  })

  it('битый долг не роняет список и не даёт отрицательный остаток', async () => {
    respondJson([
      { id: 1, typeId: 1, ownerName: 'Иван', date: '2026-08-22T00:00:00', paySum: 100 },
      { id: 2, typeId: 1, sum: 300, ownerName: 'Пётр', date: '2026-08-21T00:00:00', paySum: 100 },
    ])

    const debts = await fetchDebts(false)

    expect(debts.map(debt => debt.id)).toEqual([2])
  })
})

describe('toSaveRequest', () => {
  it('отправляет дату полуночью без смещения', () => {
    const request = toSaveRequest(draft())

    expect(request.date).toBe('2026-08-22T00:00:00')
    expect(request.date).not.toMatch(/Z$|[+-]\d{2}:\d{2}$/)
  })

  it('отправляет сумму числом с округлением до копеек', () => {
    expect(toSaveRequest(draft()).sum).toBe(1234.56)
    expect(toSaveRequest(draft({ sum: Money.parseInput('0,145')! })).sum).toBe(0.15)
  })

  it('нулевая сумма уходит нулём', () => {
    expect(toSaveRequest(draft({ sum: Money.zero })).sum).toBe(0)
  })
})

describe('toPayRequest', () => {
  it('отправляет дату платежа полуночью без смещения', () => {
    expect(toPayRequest(payment()).date).toBe('2026-08-22T00:00:00')
  })
})

describe('сохранение долга', () => {
  it('создание отправляет POST и берёт идентификатор из ответа', async () => {
    respondJson(42, 201)

    const id = await createDebt(draft())

    expect(id).toBe(42)
    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/Debts')
    await expect(lastBody()).resolves.toEqual({
      typeId: 1,
      sum: 1234.56,
      comment: 'за обед',
      ownerName: 'Иван',
      date: '2026-08-22T00:00:00',
    })
  })

  it('изменение отправляет PUT по маршруту долга с тем же телом', async () => {
    respondEmpty(204)

    await updateDebt(7, draft())

    expect(lastRequest().method).toBe('PUT')
    expect(lastUrl().pathname).toBe('/api/Debts/7')
    await expect(lastBody()).resolves.toMatchObject({ typeId: 1, ownerName: 'Иван' })
  })

  it('удаление уходит на маршрут долга', async () => {
    respondEmpty(204)

    await deleteDebt(7)

    expect(lastRequest().method).toBe('DELETE')
    expect(lastUrl().pathname).toBe('/api/Debts/7')
  })

  it('восстановление уходит на Restore', async () => {
    respondEmpty(204)

    await restoreDebt(7)

    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/Debts/7/Restore')
  })

  it('деловая ошибка сохранения приходит текстом сервера', async () => {
    respondProblem('Извините, но можно обновлять только непогашенные долги')

    await expect(updateDebt(7, draft())).rejects.toThrow('Извините, но можно обновлять только непогашенные долги')
  })
})

describe('оплата', () => {
  it('оплата уходит на Pay с суммой, комментарием и датой', async () => {
    respondEmpty(204)

    await payDebt(7, payment())

    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/Debts/7/Pay')
    await expect(lastBody()).resolves.toEqual({
      sum: 100,
      comment: 'первый взнос',
      date: '2026-08-22T00:00:00',
    })
  })

  it('оплата ничего не возвращает, оплаченную часть приносит отдельный GET по идентификатору', async () => {
    vi.mocked(apiFetch).mockImplementation(request => Promise.resolve(request.method === 'POST'
      ? new Response(null, { status: 204 })
      : new Response(
          JSON.stringify({
            id: 7,
            typeId: 1,
            sum: 1000,
            ownerName: 'Иван',
            date: '2026-08-22T00:00:00',
            paySum: 100,
            payComment: '2026.08.22 100 первый взнос',
          }),
          { status: 200, headers: { 'content-type': 'application/json' } },
        )))

    const paid = await payDebt(7, payment())
    const fresh = await fetchDebt(7)

    expect(paid).toBeUndefined()
    expect(requestLog()).toEqual(['POST /api/Debts/7/Pay', 'GET /api/Debts/7'])
    expect(fresh?.paySum.toMinorUnits()).toBe(10_000)
    expect(fresh?.payComment).toBe('2026.08.22 100 первый взнос')
  })

  it('превышение суммы долга приходит текстом сервера', async () => {
    respondProblem('Извините, но оплаченная часть долга не может превышать сумму долга')

    await expect(payDebt(7, payment())).rejects.toThrow('Извините, но оплаченная часть долга не может превышать сумму долга')
  })
})

describe('прощение', () => {
  it('прощение уходит списком долгов, категорией и припиской', async () => {
    respondEmpty(204)

    await forgiveDebts({ debtIds: [4, 5], categoryId: 3, comment: 'Перенос из долгов:' })

    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/Debts/Forgive')
    await expect(lastBody()).resolves.toEqual({
      debtIds: [4, 5],
      operationCategoryId: 3,
      operationComment: 'Перенос из долгов:',
    })
  })

  it('отказ прощения приходит текстом сервера', async () => {
    respondProblem('Извините, но прощать можно только непогашенные долги')

    await expect(forgiveDebts({ debtIds: [4], categoryId: 3, comment: null }))
      .rejects
      .toThrow('Извините, но прощать можно только непогашенные долги')
  })
})

describe('держатели', () => {
  it('слияние уходит на MergeOwners с обоими идентификаторами', async () => {
    respondEmpty(204)

    await mergeDebtOwners(3, 8)

    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/Debts/MergeOwners/3/with/8')
  })

  it('одинаковые держатели отвергаются сервером', async () => {
    respondProblem('Нужно выбрать разных держателей долга')

    await expect(mergeDebtOwners(3, 3)).rejects.toThrow('Нужно выбрать разных держателей долга')
  })

  it('список держателей подставляет имя безымянному', async () => {
    respondJson([{ id: 1, name: 'Иван' }, { id: 2, name: null }])

    const owners = await fetchDebtOwners()

    expect(lastUrl().pathname).toBe('/api/Debts/Owners')
    expect(owners).toEqual([{ id: 1, name: 'Иван' }, { id: 2, name: UNNAMED_DEBT_OWNER }])
  })

  it('подсказки без имени уходят на маршрут без имени', async () => {
    respondJson(['Иван'])

    await fetchDebtOwnerNames(0, 10)

    expect(lastUrl().pathname).toBe('/api/Debts/Owners/0/10')
  })

  it('подсказки по префиксу уходят на маршрут с именем', async () => {
    respondJson(['Иван'])

    await fetchDebtOwnerNames(0, 10, ' Ив ')

    expect(lastUrl().pathname).toBe('/api/Debts/Owners/0/10/%D0%98%D0%B2')
  })
})
