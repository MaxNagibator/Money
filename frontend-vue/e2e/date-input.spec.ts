import { expect, test } from './support/test'
import { signInAsStub } from './support/session'

const field = '[data-testid="date-text"] input'
const preview = 'date-preview'

test.describe('поле даты', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('принимает явную дату и переформатирует её', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(field).fill('1.2.2026')
    await page.locator(field).press('Enter')

    await expect(page.locator(field)).toHaveValue('01.02.2026')
    await expect(page.getByTestId(preview)).toHaveText('01.02.2026')
  })

  test('понимает алиас сегодня', async ({ page }) => {
    await page.clock.setFixedTime(new Date('2026-08-22T10:00:00'))
    await page.goto('/sandbox')

    await page.locator(field).fill('с')
    await page.locator(field).press('Enter')

    await expect(page.locator(field)).toHaveValue('22.08.2026')
  })

  test('понимает цепочку алиасов', async ({ page }) => {
    await page.clock.setFixedTime(new Date('2026-08-22T10:00:00'))
    await page.goto('/sandbox')

    await page.locator(field).fill('ппз')
    await page.locator(field).press('Enter')

    await expect(page.locator(field)).toHaveValue('25.08.2026')
  })

  test('отвергает смешение форм и держит текст пользователя', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(field).fill('1.2.2026')
    await page.locator(field).press('Enter')

    await page.locator(field).fill('позавтра')
    await page.locator(field).press('Enter')

    await expect(page.locator(field)).toHaveValue('позавтра')
    await expect(page.getByTestId(preview)).toHaveText('01.02.2026')
  })

  test('пустой ввод очищает дату', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(field).fill('1.2.2026')
    await page.locator(field).press('Enter')

    await page.locator(field).fill('')
    await page.locator(field).press('Enter')

    await expect(page.getByTestId(preview)).toHaveText('дата не задана')
  })
})
