import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  apiFetch,
  LOST_READ_MESSAGE,
  LOST_WRITE_MESSAGE,
  NetworkError,
  OFFLINE_WRITE_MESSAGE,
  setApiBaseUrl,
  setReachabilityHandler,
} from './http'
import { OFFLINE_COPY_HEADER } from './offline-caches'

afterEach(() => {
  vi.unstubAllGlobals()
  setApiBaseUrl(null)
  setReachabilityHandler(() => undefined)
})

function setOnline(online: boolean): void {
  vi.spyOn(navigator, 'onLine', 'get').mockReturnValue(online)
}

function request(method: string): Request {
  return new Request('https://money.test/api/Operations', { method })
}

describe('apiFetch без сети', () => {
  it.each(['POST', 'PUT', 'PATCH', 'DELETE'])('запись %s отклоняется до запроса понятным сообщением', async method => {
    const send = vi.fn()

    vi.stubGlobal('fetch', send)
    setOnline(false)

    const failure = apiFetch(request(method))

    await expect(failure).rejects.toBeInstanceOf(NetworkError)
    await expect(failure).rejects.toThrow(OFFLINE_WRITE_MESSAGE)
    expect(send).not.toHaveBeenCalled()
  })

  it('чтение уходит в сеть, чтобы воркер мог ответить сохранённым', async () => {
    const response = new Response('[]')

    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(response)))
    setOnline(false)

    expect(await apiFetch(request('GET'))).toBe(response)
  })

  it.each([
    ['GET', 'обрыв', LOST_READ_MESSAGE, new TypeError('Failed to fetch')],
    ['POST', 'обрыв', LOST_WRITE_MESSAGE, new TypeError('Failed to fetch')],
    ['POST', 'таймаут', LOST_WRITE_MESSAGE, new DOMException('signal timed out', 'TimeoutError')],
  ])('%s: %s сети переводится в сообщение для человека', async (method, _case, message, cause) => {

    vi.stubGlobal('fetch', vi.fn(() => Promise.reject(cause)))
    setOnline(true)

    const failure = apiFetch(request(method))

    await expect(failure).rejects.toThrow(message)
    await expect(failure).rejects.toMatchObject({ cause })
  })

  it.each([
    ['ответ сервера', new Response('[]'), true],
    ['копия из офлайн-кэша воркера', new Response('[]', { headers: { [OFFLINE_COPY_HEADER]: '1' } }), false],
  ])('%s сообщает, доступен ли сервер', async (_case, response, reachable) => {
    const handler = vi.fn()

    setReachabilityHandler(handler)
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(response)))
    setOnline(true)

    await apiFetch(request('GET'))

    expect(handler.mock.calls).toEqual([[reachable]])
  })

  it('обрыв сети сообщает о недоступном сервере', async () => {
    const handler = vi.fn()

    setReachabilityHandler(handler)
    vi.stubGlobal('fetch', vi.fn(() => Promise.reject(new TypeError('Failed to fetch'))))
    setOnline(true)

    await expect(apiFetch(request('GET'))).rejects.toBeInstanceOf(NetworkError)
    expect(handler.mock.calls).toEqual([[false]])
  })

  it('отмену запроса не выдаёт за обрыв сети', async () => {
    const aborted = new DOMException('отменено', 'AbortError')

    vi.stubGlobal('fetch', vi.fn(() => Promise.reject(aborted)))
    setOnline(true)

    await expect(apiFetch(request('GET'))).rejects.toBe(aborted)
  })
})
