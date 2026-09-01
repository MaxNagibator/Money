import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { empty, json, routeApi } from './support/api'
import { userInfo } from './support/fixtures'
import { stubScene } from './support/scene'
import { SESSION_ABOUT_TO_REFRESH, signInAsStub, STUB_USER_NAME } from './support/session'

interface RegisterRequest {
  userName: string
  email: string | null
  password: string
}

const FIRST_RENDER_TIMEOUT = 30_000
const OTHER_USER_NAME = 'пётр'

const registerName = '[data-testid="register-name"] input'
const registerEmail = '[data-testid="register-email"] input'
const registerPassword = '[data-testid="register-password"] input'
const registerConfirm = '[data-testid="register-confirm-password"] input'

async function stubRegisterEndpoint(page: Page, failure: string | null = null): Promise<RegisterRequest[]> {
  const requests: RegisterRequest[] = []

  await routeApi(page, {
    '/Accounts/Register': async ({ route }) => {
      requests.push(route.request().postDataJSON() as RegisterRequest)

      if (failure !== null) {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ title: failure, status: 400 }),
        })

        return
      }

      await route.fulfill({ status: 204, body: '' })
    },
  })

  return requests
}

async function stubTokenEndpoint(page: Page): Promise<void> {
  await routeApi(page, {
    '/connect/token': async ({ route }) => {
      const body = new URLSearchParams(route.request().postData() ?? '')

      if (body.get('password') !== 'верный') {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'invalid_grant', error_description: 'Неверный логин или пароль' }),
        })
        return
      }

      await json(route, {
        access_token: 'access',
        refresh_token: 'refresh',
        token_type: 'Bearer',
        expires_in: 3600,
        scope: 'api',
      })
    },
    '/connect/revoke': ({ route }) => empty(route),
  })
}

test.beforeEach(async ({ page }) => {
  await stubScene(page)
})

