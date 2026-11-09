import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { apiPath, empty, identifier, json, problem, routeApi } from './support/api'
import type { CategoryDto, DebtDto } from './support/fixtures'
import { debtOwners as owners, debts, FIXTURE_TODAY } from './support/fixtures'
import { signInAsStub } from './support/session'

interface SaveRequest {
  typeId: number
  sum: number
  comment: string | null
  ownerName: string
  date: string
}

interface PayRequest {
  sum: number
  comment: string | null
  date: string
}

interface ForgiveRequest {
  debtIds: number[]
  operationCategoryId: number
  operationComment: string | null
}

interface DebtsStub {
  queries: Record<string, string>[]
  created: SaveRequest[]
  updated: { id: number, body: SaveRequest }[]
  removed: number[]
  restored: number[]
  payments: { id: number, body: PayRequest }[]
  forgiven: ForgiveRequest[]
  merges: string[]
  ownerRequests: number
}

const categories: CategoryDto[] = [
  { id: 1, name: 'Продукты', parentId: null, order: null, color: null, operationTypeId: 1 },
  { id: 2, name: 'Зарплата', parentId: null, order: null, color: null, operationTypeId: 2 },
]

const TODAY = FIXTURE_TODAY
const FIRST_RENDER_TIMEOUT = 30_000

const dialogOwner = '[data-testid="debts-dialog-owner"] input'
const dialogSum = '[data-testid="debts-dialog-sum"] [data-testid="sum-numeric"] input'
const dialogDate = '[data-testid="debts-dialog-date"] input'
const dialogComment = '[data-testid="debts-dialog-comment"] textarea'
const paymentSum = '[data-testid="debts-payment-sum"] [data-testid="sum-numeric"] input'
const paymentDate = '[data-testid="debts-payment-date"] input'
const paymentComment = '[data-testid="debts-payment-comment"] input'
const forgiveCategory = '[data-testid="debts-forgive-category-search"] input'

async function stubApi(page: Page, initial: DebtDto[] = debts): Promise<DebtsStub> {
  const state = initial.map(debt => ({ ...debt }))
  const names = new Map(owners.map(owner => [owner.id, owner.name]))
  const deleted = new Set<number>()

  const stub: DebtsStub = {
    queries: [],
    created: [],
    updated: [],
    removed: [],
    restored: [],
    payments: [],
    forgiven: [],
    merges: [],
    ownerRequests: 0,
  }

  let lastId = 100

  await routeApi(page, {
    '/Categories': ({ route }) => json(route, categories),
    '/Debts': async ({ route, method, path, query }) => {
      if (path === '/Debts/Owners') {
        stub.ownerRequests += 1
        await json(route, [...names].map(([id, name]) => ({ id, name })))
        return
      }

      if (path.startsWith('/Debts/Owners/')) {
        await json(route, [])
        return
      }

      if (path.startsWith('/Debts/MergeOwners/')) {
        const fromUserId = identifier(path, 0)
        const toUserId = identifier(path, 1)

        stub.merges.push(`${fromUserId}/${toUserId}`)

        for (const debt of state) {
          if (debt.ownerName === names.get(fromUserId)) {
            debt.ownerName = names.get(toUserId) ?? debt.ownerName
          }
        }

        names.delete(fromUserId)
        await empty(route)
        return
      }

      if (path === '/Debts/Forgive') {
        const body = route.request().postDataJSON() as ForgiveRequest

        stub.forgiven.push(body)

        for (const id of body.debtIds) {
          deleted.add(id)
        }

        await empty(route)
        return
      }

      if (path.endsWith('/Pay')) {
        const id = identifier(path)
        const body = route.request().postDataJSON() as PayRequest
        const debt = state.find(candidate => candidate.id === id)

        stub.payments.push({ id, body })

        if (debt !== undefined) {
          debt.paySum += body.sum
          debt.payComment = `${body.date.slice(0, 10)} ${body.sum} ${body.comment ?? ''}`
        }

        await empty(route)
        return
      }

      if (path.endsWith('/Restore')) {
        const id = identifier(path)

        deleted.delete(id)
        stub.restored.push(id)
        await empty(route)
        return
      }

      if (path === '/Debts') {
        if (method === 'POST') {
          const body = route.request().postDataJSON() as SaveRequest

          lastId += 1
          stub.created.push(body)
          state.push({ ...body, id: lastId, paySum: 0, payComment: null, isDeleted: false })
          await json(route, lastId)
          return
        }

        const parameters = Object.fromEntries(query)

        stub.queries.push(parameters)

        await json(route, state.filter(debt => !deleted.has(debt.id) && (parameters.withPaid === 'true' || debt.paySum < debt.sum)))
        return
      }

      const id = identifier(path)
      const debt = state.find(candidate => candidate.id === id)

      if (method === 'PUT') {
        const body = route.request().postDataJSON() as SaveRequest

        stub.updated.push({ id, body })

        if (debt !== undefined) {
          Object.assign(debt, body)
        }

        await empty(route)
        return
      }

      if (method === 'DELETE') {
        deleted.add(id)
        stub.removed.push(id)
        await empty(route)
        return
      }

      await json(route, debt ?? null)
    },
  })

  return stub
}

