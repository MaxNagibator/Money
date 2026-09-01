import { systemClock } from '@/platform/clock'
import { transportFetch } from '@/api/transport/fetch'
import { API_BASE_URL, resolveApiBaseUrl } from '@/platform/http'

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresAt: number
}

interface TokenSuccess {
  access_token: string
  refresh_token: string
  expires_in: number
}

interface TokenFailure {
  error_description?: string
  title?: string
}

const TOKEN_URL = `${API_BASE_URL}/connect/token`
const FORM_CONTENT_TYPE = 'application/x-www-form-urlencoded'

export class AuthError extends Error {
}

async function exchange(form: URLSearchParams, accessToken: string | null): Promise<AuthTokens> {
  const headers = new Headers({ 'Content-Type': FORM_CONTENT_TYPE })

  if (accessToken !== null) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const response = await transportFetch(new Request(TOKEN_URL, {
    method: 'POST',
    headers,
    body: form,
  }))

  const payload: unknown = await response.json().catch(() => null)

  if (!response.ok) {
    const failure = payload as TokenFailure | null

    throw new AuthError(failure?.error_description ?? failure?.title ?? 'Не удалось получить данные авторизации')
  }

  const data = payload as TokenSuccess

  return {
    accessToken: data.access_token,
    refreshToken: data.refresh_token,
    expiresAt: systemClock.now().getTime() + data.expires_in * 1000,
  }
}

export function requestPasswordGrant(login: string, password: string): Promise<AuthTokens> {
  return exchange(new URLSearchParams({
    grant_type: 'password',
    username: login,
    password,
  }), null)
}

export function requestExternalGrant(): Promise<AuthTokens> {
  return exchange(new URLSearchParams({ grant_type: 'external' }), null)
}

export async function externalAuthUrl(provider: string, returnUrl: string): Promise<string> {
  const baseUrl = await resolveApiBaseUrl()

  return `${baseUrl}/external/login/${provider}?returnUrl=${encodeURIComponent(returnUrl)}`
}

export function requestRefreshGrant(tokens: AuthTokens): Promise<AuthTokens> {
  return exchange(new URLSearchParams({
    grant_type: 'refresh_token',
    refresh_token: tokens.refreshToken,
  }), tokens.accessToken)
}

export async function revokeRefreshToken(tokens: AuthTokens): Promise<void> {
  const form = new URLSearchParams({
    token: tokens.refreshToken,
    token_type_hint: 'refresh_token',
  })

  await transportFetch(new Request(`${API_BASE_URL}/connect/revoke`, {
    method: 'POST',
    headers: new Headers({
      'Content-Type': FORM_CONTENT_TYPE,
      'Authorization': `Bearer ${tokens.accessToken}`,
    }),
    body: form,
  }))
}
