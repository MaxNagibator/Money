import { describe, expect, it } from 'vitest'
import { OFFLINE_COPY_HEADER } from '../src/platform/offline-caches'
import { offlineCopy } from '../src/service-worker/offline-copy'

type Plugin = typeof offlineCopy

function reviveAsInServiceWorker(plugin: Plugin): Plugin {
  const revive = <T>(hook: T): T => new Function(`return (${String(hook)})`)() as T

  return {
    cacheKeyWillBeUsed: revive(plugin.cacheKeyWillBeUsed),
    fetchDidSucceed: revive(plugin.fetchDidSucceed),
    handlerDidError: revive(plugin.handlerDidError),
    cachedResponseWillBeUsed: revive(plugin.cachedResponseWillBeUsed),
  }
}

const plugin = reviveAsInServiceWorker(offlineCopy)

function makeJwt(sub: string, jti: string): string {
  const header = btoa(JSON.stringify({ alg: 'RS256', typ: 'JWT' }))
  const payload = btoa(JSON.stringify({ sub, jti }))

  return `${header}.${payload}.sig`
}

describe('изоляция кэша API между пользователями', () => {
  it('разводит ключи кэша для разных пользователей по одному URL', async () => {
    const first = new Request('https://money.test/api/Operations', {
      headers: { Authorization: `Bearer ${makeJwt('user-1', '1')}` },
    })
    const second = new Request('https://money.test/api/Operations', {
      headers: { Authorization: `Bearer ${makeJwt('user-2', '1')}` },
    })

    const firstKey = await plugin.cacheKeyWillBeUsed({ request: first })
    const secondKey = await plugin.cacheKeyWillBeUsed({ request: second })

    expect(firstKey).not.toBe(secondKey)
  })

  it('сохраняет ключ кэша при обновлении JWT того же пользователя', async () => {
    const initial = new Request('https://money.test/api/Operations', {
      headers: { Authorization: `Bearer ${makeJwt('user-1', '1')}` },
    })
    const refreshed = new Request('https://money.test/api/Operations', {
      headers: { Authorization: `Bearer ${makeJwt('user-1', '2')}` },
    })

    const initialKey = await plugin.cacheKeyWillBeUsed({ request: initial })
    const refreshedKey = await plugin.cacheKeyWillBeUsed({ request: refreshed })

    expect(initialKey).toBe(refreshedKey)
  })
})

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
