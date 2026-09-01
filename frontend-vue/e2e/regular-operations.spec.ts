import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { empty, json, optionalIdentifier, problem, routeApi } from './support/api'
import type { CategoryDto, RegularOperationDto } from './support/fixtures'
import { FIXTURE_TODAY, regularOperations } from './support/fixtures'
import { signInAsStub } from './support/session'

type SaveRequest = Omit<RegularOperationDto, 'id' | 'runTime'>

interface RegularOperationsStub {
  listRequests: number
  created: SaveRequest[]
  updated: { id: number, body: SaveRequest }[]
  removed: number[]
  restored: number[]
}

const categories: CategoryDto[] = [
  { id: 1, name: 'Продукты', parentId: null, order: null, color: '#ff0000', operationTypeId: 1 },
  { id: 2, name: 'Кафе', parentId: 1, order: null, color: null, operationTypeId: 1 },
  { id: 3, name: 'Зарплата', parentId: null, order: null, color: null, operationTypeId: 2 },
]

const TODAY = FIXTURE_TODAY
const CREATED_RUN_TIME = '2026-02-11T00:00:00'
const UPDATED_RUN_TIME = '2026-03-05T00:00:00'
const FIRST_RENDER_TIMEOUT = 30_000

const dialogName = '[data-testid="regular-operations-dialog-name"] textarea'
const dialogSum = '[data-testid="regular-operations-dialog-sum"] [data-testid="sum-numeric"] input'
const dialogCategory = '[data-testid="regular-operations-dialog-category"]'
const dialogPlace = '[data-testid="regular-operations-dialog-place"] input'
const dialogComment = '[data-testid="regular-operations-dialog-comment"] textarea'
const dialogTimeValue = '[data-testid="regular-operations-dialog-time-value"] input'
const dialogDateFrom = '[data-testid="regular-operations-dialog-date-from"] input'
const dialogDateTo = '[data-testid="regular-operations-dialog-date-to"] input'

async function stubApi(page: Page, initial: RegularOperationDto[] = regularOperations): Promise<RegularOperationsStub> {
  const state = initial.map(operation => ({ ...operation }))
  const deleted = new Set<number>()

  const stub: RegularOperationsStub = {
    listRequests: 0,
    created: [],
    updated: [],
    removed: [],
    restored: [],
  }

  let lastId = 100

  await routeApi(page, {
    '/Categories': ({ route }) => json(route, categories),
    '/Operations': async ({ route, path }) => {
      if (path.startsWith('/Operations/GetPlaces')) {
        await json(route, [])
        return
      }

      await route.fallback()
    },
    '/RegularOperations': async ({ route, method, path }) => {
      const id = optionalIdentifier(path)

      if (id !== null && path.endsWith('/Restore')) {
        deleted.delete(id)
        stub.restored.push(id)
        await empty(route)
        return
      }

      if (id !== null && method === 'DELETE') {
        deleted.add(id)
        stub.removed.push(id)
        await empty(route)
        return
      }

      if (method === 'POST') {
        const body = route.request().postDataJSON() as SaveRequest

        lastId += 1
        stub.created.push(body)
        state.push({ ...body, id: lastId, runTime: CREATED_RUN_TIME })
        await json(route, lastId)
        return
      }

      if (id !== null && method === 'PUT') {
        const body = route.request().postDataJSON() as SaveRequest
        const index = state.findIndex(operation => operation.id === id)

        stub.updated.push({ id, body })
        state[index] = { ...body, id, runTime: UPDATED_RUN_TIME }
        await empty(route)
        return
      }

      if (id === null) {
        stub.listRequests += 1
        await json(route, state.filter(operation => !deleted.has(operation.id)))
        return
      }

      await json(route, state.find(operation => operation.id === id))
    },
  })

  return stub
}

