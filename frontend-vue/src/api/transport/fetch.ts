import { ApiError } from '@/api/adapters/problem'
import { apiFetch } from '@/platform/http'

const TIMEOUT_MESSAGE = 'Сервер не ответил вовремя'

export async function transportFetch(request: Request): Promise<Response> {
  try {
    return await apiFetch(request)
  } catch (error) {
    if (error instanceof DOMException && error.name === 'TimeoutError') {
      throw new ApiError(TIMEOUT_MESSAGE)
    }

    throw error
  }
}
