import createClient from 'openapi-fetch'
import type { paths } from '@/api/generated/schema'
import { API_BASE_URL } from '@/platform/http'
import { transportFetch } from './fetch'

export type AccessTokenProvider = () => Promise<string | null>

export type AccessTokenRefresher = (staleToken: string) => Promise<string | null>

interface AuthorizedAttempt {
  token: string
  replay: Request
}

const UNAUTHORIZED_STATUS = 401

let provideAccessToken: AccessTokenProvider = () => Promise.resolve(null)
let refreshAccessToken: AccessTokenRefresher = () => Promise.resolve(null)

const attempts = new WeakMap<Request, AuthorizedAttempt>()

export function setAccessTokenProvider(provider: AccessTokenProvider): void {
  provideAccessToken = provider
}

export function setAccessTokenRefresher(refresher: AccessTokenRefresher): void {
  refreshAccessToken = refresher
}

export const apiClient = createClient<paths>({
  baseUrl: API_BASE_URL,
  fetch: transportFetch,
})

apiClient.use({
  async onRequest({ request }) {
    const token = await provideAccessToken()

    if (token === null) {
      return request
    }

    request.headers.set('Authorization', `Bearer ${token}`)
    attempts.set(request, { token, replay: request.clone() })

    return request
  },

  async onResponse({ request, response }) {
    const attempt = attempts.get(request)

    if (attempt === undefined || response.status !== UNAUTHORIZED_STATUS) {
      return undefined
    }

    attempts.delete(request)

    const token = await refreshAccessToken(attempt.token)

    if (token === null) {
      return undefined
    }

    attempt.replay.headers.set('Authorization', `Bearer ${token}`)

    // TODO: переигрывается любой метод, включая POST, поэтому 401 от шлюза после выполненного запроса продублирует его; ограничить повтор идемпотентными методами, когда такой 401 появится в логах или когда API заведёт ключи идемпотентности
    return transportFetch(attempt.replay)
  },
})

export type ApiPaths = paths
