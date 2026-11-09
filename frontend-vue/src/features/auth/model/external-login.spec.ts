import { afterEach, describe, expect, it, vi } from 'vitest'
import { setApiBaseUrl, setApiBaseUrlSource } from '@/platform/http'
import { GITHUB_PROVIDER, startExternalLogin } from './external-login'

describe('внешний вход', () => {
  afterEach(() => {
    setApiBaseUrl(null)
    vi.unstubAllGlobals()
  })

  function trackAssign(): string[] {
    const visited: string[] = []

    vi.stubGlobal('location', {
      origin: 'https://money.test',
      assign: (url: string) => visited.push(url),
    })

    return visited
  }

  it('уводит на адрес API из конфигурации, а не на origin статики', async () => {
    const visited = trackAssign()

    setApiBaseUrl('https://moneyapi.test')
    await startExternalLogin(GITHUB_PROVIDER)

    expect(visited[0]).toBe(
      'https://moneyapi.test/external/login/github?returnUrl=https%3A%2F%2Fmoney.test%2FAccount%2FCallback',
    )
  })

  it('ждёт незавершённую загрузку конфигурации', async () => {
    const visited = trackAssign()

    let resolveBaseUrl: (value: string | null) => void = () => {}
    setApiBaseUrlSource(new Promise<string | null>(resolve => {
      resolveBaseUrl = resolve
    }))

    const pending = startExternalLogin(GITHUB_PROVIDER)

    expect(visited).toHaveLength(0)

    resolveBaseUrl('https://moneyapi.test')
    await pending

    expect(visited[0]).toContain('https://moneyapi.test/external/login/github')
  })

  it('остаётся на своём origin, когда конфигурация адреса не дала', async () => {
    const visited = trackAssign()

    setApiBaseUrl(null)
    await startExternalLogin(GITHUB_PROVIDER)

    expect(visited[0]).toBe(
      '/api/external/login/github?returnUrl=https%3A%2F%2Fmoney.test%2FAccount%2FCallback',
    )
  })
})
