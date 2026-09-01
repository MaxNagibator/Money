import { test as base } from '@playwright/test'

export { expect } from '@playwright/test'

const API_PREFIX = '/api'
const UNSTUBBED_STATUS = 502

function isApiUrl(url: URL): boolean {
  return url.pathname === API_PREFIX || url.pathname.startsWith(`${API_PREFIX}/`)
}

export const test = base.extend<{ unstubbedApi: void }>({
  unstubbedApi: [
    async ({ context }, use, testInfo) => {
      await context.route(isApiUrl, async route => {
        const request = route.request()

        console.warn(`[${testInfo.title}] незаглушенный запрос: ${request.method()} ${request.url()}`)

        await route.fulfill({ status: UNSTUBBED_STATUS, body: '' })
      })

      await use()
    },
    { auto: true },
  ],
})
