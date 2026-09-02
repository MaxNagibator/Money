import type { Locator, Page } from '@playwright/test'
import { expect, test } from './support/test'
import { apiPath, created, empty, identifier, json, problem, routeApi } from './support/api'
import type { FastOperationDto, OperationDto } from './support/fixtures'
import { categories, FIXTURE_TODAY, operations, places } from './support/fixtures'
import { signInAsStub } from './support/session'

type SaveRequest = Omit<OperationDto, 'id' | 'createdTaskId'>

interface TransferRequest {
  operationIds: number[]
  categoryId: number
}

interface OperationsStub {
  queries: Record<string, string>[]
  created: SaveRequest[]
  createdBodies: string[]
  transfers: TransferRequest[]
}

const quickOperations: FastOperationDto[] = [
  { id: 20, name: 'Обед', order: 1, categoryId: 2, sum: 250, comment: 'бизнес-ланч', place: 'Пекарня' },
  { id: 21, name: 'Кофе', order: 2, categoryId: 2, sum: 100, comment: null, place: null },
]

const manyOperations: OperationDto[] = Array.from({ length: 30 }, (_, index) => ({
  id: 200 + index,
  categoryId: 1,
  sum: 10 + index,
  comment: null,
  place: null,
  date: `2026-01-${String(index + 1).padStart(2, '0')}T00:00:00`,
  createdTaskId: null,
}))

const deferredOperations: OperationDto[] = Array.from({ length: 40 }, (_, index) => ({
  id: 300 + index,
  categoryId: 1,
  sum: 10 + index,
  comment: null,
  place: null,
  date: `${new Date(Date.UTC(2026, 0, 1 + index)).toISOString().slice(0, 10)}T00:00:00`,
  createdTaskId: null,
}))

const shortOperations: OperationDto[] = Array.from({ length: 3 }, (_, index) => ({
  id: 400 + index,
  categoryId: 1,
  sum: 10 + index,
  comment: null,
  place: null,
  date: `2026-01-0${index + 1}T00:00:00`,
  createdTaskId: null,
}))

const TODAY = FIXTURE_TODAY
const FIRST_RENDER_TIMEOUT = 30_000

const dateRange = '[data-testid="operations-filter-date-range"] input'
const dateFrom = `${dateRange} >> nth=0`
const dateTo = `${dateRange} >> nth=1`
const comment = '[data-testid="operations-filter-comment"] input'
const place = '[data-testid="operations-filter-place"] input'
const categorySearch = '[data-testid="operations-filter-category-search"] input'
const dialogSum = '[data-testid="operation-dialog-sum"] [data-testid="sum-numeric"] input'
const dialogExpression = '[data-testid="operation-dialog-sum"] [data-testid="sum-expression"] input'
const dialogDate = '[data-testid="operation-dialog-date"] input'
const dialogCategory = '[data-testid="operation-dialog-category"] input'
const dialogPlace = '[data-testid="operation-dialog-place"] input'
const dialogComment = '[data-testid="operation-dialog-comment"] textarea'

