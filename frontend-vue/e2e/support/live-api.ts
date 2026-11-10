import type { Page } from '@playwright/test'

export interface LiveCredentials {
  userName: string
  password: string
}

export interface LiveTokens {
  accessToken: string
  refreshToken: string
  expiresAt: number
}

export interface WireResponse {
  status: number
  body: string
}

export interface CorsProbe {
  ok: boolean
  status: number
  error: string | null
}

interface TokenPayload {
  access_token?: string
  refresh_token?: string
  expires_in?: number
}

export function newCredentials(): LiveCredentials {
  const suffix = crypto.randomUUID().slice(0, 6)

  return { userName: `smoke${Date.now()}${suffix}`, password: 'SmokePass123!' }
}

export function probeCors(page: Page, url: string): Promise<CorsProbe> {
  return page.evaluate(async (target: string) => {
    try {
      const response = await fetch(target, { mode: 'cors' })

      return { ok: response.ok, status: response.status, error: null }
    }
    catch (error) {
      return { ok: false, status: 0, error: error instanceof Error ? error.message : 'запрос не выполнен' }
    }
  }, url)
}

export function registerLiveAccount(page: Page, apiUrl: string, credentials: LiveCredentials): Promise<WireResponse> {
  return page.evaluate(async (input: { api: string } & LiveCredentials) => {
    const response = await fetch(`${input.api}/Accounts/Register`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ userName: input.userName, email: null, password: input.password }),
    })

    return { status: response.status, body: await response.text() }
  }, { api: apiUrl, ...credentials })
}

export function requestLiveToken(page: Page, apiUrl: string, credentials: LiveCredentials): Promise<WireResponse> {
  return page.evaluate(async (input: { api: string } & LiveCredentials) => {
    const response = await fetch(`${input.api}/connect/token`, {
      method: 'POST',
      headers: { 'content-type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({
        grant_type: 'password',
        username: input.userName,
        password: input.password,
      }).toString(),
    })

    return { status: response.status, body: await response.text() }
  }, { api: apiUrl, ...credentials })
}

export function readTokens(body: string, now: number): LiveTokens | null {
  let payload: TokenPayload

  try {
    payload = JSON.parse(body) as TokenPayload
  }
  catch {
    return null
  }

  if (typeof payload.access_token !== 'string' || typeof payload.expires_in !== 'number') {
    return null
  }

  return {
    accessToken: payload.access_token,
    refreshToken: payload.refresh_token ?? '',
    expiresAt: now + payload.expires_in * 1000,
  }
}

export async function seedLiveSession(page: Page, tokens: LiveTokens): Promise<void> {
  await page.addInitScript((value: LiveTokens) => {
    localStorage.setItem('money:auth-tokens', JSON.stringify(value))
  }, tokens)
}
