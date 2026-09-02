import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { empty, identifier, json, problem, routeApi, sentSum } from './support/api'
import type { FastOperationDto } from './support/fixtures'
import { categories, fastOperations, places } from './support/fixtures'
import { signInAsStub } from './support/session'

type SaveRequest = Omit<FastOperationDto, 'id'>

interface FastOperationsStub {
  loads: number
  created: SaveRequest[]
  updated: { id: number, body: SaveRequest }[]
  removed: number[]
  restored: number[]
}

const FIRST_RENDER_TIMEOUT = 30_000

const dialogName = '[data-testid="fast-operations-dialog-name"] textarea'
const dialogSum = '[data-testid="fast-operations-dialog-sum"] [data-testid="sum-numeric"] input'
const dialogOrder = '[data-testid="fast-operations-dialog-order"] input'
const dialogCategory = '[data-testid="fast-operations-dialog-category"]'
const dialogPlace = '[data-testid="fast-operations-dialog-place"] input'
const dialogComment = '[data-testid="fast-operations-dialog-comment"] textarea'

async function stubApi(page: Page, initial: FastOperationDto[] = fastOperations): Promise<FastOperationsStub> {
  const state = initial.map(operation => ({ ...operation }))
  const deleted = new Set<number>()
  const stub: FastOperationsStub = { loads: 0, created: [], updated: [], removed: [], restored: [] }
  let lastId = 100

  await routeApi(page, {
    '/Categories': ({ route }) => json(route, categories),
    '/Operations': async ({ route, path }) => {
      if (path.startsWith('/Operations/GetPlaces')) {
        await json(route, places)
        return
      }

      await route.fallback()
    },
    '/FastOperations': async ({ route, method, path }) => {
      if (path === '/FastOperations') {
        if (method === 'POST') {
          const body = route.request().postDataJSON() as SaveRequest

          lastId += 1
          stub.created.push(body)
          state.push({ ...body, id: lastId, sum: sentSum(route.request().postData()) ?? body.sum })
          await json(route, lastId)
          return
        }

        stub.loads += 1
        await json(route, state.filter(operation => !deleted.has(operation.id)))
        return
      }

      const id = identifier(path)

      if (method === 'GET') {
        const stored = state.find(operation => operation.id === id)

        if (stored === undefined) {
          await problem(route, 'Быстрой операции нет', 404)
          return
        }

        await json(route, stored)
        return
      }

      if (path.endsWith('/Restore')) {
        deleted.delete(id)
        stub.restored.push(id)
        await empty(route)
        return
      }

      if (method === 'PUT') {
        const body = route.request().postDataJSON() as SaveRequest
        const stored = state.find(operation => operation.id === id)

        stub.updated.push({ id, body })

        if (stored !== undefined) {
          Object.assign(stored, body, { sum: sentSum(route.request().postData()) ?? body.sum })
        }

        await empty(route)
        return
      }

      deleted.add(id)
      stub.removed.push(id)
      await empty(route)
    },
  })

  return stub
}