async function stubApi(page: Page, initial: OperationDto[] = operations): Promise<OperationsStub> {
  const state = initial.map(operation => ({ ...operation }))
  const deleted = new Set<number>()
  const deletedTemplates = new Set<number>()
  const stub: OperationsStub = { queries: [], created: [], createdBodies: [], transfers: [] }
  let lastId = 100

  await routeApi(page, {
    '/Categories': ({ route }) => json(route, categories),
    '/FastOperations': async ({ route, method, path }) => {
      if (method === 'DELETE') {
        deletedTemplates.add(identifier(path))
        await empty(route)
        return
      }

      await json(route, quickOperations.filter(template => !deletedTemplates.has(template.id)))
    },
    '/Operations': async ({ route, method, path, query }) => {
      if (path.includes('/GetPlaces/')) {
        await json(route, [])
        return
      }

      if (path.includes('/InferCategory/')) {
        await json(route, {})
        return
      }

      if (path.endsWith('/UpdateBatch')) {
        const body = route.request().postDataJSON() as TransferRequest

        stub.transfers.push(body)

        for (const operation of state) {
          if (body.operationIds.includes(operation.id)) {
            operation.categoryId = body.categoryId
          }
        }

        await empty(route)
        return
      }

      if (path.endsWith('/Restore')) {
        deleted.delete(identifier(path))
        await empty(route)
        return
      }

      if (method === 'DELETE') {
        deleted.add(identifier(path))
        await empty(route)
        return
      }

      if (method === 'POST') {
        const body = route.request().postDataJSON() as SaveRequest

        lastId += 1
        stub.created.push(body)
        stub.createdBodies.push(route.request().postData() ?? '')
        state.push({ ...body, id: lastId, createdTaskId: null })
        await created(route, lastId)
        return
      }

      stub.queries.push(Object.fromEntries(query))

      await json(route, state.filter(operation => !deleted.has(operation.id)))
    },
  })

  return stub
}

async function openOperations(page: Page): Promise<void> {
  await page.goto('/operations')
  await expect(page.getByTestId('operations-day')).toHaveCount(2, { timeout: FIRST_RENDER_TIMEOUT })
}

async function fillDate(page: Page, selector: string, value: string): Promise<void> {
  await page.locator(selector).fill(value)
  await page.locator(selector).blur()
}

function filterCategory(page: Page, name: string): Locator {
  return page.getByTestId('operations-filter-category-tree')
    .locator('.mud-treeview-item', { hasText: name })
    .getByRole('checkbox')
}

async function openFilterCategories(page: Page): Promise<void> {
  await page.locator(categorySearch).click()
  await expect(page.getByTestId('operations-filter-category-popover')).toHaveClass(/mud-popover-open/)
}

async function closeFilterCategories(page: Page): Promise<void> {
  await page.getByTestId('operations-filter-category-toggle').click()
  await expect(page.getByTestId('operations-filter-category-popover')).not.toHaveClass(/mud-popover-open/)
}

async function selectFilterCategory(page: Page, name: string): Promise<void> {
  await openFilterCategories(page)
  await filterCategory(page, name).check()
  await closeFilterCategories(page)
}

async function goToSection(page: Page, title: string): Promise<void> {
  const menu = page.getByTestId('nav-menu')
  const link = menu.getByRole('link', { name: title, exact: true })

  if (await link.isVisible() === false) {
    await menu.getByText('Операции', { exact: true }).click()
  }

  await link.click()
}

async function selectDialogCategory(page: Page, name: string): Promise<void> {
  await page.locator(dialogCategory).fill(name)
  await page.getByTestId('category-suggestions').getByText(name, { exact: true }).click()
}

