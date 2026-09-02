import type { Page, Route } from '@playwright/test'

export interface ApiCall {
  route: Route
  method: string
  path: string
  query: URLSearchParams
}

export type ApiHandler = (call: ApiCall) => Promise<void> | void

export type ApiHandlers = Record<string, ApiHandler>

const API_PREFIX = '/api'
const WIRE_LITERAL = Symbol('wireNumber')
const PLACEHOLDER_PREFIX = 'wire-number:'
const SENT_SUM_PATTERN = /"sum":(-?\d+(?:\.\d+)?)/

function serialize(body: unknown): string {
  const literals = new Map<string, string>()

  let text = JSON.stringify(body, (_key, value: unknown) => {
    if (typeof value !== 'object' || value === null || !(WIRE_LITERAL in value)) {
      return value
    }

    const placeholder = `${PLACEHOLDER_PREFIX}${literals.size}`

    literals.set(placeholder, (value as Record<symbol, string>)[WIRE_LITERAL]!)

    return placeholder
  })

  for (const [placeholder, literal] of literals) {
    text = text.replace(`"${placeholder}"`, literal)
  }

  return text
}

export function apiPath(url: URL): string {
  const path = url.pathname

  if (path === API_PREFIX) {
    return '/'
  }

  return path.startsWith(`${API_PREFIX}/`) ? path.slice(API_PREFIX.length) : path
}

export function coversPath(key: string, path: string): boolean {
  return path === key || path.startsWith(`${key}/`)
}

export async function routeApi(page: Page, handlers: ApiHandlers): Promise<void> {
  for (const [key, handler] of Object.entries(handlers)) {
    await page.route(url => coversPath(key, apiPath(url)), async route => {
      if (route.request().isNavigationRequest()) {
        await route.fallback()
        return
      }

      const url = new URL(route.request().url())

      await handler({
        route,
        method: route.request().method(),
        path: apiPath(url),
        query: url.searchParams,
      })
    })
  }
}

export function wireNumber(literal: string): number {
  return { [WIRE_LITERAL]: literal } as unknown as number
}

export function sentSum(raw: string | null): number | null {
  const literal = raw === null ? null : SENT_SUM_PATTERN.exec(raw)?.[1]

  return literal === undefined || literal === null ? null : wireNumber(literal)
}

export function json(route: Route, body: unknown): Promise<void> {
  return route.fulfill({ status: 200, contentType: 'application/json', body: serialize(body) })
}

export function created(route: Route, id: number): Promise<void> {
  return route.fulfill({ status: 201, contentType: 'application/json', body: String(id) })
}

export function empty(route: Route): Promise<void> {
  return route.fulfill({ status: 200, body: '' })
}

export function problem(route: Route, title: string, status: number = 500): Promise<void> {
  return route.fulfill({ status, contentType: 'application/problem+json', body: JSON.stringify({ title }) })
}

export function identifiers(path: string): number[] {
  return path.split('/').filter(part => /^\d+$/.test(part)).map(Number)
}

export function identifier(path: string, index: number = -1): number {
  const found = optionalIdentifier(path, index)

  if (found === null) {
    throw new Error(`В пути ${path} нет идентификатора №${index}`)
  }

  return found
}

export function optionalIdentifier(path: string, index: number = -1): number | null {
  return identifiers(path).at(index) ?? null
}
