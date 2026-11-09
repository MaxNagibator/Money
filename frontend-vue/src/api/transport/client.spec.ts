import { beforeEach, describe, expect, it, vi } from 'vitest'
import { apiFetch } from '@/platform/http'
import { apiClient, setAccessTokenProvider, setAccessTokenRefresher } from './client'

vi.mock('@/platform/http', () => ({
  API_BASE_URL: 'https://money.test/api',
  apiFetch: vi.fn(),
}))

const apiFetchMock = vi.mocked(apiFetch)

let refreshAccessToken = vi.fn<(staleToken: string) => Promise<string | null>>()

beforeEach(() => {
  apiFetchMock.mockReset()
  refreshAccessToken = vi.fn<(staleToken: string) => Promise<string | null>>()

  setAccessTokenProvider(() => Promise.resolve('протухший'))
  setAccessTokenRefresher(refreshAccessToken)
})

describe('ответ 401 на обычном запросе', () => {
  it('обновляет токен один раз и повторяет запрос с новым', async () => {
    respond(unauthorized(), listOfCategories())
    refreshAccessToken.mockResolvedValue('свежий')

    const { data, error } = await apiClient.GET('/Categories')

    expect(error).toBeUndefined()
    expect(data).toEqual([])
    expect(refreshAccessToken.mock.calls).toEqual([['протухший']])
    expect(authorizations()).toEqual(['Bearer протухший', 'Bearer свежий'])
  })

  it('несостоявшееся обновление оставляет отказ и запрос не повторяет', async () => {
    respond(unauthorized(), listOfCategories())
    refreshAccessToken.mockResolvedValue(null)

    const { data, response } = await apiClient.GET('/Categories')

    expect(data).toBeUndefined()
    expect(response.status).toBe(401)
    expect(apiFetchMock).toHaveBeenCalledTimes(1)
  })

  it('повторный отказ не уходит на второй круг обновления', async () => {
    respond(unauthorized(), unauthorized(), listOfCategories())
    refreshAccessToken.mockResolvedValue('свежий')

    const { response } = await apiClient.GET('/Categories')

    expect(response.status).toBe(401)
    expect(refreshAccessToken).toHaveBeenCalledTimes(1)
    expect(apiFetchMock).toHaveBeenCalledTimes(2)
  })

  it('каждый параллельный запрос сообщает об отказе со своим токеном', async () => {
    respond(unauthorized(), unauthorized(), listOfCategories(), listOfCategories())
    refreshAccessToken.mockResolvedValue('свежий')

    await Promise.all([apiClient.GET('/Categories'), apiClient.GET('/Cars')])

    expect(refreshAccessToken.mock.calls).toEqual([['протухший'], ['протухший']])
  })

  it('запрос без токена сессию не трогает', async () => {
    setAccessTokenProvider(() => Promise.resolve(null))
    respond(unauthorized())

    const { response } = await apiClient.GET('/Categories')

    expect(response.status).toBe(401)
    expect(refreshAccessToken).not.toHaveBeenCalled()
    expect(apiFetchMock).toHaveBeenCalledTimes(1)
  })
})

function respond(...responses: Response[]): void {
  apiFetchMock.mockImplementation(() => Promise.resolve(responses.shift() ?? listOfCategories()))
}

function unauthorized(): Response {
  return new Response('{"title":"Не авторизован"}', {
    status: 401,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

function listOfCategories(): Response {
  return new Response('[]', { status: 200, headers: { 'Content-Type': 'application/json' } })
}

function authorizations(): (string | null)[] {
  return apiFetchMock.mock.calls.map(([request]) => request.headers.get('Authorization'))
}
