import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { stubScene } from './support/scene'
import { signInAsStub } from './support/session'

const FIRST_RENDER_TIMEOUT = 15_000
const UNMOUNT_TIMEOUT = 200

const quickActivator = '[data-testid="operations-quick"] button'

async function openQuickMenu(page: Page): Promise<void> {
  await page.goto('/operations')
  await expect(page.getByTestId('operations-quick')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
  await page.getByTestId('operations-quick').click()
  await expect(page.getByTestId('operations-quick-list')).toBeVisible()
}

async function openDebtDialog(page: Page): Promise<void> {
  await page.goto('/debts')
  await expect(page.getByTestId('debts-add-1')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
  await page.getByTestId('debts-add-1').click()
  await expect(page.getByTestId('debts-dialog')).toBeVisible()
}

test.describe('всплывающие слои', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
    await stubScene(page)
  })

  test('закрытый поповер убирает содержимое сразу, без ожидания перехода', async ({ page }) => {
    await openQuickMenu(page)

    await page.keyboard.press('Escape')

    await expect(page.getByTestId('operations-quick-list')).toHaveCount(0, { timeout: UNMOUNT_TIMEOUT })
  })

  test('меню, открытое мышью, держит фокус на обёртке, Tab закрывает и возвращает его активатору', async ({ page }) => {
    await openQuickMenu(page)

    await expect(page.getByTestId('menu-wrapper')).toBeFocused()

    await page.keyboard.press('Tab')

    await expect(page.getByTestId('operations-quick-list')).toHaveCount(0)
    await expect(page.locator(quickActivator)).toBeFocused()
  })

  test('Tab не выходит из диалога и заворачивается по кругу', async ({ page }) => {
    await openDebtDialog(page)

    const close = page.locator('.mud-dialog .mud-button-close')
    const save = page.getByTestId('debts-dialog-save')

    await page.keyboard.press('Tab')
    await expect(close).toBeFocused()

    await page.keyboard.press('Shift+Tab')
    await expect(save).toBeFocused()

    await page.keyboard.press('Tab')
    await expect(close).toBeFocused()
  })

  test('закрытие диалога возвращает фокус тому, кто его открыл', async ({ page }) => {
    await openDebtDialog(page)

    await page.getByTestId('debts-dialog-cancel').click()

    await expect(page.getByTestId('debts-dialog')).toHaveCount(0)
    await expect(page.getByTestId('debts-add-1')).toBeFocused()
  })

  test('диалог блокирует прокрутку страницы под собой и снимает блокировку при закрытии', async ({ page }) => {
    await openDebtDialog(page)

    await expect(page.locator('body')).toHaveClass(/scroll-locked/)

    await page.getByTestId('debts-dialog-cancel').click()

    await expect(page.locator('body')).not.toHaveClass(/scroll-locked/)
  })
})

test.describe('ящик меню на узком экране', () => {
  test.use({ viewport: { width: 800, height: 900 } })

  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
    await stubScene(page)
  })

  test('ниже брейкпоинта ящик выезжает поверх подложки, клик по ней закрывает', async ({ page }) => {
    await page.goto('/operations')

    const drawer = page.locator('.mud-drawer')
    const overlay = page.locator('.mud-drawer-overlay')

    await expect(drawer).toHaveClass(/mud-drawer--closed/, { timeout: FIRST_RENDER_TIMEOUT })
    await expect(overlay).toHaveCount(0)

    await page.getByRole('button', { name: 'Открыть меню' }).click()

    await expect(drawer).toHaveClass(/mud-drawer--open/)
    await expect(drawer).not.toHaveClass(/mud-drawer--initial/)
    await expect(overlay).toBeVisible()
    await expect(overlay.locator('.mud-overlay-scrim.mud-overlay-dark')).toHaveCount(1)

    await overlay.click()

    await expect(drawer).toHaveClass(/mud-drawer--closed/)
    await expect(overlay).toHaveCount(0)
  })
})
