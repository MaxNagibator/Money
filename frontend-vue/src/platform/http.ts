import { OFFLINE_COPY_HEADER } from './offline-caches'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api'
export const API_TIMEOUT_MS = 100_000

export const OFFLINE_WRITE_MESSAGE = 'Нет сети, данные не отправлены. Повторите, когда связь появится'
export const LOST_WRITE_MESSAGE = 'Связь с сервером прервалась, данные могли не сохраниться. Проверьте, когда связь появится'
export const LOST_READ_MESSAGE = 'Нет связи с сервером. Проверьте подключение к интернету'

const READ_METHODS: ReadonlySet<string> = new Set(['GET', 'HEAD'])

export class NetworkError extends Error {
}

let reachabilityHandler: (reachable: boolean) => void = () => undefined

export function isOffline(): boolean {
  return typeof navigator !== 'undefined' && !navigator.onLine
}

export function setReachabilityHandler(handler: (reachable: boolean) => void): void {
  reachabilityHandler = handler
}

let targetBaseUrl: string | null = null
let pendingBaseUrl: Promise<void> | null = null

export function setApiBaseUrl(baseUrl: string | null): void {
  pendingBaseUrl = null
  targetBaseUrl = baseUrl
}

export function setApiBaseUrlSource(source: Promise<string | null>): void {
  const pending = source.then(baseUrl => {
    if (pendingBaseUrl === pending) {
      targetBaseUrl = baseUrl
      pendingBaseUrl = null
    }
  })

  pendingBaseUrl = pending
}

export async function resolveApiBaseUrl(): Promise<string> {
  if (pendingBaseUrl !== null) {
    await pendingBaseUrl
  }

  return targetBaseUrl ?? API_BASE_URL
}

export async function apiFetch(input: Request): Promise<Response> {
  const isRead = READ_METHODS.has(input.method.toUpperCase())

  if (!isRead && isOffline()) {
    throw new NetworkError(OFFLINE_WRITE_MESSAGE)
  }

  const request = await retarget(input)

  try {
    const response = await fetch(request, {
      credentials: 'include',
      signal: AbortSignal.any([input.signal, AbortSignal.timeout(API_TIMEOUT_MS)]),
    })

    reachabilityHandler(response.headers.get(OFFLINE_COPY_HEADER) === null)

    return response
  } catch (error) {
    if (error instanceof TypeError || (error instanceof DOMException && error.name === 'TimeoutError')) {
      reachabilityHandler(false)

      throw new NetworkError(isRead ? LOST_READ_MESSAGE : LOST_WRITE_MESSAGE, { cause: error })
    }

    throw error
  }
}

export function retargetUrl(url: string, from: string, to: string): string {
  const source = new URL(url)
  const origin = new URL(from, source.origin)
  const target = new URL(to, source.origin)
  const prefix = trimSlash(origin.pathname)

  if (source.origin !== origin.origin || !source.pathname.startsWith(prefix)) {
    return url
  }

  source.protocol = target.protocol
  source.host = target.host
  source.pathname = `${trimSlash(target.pathname)}${source.pathname.slice(prefix.length)}`

  return source.toString()
}

async function retarget(request: Request): Promise<Request> {
  if (pendingBaseUrl !== null) {
    await pendingBaseUrl
  }

  if (targetBaseUrl === null || targetBaseUrl === API_BASE_URL) {
    return request
  }

  const url = retargetUrl(request.url, API_BASE_URL, targetBaseUrl)

  if (url === request.url) {
    return request
  }

  return new Request(url, {
    method: request.method,
    headers: request.headers,
    body: request.body === null ? null : await request.arrayBuffer(),
    mode: request.mode,
    credentials: request.credentials,
    cache: request.cache,
    redirect: request.redirect,
    referrer: request.referrer,
    integrity: request.integrity,
  })
}

function trimSlash(pathname: string): string {
  return pathname.endsWith('/') ? pathname.slice(0, -1) : pathname
}