async function openFastOperations(page: Page): Promise<void> {
  await page.goto('/fast-operations')
  await expect(page.getByTestId('fast-operations-card-10')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

async function selectOption(page: Page, name: string): Promise<void> {
  await page.locator('.mud-popover-open').getByText(name, { exact: true }).click()
}

test.describe('быстрые операции', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('создание уходит с полями формы и добавляет карточку', async ({ page }) => {
    const stub = await stubApi(page)

    await openFastOperations(page)

    await page.getByTestId('fast-operations-add-1').click()

    await page.locator(dialogName).fill('Обед')
    await page.locator(dialogSum).fill('250,50')
    await page.locator(dialogOrder).fill('2')
    await page.locator(dialogComment).fill('каждый день')

    await page.locator(dialogPlace).fill('Пекарня на')
    await page.getByTestId('place-suggestions').getByText('Пекарня на углу', { exact: true }).click()

    await page.locator(dialogCategory).fill('Кафе')
    await selectOption(page, 'Кафе')

    await page.getByTestId('fast-operations-dialog-save').click()

    await expect(page.getByTestId('fast-operations-notice')).toContainText('Успех!')
    await expect(page.getByTestId('fast-operations-dialog')).toHaveCount(0)

    expect(stub.created).toEqual([{
      categoryId: 2,
      sum: 250.5,
      name: 'Обед',
      comment: 'каждый день',
      place: 'Пекарня на углу',
      order: 2,
    }])

    const created = page.getByTestId('fast-operations-card-101')

    await expect(created).toContainText('Обед')
    await expect(page.getByTestId('fast-operations-sum-101')).toHaveText('250,50')
    await expect(page.getByTestId('fast-operations-category-101')).toHaveText('Кафе')
    await expect(page.getByTestId('fast-operations-place-101')).toHaveText('Пекарня на углу')
    await expect(page.getByTestId('fast-operations-order-101')).toContainText('2')
  })

  test('изменение открывается заполненным и уходит по идентификатору', async ({ page }) => {
    const stub = await stubApi(page)

    await openFastOperations(page)

    await page.getByTestId('fast-operations-edit-10').click()

    const dialog = page.getByRole('dialog')

    await expect(dialog).toContainText('Обновить')
    await expect(page.locator(dialogName)).toHaveValue('Кофе')
    await expect(page.locator(dialogSum)).toHaveValue('250,50')
    await expect(page.locator(dialogOrder)).toHaveValue('1')
    await expect(page.locator(dialogComment)).toHaveValue('утро')
    await expect(page.locator(dialogPlace)).toHaveValue('Пекарня')
    await expect(page.getByTestId('fast-operations-dialog-category')).toHaveValue('Кафе')

    await page.locator(dialogName).fill('Кофе большой')
    await page.locator(dialogSum).fill('300,5')

    await page.getByTestId('fast-operations-dialog-save').click()

    await expect(page.getByTestId('fast-operations-notice')).toContainText('Успех!')

    expect(stub.updated).toEqual([{
      id: 10,
      body: {
        categoryId: 2,
        sum: 300.5,
        name: 'Кофе большой',
        comment: 'утро',
        place: 'Пекарня',
        order: 1,
      },
    }])

    await expect(page.getByTestId('fast-operations-card-10')).toContainText('Кофе большой')
    await expect(page.getByTestId('fast-operations-sum-10')).toHaveText('300,5')
    expect(stub.created).toHaveLength(0)
  })

  test('незаполненные наименование и категория показывают ошибки и не уходят в запрос', async ({ page }) => {
    const stub = await stubApi(page)

    await openFastOperations(page)

    await page.getByTestId('fast-operations-add').click()

    await page.locator(dialogSum).fill('100')
    await page.getByTestId('fast-operations-dialog-save').click()

    await expect(page.getByText('Наименование обязательно')).toBeVisible()
    await expect(page.getByText('Категория обязательна')).toBeVisible()
    await expect(page.getByTestId('fast-operations-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('нераспознанная сумма прерывает сохранение и оставляет диалог открытым', async ({ page }) => {
    const stub = await stubApi(page)

    await openFastOperations(page)

    await page.getByTestId('fast-operations-add-1').click()

    await page.locator(dialogName).fill('Обед')
    await page.locator(dialogCategory).fill('Кафе')
    await selectOption(page, 'Кафе')
    await page.locator(dialogSum).fill('абв')

    await page.getByTestId('fast-operations-dialog-save').click()

    await expect(page.getByTestId('fast-operations-notice')).toContainText("Нераспознано значение в поле 'сумма'.")
    await expect(page.getByTestId('fast-operations-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('удаление помечает карточку, восстановление возвращает её действия', async ({ page }) => {
    const stub = await stubApi(page)

    await openFastOperations(page)

    const card = page.getByTestId('fast-operations-card-10')

    await page.getByTestId('fast-operations-delete-10').click()

    await expect(page.getByTestId('fast-operations-restore-10')).toBeVisible()
    await expect(page.getByTestId('fast-operations-edit-10')).toHaveCount(0)
    await expect(card).toHaveClass(/fast-operation-card--deleted/)
    await expect(page.getByTestId('fast-operations-card-11')).toBeVisible()

    await page.getByTestId('fast-operations-restore-10').click()

    await expect(page.getByTestId('fast-operations-edit-10')).toBeVisible()
    await expect(page.getByTestId('fast-operations-restore-10')).toHaveCount(0)
    await expect(card).not.toHaveClass(/fast-operation-card--deleted/)

    expect(stub.removed).toEqual([10])
    expect(stub.restored).toEqual([10])
  })

  test('пометка удаления живёт до перезагрузки, после неё карточки нет', async ({ page }) => {
    await stubApi(page)

    await openFastOperations(page)

    await page.getByTestId('fast-operations-delete-10').click()

    await expect(page.getByTestId('fast-operations-restore-10')).toBeVisible()

    await page.reload()

    await expect(page.getByTestId('fast-operations-card-11')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('fast-operations-card-10')).toHaveCount(0)
    await expect(page.getByTestId('fast-operations-empty')).toHaveCount(0)
  })

  test('пустой ответ показывает заглушку вместо карточек', async ({ page }) => {
    await stubApi(page, [])

    await page.goto('/fast-operations')

    await expect(page.getByTestId('fast-operations-empty')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('fast-operations-empty')).toContainText('Шаблонов пока нет')
    await expect(page.getByTestId('fast-operations-error')).toHaveCount(0)
  })

  test('отказ загрузки показывает сообщение сервера, повтор возвращает список', async ({ page }) => {
    await stubApi(page)

    let isBroken = true

    await routeApi(page, {
      '/FastOperations': async ({ route, method, path }) => {
        if (!isBroken || method !== 'GET' || path !== '/FastOperations') {
          await route.fallback()
          return
        }

        await problem(route, 'Быстрые операции временно недоступны')
      },
    })

    await page.goto('/fast-operations')

    await expect(page.getByTestId('fast-operations-error')).toContainText('Быстрые операции временно недоступны', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('fast-operations-empty')).toHaveCount(0)

    isBroken = false
    await page.getByTestId('fast-operations-retry').click()

    await expect(page.getByTestId('fast-operations-card-10')).toBeVisible()
    await expect(page.getByTestId('fast-operations-error')).toHaveCount(0)
  })
})
