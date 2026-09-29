import { describe, expect, it } from 'vitest'
import { OFFLINE_COPY_HEADER } from '../src/platform/offline-caches'
import { offlineCopy } from '../src/service-worker/offline-copy'

type Plugin = typeof offlineCopy

function reviveAsInServiceWorker(plugin: Plugin): Plugin {
  const revive = <T>(hook: T): T => new Function(`return (${String(hook)})`)() as T

  return {
    fetchDidSucceed: revive(plugin.fetchDidSucceed),
    handlerDidError: revive(plugin.handlerDidError),
    cachedResponseWillBeUsed: revive(plugin.cachedResponseWillBeUsed),
  }
}

const plugin = reviveAsInServiceWorker(offlineCopy)

describe('метка сохранённой копии ответа API', () => {
  it('переживает перенос в воркер и помечает копию заголовком, который читает приложение', async () => {
    const cached = new Response('[]', { status: 200, headers: { 'Content-Type': 'application/json' } })

    const marked = await plugin.cachedResponseWillBeUsed({ cachedResponse: cached })

    expect(marked?.headers.get(OFFLINE_COPY_HEADER)).toBe('1')
    expect(marked?.headers.get('Content-Type')).toBe('application/json')
    expect(await marked?.text()).toBe('[]')
  })

  it('отсутствие копии в кэше пропускает как есть', async () => {
    expect(await plugin.cachedResponseWillBeUsed({ cachedResponse: null })).toBeNull()
  })
})

describe('ответ сервера с ошибкой', () => {
  it.each([500, 502, 503, 504])('%i уводит к копии из кэша, а без копии отдаётся как есть', async status => {
    const state = {}
    const response = new Response('сбой', { status })

    await expect(plugin.fetchDidSucceed({ response, state })).rejects.toThrow(String(status))
    expect(await plugin.handlerDidError({ state })).toBe(response)
  })

  it.each([200, 204, 401, 403, 404])('%i отдаётся как есть, без копии из кэша', async status => {
    const state = {}
    const response = new Response(null, { status })

    expect(await plugin.fetchDidSucceed({ response, state })).toBe(response)
    expect(await plugin.handlerDidError({ state })).toBeUndefined()
  })

  it('обрыв сети без копии в кэше подменять нечем', async () => {
    expect(await plugin.handlerDidError({ state: {} })).toBeUndefined()
  })
})
