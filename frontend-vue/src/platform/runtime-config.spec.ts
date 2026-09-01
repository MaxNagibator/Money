import { afterEach, describe, expect, it, vi } from 'vitest'
import { apiFetch, retargetUrl, setApiBaseUrl, setApiBaseUrlSource } from './http'
import { normalizeEndpoint, readApiEndpoint } from './runtime-config'

describe('readApiEndpoint', () => {
  it('берёт первый адрес API из файла конфигурации рядом со статикой', () => {
    expect(readApiEndpoint({ Services: { api: { https: ['moneyapi.keep2space.ru'] } } }))
      .toBe('https://moneyapi.keep2space.ru')
  })

  it('оставляет адрес со схемой как есть', () => {
    expect(readApiEndpoint({ Services: { api: { https: ['http://localhost:5249'] } } }))
      .toBe('http://localhost:5249')
  })

  it('пропускает пустые адреса', () => {
    expect(readApiEndpoint({ Services: { api: { https: ['  ', 'localhost:7124'] } } }))
      .toBe('https://localhost:7124')
  })

  it.each([
    [null],
    [{}],
    [{ Services: {} }],
    [{ Services: { api: {} } }],
    [{ Services: { api: { https: [] } } }],
    [{ Services: { api: { https: [42] } } }],
  ])('возвращает null на конфигурации без адреса: %j', config => {
    expect(readApiEndpoint(config)).toBeNull()
  })

  it('дописывает https к адресу без схемы', () => {
    expect(normalizeEndpoint('moneyapi.keep2space.ru')).toBe('https://moneyapi.keep2space.ru')
  })
})

describe('retargetUrl', () => {
  it.each([
    ['https://money.test/api/Categories', 'https://moneyapi.test/Categories'],
    ['https://money.test/api/Operations/1?from=2026-02-01', 'https://moneyapi.test/Operations/1?from=2026-02-01'],
  ])('переносит %s на адрес из конфигурации', (url, expected) => {
    expect(retargetUrl(url, '/api', 'https://moneyapi.test')).toBe(expected)
  })

  it('сохраняет префикс пути целевого адреса', () => {
    expect(retargetUrl('https://money.test/api/Cars', '/api', 'https://gateway.test/money/'))
      .toBe('https://gateway.test/money/Cars')
  })

  it('не трогает адрес вне префикса', () => {
    expect(retargetUrl('https://money.test/appsettings.json', '/api', 'https://moneyapi.test'))
      .toBe('https://money.test/appsettings.json')
  })
})

describe('apiFetch', () => {
  afterEach(() => {
    setApiBaseUrl(null)
    vi.unstubAllGlobals()
  })

  it('переносит тело запроса на новый адрес готовым буфером, а не потоком', async () => {
    const sent: Request[] = []
    vi.stubGlobal('fetch', (request: Request) => {
      sent.push(request)
      return Promise.resolve(new Response(null, { status: 204 }))
    })

    setApiBaseUrl('http://127.0.0.1:5252')

    const source = new Request('http://money.test/api/connect/token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: 'grant_type=password',
    })

    await apiFetch(source)

    expect(sent[0]?.url).toBe('http://127.0.0.1:5252/connect/token')
    expect(await sent[0]?.text()).toBe('grant_type=password')
    expect(source.bodyUsed).toBe(true)
  })

  it('ждёт незавершённую загрузку конфигурации, а не уходит на прежний адрес', async () => {
    const sent: Request[] = []
    vi.stubGlobal('fetch', (request: Request) => {
      sent.push(request)
      return Promise.resolve(new Response(null, { status: 204 }))
    })

    let resolveBaseUrl: (value: string | null) => void = () => {}
    setApiBaseUrlSource(new Promise<string | null>(resolve => {
      resolveBaseUrl = resolve
    }))

    const pending = apiFetch(new Request('http://money.test/api/Categories'))

    resolveBaseUrl('http://127.0.0.1:5252')
    await pending

    expect(sent[0]?.url).toBe('http://127.0.0.1:5252/Categories')
  })
})
