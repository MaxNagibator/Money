import { API_TIMEOUT_MS } from './http'

const CONFIG_URL = '/appsettings.json'

const JSON_CONTENT_TYPE = 'application/json'

export function readApiEndpoint(config: unknown): string | null {
  if (typeof config !== 'object' || config === null) {
    return null
  }

  const services = (config as { Services?: unknown }).Services

  if (typeof services !== 'object' || services === null) {
    return null
  }

  const api = (services as { api?: unknown }).api

  if (typeof api !== 'object' || api === null) {
    return null
  }

  const addresses = (api as { https?: unknown }).https

  if (!Array.isArray(addresses)) {
    return null
  }

  const endpoint = (addresses as unknown[]).find(
    (address): address is string => typeof address === 'string' && address.trim() !== '',
  )

  return endpoint === undefined ? null : normalizeEndpoint(endpoint.trim())
}

export function normalizeEndpoint(endpoint: string): string {
  return /^https?:\/\//i.test(endpoint) ? endpoint : `https://${endpoint}`
}

export async function loadApiBaseUrl(): Promise<string | null> {
  try {
    const response = await fetch(CONFIG_URL, { cache: 'no-cache', signal: AbortSignal.timeout(API_TIMEOUT_MS) })

    if (!response.ok || !(response.headers.get('content-type') ?? '').includes(JSON_CONTENT_TYPE)) {
      return null
    }

    return readApiEndpoint(await response.json())
  }
  catch {
    return null
  }
}
