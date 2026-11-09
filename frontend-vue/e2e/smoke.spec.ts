import { expect, test } from './support/test'
import { signInAsStub } from './support/session'

test('главная отвечает и показывает промо', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByTestId('home-history-toggle')).toBeVisible()
})

test('заголовок вкладки следует за маршрутом', async ({ page }) => {
  await page.goto('/')

  await expect(page).toHaveTitle('Проект «Филочки»')

  await page.goto('/nope')

  await expect(page).toHaveTitle('Страница не найдена')
})

test('меню разделов ведёт на страницу раздела', async ({ page }) => {
  await signInAsStub(page)
  await page.goto('/')

  await page.getByTestId('nav-menu').getByRole('link', { name: 'Настройки' }).click()

  await expect(page).toHaveURL(/\/settings$/)
  await expect(page.getByTestId('settings-mode')).toBeVisible()
})

test('неизвестный путь показывает страницу не найдена', async ({ page }) => {
  await page.goto('/nope')

  await expect(page.getByText('Извините, но мы не можем найти запрашиваемую страницу.')).toBeVisible()
})

test('логотип и название в шапке меню ведут на главную', async ({ page }) => {
  await page.goto('/nope')

  const brand = page.getByTestId('brand-home')

  await expect(brand).toHaveText('Филочки')
  await expect(brand.locator('img')).toHaveAttribute('src', '/icon-192.png')

  await brand.click()

  await expect(page).toHaveURL(/\/$/)
})

test('интерфейс рисуется шрифтом Roboto', async ({ page }) => {
  await page.goto('/')

  await expect(page.getByTestId('brand-home')).toBeVisible()

  const roboto = await page.evaluate(async () => {
    await document.fonts.ready

    const faces = [...document.fonts].filter(face => face.family === 'Roboto')

    return {
      weights: [...new Set(faces.map(face => face.weight))].sort(),
      isRendered: document.fonts.check('16px Roboto', 'Филочки'),
    }
  })

  expect(roboto.weights).toEqual(['300', '400', '500', '700'])
  expect(roboto.isRendered).toBe(true)

  const family = await page.getByTestId('nav-menu').evaluate(element => getComputedStyle(element).fontFamily)

  expect(family).toContain('Roboto')
})

test('переключатель темы меняет яркость и режим', async ({ page }) => {
  await page.goto('/')

  const themeSwitch = page.getByTestId('theme-switch')
  const modeSwitch = page.getByTestId('theme-mode-switch').locator('.mud-badge')

  await expect(themeSwitch).toHaveAttribute('aria-label', 'Переключить на темную тему')
  await expect(modeSwitch).toHaveClass(/mud-theme-secondary/)

  await themeSwitch.click()

  await expect(themeSwitch).toHaveAttribute('aria-label', 'Переключить на светлую тему')
  await expect(modeSwitch).toHaveClass(/mud-theme-primary/)
  await expect(page.locator('html')).toHaveAttribute('data-mud-theme', 'dark')

  await page.reload()

  await expect(page.getByTestId('theme-switch')).toHaveAttribute('aria-label', 'Переключить на светлую тему')

  await modeSwitch.click()

  await expect(page.getByTestId('theme-switch')).toHaveAttribute('aria-label', 'Переключить на темную тему')
})