test.describe('вход', () => {
  test.beforeEach(async ({ page }) => {
    await stubTokenEndpoint(page)
  })

  test('закрытый раздел уводит на форму входа', async ({ page }) => {
    await page.goto('/debts')

    await expect(page).toHaveURL('/login?returnUrl=/debts')
    await expect(page.getByTestId('login-form')).toBeVisible()
  })

  test('после входа возвращает на запрошенную страницу', async ({ page }) => {
    await page.goto('/debts')

    await page.locator('[data-testid="login-name"] input').fill('пользователь')
    await page.locator('[data-testid="login-password"] input').fill('верный')
    await page.getByTestId('login-submit').click()

    await expect(page).toHaveURL('/debts')
    await expect(page.getByTestId('debts-toggle-forgive')).toBeVisible()
    await expect(page.getByTestId('nav-sign-out')).toBeVisible()
  })

  test('выход уводит на главную, а не на форму входа', async ({ page }) => {
    await page.goto('/debts')

    await page.locator('[data-testid="login-name"] input').fill('пользователь')
    await page.locator('[data-testid="login-password"] input').fill('верный')
    await page.getByTestId('login-submit').click()

    await expect(page.getByTestId('nav-sign-out')).toBeVisible()

    await page.getByTestId('nav-sign-out').click()

    await expect(page).toHaveURL('/')
    await expect(page.getByTestId('nav-sign-out')).toBeHidden()
  })

  test('кнопка демо входит общей учётной записью без ввода полей', async ({ page }) => {
    const grants: URLSearchParams[] = []

    await routeApi(page, {
      '/connect/token': async ({ route }) => {
        grants.push(new URLSearchParams(route.request().postData() ?? ''))

        await json(route, {
          access_token: 'access',
          refresh_token: 'refresh',
          token_type: 'Bearer',
          expires_in: 3600,
          scope: 'api',
        })
      },
    })

    await page.goto('/login')

    await page.getByTestId('login-demo').click()

    await expect(page).toHaveURL('/operations')
    await expect(page.getByTestId('nav-sign-out')).toBeVisible()

    expect(grants).toHaveLength(1)
    expect(grants[0]?.get('username')).toBe('demo')
    expect(grants[0]?.get('password')).toBe('demo1234')
    expect(grants[0]?.get('grant_type')).toBe('password')
  })

  test('неверный пароль показывает ошибку сервера', async ({ page }) => {
    await page.goto('/login')

    await page.locator('[data-testid="login-name"] input').fill('пользователь')
    await page.locator('[data-testid="login-password"] input').fill('неверный')
    await page.getByTestId('login-submit').click()

    await expect(page.getByTestId('login-error')).toHaveText('Ошибка во время входа Неверный логин или пароль')
    await expect(page).toHaveURL('/login')
  })

  test('кнопки внешних сервисов уводят на провайдера с возвратом в Account/Callback', async ({ page }) => {
    await page.route('**/api/external/login/**', route => route.fulfill({
      status: 200,
      contentType: 'text/html',
      body: '<html lang="ru"><body>провайдер</body></html>',
    }))

    await page.goto('/login')
    await page.getByTestId('login-external-github').click()

    await expect(page).toHaveURL(/\/api\/external\/login\/github\?returnUrl=.*%2FAccount%2FCallback$/)

    await page.goto('/login')
    await page.getByTestId('login-external-auth').click()

    await expect(page).toHaveURL(/\/api\/external\/login\/auth\?returnUrl=.*%2FAccount%2FCallback$/)
  })

  test('вход другим пользователем не оставляет данные прежнего', async ({ page }) => {
    await signInAsStub(page)

    let currentName = STUB_USER_NAME

    await routeApi(page, {
      '/Categories': ({ route }) => json(route, []),
      '/Operations': ({ route }) => json(route, []),
      '/connect/userinfo': ({ route }) => json(route, { ...userInfo, name: currentName }),
    })

    await page.goto('/login')

    await expect(page.getByTestId('nav-profile')).toContainText(STUB_USER_NAME, { timeout: FIRST_RENDER_TIMEOUT })

    currentName = OTHER_USER_NAME

    await page.locator('[data-testid="login-name"] input').fill(OTHER_USER_NAME)
    await page.locator('[data-testid="login-password"] input').fill('верный')
    await page.getByTestId('login-submit').click()

    await expect(page).toHaveURL('/operations')
    await expect(page.getByTestId('nav-profile')).toContainText(OTHER_USER_NAME)
    await expect(page.getByTestId('nav-profile')).not.toContainText(STUB_USER_NAME)
  })

  test('протухшая сессия уводит с открытого экрана на вход с возвратом', async ({ page }) => {
    await signInAsStub(page, SESSION_ABOUT_TO_REFRESH)

    let isRefreshBroken = false

    await routeApi(page, {
      '/Categories': ({ route }) => json(route, []),
      '/Operations': ({ route }) => json(route, []),
      '/connect/token': async ({ route }) => {
        if (isRefreshBroken) {
          await route.fulfill({
            status: 400,
            contentType: 'application/json',
            body: JSON.stringify({ error: 'invalid_grant', error_description: 'Срок действия сессии истёк' }),
          })

          return
        }

        await json(route, {
          access_token: 'access',
          refresh_token: 'refresh',
          token_type: 'Bearer',
          expires_in: SESSION_ABOUT_TO_REFRESH / 1000,
          scope: 'api',
        })
      },
    })

    await page.goto('/operations')

    await expect(page.getByTestId('operations-filter-search')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })

    isRefreshBroken = true

    await page.locator('[data-testid="operations-filter-comment"] input').fill('кофе')
    await page.getByTestId('operations-filter-search').click()

    await expect(page).toHaveURL('/login?returnUrl=/operations')
    await expect(page.getByTestId('login-form')).toBeVisible()
    await expect(page.getByTestId('nav-sign-out')).toBeHidden()
  })

  test('выход закрывает разделы снова', async ({ page }) => {
    await page.goto('/login')

    await page.locator('[data-testid="login-name"] input').fill('пользователь')
    await page.locator('[data-testid="login-password"] input').fill('верный')
    await page.getByTestId('login-submit').click()

    await expect(page.getByTestId('nav-sign-out')).toBeVisible()

    await page.getByTestId('nav-sign-out').click()

    await expect(page).toHaveURL('/')

    await page.goto('/cars')

    await expect(page).toHaveURL('/login?returnUrl=/cars')
  })
})

