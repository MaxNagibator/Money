import type { Locator, Page } from '@playwright/test'
import { expect, test } from './support/test'
import { json, problem, routeApi } from './support/api'
import type { OperationDto } from './support/fixtures'
import { categories, FIXTURE_TODAY, operations } from './support/fixtures'
import { signInAsStub } from './support/session'

interface StatisticsStub {
  queries: Record<string, string>[]
}

const TODAY = FIXTURE_TODAY
const FIRST_RENDER_TIMEOUT = 30_000
const DATE_LENGTH = 10

const dateRange = '[data-testid="operations-filter-date-range"] input'
const dateFrom = `${dateRange} >> nth=0`
const dateTo = `${dateRange} >> nth=1`
const comment = '[data-testid="operations-filter-comment"] input'

async function stubApi(page: Page, seed: OperationDto[] = operations): Promise<StatisticsStub> {
  const stub: StatisticsStub = { queries: [] }

  await routeApi(page, {
    '/Categories': ({ route }) => json(route, categories),
    '/Operations': async ({ route, path, query }) => {
      if (path.includes('/GetPlaces/') || path.includes('/InferCategory/')) {
        await json(route, [])
        return
      }

      const parameters = Object.fromEntries(query)

      stub.queries.push(parameters)

      await json(route, seed.filter(operation => matches(operation, parameters)))
    },
  })

  return stub
}

function matches(operation: OperationDto, query: Record<string, string>): boolean {
  const date = operation.date.slice(0, DATE_LENGTH)
  const from = query.DateFrom?.slice(0, DATE_LENGTH)
  const to = query.DateTo?.slice(0, DATE_LENGTH)
  const ids = query.CategoryIds?.split(',')

  if (from !== undefined && date < from) {
    return false
  }

  if (to !== undefined && date >= to) {
    return false
  }

  if (ids !== undefined && !ids.includes(String(operation.categoryId))) {
    return false
  }

  return query.Comment === undefined || operation.comment === query.Comment
}

function tree(page: Page, typeId: number): Locator {
  return page.getByTestId(`statistics-tree-${typeId}`)
}

function totalChip(page: Page, typeId: number): Locator {
  return tree(page, typeId).getByText('Всего', { exact: true }).locator('..').locator('.mud-chip')
}

async function openStatistics(page: Page): Promise<void> {
  await page.goto('/statistics')
  await expect(totalChip(page, 1)).toContainText('300,50', { timeout: FIRST_RENDER_TIMEOUT })
}

async function fillDate(page: Page, selector: string, value: string): Promise<void> {
  await page.locator(selector).fill(value)
  await page.locator(selector).blur()
}