async function openScreen(page: Page): Promise<void> {
  await page.goto('/regular-operations')
  await expect(page.getByTestId('regular-operations-card-1')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

async function fillField(page: Page, selector: string, value: string): Promise<void> {
  await page.locator(selector).fill(value)
  await page.locator(selector).blur()
}

async function selectOption(page: Page, name: string): Promise<void> {
  await page.locator('.mud-popover-open').getByText(name, { exact: true }).click()
}

async function selectTimeType(page: Page, name: string): Promise<void> {
  await page.getByTestId('regular-operations-dialog-time-type').click()
  await selectOption(page, name)
}

async function selectCategory(page: Page, name: string): Promise<void> {
  await page.locator(dialogCategory).fill(name)
  await selectOption(page, name)
}

test.describe('регулярные операции', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('создание из диалога уходит POST с полями формы и добавляет карточку', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openScreen(page)

    await page.getByTestId('regular-operations-add-1').click()

    await expect(page.getByRole('dialog')).toContainText('Создать')
    await expect(page.locator(dialogDateFrom)).toHaveValue('03.02.2026')
    await expect(page.locator(dialogSum)).toHaveValue('0')
    await expect(page.locator(dialogTimeValue)).toHaveValue('1')

    await page.locator(dialogName).fill('Интернет')
    await selectTimeType(page, 'Каждую неделю')
    await fillField(page, dialogTimeValue, '3')
    await fillField(page, dialogSum, '750,50')
    await selectCategory(page, 'Кафе')
    await fillField(page, dialogPlace, 'Онлайн')
    await page.locator(dialogComment).fill('домашний')
    await fillField(page, dialogDateTo, '31.12.2026')

    await page.getByTestId('regular-operations-dialog-save').click()

    await expect(page.getByTestId('regular-operations-notice')).toContainText('Успех!')
    expect(stub.created).toEqual([{
      name: 'Интернет',
      categoryId: 2,
      sum: 750.5,
      comment: 'домашний',
      place: 'Онлайн',
      timeTypeId: 2,
      timeValue: 3,
      dateFrom: '2026-02-03T00:00:00',
      dateTo: '2026-12-31T00:00:00',
    }])

    await expect(page.getByTestId('regular-operations-name-101')).toHaveText('Интернет')
    await expect(page.getByTestId('regular-operations-sum-101')).toHaveText('750,50')
    await expect(page.getByTestId('regular-operations-category-101')).toContainText('Кафе')
    await expect(page.getByTestId('regular-operations-run-time-101')).toContainText('11.02.2026')
    expect(stub.listRequests).toBe(1)
  })

  test('правка из карточки открывает заполненный диалог и уходит PUT по маршруту', async ({ page }) => {
    const stub = await stubApi(page)

    await openScreen(page)

    await page.getByTestId('regular-operations-edit-1').click()

    await expect(page.getByRole('dialog')).toContainText('Обновить')
    await expect(page.locator(dialogName)).toHaveValue('Аренда')
    await expect(page.locator(dialogSum)).toHaveValue('1000')
    await expect(page.getByTestId('regular-operations-dialog-category')).toHaveValue('Продукты')
    await expect(page.locator(dialogTimeValue)).toHaveValue('5')
    await expect(page.locator(dialogDateFrom)).toHaveValue('01.01.2026')
    await expect(page.locator(dialogDateTo)).toHaveValue('')

    await page.locator(dialogName).fill('Аренда квартиры')
    await fillField(page, dialogSum, '1200')

    await page.getByTestId('regular-operations-dialog-save').click()

    await expect(page.getByTestId('regular-operations-notice')).toContainText('Успех!')
    expect(stub.updated).toEqual([{
      id: 1,
      body: {
        name: 'Аренда квартиры',
        categoryId: 1,
        sum: 1200,
        comment: null,
        place: null,
        timeTypeId: 3,
        timeValue: 5,
        dateFrom: '2026-01-01T00:00:00',
        dateTo: null,
      },
    }])

    await expect(page.getByTestId('regular-operations-name-1')).toHaveText('Аренда квартиры')
    await expect(page.getByTestId('regular-operations-sum-1')).toHaveText('1200')
    await expect(page.getByTestId('regular-operations-run-time-1')).toContainText('05.03.2026')
    expect(stub.listRequests).toBe(1)
  })

  test('неверный ввод оставляет диалог открытым и не шлёт запрос', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openScreen(page)

    await page.getByTestId('regular-operations-add').click()

    await page.locator(dialogName).fill('   ')
    await fillField(page, dialogTimeValue, '40')
    await fillField(page, dialogDateTo, '01.01.2025')

    await page.getByTestId('regular-operations-dialog-save').click()

    await expect(page.getByTestId('regular-operations-dialog-name')).toContainText('Заполни меня')
    await expect(page.getByTestId('regular-operations-dialog-category').locator('xpath=../../..')).toContainText('Заполни меня')
    await expect(page.getByTestId('regular-operations-dialog-time-value')).toContainText('Значение от 1 до 31')
    await expect(page.getByTestId('regular-operations-dialog-date-to')).toContainText('Не раньше даты начала')
    await expect(page.getByTestId('regular-operations-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('нераспознанная дата окончания не сохраняет правило без даты', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openScreen(page)

    await page.getByTestId('regular-operations-add').click()

    await page.locator(dialogName).fill('Аренда')
    await fillField(page, dialogDateTo, '31.02.2026')

    await page.getByTestId('regular-operations-dialog-save').click()

    await expect(page.getByTestId('regular-operations-dialog-date-to')).toContainText('Неверный формат даты.')
    await expect(page.getByTestId('regular-operations-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('удаление помечает карточку на месте, восстановление снимает пометку', async ({ page }) => {
    const stub = await stubApi(page)

    await openScreen(page)

    const card = page.getByTestId('regular-operations-card-1')

    await page.getByTestId('regular-operations-delete-1').click()

    await expect(page.getByTestId('regular-operations-restore-1')).toBeVisible()
    await expect(page.getByTestId('regular-operations-edit-1')).toHaveCount(0)
    await expect(card).toHaveClass(/regular-operation-card--deleted/)
    await expect(page.getByTestId('regular-operations-card-2')).toBeVisible()
    await expect.poll(() => stub.removed).toEqual([1])

    await page.getByTestId('regular-operations-restore-1').click()

    await expect(page.getByTestId('regular-operations-edit-1')).toBeVisible()
    await expect(page.getByTestId('regular-operations-restore-1')).toHaveCount(0)
    await expect(card).not.toHaveClass(/regular-operation-card--deleted/)
    await expect.poll(() => stub.restored).toEqual([1])
  })

  test('удалённая операция не переживает перезагрузку и восстановить её нечем', async ({ page }) => {
    await stubApi(page)

    await openScreen(page)

    await page.getByTestId('regular-operations-delete-1').click()

    await expect(page.getByTestId('regular-operations-restore-1')).toBeVisible()

    await page.reload()

    await expect(page.getByTestId('regular-operations-card-2')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('regular-operations-card-1')).toHaveCount(0)
    await expect(page.getByTestId('regular-operations-restore-1')).toHaveCount(0)
  })

  test('пустой ответ показывает заглушку вместо карточек', async ({ page }) => {
    await stubApi(page, [])

    await page.goto('/regular-operations')

    await expect(page.getByTestId('regular-operations-empty')).toContainText(
      'Регулярных операций пока нет. Добавьте первую.',
      { timeout: FIRST_RENDER_TIMEOUT },
    )
  })

  test('отказ загрузки показывает сообщение сервера и повторяется по кнопке', async ({ page }) => {
    await stubApi(page)

    let isBroken = true

    await routeApi(page, {
      '/RegularOperations': async ({ route, path }) => {
        if (!isBroken || path !== '/RegularOperations') {
          await route.fallback()
          return
        }

        await problem(route, 'Регулярные операции временно недоступны')
      },
    })

    await page.goto('/regular-operations')

    await expect(page.getByTestId('regular-operations-error')).toContainText(
      'Регулярные операции временно недоступны',
      { timeout: FIRST_RENDER_TIMEOUT },
    )
    await expect(page.getByTestId('regular-operations-empty')).toHaveCount(0)

    isBroken = false
    await page.getByTestId('regular-operations-retry').click()

    await expect(page.getByTestId('regular-operations-card-1')).toBeVisible()
    await expect(page.getByTestId('regular-operations-error')).toHaveCount(0)
  })

  test('Home и End переводят выбор на первый и последний пункт, Alt со стрелкой не двигает выбор', async ({ page }) => {
    await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openScreen(page)
    await page.getByTestId('regular-operations-add-1').click()

    const timeType = page.getByTestId('regular-operations-dialog-time-type')
    await expect(timeType).toHaveValue('Каждый месяц')

    await timeType.focus()
    await timeType.press('Home')
    await expect(timeType).toHaveValue('Каждый день')

    await timeType.press('ArrowDown')
    await expect(page.locator('.mud-popover-open')).toBeVisible()

    await timeType.press('Alt+ArrowDown')
    await expect(page.locator('.mud-popover-open')).toBeVisible()
    await expect(timeType).toHaveValue('Каждый день')

    await timeType.press('End')
    await expect(timeType).toHaveValue('Каждый месяц')
  })

  test('буква в выпадающем списке переводит выбор на пункт с этой буквы', async ({ page }) => {
    await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openScreen(page)
    await page.getByTestId('regular-operations-add-1').click()

    const timeType = page.getByTestId('regular-operations-dialog-time-type')
    await expect(timeType).toHaveValue('Каждый месяц')

    await timeType.focus()
    await timeType.press('x')
    await expect(timeType).toHaveValue('Каждый день')

    await timeType.press('ArrowDown')
    await expect(page.locator('.mud-popover-open')).toBeVisible()

    await timeType.dispatchEvent('keydown', { key: 'к' })
    await expect(timeType).toHaveValue('Каждую неделю')

    await timeType.dispatchEvent('keydown', { key: 'к' })
    await expect(timeType).toHaveValue('Каждый месяц')
  })

  test('Ctrl+Shift+Backspace очищает поле с автодополнением', async ({ page }) => {
    await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openScreen(page)
    await page.getByTestId('regular-operations-add-1').click()

    await selectCategory(page, 'Продукты')
    await expect(page.locator(dialogCategory)).toHaveValue('Продукты')

    await page.locator(dialogCategory).focus()
    await page.locator(dialogCategory).press('Control+Shift+Backspace')

    await expect(page.locator(dialogCategory)).toHaveValue('')
  })
})
