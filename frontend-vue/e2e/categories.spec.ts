import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { created, empty, identifier, json, problem, routeApi } from './support/api'
import type { CategoryDto } from './support/fixtures'
import { categories } from './support/fixtures'
import { signInAsStub } from './support/session'

const CREATED_ID = 100

async function stubCategories(page: Page, initial: CategoryDto[] = categories): Promise<void> {
  const state = [...initial]

  await routeApi(page, {
    '/Categories': async ({ route, method, path }) => {
      if (path !== '/Categories') {
        if (method === 'GET') {
          const stored = state.find(category => category.id === identifier(path))

          await (stored === undefined ? problem(route, 'Категории нет', 404) : json(route, stored))
          return
        }

        await empty(route)
        return
      }

      if (method === 'POST') {
        const body = route.request().postDataJSON() as Omit<CategoryDto, 'id'>

        state.push({ ...body, id: CREATED_ID })
        await created(route, CREATED_ID)
        return
      }

      await json(route, state)
    },
  })
}

test.describe('категории', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
    await stubCategories(page)
  })

  test('показывает дерево по типам операций', async ({ page }) => {
    await page.goto('/categories')

    const tree = page.getByTestId('category-tree').first()

    await expect(tree.getByText('Еда')).toBeVisible()
    await expect(tree.getByText('Кафе')).toBeHidden()

    await tree.getByRole('button', { name: 'Развернуть', exact: true }).click()

    await expect(tree.getByText('Кафе')).toBeVisible()

    await page.getByRole('tab', { name: 'Доходы' }).click()

    await expect(page.getByText('Зарплата')).toBeVisible()
  })

  test('создаёт категорию верхнего уровня', async ({ page }) => {
    await page.goto('/categories')

    await page.getByTestId('category-add-root').click()
    await page.locator('[data-testid="category-name"] input').fill('Транспорт')
    await page.getByTestId('category-save').click()

    await expect(page.getByTestId('categories-notice')).toContainText('Категория сохранена')
    await expect(page.getByTestId('category-tree').first().getByText('Транспорт')).toBeVisible()
  })

  test('требует наименование', async ({ page }) => {
    await page.goto('/categories')

    await page.getByTestId('category-add-root').click()
    await page.getByTestId('category-save').click()

    await expect(page.getByText('Заполни меня')).toBeVisible()
    await expect(page.getByTestId('category-dialog')).toBeVisible()
  })

  test('удаление показывает кнопку восстановления', async ({ page }) => {
    await page.goto('/categories')

    const tree = page.getByTestId('category-tree').first()

    await tree.getByTestId('category-remove').first().click()

    await expect(tree.getByTestId('category-restore')).toBeVisible()

    await tree.getByTestId('category-restore').click()

    await expect(tree.getByTestId('category-restore')).toBeHidden()
    await expect(tree.getByTestId('category-remove').first()).toBeVisible()
  })
})
