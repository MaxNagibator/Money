import type { Locator, Page } from '@playwright/test'
import { expect, test } from './support/test'
import { signInAsStub } from './support/session'

const FIRST_RENDER_TIMEOUT = 30_000
const EVENING = '2026-02-03T20:00:00+03:00'

const dividers = '[data-testid="settings-dividers"]'
const alignDecimals = '[data-testid="settings-align-decimals"]'
const chartColors = '[data-testid="settings-chart-colors"]'
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

function switchIn(page: Page, selector: string): Locator {
  return page.locator(selector).getByRole('switch')
}

async function enableSchedule(page: Page): Promise<void> {
  await page.locator(scheduleEnabled).click()
  await expect(switchIn(page, scheduleEnabled)).toBeChecked()
  await expect(page.locator(scheduleStart)).toBeEnabled()
}

test.describe('настройки', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('настройки отображения сохраняются без кнопки и переживают перезагрузку', async ({ page }) => {
    await openSettings(page)

    await expect(switchIn(page, dividers)).not.toBeChecked()
    await expect(page.getByTestId('settings-alternate-none')).toHaveAttribute('aria-checked', 'true')

    await page.locator(dividers).click()
    await page.getByTestId('settings-alternate-border').click()
    await page.locator(alignDecimals).click()

    await page.reload()

    await expect(switchIn(page, dividers)).toBeChecked({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('settings-alternate-border')).toHaveAttribute('aria-checked', 'true')
    await expect(page.getByTestId('settings-alternate-none')).toHaveAttribute('aria-checked', 'false')
    await expect(switchIn(page, alignDecimals)).not.toBeChecked()
  })

  test('переключатель работает с клавиатуры', async ({ page }) => {
    await openSettings(page)

    const chart = switchIn(page, chartColors)
    await expect(chart).toBeChecked()

    await chart.focus()
    await page.keyboard.press('Space')

    await expect(chart).not.toBeChecked()
    await expect(chart).toHaveAttribute('aria-checked', 'false')

    await page.keyboard.press('Space')

    await expect(chart).toBeChecked()
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
    await expect(page.getByTestId('settings-mode-status')).toHaveText(/тема, по расписанию$/)

    await page.reload()

    await expect(switchIn(page, scheduleEnabled)).toBeChecked({ timeout: FIRST_RENDER_TIMEOUT })
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

  test('ручной режим гасит карточку расписания', async ({ page }) => {
    await openSettings(page)
    await enableSchedule(page)

    await page.getByTestId('settings-mode-light').click()

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Сейчас светлая тема, выбрана вручную')
    await expect(switchIn(page, scheduleEnabled)).toBeDisabled()
    await expect(page.locator(scheduleStart)).toBeDisabled()
    await expect(page.locator(scheduleInterval)).toBeDisabled()
    await expect(page.getByTestId('settings-schedule-apply')).toBeDisabled()

    await page.getByTestId('settings-mode-system').click()

    await expect(switchIn(page, scheduleEnabled)).toBeEnabled()
    await expect(page.locator(scheduleStart)).toBeEnabled()
    await expect(page.getByTestId('settings-mode-status')).toHaveText(/тема, по расписанию$/)
  })

  test('сегмент «Тёмная» уводит в ручной режим и переживает перезагрузку', async ({ page }) => {
    await openSettings(page)

    await expect(page.getByTestId('settings-mode-system')).toHaveAttribute('aria-checked', 'true')
    await expect(page.getByTestId('settings-mode-status')).toHaveText('Сейчас светлая тема, как в системе')

    await page.getByTestId('settings-mode-dark').click()

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Сейчас тёмная тема, выбрана вручную')
    await expect(page.getByTestId('settings-mode-dark')).toHaveAttribute('aria-checked', 'true')
    await expect(page.getByTestId('settings-mode-system')).toHaveAttribute('aria-checked', 'false')
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')

    await page.reload()

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Сейчас тёмная тема, выбрана вручную', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')
  })

  test('включённое расписание внутри интервала применяет тёмную тему', async ({ page }) => {
    await page.clock.setFixedTime(new Date(EVENING))
    await openSettings(page)

    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')

    await enableSchedule(page)

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Сейчас тёмная тема, по расписанию')
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')

    await page.locator(scheduleEnabled).click()

    await expect(page.getByTestId('settings-mode-status')).toHaveText('Сейчас светлая тема, как в системе')
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')
  })

  test('стрелки переключателя отслеживания задают значение, а не инвертируют его', async ({ page }) => {
    await openSettings(page)
    await enableSchedule(page)

    const tracking = switchIn(page, locationChanged)
    await expect(tracking).toBeChecked()

    await tracking.focus()
    await page.keyboard.press('ArrowRight')
    await expect(tracking).toBeChecked()

    await page.keyboard.press('ArrowLeft')
    await expect(tracking).not.toBeChecked()

    await page.keyboard.press('ArrowLeft')
    await expect(tracking).not.toBeChecked()

    await page.keyboard.press('ArrowRight')
    await expect(tracking).toBeChecked()
  })

  test('режим темы – радиогруппа со стрелками и одним элементом в порядке Tab', async ({ page }) => {
    await openSettings(page)

    const group = page.getByRole('radiogroup', { name: 'Режим темы' })
    await expect(group.getByRole('radio')).toHaveCount(3)
    await expect(page.getByTestId('settings-mode-system')).toHaveAttribute('tabindex', '0')
    await expect(page.getByTestId('settings-mode-light')).toHaveAttribute('tabindex', '-1')

    await page.getByTestId('settings-mode-system').focus()
    await page.keyboard.press('ArrowRight')

    await expect(page.getByTestId('settings-mode-light')).toBeFocused()
    await expect(group.getByRole('radio', { name: 'Светлая' })).toBeChecked()
    await expect(page.getByTestId('settings-mode-status')).toHaveText('Сейчас светлая тема, выбрана вручную')

    await page.keyboard.press('ArrowLeft')

    await expect(page.getByTestId('settings-mode-system')).toBeFocused()
    await expect(page.getByTestId('settings-mode-system')).toHaveAttribute('aria-checked', 'true')
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
    await expect(switchIn(page, locationChanged)).not.toBeChecked()

    await page.clock.setFixedTime(new Date(EVENING))
    await page.getByTestId('brand-home').click()

    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')

    await page.getByTestId('nav-menu').getByRole('link', { name: 'Настройки' }).click()
    await page.locator(locationChanged).click()
    await expect(switchIn(page, locationChanged)).toBeChecked()
    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'light')

    await page.getByTestId('brand-home').click()

    await expect(page.locator(application)).toHaveAttribute('data-mud-theme', 'dark')
  })
})
