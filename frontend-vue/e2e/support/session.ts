import type { Page } from '@playwright/test'
import { json, routeApi } from './api'
import { STUB_USER_NAME, userInfo } from './fixtures'

export { STUB_USER_NAME }

export const SESSION_HOUR = 3_600_000

export const SESSION_ABOUT_TO_REFRESH = 20_000

export async function signInAsStub(page: Page, expiresIn: number = SESSION_HOUR): Promise<void> {
  await page.addInitScript((lifetime: number) => {
    localStorage.setItem('money:auth-tokens', JSON.stringify({
      accessToken: 'access',
      refreshToken: 'refresh',
      expiresAt: Date.now() + lifetime,
    }))
  }, expiresIn)

  await routeApi(page, {
    '/connect/userinfo': ({ route }) => json(route, userInfo),
  })
}
