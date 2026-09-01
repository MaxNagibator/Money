import { expect, test } from './support/test'
import { signInAsStub } from './support/session'

const numeric = '[data-testid="sum-numeric"] input'
const expression = '[data-testid="sum-expression"] input'

test.describe('поле суммы', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('принимает обычное число в числовом режиме', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(numeric).fill('1234,56')
    await page.locator(numeric).blur()

    await expect(page.locator(numeric)).toHaveValue('1234,56')
    await expect(page.getByTestId('sum-preview')).toHaveText(/1\s?234,56/)
  })

  test('переключается в режим выражения по знаку операции', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(numeric).fill('1')
    await page.locator(numeric).press('+')

    await expect(page.locator(expression)).toBeVisible()
    await expect(page.locator(numeric)).toHaveCount(0)
  })

  test('вычисляет выражение и возвращается в числовой режим', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(numeric).press('+')
    await page.locator(expression).fill('1+5 555,00')
    await page.locator(expression).press('Enter')

    await expect(page.locator(numeric)).toHaveValue('5556')
    await expect(page.getByTestId('sum-preview')).toHaveText(/5\s?556,00/)
  })

  test('возвращает выражение при фокусе и позволяет его дополнить', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(numeric).press('+')
    await page.locator(expression).fill('1+2')
    await page.locator(expression).press('Enter')

    await expect(page.locator(numeric)).toHaveValue('3')

    await page.locator(numeric).focus()
    await expect(page.locator(expression)).toHaveValue('1+2')

    await page.locator(expression).fill('1+2+5')
    await page.locator(expression).press('Enter')

    await expect(page.locator(numeric)).toHaveValue('8')
  })

  test('держит режим выражения открытым при ошибке', async ({ page }) => {
    await page.goto('/sandbox')

    await page.locator(numeric).fill('2,5')
    await page.locator(numeric).press('+')
    await page.locator(expression).fill('(1+2')
    await page.locator(expression).press('Enter')

    await expect(page.locator(expression)).toBeVisible()
    await expect(page.getByText('Не удалось вычислить сумму')).toBeVisible()
    await expect(page.getByTestId('sum-preview')).toHaveText(/2,50/)
  })
})