test.describe('регистрация', () => {
  test.beforeEach(async ({ page }) => {
    await stubTokenEndpoint(page)
  })

  test('новый аккаунт создаётся и сразу входит', async ({ page }) => {
    const requests = await stubRegisterEndpoint(page)

    await page.goto('/debts')
    await page.getByTestId('login-to-register').click()

    await expect(page).toHaveURL('/register?returnUrl=/debts')

    await page.locator(registerName).fill('  ivan  ')
    await page.locator(registerEmail).fill('ivan@money.test')
    await page.locator(registerPassword).fill('верный')
    await page.locator(registerConfirm).fill('верный')
    await page.getByTestId('register-submit').click()

    await expect(page).toHaveURL('/debts')
    await expect(page.getByTestId('nav-sign-out')).toBeVisible()

    expect(requests).toEqual([{ userName: 'ivan', email: 'ivan@money.test', password: 'верный' }])
  })

  test('негодные поля не уходят на сервер', async ({ page }) => {
    const requests = await stubRegisterEndpoint(page)

    await page.goto('/register')
    await page.getByTestId('register-submit').click()

    await expect(page.getByText('Логин обязателен.')).toBeVisible()
    await expect(page.getByText('Пароль обязателен.')).toBeVisible()

    await page.locator(registerName).fill('ivan')
    await page.locator(registerEmail).fill('почта-без-собаки')
    await page.locator(registerPassword).fill('верный')
    await page.locator(registerConfirm).fill('другой')
    await page.getByTestId('register-submit').click()

    await expect(page.getByText('Некорректный email.')).toBeVisible()
    await expect(page.getByText('Пароли не совпадают.')).toBeVisible()
    await expect(page).toHaveURL('/register')

    expect(requests).toEqual([])
  })

  test('отказ сервера остаётся на форме', async ({ page }) => {
    await stubRegisterEndpoint(page, 'Логин занят')

    await page.goto('/register')

    await page.locator(registerName).fill('ivan')
    await page.locator(registerPassword).fill('верный')
    await page.locator(registerConfirm).fill('верный')
    await page.getByTestId('register-submit').click()

    await expect(page.getByTestId('register-error')).toHaveText('Ошибка во время регистрации Логин занят')
    await expect(page).toHaveURL('/register')
    await expect(page.getByTestId('nav-sign-out')).toBeHidden()
  })

  test('показ пароля убирает подтверждение', async ({ page }) => {
    await page.goto('/register')

    await expect(page.locator(registerPassword)).toHaveAttribute('type', 'password')
    await expect(page.locator(registerConfirm)).toBeVisible()

    await page.getByTestId('register-password-visibility').click()

    await expect(page.locator(registerPassword)).toHaveAttribute('type', 'text')
    await expect(page.locator(registerConfirm)).toBeHidden()
  })
})

test.describe('меню и авторизация', () => {
  test('без сессии в меню только регистрация и вход', async ({ page }) => {
    await page.goto('/')

    const menu = page.getByTestId('nav-menu')

    await expect(menu.getByRole('link')).toHaveText(['Регистрация', 'Вход'])
    await expect(page.getByTestId('nav-sign-out')).toBeHidden()

    await menu.getByTestId('nav-sign-up').click()

    await expect(page).toHaveURL('/register')
  })

  test('с сессией пункт профиля подписан именем пользователя', async ({ page }) => {
    await signInAsStub(page)

    await page.goto('/')

    await expect(page.getByTestId('nav-profile')).toContainText(STUB_USER_NAME)
    await expect(page.getByTestId('nav-sign-up')).toBeHidden()
  })

  test('песочница закрыта без входа и не стоит в меню', async ({ page }) => {
    await page.goto('/sandbox')

    await expect(page).toHaveURL('/login?returnUrl=/sandbox')
    await expect(page.getByTestId('nav-menu').getByRole('link', { name: 'Песочница' })).toBeHidden()
  })

  test('адреса блокировки и отказа объясняют причину отдельной страницей', async ({ page }) => {
    await page.goto('/Account/Lockout')

    await expect(page).toHaveURL('/Account/Lockout')
    await expect(page).toHaveTitle('Временная блокировка аккаунта')
    await expect(page.getByTestId('account-notice')).toHaveText('Извините, но ваш аккаунт был временно заблокирован.')
    await expect(page.getByTestId('account-notice-paragraph')).toHaveText([
      'Это могло произойти из-за нескольких неудачных попыток входа. Пожалуйста, попробуйте снова через некоторое время.',
      'Если вы считаете, что это ошибка, свяжитесь с нашей службой поддержки.',
    ])
    await expect(page.getByTestId('login-form')).toHaveCount(0)

    await page.goto('/Account/AccessDenied')

    await expect(page).toHaveTitle('Извините, доступ запрещен')
    await expect(page.getByTestId('account-notice'))
      .toHaveText('Извините, но у вас нет разрешения на доступ к этому ресурсу. Если вы считаете, что это ошибка, пожалуйста, свяжитесь с администратором.')
    await expect(page.getByTestId('account-notice-paragraph')).toHaveCount(0)

    await page.goto('/Account/InvalidUser')

    await expect(page).toHaveTitle('Ошибка входа')
    await expect(page.getByTestId('account-notice'))
      .toHaveText('Извините, но мы не можем найти вашу учетную запись. Пожалуйста, проверьте введенные данные и попробуйте снова.')

    await page.goto('/login')

    await expect(page.getByTestId('account-notice')).toHaveCount(0)
  })
})