async function openDebts(page: Page): Promise<void> {
  await page.goto('/debts')
  await expect(page.getByTestId('debts-card-1')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

async function selectOption(page: Page, name: string): Promise<void> {
  await page.locator('.mud-popover-open').getByText(name, { exact: true }).click()
}

async function fillField(page: Page, selector: string, value: string): Promise<void> {
  await page.locator(selector).fill(value)
  await page.locator(selector).blur()
}

test.describe('долги', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('показ погашенных перезапрашивает список с withPaid', async ({ page }) => {
    const stub = await stubApi(page)

    await openDebts(page)

    await expect(page.getByTestId('debts-card-3')).toHaveCount(0)
    expect(stub.queries).toEqual([{ withPaid: 'false' }])

    await page.getByTestId('debts-toggle-paid').click()

    await expect(page.getByTestId('debts-card-3')).toBeVisible()
    expect(stub.queries.at(-1)).toEqual({ withPaid: 'true' })

    await page.getByTestId('debts-toggle-paid').click()

    await expect(page.getByTestId('debts-card-3')).toHaveCount(0)
  })

  test('первый фокус в поле держателя сразу показывает подсказки', async ({ page }) => {
    await stubApi(page)
    await page.route(url => apiPath(url).startsWith('/Debts/Owners/'), route => json(route, ['Анна', 'Сергей']))

    await openDebts(page)

    await page.getByTestId('debts-add-1').click()
    await page.locator(dialogOwner).focus()

    await expect(page.getByTestId('owner-suggestions')).toContainText('Анна')
  })

  test('создание из диалога уходит POST и добавляет держателя', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openDebts(page)

    await page.getByTestId('debts-add-1').click()

    await expect(page.getByTestId('debts-dialog-type')).toHaveValue('Нужно забрать')
    await expect(page.locator(dialogDate)).toHaveValue('03.02.2026')
    await expect(page.locator(dialogSum)).toHaveValue('0')

    await fillField(page, dialogOwner, 'Сергей')
    await fillField(page, dialogSum, '750,50')
    await fillField(page, dialogDate, '04.02.2026')
    await page.locator(dialogComment).fill('за краску')

    await page.getByTestId('debts-dialog-save').click()

    await expect(page.getByTestId('debts-notice')).toContainText('Успех!')
    expect(stub.created).toEqual([{
      typeId: 1,
      sum: 750.5,
      comment: 'за краску',
      ownerName: 'Сергей',
      date: '2026-02-04T00:00:00',
    }])

    const owner = page.getByTestId('debts-owner-Сергей')

    await expect(owner).toBeVisible()
    await expect(owner.getByTestId('debts-card-remainder-101')).toHaveText('750,50')
  })

  test('правка из карточки уходит PUT и переносит долг к новому держателю', async ({ page }) => {
    const stub = await stubApi(page)

    await openDebts(page)

    const card = page.getByTestId('debts-card-2')

    await card.click()
    await card.getByTestId('debts-card-edit-2').click()

    await expect(page.locator(dialogOwner)).toHaveValue('Анна')
    await expect(page.locator(dialogSum)).toHaveValue('500')

    await fillField(page, dialogOwner, 'Анна Смирнова')
    await fillField(page, dialogSum, '620')

    await page.getByTestId('debts-dialog-save').click()

    await expect(page.getByTestId('debts-notice')).toContainText('Успех!')
    expect(stub.updated).toEqual([{
      id: 2,
      body: {
        typeId: 2,
        sum: 620,
        comment: null,
        ownerName: 'Анна Смирнова',
        date: '2026-01-10T00:00:00',
      },
    }])

    await expect(page.getByTestId('debts-owner-Анна Смирнова')).toBeVisible()
    await expect(page.getByTestId('debts-owner-Анна')).toHaveCount(0)
  })

  test('незаполненная форма и нераспознанная сумма не отправляют долг', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openDebts(page)

    await page.getByTestId('debts-add').click()
    await page.getByTestId('debts-dialog-save').click()

    await expect(page.getByText('Заполни меня')).toHaveCount(2)
    await expect(page.getByTestId('debts-dialog')).toBeVisible()

    await page.getByTestId('debts-dialog-type').click()
    await selectOption(page, 'Нужно отдать')
    await fillField(page, dialogOwner, 'Сергей')
    await fillField(page, dialogSum, 'абв')

    await page.getByTestId('debts-dialog-save').click()

    await expect(page.getByTestId('debts-dialog-notice')).toHaveText("Нераспознано значение в поле 'сумма'.")
    await expect(page.getByTestId('debts-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('удаление помечает карточку, восстановление возвращает действия', async ({ page }) => {
    const stub = await stubApi(page)

    await openDebts(page)

    const card = page.getByTestId('debts-card-1')

    await card.click()
    await card.getByTestId('debts-card-delete-1').click()

    await expect(card).toHaveClass(/debt-card--deleted/)
    await expect(card.getByTestId('debts-card-restore-1')).toBeVisible()
    await expect(card.getByTestId('debts-card-edit-1')).toHaveCount(0)
    await expect(card.getByTestId('debts-card-pay-1')).toBeDisabled()
    await expect.poll(() => stub.removed).toEqual([1])

    await card.getByTestId('debts-card-restore-1').click()

    await expect(card).not.toHaveClass(/debt-card--deleted/)
    await expect(card.getByTestId('debts-card-edit-1')).toBeVisible()
    await expect.poll(() => stub.restored).toEqual([1])
  })

  test('пометка удаления и показ погашенных не переживают перезагрузку', async ({ page }) => {
    const stub = await stubApi(page)

    await openDebts(page)

    const card = page.getByTestId('debts-card-1')

    await card.click()
    await card.getByTestId('debts-card-delete-1').click()
    await expect(card).toHaveClass(/debt-card--deleted/)

    await page.getByTestId('debts-toggle-paid').click()
    await expect(page.getByTestId('debts-card-3')).toBeVisible()

    await page.reload()

    await expect(page.getByTestId('debts-card-2')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('debts-card-1')).toHaveCount(0)
    await expect(page.getByTestId('debts-card-3')).toHaveCount(0)
    expect(stub.queries.at(-1)).toEqual({ withPaid: 'false' })
  })

  test('оплата уходит остатком и обновляет карточку', async ({ page }) => {
    const stub = await stubApi(page)

    await page.clock.setFixedTime(new Date(TODAY))
    await openDebts(page)

    const card = page.getByTestId('debts-card-1')

    await expect(card.getByTestId('debts-card-remainder-1')).toHaveText('750,00')

    await card.click()
    await card.getByTestId('debts-card-pay-1').click()

    await expect(page.locator(paymentSum)).toHaveValue('750')
    await expect(page.locator(paymentDate)).toHaveValue('03.02.2026')

    await page.locator(paymentComment).fill('последний взнос')
    await page.getByTestId('debts-payment-save').click()

    await expect.poll(() => stub.payments).toEqual([{
      id: 1,
      body: { sum: 750, comment: 'последний взнос', date: '2026-02-03T00:00:00' },
    }])

    await expect(page.getByTestId('debts-payment')).toHaveCount(0)
    await expect(card.getByTestId('debts-card-remainder-1')).toHaveText('0,00')
    await expect(card).toHaveClass(/debt-card--paid/)
  })

  test('прощение уходит выбранными долгами и категорией расходов', async ({ page }) => {
    const stub = await stubApi(page)

    await openDebts(page)

    await expect(page.getByTestId('debts-card-select-1')).toHaveCount(0)

    await page.getByTestId('debts-toggle-forgive').click()

    await expect(page.getByTestId('debts-card-select-1')).toBeVisible()
    await expect(page.getByTestId('debts-card-select-2')).toHaveCount(0)

    await page.getByTestId('debts-forgive-select-all').click()

    await expect(page.getByTestId('debts-forgive-count')).toHaveText('Выбрано долгов: 1')

    await page.locator(forgiveCategory).click()
    await page.getByTestId('debts-forgive-category-tree')
      .locator('.mud-treeview-item', { hasText: 'Продукты' })
      .first()
      .click()
    await page.getByTestId('debts-forgive-category-toggle').click()
    await page.getByTestId('debts-forgive-submit').click()

    await expect(page.getByTestId('debts-notice')).toContainText('Успешно прощено долгов: 1')
    expect(stub.forgiven).toEqual([{
      debtIds: [1],
      operationCategoryId: 1,
      operationComment: 'Перенос из долгов:',
    }])

    await expect(page.getByTestId('debts-card-1')).toHaveCount(0)
  })

  test('панель прощения переживает клик по карточке, отметка копится на ней', async ({ page }) => {
    await stubApi(page)

    await openDebts(page)

    await page.getByTestId('debts-toggle-forgive').click()
    await page.getByTestId('debts-card-select-1').click()

    await expect(page.getByTestId('debts-forgive-count')).toHaveText('Выбрано долгов: 1')
    await expect(page.getByTestId('debts-forgive-submit')).toBeVisible()
  })

  test('держатели грузятся при открытии панели, объединение перечитывает долги', async ({ page }) => {
    const stub = await stubApi(page)

    await openDebts(page)

    expect(stub.ownerRequests).toBe(0)

    await page.getByTestId('debts-toggle-merge').click()

    await expect.poll(() => stub.ownerRequests).toBe(1)

    await page.getByTestId('debts-merge-from').click()
    await selectOption(page, 'Пётр')
    await page.getByTestId('debts-merge-to').click()
    await selectOption(page, 'Анна')

    await page.getByTestId('debts-merge-swap').click()

    await expect(page.getByTestId('debts-merge-from')).toHaveValue('Анна')
    await expect(page.getByTestId('debts-merge-to')).toHaveValue('Пётр')

    await page.getByTestId('debts-merge-submit').click()

    await expect(page.getByTestId('debts-notice')).toContainText('Долги объединены')
    expect(stub.merges).toEqual(['20/10'])
    await expect(page.getByTestId('debts-type-2').getByTestId('debts-owner-Пётр')).toBeVisible()
  })

  test('пустой ответ показывает заглушку вместо разделов', async ({ page }) => {
    await stubApi(page, [])

    await page.goto('/debts')

    await expect(page.getByTestId('debts-empty')).toHaveText('Нет данных для отображения', { timeout: FIRST_RENDER_TIMEOUT })
  })

  test('отказ загрузки показывает сообщение сервера вместо списка', async ({ page }) => {
    await stubApi(page)

    await routeApi(page, {
      '/Debts': async ({ route, path }) => {
        if (path !== '/Debts') {
          await route.fallback()
          return
        }

        await problem(route, 'Долги временно недоступны')
      },
    })

    await page.goto('/debts')

    await expect(page.getByTestId('debts-error')).toHaveText('Долги временно недоступны', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('debts-empty')).toHaveCount(0)
  })
})
