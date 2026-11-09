type PluginState = Record<string, unknown>

interface OfflineCopyPlugin {
  cacheKeyWillBeUsed: (param: { request: Request }) => Promise<Request | string>
  fetchDidSucceed: (param: { response: Response, state?: PluginState }) => Promise<Response>
  handlerDidError: (param: { state?: PluginState }) => Promise<Response | undefined>
  cachedResponseWillBeUsed: (param: { cachedResponse?: Response | null }) => Promise<Response | null>
}

export const offlineCopy: OfflineCopyPlugin = {
  cacheKeyWillBeUsed: async ({ request }: { request: Request }) => {
    const raw = (request.headers.get('Authorization') ?? '').replace(/^Bearer\s+/i, '').trim()

    if (!raw) {
      return request
    }

    let identity = `tok:${raw}`
    const parts = raw.split('.')

    if (parts.length === 3 && parts[1]) {
      try {
        const padded = parts[1].replace(/-/g, '+').replace(/_/g, '/')
        const json = JSON.parse(atob(padded)) as { sub?: unknown }

        if (typeof json.sub === 'string' && json.sub.length > 0) {
          identity = `sub:${json.sub}`
        }
      } catch {
        identity = `tok:${raw}`
      }
    }

    const bytes = new TextEncoder().encode(identity)
    const digest = await crypto.subtle.digest('SHA-256', bytes)
    const scope = Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, '0')).join('').slice(0, 32)
    const url = new URL(request.url)

    url.searchParams.set('__money_scope', scope)

    return url.toString()
  },
  fetchDidSucceed: ({ response, state }: { response: Response, state?: PluginState }) => {
    if (response.status < 500 || !state) {
      return Promise.resolve(response)
    }

    state.serverError = response

    return Promise.reject(new Error(`Сервер ответил ${String(response.status)}, отдаётся сохранённая копия`))
  },
  handlerDidError: ({ state }: { state?: PluginState }) => {
    const serverError = state?.serverError

    return Promise.resolve(serverError instanceof Response ? serverError : undefined)
  },
  cachedResponseWillBeUsed:({ cachedResponse }: { cachedResponse?: Response | null }) => {
    if (!cachedResponse) {
      return Promise.resolve(cachedResponse ?? null)
    }

    const headers = new Headers(cachedResponse.headers)

    headers.set('X-Money-Offline-Copy', '1')

    return Promise.resolve(new Response(cachedResponse.body, {
      status: cachedResponse.status,
      statusText: cachedResponse.statusText,
      headers,
    }))
  },
}
