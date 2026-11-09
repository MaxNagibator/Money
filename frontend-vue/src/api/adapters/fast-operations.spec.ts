import { describe, expect, it, vi } from 'vitest'
import type { FastOperationDraft } from '@/api/adapters/fast-operations'
import {
  createFastOperation,
  deleteFastOperation,
  fetchFastOperations,
  restoreFastOperation,
  toFastOperation,
  updateFastOperation,
} from '@/api/adapters/fast-operations'
import { ApiError } from '@/api/adapters/problem'
import { Money } from '@/domain/money'
import { apiFetch } from '@/platform/http'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

function draft(extra: Partial<FastOperationDraft> = {}): FastOperationDraft {
  return {
    name: 'Кофе',
    order: 2,
    categoryId: 3,
    sum: Money.parseInput('1234,56')!,
    comment: 'утро',
    place: 'Кафе',
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

async function lastBody(): Promise<Record<string, unknown>> {
  return JSON.parse(await lastRequest().text()) as Record<string, unknown>
}

describe('чтение списка', () => {
  it('читает сумму в Money и не помечает записи удалёнными', async () => {
    respondJson([
      { id: 1, name: 'Кофе', categoryId: 3, sum: 1234.56, order: null, comment: null, place: null },
    ])

    const operations = await fetchFastOperations()

    expect(new URL(lastRequest().url).pathname).toBe('/api/FastOperations')
    expect(operations).toHaveLength(1)
    expect(operations[0]!.sum.toDecimal().toString()).toBe('1234.56')
    expect(operations[0]!.isDeleted).toBe(false)
  })

  it.each([
    ['через response.json()', true],
    ['через разбор текста ответа', false],
  ])('сохраняет масштаб и точность суммы %s', async (_path, withContentLength) => {
    const body = '[{"id":1,"categoryId":3,"sum":100.00},{"id":2,"categoryId":3,"sum":9007199254740993.55}]'
    const headers = new Headers({ 'Content-Type': 'application/json' })

    if (withContentLength) {
      headers.set('Content-Length', String(body.length))
    }

    vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(body, { status: 200, headers })))

    const operations = await fetchFastOperations()

    expect(operations[0]!.sum.wireScale).toBe(2)
    expect(operations[1]!.sum.toDecimal().toString()).toBe('9007199254740993.55')
  })

  it('подставляет пустое имя вместо отсутствующего и ноль вместо отсутствующей суммы', () => {
    const [operation] = toFastOperation({ id: 7, name: null, categoryId: 3 })

    expect(operation!.name).toBe('')
    expect(operation!.sum.isZero).toBe(true)
  })

  it('отказ загрузки отличим от пустого списка', async () => {
    respondJson({ title: 'Отказ', detail: 'Нет доступа' }, 401)

    await expect(fetchFastOperations()).rejects.toThrow(ApiError)
  })
})

describe('сохранение', () => {
  it('создание уходит на POST и берёт идентификатор из ответа', async () => {
    respondJson(42, 201)

    const id = await createFastOperation(draft())

    expect(lastRequest().method).toBe('POST')
    expect(new URL(lastRequest().url).pathname).toBe('/api/FastOperations')
    expect(id).toBe(42)
  })

  it('изменение уходит на PUT по маршруту с идентификатором', async () => {
    respondEmpty(204)

    await updateFastOperation(5, draft())

    expect(lastRequest().method).toBe('PUT')
    expect(new URL(lastRequest().url).pathname).toBe('/api/FastOperations/5')
    expect(await lastBody()).not.toHaveProperty('id')
  })

  it('в тело уходит распознанная сумма, а не ноль, и доли копейки не срезаются', async () => {
    respondEmpty(204)

    await updateFastOperation(5, draft({ sum: Money.parseInput('0,015')! }))

    expect(await lastBody()).toMatchObject({ sum: 0.015, name: 'Кофе', categoryId: 3, order: 2 })
  })
})

describe('удаление и восстановление', () => {
  it('удаление и восстановление уходят на свои маршруты', async () => {
    respondEmpty(204)

    await deleteFastOperation(9)

    expect(lastRequest().method).toBe('DELETE')
    expect(new URL(lastRequest().url).pathname).toBe('/api/FastOperations/9')

    await restoreFastOperation(9)

    expect(lastRequest().method).toBe('POST')
    expect(new URL(lastRequest().url).pathname).toBe('/api/FastOperations/9/Restore')
  })

  it('отказ восстановления приходит текстом сервера', async () => {
    respondJson({ title: 'Не найдено', detail: 'Быстрая операция не найдена' }, 404)

    await expect(restoreFastOperation(9)).rejects.toThrow('Быстрая операция не найдена')
  })
})
