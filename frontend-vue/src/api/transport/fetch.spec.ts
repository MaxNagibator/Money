import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '@/api/adapters/problem'
import { transportFetch } from '@/api/transport/fetch'
import { apiFetch } from '@/platform/http'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

function respond(response: Response): void {
  vi.mocked(apiFetch).mockResolvedValue(response)
}

function debtsRequest(): Request {
  return new Request('https://money.test/api/Debts')
}

describe('transportFetch', () => {
  it('подставляет в JSON точный литерал суммы и сохраняет остальной ответ', async () => {
    respond(new Response('[{"id":1,"sum":100.00,"paySum":0.10}]', {
      status: 200,
      headers: { 'Content-Type': 'application/json', 'Content-Length': '38', 'X-Request-Id': 'сорок два' },
    }))

    const response = await transportFetch(debtsRequest())

    expect(response.status).toBe(200)
    expect(response.headers.get('X-Request-Id')).toBe('сорок два')
    expect(response.headers.get('Content-Type')).toBe('application/json')
    expect(response.headers.get('Content-Length')).toBeNull()
    expect(await response.text()).toBe('[{"id":1,"sum":"100.00","paySum":"0.10"}]')
  })

  it('оставляет отказ с телом problem+json разбираемым', async () => {
    respond(new Response('{"title":"Нет доступа"}', {
      status: 403,
      headers: { 'Content-Type': 'application/problem+json' },
    }))

    const response = await transportFetch(debtsRequest())

    expect(response.ok).toBe(false)
    expect(response.status).toBe(403)
    expect(await response.json()).toEqual({ title: 'Нет доступа' })
  })

  it.each([
    ['не JSON', new Response('просто текст', { status: 200, headers: { 'Content-Type': 'text/plain' } })],
    ['без тела', new Response(null, { status: 204 })],
  ])('пропускает насквозь ответ %s', async (_case, original) => {
    respond(original)

    expect(await transportFetch(debtsRequest())).toBe(original)
  })

  it('переводит истёкшее ожидание в ошибку API', async () => {
    vi.mocked(apiFetch).mockRejectedValue(new DOMException('прервано', 'TimeoutError'))

    await expect(transportFetch(debtsRequest())).rejects.toBeInstanceOf(ApiError)
  })

  it('переводит истёкшее при чтении тела ожидание в ошибку API', async () => {
    const response = new Response('[{"id":1,"sum":100.00}]', {
      status: 200,
      headers: { 'Content-Type': 'application/json' },
    })

    vi.spyOn(response, 'text').mockRejectedValue(new DOMException('прервано', 'TimeoutError'))
    respond(response)

    await expect(transportFetch(debtsRequest())).rejects.toBeInstanceOf(ApiError)
  })

  it('прочие отказы сети пропускает как есть', async () => {
    const failure = new TypeError('сеть недоступна')

    vi.mocked(apiFetch).mockRejectedValue(failure)

    await expect(transportFetch(debtsRequest())).rejects.toBe(failure)
  })
})
