import { afterAll, afterEach } from 'vitest'

const leaked: string[] = []

function describeRequest(input: RequestInfo | URL, init?: RequestInit): string {
  if (input instanceof Request) {
    return `${(init?.method ?? input.method).toUpperCase()} ${input.url}`
  }

  const url = input instanceof URL ? input.href : input
  return `${(init?.method ?? 'GET').toUpperCase()} ${url}`
}

globalThis.fetch = (input: RequestInfo | URL, init?: RequestInit) => {
  const request = describeRequest(input, init)
  leaked.push(request)
  return Promise.reject(new TypeError(`Незаглушенный сетевой запрос в модульном тесте: ${request}`))
}

function assertNoLeaks(): void {
  if (leaked.length === 0) {
    return
  }

  const requests = leaked.splice(0).join('\n  ')
  throw new Error(`Тест ушёл в сеть мимо заглушки, заглушите fetch или вызывающую функцию:\n  ${requests}`)
}

afterEach(assertNoLeaks)
afterAll(assertNoLeaks)
