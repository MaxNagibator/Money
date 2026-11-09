import { describe, expect, it, vi } from 'vitest'
import type { OperationDraft, OperationFilter } from '@/api/adapters/operations'
import {
  buildOperationsQuery,
  createOperation,
  deleteOperation,
  fetchOperations,
  fetchPlaces,
  inferCategory,
  restoreOperation,
  toOperation,
  toSaveRequest,
  updateOperation,
} from '@/api/adapters/operations'
import { ApiError } from '@/api/adapters/problem'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { apiFetch } from '@/platform/http'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

const food: Category = {
  id: 3,
  parentId: null,
  name: 'Еда',
  color: null,
  order: null,
  operationTypeId: 1,
  isDeleted: false,
}

const knownCategories = new Map([[food.id, food]])

const emptyFilter: OperationFilter = {
  dateFrom: null,
  dateTo: null,
  categoryIds: [],
  comment: null,
  place: null,
}

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function draft(extra: Partial<OperationDraft> = {}): OperationDraft {
  return {
    categoryId: food.id,
    sum: Money.parseInput('1234,56')!,
    comment: 'кофе',
    place: 'Кафе',
    date: localDate('2026-08-22'),
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

async function lastBody(): Promise<Record<string, unknown>> {
  return JSON.parse(await lastRequest().text()) as Record<string, unknown>
}

describe('buildOperationsQuery', () => {
  it('переводит включительный конец периода в следующий день', () => {
    const query = buildOperationsQuery({
      ...emptyFilter,
      dateFrom: localDate('2026-08-01'),
      dateTo: localDate('2026-08-31'),
    })

    expect(query.DateFrom).toBe('2026-08-01T00:00:00')
    expect(query.DateTo).toBe('2026-09-01T00:00:00')
  })

  it('переносит конец периода через границу месяца', () => {
    const query = buildOperationsQuery({ ...emptyFilter, dateTo: localDate('2026-12-31') })

    expect(query.DateTo).toBe('2027-01-01T00:00:00')
  })

  it('склеивает выбранные категории в один параметр', () => {
    const query = buildOperationsQuery({ ...emptyFilter, categoryIds: [1, 2, 5] })

    expect(query.CategoryIds).toBe('1,2,5')
  })

  it('не добавляет пустые параметры', () => {
    const query = buildOperationsQuery({ ...emptyFilter, comment: '   ', place: '' })

    expect(Object.keys(query)).toEqual([])
  })
})

describe('fetchOperations', () => {
  it('уносит фильтр в строку запроса', async () => {
    respondJson([])

    await fetchOperations({
      dateFrom: localDate('2026-08-01'),
      dateTo: localDate('2026-08-31'),
      categoryIds: [1, 2, 5],
      comment: ' кофе ',
      place: null,
    }, [food])

    const url = lastUrl()

    expect(url.pathname).toBe('/api/Operations')
    expect(url.searchParams.get('DateFrom')).toBe('2026-08-01T00:00:00')
    expect(url.searchParams.get('DateTo')).toBe('2026-09-01T00:00:00')
    expect(url.searchParams.get('CategoryIds')).toBe('1,2,5')
    expect(url.searchParams.get('Comment')).toBe('кофе')
    expect(url.searchParams.has('Place')).toBe(false)
  })

  it('пустой фильтр уходит без параметров', async () => {
    respondJson([])

    await fetchOperations(emptyFilter, [food])

    expect(lastUrl().search).toBe('')
  })

  it('держит операцию с неизвестной категорией в выдаче', async () => {
    respondJson([
      { id: 1, categoryId: food.id, sum: 100, date: '2026-08-22T00:00:00' },
      { id: 2, categoryId: 99, sum: 200, date: '2026-08-22T00:00:00' },
    ])

    const operations = await fetchOperations(emptyFilter, [food])

    expect(operations.map(operation => operation.id)).toEqual([1, 2])
    expect(operations[1]?.categoryId).toBe(99)
    expect(operations[1]?.category).toBeNull()
  })

  it('помечает нераспознанную категорию и без списка категорий', () => {
    const operations = toOperation({ id: 1, categoryId: 99, sum: 100, date: '2026-08-22T00:00:00' }, new Map())

    expect(operations).toHaveLength(1)
    expect(operations[0]?.category).toBeNull()
  })

  it('отбрасывает операцию без суммы, в отличие от неизвестной категории', async () => {
    respondJson([
      { id: 1, categoryId: food.id, date: '2026-08-22T00:00:00' },
      { id: 2, categoryId: food.id, sum: 100, date: '2026-08-22T00:00:00' },
    ])

    const operations = await fetchOperations(emptyFilter, [food])

    expect(operations.map(operation => operation.id)).toEqual([2])
  })

  it('поднимает ApiError с текстом из ProblemDetails', async () => {
    respondJson({ title: 'Bad Request', detail: 'Извините, дата обязательна' }, 400)

    await expect(fetchOperations(emptyFilter, [food])).rejects.toThrow(ApiError)
    await expect(fetchOperations(emptyFilter, [food])).rejects.toThrow('Извините, дата обязательна')
  })
})

describe('toOperation', () => {
  it('читает дату ответа как LocalDate', () => {
    const operations = toOperation({ id: 1, categoryId: food.id, sum: 100, date: '2026-08-22T00:00:00' }, knownCategories)

    expect(operations[0]?.date).toBe('2026-08-22')
  })

  it('приходящие операции не помечены удалёнными', () => {
    const operations = toOperation({ id: 1, categoryId: food.id, sum: 100, date: '2026-08-22T00:00:00' }, knownCategories)

    expect(operations[0]?.isDeleted).toBe(false)
  })

  it.each([
    [1234.56, '1234.56'],
    [0.145, '0.145'],
    [12_345_678.91, '12345678.91'],
  ])('читает сумму %f как %s без промежуточного округления', (sum, expected) => {
    const operations = toOperation({ id: 1, categoryId: food.id, sum, date: '2026-08-22T00:00:00' }, knownCategories)

    expect(operations[0]?.sum.toDecimal().toString()).toBe(expected)
  })
})

describe('toSaveRequest', () => {
  it('отправляет дату полуночью без смещения', () => {
    const request = toSaveRequest(draft())

    expect(request.date).toBe('2026-08-22T00:00:00')
    expect(request.date).not.toMatch(/Z$|[+-]\d{2}:\d{2}$/)
  })

})

describe('сохранение', () => {
  it('создание отправляет POST и берёт идентификатор из ответа', async () => {
    respondJson(42, 201)

    const id = await createOperation(draft())

    expect(id).toBe(42)
    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/Operations')
    await expect(lastBody()).resolves.toEqual({
      categoryId: food.id,
      sum: 1234.56,
      comment: 'кофе',
      place: 'Кафе',
      date: '2026-08-22T00:00:00',
    })
  })

  it.each([
    ['1234,56', '1234.56'],
    ['0,145', '0.145'],
    ['1234,564', '1234.564'],
    ['3,3333333333333333333333333333', '3.3333333333333333333333333333'],
  ])('сумма %j уходит числом %s без округления до копеек', async (input, expected) => {
    respondJson(42, 201)

    await createOperation(draft({ sum: Money.parseInput(input)! }))

    expect(await lastRequest().text()).toContain(`"sum":${expected}`)
  })

  it('изменение отправляет PUT по маршруту с идентификатором', async () => {
    respondEmpty(204)

    await updateOperation(7, draft())

    expect(lastRequest().method).toBe('PUT')
    expect(lastUrl().pathname).toBe('/api/Operations/7')
    await expect(lastBody()).resolves.toMatchObject({ date: '2026-08-22T00:00:00' })
  })

  it('пустой ответ создания считается ошибкой', async () => {
    respondEmpty(201)

    await expect(createOperation(draft())).rejects.toThrow(ApiError)
  })
})

describe('удаление и восстановление', () => {
  it('удаление уходит на маршрут операции', async () => {
    respondEmpty(204)

    await deleteOperation(7)

    expect(lastRequest().method).toBe('DELETE')
    expect(lastUrl().pathname).toBe('/api/Operations/7')
  })

  it('восстановление уходит на Restore', async () => {
    respondEmpty(204)

    await restoreOperation(7)

    expect(lastRequest().method).toBe('POST')
    expect(lastUrl().pathname).toBe('/api/Operations/7/Restore')
  })
})

describe('inferCategory', () => {
  it('пустой ответ означает отсутствие подсказки', async () => {
    respondEmpty(204)

    await expect(inferCategory('Кафе', 1)).resolves.toBeNull()
  })

  it('возвращает подсказанную категорию', async () => {
    respondJson({ categoryId: 12 })

    await expect(inferCategory('Кафе', 1, 10)).resolves.toBe(12)

    const url = lastUrl()

    expect(decodeURIComponent(url.pathname)).toBe('/api/Operations/InferCategory/Кафе')
    expect(url.searchParams.get('operationType')).toBe('1')
    expect(url.searchParams.get('count')).toBe('10')
  })
})

describe('fetchPlaces', () => {
  it('без имени зовёт маршрут из двух сегментов', async () => {
    respondJson(['Кафе'])

    await expect(fetchPlaces(0, 10)).resolves.toEqual(['Кафе'])
    expect(lastUrl().pathname).toBe('/api/Operations/GetPlaces/0/10')
  })

  it('пустое имя не добавляет сегмент', async () => {
    respondJson([])

    await fetchPlaces(0, 10, '   ')

    expect(lastUrl().pathname).toBe('/api/Operations/GetPlaces/0/10')
  })

  it('имя уходит отдельным сегментом', async () => {
    respondJson(['Кафе'])

    await fetchPlaces(5, 10, 'Ка')

    expect(decodeURIComponent(lastUrl().pathname)).toBe('/api/Operations/GetPlaces/5/10/Ка')
  })
})
