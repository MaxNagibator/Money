import { ApiError } from '@/api/adapters/problem'
import { apiFetch } from '@/platform/http'
import { rewriteWireJson } from './wire-json'

const TIMEOUT_MESSAGE = 'Сервер не ответил вовремя'
const JSON_CONTENT_TYPE = /^application\/(?:[\w.+-]+\+)?json\b/i
const BODYLESS_STATUSES: ReadonlySet<number> = new Set([204, 205, 304])

export async function transportFetch(request: Request): Promise<Response> {
  try {
    return await preserveWireScale(await apiFetch(request))
  } catch (error) {
    if (error instanceof DOMException && error.name === 'TimeoutError') {
      throw new ApiError(TIMEOUT_MESSAGE)
    }

    throw error
  }
}

async function preserveWireScale(response: Response): Promise<Response> {
  if (response.body === null || BODYLESS_STATUSES.has(response.status)) {
    return response
  }

  if (!JSON_CONTENT_TYPE.test(response.headers.get('Content-Type') ?? '')) {
    return response
  }

  const text = await response.text()
  const headers = new Headers(response.headers)

  headers.delete('Content-Length')

  return new Response(rewriteWireJson(text) ?? text, {
    status: response.status,
    statusText: response.statusText,
    headers,
  })
}
