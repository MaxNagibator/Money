import type { Locator, Page } from '@playwright/test'
import { expect, test } from './support/test'

const FIRST_RENDER_TIMEOUT = 30_000

const RIPPLE = '.mud-ripple-effect'

async function openHome(page: Page): Promise<void> {
  await page.goto('/')
  await expect(page.getByTestId('home-history-toggle')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

async function press(page: Page, control: Locator): Promise<void> {
  await control.hover()
  await page.mouse.down()
}

test.describe('рябь MudBlazor', () => {
  test('нажатие рождает рябь внутри контрола и отпускание её убирает', async ({ page }) => {
    await openHome(page)

    const toggle = page.getByTestId('home-history-toggle')

    await expect(toggle).toHaveClass(/mud-ripple/)
    await expect(toggle.locator(RIPPLE)).toHaveCount(0)

    await press(page, toggle)

    await expect(toggle.locator(RIPPLE)).toHaveCount(1)
    await expect(toggle.locator(RIPPLE)).toHaveClass(/mud-ripple-effect-expanding/)

    await page.mouse.up()

    await expect(toggle.locator(RIPPLE)).toHaveCount(0, { timeout: 5_000 })
  })

  test('кнопка-иконка ряби тоже поддаётся', async ({ page }) => {
    await openHome(page)

    const themeSwitch = page.getByTestId('theme-switch')

    await expect(themeSwitch).toHaveClass(/mud-ripple/)

    await press(page, themeSwitch)

    await expect(themeSwitch.locator(RIPPLE)).toHaveCount(1)

    await page.mouse.up()
  })
})
