import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { empty, json, problem, routeApi } from './support/api'
import { signInAsStub } from './support/session'

type Claims = Record<string, string>

interface PasswordChangeRequest {
  currentPassword: string
  newPassword: string
}

interface ProfileStub {
  userinfoRequests: number
  confirmed: string[]
  resends: number
  passwords: PasswordChangeRequest[]
}

const claims: Claims = {
  name: 'Пётр',
  email: 'peter@money.ru',
  email_verified: 'false',
}

const FIRST_RENDER_TIMEOUT = 30_000

const code = '[data-testid="profile-confirm-code"] input'
const currentPassword = '[data-testid="profile-current-password"] input'
const newPassword = '[data-testid="profile-new-password"] input'

async function stubApi(page: Page, initial: Claims = claims): Promise<ProfileStub> {
  const state: Claims = { ...initial }
  const stub: ProfileStub = { userinfoRequests: 0, confirmed: [], resends: 0, passwords: [] }

  await routeApi(page, {
    '/connect/userinfo': async ({ route }) => {
      stub.userinfoRequests += 1

      await json(route, state)
    },
    '/Accounts': async ({ route, path }) => {
      if (path === '/Accounts/ConfirmEmail') {
        const body = route.request().postDataJSON() as { confirmCode: string }

        stub.confirmed.push(body.confirmCode)
        state.email_verified = 'true'
        await empty(route)
        return
      }

      if (path === '/Accounts/ResendConfirmCode') {
        stub.resends += 1
        await empty(route)
        return
      }

      if (path === '/Accounts/ChangePassword') {
        stub.passwords.push(route.request().postDataJSON() as PasswordChangeRequest)
        await empty(route)
        return
      }

      await route.fallback()
    },
  })

  return stub
}

async function openProfile(page: Page): Promise<void> {
  await page.goto('/profile')
  await expect(page.getByTestId('profile-greeting')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

test.describe('профиль', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('код из поля уходит на подтверждение и прячет карточку', async ({ page }) => {
    const stub = await stubApi(page)

    await openProfile(page)

    await expect(page.getByTestId('profile-greeting')).toContainText('Добро пожаловать, Пётр!')
    await expect(page.getByTestId('profile-email-status')).toHaveText('Email не подтверждён')

    await page.locator(code).fill('123456')
    await page.getByTestId('profile-confirm-submit').click()

    await expect(page.getByTestId('profile-notice')).toContainText('Почта успешно подтверждена!')
    expect(stub.confirmed).toEqual(['123456'])

    await expect(page.getByTestId('profile-confirm-card')).toHaveCount(0)
    await expect(page.getByTestId('profile-email-status')).toHaveText('Email подтверждён: peter@money.ru')
  })

  test('подтверждение переживает перезагрузку страницы', async ({ page }) => {
    await stubApi(page)

    await openProfile(page)

    await page.locator(code).fill('123456')
    await page.getByTestId('profile-confirm-submit').click()

    await expect(page.getByTestId('profile-confirm-card')).toHaveCount(0)

    await page.reload()

    await expect(page.getByTestId('profile-email-status')).toHaveText('Email подтверждён: peter@money.ru', {
      timeout: FIRST_RENDER_TIMEOUT,
    })
    await expect(page.getByTestId('profile-confirm-card')).toHaveCount(0)
  })

  test('код не из шести символов показывает сообщение и не уходит на сервер', async ({ page }) => {
    const stub = await stubApi(page)

    await openProfile(page)

    await page.getByTestId('profile-confirm-submit').click()

    await expect(page.getByTestId('profile-confirm-card')).toContainText('Пожалуйста, введите код из письма')

    await page.locator(code).fill('123')
    await page.getByTestId('profile-confirm-submit').click()

    await expect(page.getByTestId('profile-confirm-card')).toContainText('Код должен содержать 6 символов')
    expect(stub.confirmed).toHaveLength(0)
  })

  test('оба пароля уходят на сервер, успех очищает поля', async ({ page }) => {
    const stub = await stubApi(page)

    await openProfile(page)

    await page.locator(currentPassword).fill('старый')
    await page.locator(newPassword).fill('новый')
    await page.getByTestId('profile-password-submit').click()

    await expect(page.getByTestId('profile-notice')).toContainText('Пароль успешно поменян!')
    expect(stub.passwords).toEqual([{ currentPassword: 'старый', newPassword: 'новый' }])

    await expect(page.locator(currentPassword)).toHaveValue('')
    await expect(page.locator(newPassword)).toHaveValue('')
  })

  test('пустые поля пароля отклоняют отправку с сообщением', async ({ page }) => {
    const stub = await stubApi(page)

    await openProfile(page)

    await page.locator(currentPassword).fill('старый')
    await page.getByTestId('profile-password-submit').click()

    await expect(page.getByTestId('profile-password-card').getByText('Обязательно')).toHaveCount(1)
    expect(stub.passwords).toHaveLength(0)

    await page.locator(newPassword).fill('новый')
    await page.getByTestId('profile-password-submit').click()

    await expect.poll(() => stub.passwords).toHaveLength(1)
  })

  test('переключатель видимости раскрывает оба поля пароля', async ({ page }) => {
    await stubApi(page)

    await openProfile(page)

    await expect(page.locator(currentPassword)).toHaveAttribute('type', 'password')
    await expect(page.locator(newPassword)).toHaveAttribute('type', 'password')

    await page.getByTestId('profile-password-visibility').click()

    await expect(page.locator(currentPassword)).toHaveAttribute('type', 'text')
    await expect(page.locator(newPassword)).toHaveAttribute('type', 'text')

    await page.getByTestId('profile-new-password-visibility').click()

    await expect(page.locator(currentPassword)).toHaveAttribute('type', 'password')
    await expect(page.locator(newPassword)).toHaveAttribute('type', 'password')
  })

  test('пустой ответ userinfo рисует приветствие Anonymous без адреса', async ({ page }) => {
    await stubApi(page, {})

    await openProfile(page)

    await expect(page.getByTestId('profile-greeting')).toContainText('Добро пожаловать, Anonymous!')
    await expect(page.getByTestId('profile-email-status')).toHaveText('Email не подтверждён')
    await expect(page.getByTestId('profile-confirm-card')).toBeVisible()
  })

  test('отказ загрузки профиля показывает сообщение сервера вместо карточек', async ({ page }) => {
    await stubApi(page)

    await routeApi(page, {
      '/connect/userinfo': ({ route }) => problem(route, 'Профиль временно недоступен'),
    })

    await page.goto('/profile')

    await expect(page.getByTestId('profile-error')).toContainText('Профиль временно недоступен', {
      timeout: FIRST_RENDER_TIMEOUT,
    })
    await expect(page.getByTestId('profile-greeting')).toHaveCount(0)
    await expect(page.getByTestId('profile-password-card')).toHaveCount(0)
  })
})