test.describe('операции', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('поиск уходит ровно по заданному фильтру', async ({ page }) => {
    const stub = await stubApi(page)

    await openOperations(page)

    await fillDate(page, dateFrom, '01.02.2026')
    await fillDate(page, dateTo, '05.02.2026')
    await selectFilterCategory(page, 'Еда')
    await page.getByTestId('operations-filter-search').click()

    await expect.poll(() => stub.queries.at(-1)).toEqual({
      DateFrom: '2026-02-01T00:00:00',
      DateTo: '2026-02-06T00:00:00',
      CategoryIds: '1,2',
    })
  })

  test('даты и интервал переживают перезагрузку, остальные фильтры нет', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openOperations(page)

    await page.getByTestId('operations-filter-interval-month').click()

    await expect(page.locator(dateFrom)).toHaveValue('01.02.2026')
    await expect(page.locator(dateTo)).toHaveValue('28.02.2026')

    await page.locator(comment).fill('обед')
    await page.locator(place).fill('Кафе у дома')
    await selectFilterCategory(page, 'Еда')
    await page.getByTestId('operations-filter-search').click()

    await expect.poll(() => stub.queries.at(-1)).toEqual({
      DateFrom: '2026-02-01T00:00:00',
      DateTo: '2026-03-01T00:00:00',
      CategoryIds: '1,2',
      Comment: 'обед',
      Place: 'Кафе у дома',
    })

    await page.reload()

    await expect(page.locator(dateFrom)).toHaveValue('01.02.2026')
    await expect(page.locator(dateTo)).toHaveValue('28.02.2026')
    await expect(page.getByTestId('operations-filter-interval-month')).toHaveAttribute('aria-checked', 'true')
    await expect(page.locator(comment)).toHaveValue('')
    await expect(page.locator(place)).toHaveValue('')

    await expect.poll(() => stub.queries.at(-1)).toEqual({
      DateFrom: '2026-02-01T00:00:00',
      DateTo: '2026-03-01T00:00:00',
    })
  })

  test('категории и комментарий переживают переход на статистику и обратно', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openOperations(page)

    await page.locator(comment).fill('обед')
    await selectFilterCategory(page, 'Еда')
    await page.getByTestId('operations-filter-search').click()

    await expect.poll(() => stub.queries.at(-1)).toEqual({
      CategoryIds: '1,2',
      Comment: 'обед',
    })

    const applied = stub.queries.length

    await goToSection(page, 'Статистика')

    await expect(page.getByTestId('statistics-tree-1')).toBeVisible()
    await expect(page.locator(comment)).toHaveValue('обед')
    await openFilterCategories(page)
    await expect(filterCategory(page, 'Еда')).toBeChecked()
    await closeFilterCategories(page)
    expect(stub.queries).toHaveLength(applied)

    await goToSection(page, 'Список')

    await expect(page.getByTestId('operations-day')).toHaveCount(2)
    await expect(page.locator(comment)).toHaveValue('обед')
    await openFilterCategories(page)
    await expect(filterCategory(page, 'Еда')).toBeChecked()
    await closeFilterCategories(page)
    expect(stub.queries).toHaveLength(applied)
  })

  test('поиск по дереву категорий скрывает чужие узлы и не трогает соседний список', async ({ page }) => {
    await stubApi(page)
    await openOperations(page)
    await openFilterCategories(page)

    const tree = page.getByTestId('operations-filter-category-tree')

    await page.locator(categorySearch).fill('Зарп')

    await expect(tree.locator('.mud-treeview-item', { hasText: 'Зарплата' }).first()).toBeVisible()
    await expect(tree.locator('.mud-treeview-item', { hasText: 'Еда' }).first()).toBeHidden()

    await closeFilterCategories(page)
    await page.getByTestId('operations-filter-transfer').click()
    await page.locator('[data-testid="operations-filter-transfer-category-search"] input').click()

    await expect(page.getByTestId('operations-filter-transfer-category-tree')
      .locator('.mud-treeview-item', { hasText: 'Еда' })
      .first()).toBeVisible()

    await page.getByTestId('operations-filter-transfer-category-toggle').click()
    await page.getByTestId('operations-filter-transfer-close').click()
    await openFilterCategories(page)
    await page.locator(categorySearch).fill('')

    await expect(tree.locator('.mud-treeview-item', { hasText: 'Еда' }).first()).toBeVisible()
  })

  test('создаёт операцию из диалога и меняет итог периода', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openOperations(page)
    await expect(page.getByTestId('operations-total-1')).toContainText('300,50')

    await page.getByTestId('operations-add-1').click()

    await page.locator(dialogSum).press('+')
    await page.locator(dialogExpression).fill('100+50')
    await page.locator(dialogExpression).press('Enter')
    await expect(page.locator(dialogSum)).toHaveValue('150')

    await fillDate(page, dialogDate, 'з')
    await expect(page.locator(dialogDate)).toHaveValue('04.02.2026')

    await selectDialogCategory(page, 'Кафе')
    await page.getByTestId('operation-dialog-save').click()

    await expect(page.getByTestId('operations-notice')).toContainText('Операция сохранена')
    expect(stub.created).toEqual([{
      categoryId: 2,
      sum: 150,
      comment: null,
      place: null,
      date: '2026-02-04T00:00:00',
    }])

    const day = page.locator('[data-date="2026-02-04"]')

    await expect(day.getByTestId('operation-item-category')).toHaveText('Кафе')
    await expect(day.getByTestId('operation-item-sum')).toHaveText('150,00')
    await expect(page.getByTestId('operations-total-1')).toContainText('450,50')
  })

  test('сумма из выражения уходит на сервер всеми цифрами, а не округлённой до копеек', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openOperations(page)

    await page.getByTestId('operations-add-1').click()

    await page.locator(dialogSum).press('+')
    await page.locator(dialogExpression).fill('10/3')
    await page.locator(dialogExpression).press('Enter')
    await expect(page.locator(dialogSum)).toHaveValue('3,333333333333333333333333333')

    await selectDialogCategory(page, 'Кафе')
    await page.getByTestId('operation-dialog-save').click()

    await expect(page.getByTestId('operations-notice')).toContainText('Операция сохранена')
    expect(stub.createdBodies).toHaveLength(1)
    expect(stub.createdBodies[0]).toContain('"sum":3.333333333333333333333333333')
  })

  test('нераспознанная сумма не сохраняет операцию', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openOperations(page)

    await page.getByTestId('operations-add-1').click()

    await page.locator(dialogSum).fill('100')
    await page.locator(dialogSum).blur()
    await selectDialogCategory(page, 'Кафе')

    await page.locator(dialogSum).fill('абв')
    await page.locator(dialogSum).blur()
    await page.getByTestId('operation-dialog-save').click()

    await expect(page.getByTestId('operations-notice')).toContainText("Нераспознано значение в поле 'сумма'.")
    await expect(page.getByTestId('operation-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('отказ сохранения оставляет диалог открытым с введёнными значениями', async ({ page }) => {
    const stub = await stubApi(page)

    let isBroken = true

    await routeApi(page, {
      '/Operations': async ({ route, method, path }) => {
        if (!isBroken || method !== 'POST' || path !== '/Operations') {
          await route.fallback()
          return
        }

        await problem(route, 'Операция не сохранена')
      },
    })

    await page.clock.setFixedTime(new Date(TODAY))
    await openOperations(page)

    await page.getByTestId('operations-add-1').click()

    await page.locator(dialogSum).fill('150')
    await page.locator(dialogSum).blur()
    await selectDialogCategory(page, 'Кафе')
    await page.locator(dialogPlace).fill('Пекарня')
    await page.locator(dialogComment).fill('обед')

    await page.getByTestId('operation-dialog-save').click()

    await expect(page.getByTestId('operations-notice')).toContainText('Операция не сохранена')
    await expect(page.getByTestId('operation-dialog')).toBeVisible()
    await expect(page.locator(dialogSum)).toHaveValue('150')
    await expect(page.locator(dialogDate)).toHaveValue('03.02.2026')
    await expect(page.locator(dialogCategory)).toHaveValue('Кафе')
    await expect(page.locator(dialogPlace)).toHaveValue('Пекарня')
    await expect(page.locator(dialogComment)).toHaveValue('обед')
    expect(stub.created).toHaveLength(0)

    isBroken = false
    await page.getByTestId('operation-dialog-save').click()

    await expect(page.getByTestId('operations-notice')).toContainText('Операция сохранена')
    await expect(page.getByTestId('operation-dialog')).toHaveCount(0)
    expect(stub.created).toEqual([{
      categoryId: 2,
      sum: 150,
      comment: 'обед',
      place: 'Пекарня',
      date: '2026-02-03T00:00:00',
    }])
  })

  test('сохранение в полёте не закрывает заново открытый диалог', async ({ page }) => {
    const stub = await stubApi(page)

    let hold!: () => void
    let arrived!: () => void

    const held = new Promise<void>(resolve => {
      hold = resolve
    })

    const reached = new Promise<void>(resolve => {
      arrived = resolve
    })

    let isHeld = true

    await routeApi(page, {
      '/Operations': async ({ route, method, path }) => {
        if (!isHeld || method !== 'POST' || path !== '/Operations') {
          await route.fallback()
          return
        }

        isHeld = false
        arrived()
        await held
        await route.fallback()
      },
    })

    await page.clock.setFixedTime(new Date(TODAY))
    await openOperations(page)

    await page.getByTestId('operations-add-1').click()

    await page.locator(dialogSum).fill('150')
    await page.locator(dialogSum).blur()
    await selectDialogCategory(page, 'Кафе')

    await page.getByTestId('operation-dialog-save').click()
    await reached

    await page.getByTestId('operation-dialog-cancel').click()
    await expect(page.getByTestId('operation-dialog')).toHaveCount(0)

    await page.getByTestId('operations-add-1').click()
    await page.locator(dialogSum).fill('200')
    await page.locator(dialogSum).blur()

    hold()

    await expect(page.getByTestId('operations-notice')).toContainText('Операция сохранена')
    await expect(page.getByTestId('operation-dialog')).toBeVisible()
    await expect(page.locator(dialogSum)).toHaveValue('200')
    expect(stub.created).toHaveLength(1)
  })

  test('удалённая строка остаётся зачёркнутой и переживает ответ сервера без неё', async ({ page }) => {
    await stubApi(page)

    await openOperations(page)
    await expect(page.getByTestId('operation-item')).toHaveCount(3)

    const removed = page.locator('[data-operation-id="10"]')
    const other = page.locator('[data-operation-id="11"]')

    await removed.getByTestId('operation-item-remove').click()

    await expect(removed).toHaveClass(/operation-deleted/)
    await expect(removed.getByTestId('operation-item-restore')).toBeVisible()
    await expect(page.locator('[data-date="2026-02-05"]').getByTestId('operation-item')).toHaveCount(1)

    await other.getByTestId('operation-item-remove').click()
    await expect(other.getByTestId('operation-item-restore')).toBeVisible()

    await other.getByTestId('operation-item-restore').click()

    await expect(other.getByTestId('operation-item-remove')).toBeVisible()
    await expect(other).not.toHaveClass(/operation-deleted/)
    await expect(page.getByTestId('operation-item')).toHaveCount(3)
    await expect(removed).toHaveClass(/operation-deleted/)
  })

  test('перевёрнутый диапазон не уходит в запрос', async ({ page }) => {
    const stub = await stubApi(page)

    await openOperations(page)

    await fillDate(page, dateFrom, '05.02.2026')
    await fillDate(page, dateTo, '01.02.2026')

    await expect(page.getByTestId('operations-filter-range-error')).toHaveText('Дата «по» раньше даты «с»')

    await page.getByTestId('operations-filter-search').click()

    await fillDate(page, dateTo, '10.02.2026')
    await expect(page.getByTestId('operations-filter-range-error')).toHaveCount(0)

    await page.getByTestId('operations-filter-search').click()

    await expect.poll(() => stub.queries).toEqual([
      {},
      { DateFrom: '2026-02-05T00:00:00', DateTo: '2026-02-11T00:00:00' },
    ])
  })

  test('первый фокус в поле места сразу показывает подсказки', async ({ page }) => {
    await stubApi(page)
    await page.route(url => apiPath(url).startsWith('/Operations/GetPlaces/'), route => json(route, places))

    await openOperations(page)

    await page.getByTestId('operations-add-1').click()
    await page.locator(dialogPlace).focus()

    await expect(page.getByTestId('place-suggestions')).toContainText('Пекарня на углу')
  })

  test('пустое поле категории сбрасывает выбор, первая стрелка вниз берёт первый пункт', async ({ page }) => {
    const stub = await stubApi(page)

    await openOperations(page)

    await page.getByTestId('operations-day-quick-2026-02-01').click()
    await page.getByTestId('operations-day-quick-2026-02-01-item-20').click()

    await expect(page.locator(dialogCategory)).toHaveValue('Кафе')

    await page.locator(dialogCategory).fill('')
    await page.getByTestId('operation-dialog-save').click()

    await expect(page.getByText('Категория обязательна')).toBeVisible()
    expect(stub.created).toHaveLength(0)

    await page.locator(dialogCategory).click()
    await page.keyboard.press('ArrowDown')
    await page.keyboard.press('Tab')

    await expect(page.locator(dialogCategory)).toHaveValue('Еда')
  })

  test('быстрая операция из карточки дня открывает диалог с подставленными полями', async ({ page }) => {
    const stub = await stubApi(page)

    await openOperations(page)

    await page.getByTestId('operations-day-quick-2026-02-01').click()
    await page.getByTestId('operations-day-quick-2026-02-01-item-20').click()

    await expect(page.locator(dialogSum)).toHaveValue('250')
    await expect(page.locator(dialogDate)).toHaveValue('01.02.2026')
    await expect(page.locator(dialogCategory)).toHaveValue('Кафе')
    await expect(page.locator(dialogPlace)).toHaveValue('Пекарня')
    await expect(page.locator(dialogComment)).toHaveValue('бизнес-ланч')

    await page.getByTestId('operation-dialog-save').click()

    await expect(page.getByTestId('operations-notice')).toContainText('Операция сохранена')
    expect(stub.created).toEqual([{
      categoryId: 2,
      sum: 250,
      comment: 'бизнес-ланч',
      place: 'Пекарня',
      date: '2026-02-01T00:00:00',
    }])
  })

  test('удалённый шаблон не предлагается в быстром добавлении', async ({ page }) => {
    await stubApi(page)

    await openOperations(page)

    await goToSection(page, 'Быстрые')

    await expect(page.getByTestId('fast-operations-card-20')).toBeVisible()

    await page.getByTestId('fast-operations-delete-20').click()

    await expect(page.getByTestId('fast-operations-restore-20')).toBeVisible()

    await goToSection(page, 'Список')

    await expect(page.getByTestId('operations-day')).toHaveCount(2)

    await page.getByTestId('operations-quick').click()

    const menu = page.getByTestId('operations-quick-list')

    await expect(menu.getByTestId('operations-quick-item-21')).toBeVisible()
    await expect(menu.getByTestId('operations-quick-item-20')).toHaveCount(0)
  })

  test('переключатели отображения меняют список операций', async ({ page }) => {
    await stubApi(page)

    await openOperations(page)

    const day = page.locator('[data-date="2026-02-01"]')
    const rows = day.locator('.mud-card-content')

    await expect(rows).toHaveClass(/operations-money-align/)
    await expect(rows).toHaveAttribute('style', /--money-col-width: 9ch/)
    await expect(day.locator('.operation-row-alt')).toHaveCount(0)
    await expect(day.locator('hr')).toHaveCount(0)

    await page.goto('/settings')
    await expect(page.getByTestId('settings-dividers')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })

    await page.getByTestId('settings-dividers').click()
    await page.getByTestId('settings-alternate-border').click()
    await page.getByTestId('settings-align-decimals').click()

    await openOperations(page)

    await expect(day.locator('.operation-row-alt')).toHaveCount(1)
    await expect(day.locator('hr')).toHaveCount(1)
    await expect(rows).not.toHaveClass(/operations-money-align/)
  })

  test('иконка раскрытия разворачивает ветку дерева категорий', async ({ page }) => {
    await stubApi(page)

    await page.goto('/operations')
    await openFilterCategories(page)

    const tree = page.getByTestId('operations-filter-category-tree')
    const child = tree.getByText('Кафе', { exact: true })

    await expect(child).toBeHidden()

    await tree.locator('.mud-treeview-item-expand-button').first().click()

    await expect(child).toBeVisible()
  })

  test('длинный список держит в разметке только окно прокрутки', async ({ page }) => {
    await stubApi(page, deferredOperations)

    await page.goto('/operations')

    const days = page.getByTestId('operations-day')
    const first = page.locator('[data-date="2026-02-09"]')
    const last = page.locator('[data-date="2026-01-01"]')

    await expect(first).toHaveCount(1, { timeout: FIRST_RENDER_TIMEOUT })
    await expect(last).toHaveCount(0)
    expect(await days.count()).toBeLessThan(40)

    await expect.poll(async () => {
      await page.evaluate(() => {
        window.scrollTo(0, document.documentElement.scrollHeight)
      })

      return last.count()
    }).toBe(1)

    await expect(last.getByTestId('operation-item')).toHaveCount(1)
    await expect(last.getByTestId('operations-day-date')).toHaveText('01.01.2026')
    await expect(first).toHaveCount(0)

    const button = page.getByTestId('operations-list-top')

    await expect(button).toBeVisible()
    await button.click()

    await expect.poll(() => page.evaluate(() => document.documentElement.scrollTop)).toBe(0)
    await expect(first).toHaveCount(1)
  })

  test('список короче окна прокрутки рисуется целиком', async ({ page }) => {
    await stubApi(page, shortOperations)

    await page.goto('/operations')

    const days = page.getByTestId('operations-day')

    await expect(days).toHaveCount(3, { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.locator('[data-date="2026-01-01"]')).toHaveCount(1)
  })

  test('кнопка возврата наверх приходит с прокруткой списка', async ({ page }) => {
    await stubApi(page, manyOperations)

    await page.goto('/operations')

    const days = page.getByTestId('operations-list-days')
    const button = page.getByTestId('operations-list-top')

    await expect(days).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(button).toBeHidden()

    await page.evaluate(() => {
      window.scrollTo(0, 800)
    })

    await expect(button).toBeVisible()

    await button.click()

    await expect.poll(() => page.evaluate(() => document.documentElement.scrollTop)).toBe(0)
    await expect(button).toBeHidden()
  })

  test('перенос уходит идентификаторами показанных операций и выбранной категорией', async ({ page }) => {
    const stub = await stubApi(page)

    await openOperations(page)

    await page.getByTestId('operations-filter-transfer').click()

    const panel = page.getByTestId('operations-filter-transfer-panel')

    await expect(panel).toHaveClass(/mud-popover-open/)
    await expect(page.getByTestId('operations-filter-transfer-submit')).toBeDisabled()

    await page.locator('[data-testid="operations-filter-transfer-category-search"] input').click()
    await page.getByTestId('operations-filter-transfer-category-tree')
      .locator('.mud-treeview-item', { hasText: 'Зарплата' })
      .first()
      .click()
    await page.getByTestId('operations-filter-transfer-category-toggle').click()
    await page.getByTestId('operations-filter-transfer-submit').click()

    await expect.poll(() => stub.transfers).toEqual([{ operationIds: [10, 11, 12], categoryId: 3 }])
    await expect(page.getByTestId('operations-notice')).toContainText('Операции перенесены')
    await expect(panel).not.toHaveClass(/mud-popover-open/)
  })

  test('перенос закрывается без запроса', async ({ page }) => {
    const stub = await stubApi(page)

    await openOperations(page)

    await page.getByTestId('operations-filter-transfer').click()
    await page.getByTestId('operations-filter-transfer-close').click()

    await expect(page.getByTestId('operations-filter-transfer-panel')).not.toHaveClass(/mud-popover-open/)
    expect(stub.transfers).toHaveLength(0)
  })

  test('показ пустых дней достраивает дни без запроса', async ({ page }) => {
    const stub = await stubApi(page)

    await openOperations(page)

    await page.getByTestId('operations-filter-empty-days').click()

    await expect(page.getByTestId('operations-day')).toHaveCount(5)
    await expect(page.locator('[data-date="2026-02-03"]').getByTestId('operation-item')).toHaveCount(0)
    expect(stub.queries).toHaveLength(1)
  })
})
