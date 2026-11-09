import { expect, test } from '@playwright/test'
import {
  newCredentials,
  probeCors,
  readTokens,
  registerLiveAccount,
  requestLiveToken,
  seedLiveSession,
} from './support/live-api'

const apiUrl = (process.env.API_URL ?? 'http://127.0.0.1:5000').replace(/\/+$/, '')

test('живой стенд отвечает анониму и под сессией', async ({ page }, testInfo) => {
  test.setTimeout(180_000)

  const consoleErrors: string[] = []
  const badResponses: string[] = []
  const apiFailures: string[] = []

  page.on('console', message => {
    if (message.type() === 'error') {
      consoleErrors.push(message.text())
    }
  })

  page.on('pageerror', error => {
    consoleErrors.push(error.message)
  })

  page.on('response', response => {
    const url = response.url()
    const status = response.status()

    if (status >= 400) {
      badResponses.push(`${status} ${url}`)
    }

    if (status >= 500 && url.startsWith(apiUrl)) {
      apiFailures.push(`${status} ${url}`)
    }
  })

  const credentials = newCredentials()

  await test.step('аноним видит главную и достаёт конфигурацию входа', async () => {
    await page.goto('/')

    await expect(page.getByTestId('home-history-toggle')).toBeVisible()
    await expect(page.getByTestId('brand-home')).toBeVisible()

    const probe = await probeCors(page, `${apiUrl}/.well-known/openid-configuration`)

    expect(probe, 'запрос к API из страницы').toEqual({ ok: true, status: 200, error: null })

    await page.screenshot({ path: testInfo.outputPath('anonymous.png'), fullPage: true })
  })

  const tokens = await test.step('регистрация и вход по паролю', async () => {
    const registered = await registerLiveAccount(page, apiUrl, credentials)

    expect(registered.status, `ответ регистрации: ${registered.body.slice(0, 300)}`).toBe(204)

    const granted = await requestLiveToken(page, apiUrl, credentials)

    expect(granted.status, `ответ выдачи токена: ${granted.body.slice(0, 300)}`).toBe(200)

    const parsed = readTokens(granted.body, Date.now())

    if (parsed === null) {
      throw new Error(`Токен не разобран, тело ответа: ${granted.body.slice(0, 300)}`)
    }

    return parsed
  })

  await test.step('под сессией открывается список категорий', async () => {
    await seedLiveSession(page, tokens)

    const categories = page.waitForResponse(
      response => response.url().startsWith(`${apiUrl}/Categories`) && response.request().method() === 'GET',
      { timeout: 60_000 },
    )

    await page.goto('/categories')

    const response = await categories

    expect(response.status(), 'GET /Categories под живым токеном').toBe(200)

    await expect(page.getByTestId('category-add-root')).toBeVisible()
    await expect(page.getByTestId('categories-error')).toHaveCount(0)

    await page.screenshot({ path: testInfo.outputPath('authenticated.png'), fullPage: true })
  })

  expect(consoleErrors, 'ошибки в консоли браузера').toEqual([])
  expect(badResponses, 'ответы со статусом 400 и выше').toEqual([])
  expect(apiFailures, 'ответы API со статусом 500 и выше').toEqual([])
})
