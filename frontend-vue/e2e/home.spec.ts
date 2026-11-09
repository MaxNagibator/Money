import type { Page } from '@playwright/test'
import { expect, test } from './support/test'

const FIRST_RENDER_TIMEOUT = 30_000

const PROMO_CARDS = ['mistress', 'credit', 'debts', 'income', 'assistant']

async function openHome(page: Page): Promise<void> {
  await page.goto('/')
  await expect(page.getByTestId('home-history-toggle')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

async function showHistory(page: Page): Promise<void> {
  await page.getByTestId('home-history-toggle').click()
  await expect(page.getByTestId('home-history-panel')).toBeVisible()
}

test.describe('главная', () => {
  test('промо показывает карточки проблем и решение по порядку', async ({ page }) => {
    await openHome(page)

    await expect(page.getByRole('heading', { name: 'Проект «Филочки»', exact: true })).toBeVisible()
    await expect(page.getByText('Этот проект поможет вам контролировать свои финансы')).toBeVisible()

    const cards = page.locator('[data-testid^="home-promo-"]')

    await expect(cards).toHaveCount(PROMO_CARDS.length)

    for (const [index, id] of PROMO_CARDS.entries()) {
      await expect(cards.nth(index)).toHaveAttribute('data-testid', `home-promo-${id}`)
    }

    await expect(page.getByTestId('home-promo-debts')).toContainText('Друзья постоянно просят вернуть сотку')
    await expect(page.getByRole('heading', { name: 'Но решение всех проблем – это проект «Филочки»!' })).toBeVisible()
    await expect(page.getByRole('heading', { name: 'Время взять под контроль свои деньги и перестать жить в долг!' })).toBeVisible()
  })

  test('история свёрнута до нажатия и сворачивается повторным нажатием', async ({ page }) => {
    await openHome(page)

    const toggle = page.getByTestId('home-history-toggle')

    await expect(page.getByTestId('home-history-panel')).toBeHidden()
    await expect(toggle).toHaveText('Показать')
    await expect(toggle).toHaveAttribute('aria-expanded', 'false')
    await expect(toggle).toHaveAttribute('aria-controls', 'home-history-panel')

    await showHistory(page)

    await expect(toggle).toHaveText('Скрыть')
    await expect(toggle).toHaveAttribute('aria-expanded', 'true')

    await toggle.click()

    await expect(page.getByTestId('home-history-panel')).toBeHidden()
    await expect(toggle).toHaveText('Показать')
    await expect(toggle).toHaveAttribute('aria-expanded', 'false')
  })

  test('раскрытая история не переживает перезагрузку страницы', async ({ page }) => {
    await openHome(page)
    await showHistory(page)

    await page.reload()
    await expect(page.getByTestId('home-history-toggle')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })

    await expect(page.getByTestId('home-history-panel')).toBeHidden()
    await expect(page.getByTestId('home-history-toggle')).toHaveText('Показать')
  })

  test('запись истории показывает дату, счётчик и описания правок', async ({ page }) => {
    await openHome(page)
    await showHistory(page)

    const latest = page.getByTestId('home-history-entry-1.3.17')

    await expect(latest).toContainText('v.1.3.17')
    await expect(latest).toContainText('04.05.2026')
    await expect(latest).toContainText('10 изменений')
    await expect(latest.getByText('При добавлении машинки фокус сразу ставится на поле названия.')).toBeVisible()

    const oldest = page.getByTestId('home-history-entry-0.1.7')

    await expect(oldest).toContainText('26.05.2017')
    await expect(oldest).not.toContainText('изменений')
  })

  test('главная и история открыты без входа', async ({ page }) => {
    await openHome(page)

    await expect(page).toHaveURL('/')
    await expect(page.getByTestId('home-promo-mistress')).toBeVisible()

    await showHistory(page)

    await expect(page.getByTestId('home-history-entry-1.3.17')).toBeVisible()
  })
})
