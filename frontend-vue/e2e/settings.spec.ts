import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { signInAsStub } from './support/session'

const FIRST_RENDER_TIMEOUT = 30_000
const EVENING = '2026-02-03T20:00:00+03:00'

const dividers = '[data-testid="settings-dividers"]'
const alignDecimals = '[data-testid="settings-align-decimals"]'
const widthSlider = '[data-testid="settings-money-width"]'
const scheduleEnabled = '[data-testid="settings-schedule-enabled"]'
const scheduleStart = '[data-testid="settings-schedule-start"] input'
const scheduleEnd = '[data-testid="settings-schedule-end"] input'
const scheduleInterval = '[data-testid="settings-schedule-interval"] input'
const locationChanged = '[data-testid="settings-location-changed"]'
const application = 'html'

async function openSettings(page: Page): Promise<void> {
  await page.goto('/settings')
  await expect(page.getByTestId('settings-mode-status')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

async function enableSchedule(page: Page): Promise<void> {
  await page.getByTestId('settings-mode-schedule').click()
  await expect(page.locator(scheduleEnabled)).toHaveClass(/mud-primary-text/)
  await expect(page.locator(scheduleStart)).toBeEnabled()
}

test.describe('настройки', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('настройки отображения сохраняются без кнопки и переживают перезагрузку', async ({ page }) => {
    await openSettings(page)

    await expect(page.locator(dividers)).not.toHaveClass(/mud-primary-text/)
    await expect(page.getByTestId('settings-alternate-none')).toHaveAttribute('aria-checked', 'true')

    await page.locator(dividers).click()
    await page.getByTestId('settings-alternate-border').click()
    await page.locator(alignDecimals).click()

    await page.reload()

    await expect(page.locator(dividers)).toHaveClass(/mud-primary-text/, { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('settings-alternate-border')).toHaveAttribute('aria-checked', 'true')
    await expect(page.getByTestId('settings-alternate-none')).toHaveAttribute('aria-checked', 'false')
    await expect(page.locator(alignDecimals)).not.toHaveClass(/mud-primary-text/)
  })

  test('ползунок ширины меняет значение с клавиатуры и гаснет без выравнивания', async ({ page }) => {
    await openSettings(page)

    await expect(page.getByTestId('settings-money-width-value')).toHaveText('9 симв.')

    await page.locator(widthSlider).getByRole('slider').press('ArrowRight')

    await expect(page.getByTestId('settings-money-width-value')).toHaveText('10 симв.')

    await page.locator(alignDecimals).click()

    await expect(page.locator(widthSlider).getByRole('slider')).toBeDisabled()

    await page.reload()

    await expect(page.getByTestId('settings-money-width-value')).toHaveText('10 симв.', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.locator(widthSlider).getByRole('slider')).toBeDisabled()
  })

  test('расписание применяется кнопкой и переживает перезагрузку', async ({ page }) => {
    await openSettings(page)
    await enableSchedule(page)

    await page.locator(scheduleStart).fill('20:00')
    await page.locator(scheduleEnd).fill('07:00')
    await page.locator(scheduleInterval).fill('00:15')
    await page.getByTestId('settings-schedule-apply').click()

    await expect(page.getByTestId('settings-schedule-notice')).toHaveText('Расписание обновлено')
    await expect(page.getByTestId('settings-schedule-error')).toHaveCount(0)
    await expect(page.getByTestId('settings-auto-status')).toHaveText('Текущий автоматический режим: По расписанию')

    await page.reload()

    await expect(page.locator(scheduleEnabled)).toHaveClass(/mud-primary-text/, { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.locator(scheduleStart)).toHaveValue('20:00')
    await expect(page.locator(scheduleEnd)).toHaveValue('07:00')
    await expect(page.locator(scheduleInterval)).toHaveValue('00:15')
  })

  test('совпадающие границы расписания показывают ошибку и ничего не меняют', async ({ page }) => {
    await openSettings(page)
    await enableSchedule(page)

    await page.locator(scheduleStart).fill('20:00')
    await page.locator(scheduleEnd).fill('20:00')
    await page.getByTestId('settings-schedule-apply').click()

    await expect(page.getByTestId('settings-schedule-error')).toHaveText('Время начала и окончания не должны совпадать')
    await expect(page.getByTestId('settings-schedule-notice')).toHaveCount(0)

    await page.reload()

    await expect(page.locator(scheduleStart)).toHaveValue('18:00', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.locator(scheduleEnd)).toHaveValue('06:00')
  })

  test('интервал вне границ показывает ошибку и не сохраняется', async ({ page }) => {
    await openSettings(page)
    await enableSchedule(page)

    await page.locator(scheduleInterval).fill('00:00')
    await page.getByTestId('settings-schedule-apply').click()

    await expect(page.getByTestId('settings-schedule-error')).toHaveText('Интервал проверки задаётся в формате ЧЧ:ММ, от 00:01 до 23:59')

    await page.locator(scheduleInterval).fill('00:30')
    await page.getByTestId('settings-schedule-apply').click()

    await expect(page.getByTestId('settings-schedule-notice')).toHaveText('Расписание обновлено')

    await page.reload()

    await expect(page.locator(scheduleInterval)).toHaveValue('00:30', { timeout: FIRST_RENDER_TIMEOUT })
  })

  test('ручной режим гасит карточку расписания и зачёркивает автоматический статус', async ({ page }) => {
    await openSettings(page)
    await enableSchedule(page)

    await page.getByTestId('settings-mode-manual').click()

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Текущий режим темной темы: Ручной')
    await expect(page.getByTestId('settings-auto-status')).toHaveAttribute('style', /text-decoration: line-through/)
    await expect(page.locator(scheduleEnabled)).toBeDisabled()
    await expect(page.locator(scheduleStart)).toBeDisabled()
    await expect(page.locator(scheduleInterval)).toBeDisabled()
    await expect(page.getByTestId('settings-schedule-apply')).toBeDisabled()

    await page.getByTestId('settings-mode-manual').click()

    await expect(page.locator(scheduleStart)).toBeEnabled()
    await expect(page.getByTestId('settings-auto-status')).not.toHaveAttribute('style', /text-decoration: line-through/)
  })

  test('кнопка яркости уводит в ручной режим и переживает перезагрузку', async ({ page }) => {
    await openSettings(page)

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Текущий режим темной темы: Авто')
    await expect(page.getByTestId('settings-applied-status')).toHaveText('Текущий режим темной темы: Светлый')

    await page.getByTestId('settings-brightness').click()

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Текущий режим темной темы: Ручной')
    await expect(page.getByTestId('settings-applied-status')).toHaveText('Текущий режим темной темы: Темный')
    await expect(page.getByTestId('settings-mode-manual')).toHaveClass(/mud-primary-text/)
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')

    await page.reload()

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Текущий режим темной темы: Ручной', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')
  })

  test('включённое расписание внутри интервала применяет тёмную тему', async ({ page }) => {
    await page.clock.setFixedTime(new Date(EVENING))
    await openSettings(page)

    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')

    await enableSchedule(page)

    await expect(page.getByTestId('settings-applied-status')).toHaveText('Текущий режим темной темы: Темный')
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')

    await page.locator(scheduleEnabled).click()

    await expect(page.getByTestId('settings-applied-status')).toHaveText('Текущий режим темной темы: Светлый')
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')
  })

  test('отслеживание перехода по страницам пересчитывает автоматическую тему', async ({ page }) => {
    await page.clock.setFixedTime(new Date('2026-02-03T12:00:00+03:00'))
    await openSettings(page)
    await enableSchedule(page)

    await page.locator(scheduleStart).fill('18:00')
    await page.locator(scheduleEnd).fill('06:00')
    await page.getByTestId('settings-schedule-apply').click()

    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')

    await page.locator(locationChanged).click()
    await expect(page.locator(locationChanged)).not.toHaveClass(/mud-primary-text/)

    await page.clock.setFixedTime(new Date(EVENING))
    await page.getByTestId('brand-home').click()

    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')

    await page.getByTestId('nav-menu').getByRole('link', { name: 'Настройки' }).click()
    await page.locator(locationChanged).click()
    await expect(page.locator(locationChanged)).toHaveClass(/mud-primary-text/)
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')

    await page.getByTestId('brand-home').click()

    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')
  })
})