test.describe('статистика', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('панели перестраиваются только после поиска и по заданному фильтру', async ({ page }) => {
    const stub = await stubApi(page)

    await openStatistics(page)

    await fillDate(page, dateFrom, '05.02.2026')
    await fillDate(page, dateTo, '05.02.2026')
    await page.locator(comment).fill('обед')

    await expect(totalChip(page, 1)).toContainText('300,50')
    expect(stub.queries).toHaveLength(1)

    await page.getByTestId('operations-filter-search').click()

    await expect.poll(() => stub.queries.at(-1)).toEqual({
      DateFrom: '2026-02-05T00:00:00',
      DateTo: '2026-02-06T00:00:00',
      Comment: 'обед',
    })

    await expect(totalChip(page, 1)).toContainText('100,50')
    await expect(page.getByTestId('statistics-empty-2')).toHaveText('За выбранный период операций нет')
    await expect(page.getByTestId('statistics-pie-2')).toHaveCount(0)
  })

  test('период переживает перезагрузку, комментарий нет', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openStatistics(page)

    await page.getByTestId('operations-filter-interval-month').click()
    await page.locator(comment).fill('обед')
    await page.getByTestId('operations-filter-search').click()

    await expect.poll(() => stub.queries.at(-1)).toEqual({
      DateFrom: '2026-02-01T00:00:00',
      DateTo: '2026-03-01T00:00:00',
      Comment: 'обед',
    })

    await page.reload()

    await expect(page.locator(dateFrom)).toHaveValue('01.02.2026', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.locator(dateTo)).toHaveValue('28.02.2026')
    await expect(page.locator(comment)).toHaveValue('')

    await expect.poll(() => stub.queries.at(-1)).toEqual({
      DateFrom: '2026-02-01T00:00:00',
      DateTo: '2026-03-01T00:00:00',
    })

    await expect(totalChip(page, 1)).toContainText('300,50')
  })

  test('кнопки разворачивают и сворачивают дерево своей панели', async ({ page }) => {
    await stubApi(page)

    await openStatistics(page)

    await expect(tree(page, 1).getByText('Кафе')).toBeHidden()

    await page.getByTestId('statistics-expand-1').click()

    await expect(tree(page, 1).getByText('Кафе')).toBeVisible()
    await expect(tree(page, 1).getByText('Еда')).toHaveCount(2)
    await expect(tree(page, 2).getByText('Зарплата')).toBeHidden()

    await page.getByTestId('statistics-collapse-1').click()

    await expect(tree(page, 1).getByText('Кафе')).toBeHidden()
    await expect(totalChip(page, 1)).toContainText('300,50')
  })

  test('заголовок панели сворачивает её содержимое, не трогая соседнюю', async ({ page }) => {
    await stubApi(page)

    await openStatistics(page)

    await expect(page.getByTestId('statistics-bar-1')).toBeVisible()

    await page.getByTestId('statistics-panel-toggle-1').click()

    await expect(page.getByTestId('statistics-bar-1')).toBeHidden()
    await expect(page.getByTestId('statistics-bar-2')).toBeVisible()

    await page.getByTestId('statistics-panel-toggle-1').click()

    await expect(page.getByTestId('statistics-bar-1')).toBeVisible()
  })

  test('панель сворачивается анимацией высоты, а не мгновенно', async ({ page }) => {
    await stubApi(page)

    await openStatistics(page)

    const collapse = page.locator('[data-testid="statistics-panel-1"] .mud-collapse-container').first()

    await expect(collapse).toHaveClass(/mud-collapse-entered/)
    await expect(collapse).toHaveCSS('transition-property', 'grid-template-rows')
    await expect(collapse).toHaveCSS('transition-duration', '0.3s')

    await page.getByTestId('statistics-panel-toggle-1').click()

    await expect(collapse).toHaveClass(/invisible/)

    await page.getByTestId('statistics-panel-toggle-1').click()

    await expect(collapse).toHaveClass(/mud-collapse-entered/)
  })

  test('пустая выдача даёт заглушку в обеих панелях вместо диаграмм', async ({ page }) => {
    await stubApi(page, [])

    await page.goto('/statistics')

    await expect(page.getByTestId('statistics-empty-1')).toHaveText('За выбранный период операций нет', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('statistics-empty-2')).toBeVisible()
    await expect(page.getByTestId('statistics-bar-1')).toHaveCount(0)
    await expect(page.getByTestId('statistics-tree-1')).toHaveCount(0)
  })

  test('отказ загрузки операций показывает сообщение сервера вместо панелей', async ({ page }) => {
    await stubApi(page)

    await routeApi(page, {
      '/Operations': ({ route }) => problem(route, 'Операции временно недоступны'),
    })

    await page.goto('/statistics')

    await expect(page.getByTestId('statistics-error')).toHaveText('Операции временно недоступны', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('statistics-tree-1')).toHaveCount(0)
    await expect(page.getByTestId('statistics-categories-error')).toHaveCount(0)
  })

  test('отказ загрузки категорий предупреждает отдельно от операций', async ({ page }) => {
    await stubApi(page)

    await routeApi(page, {
      '/Categories': ({ route }) => problem(route, 'Категории временно недоступны'),
    })

    await page.goto('/statistics')

    await expect(page.getByTestId('statistics-categories-error')).toHaveText('Категории временно недоступны', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('statistics-error')).toHaveCount(0)
    await expect(page.getByTestId('statistics-empty-1')).toBeVisible()
  })
})
