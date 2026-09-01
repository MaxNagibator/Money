import { describe, expect, it, vi } from 'vitest'
import { createCar, fetchCars, toCar } from '@/api/adapters/cars'
import { ApiError } from '@/api/adapters/problem'
import { apiFetch } from '@/platform/http'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

function respondJson(body: unknown, status = 200): void {
  vi.mocked(apiFetch).mockImplementation(() => Promise.resolve(new Response(JSON.stringify(body), { status })))
}

describe('fetchCars', () => {
  it('сохраняет порядок ответа сервера', async () => {
    respondJson([{ id: 3, name: 'Ока' }, { id: 1, name: 'Нива' }])

    const cars = await fetchCars()

    expect(cars).toEqual([{ id: 3, name: 'Ока' }, { id: 1, name: 'Нива' }])
  })

  it('пустой список не считается ошибкой', async () => {
    respondJson([])

    await expect(fetchCars()).resolves.toEqual([])
  })

  it('авто без имени не роняет список', () => {
    expect(toCar({ id: 1, name: null })).toEqual([])
    expect(toCar({ id: 1, name: 'Нива' })).toEqual([{ id: 1, name: 'Нива' }])
  })
})

describe('createCar', () => {
  it('создание уходит на /Cars и возвращает идентификатор', async () => {
    respondJson(7, 201)

    await expect(createCar('Нива')).resolves.toBe(7)

    const request = vi.mocked(apiFetch).mock.calls.at(-1)![0]

    expect(request.method).toBe('POST')
    expect(new URL(request.url).pathname).toBe('/api/Cars')
    expect(JSON.parse(await request.text())).toEqual({ name: 'Нива' })
  })

  it('сообщение сервера доходит до экрана', async () => {
    respondJson({ detail: 'Извините, наименование слишком длинное' }, 400)

    await expect(createCar('Нива')).rejects.toThrow(ApiError)
    await expect(createCar('Нива')).rejects.toThrow('Извините, наименование слишком длинное')
  })
})
