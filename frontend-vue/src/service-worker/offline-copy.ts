type PluginState = Record<string, unknown>

interface OfflineCopyPlugin {
  fetchDidSucceed: (param: { response: Response, state?: PluginState }) => Promise<Response>
  handlerDidError: (param: { state?: PluginState }) => Promise<Response | undefined>
  cachedResponseWillBeUsed: (param: { cachedResponse?: Response | null }) => Promise<Response | null>
}

export const offlineCopy: OfflineCopyPlugin = {
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
