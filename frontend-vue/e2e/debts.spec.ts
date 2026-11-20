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

interface PayOwnerRequest extends PayRequest {
  ownerName: string
  typeId: number
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
  ownerPayments: { id: number, body: PayOwnerRequest }[]
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

test.describe('отчёты по долгам', () => {
  test('оба режима доступны только для чтения и сохраняют фильтры, сводку и выбор режима', async ({ page }) => {
    const base = { ...debts[0]!, ownerName: 'Иван', paySum: 0, payComment: null }
    const stub = await stubApi(page, [
      { ...base, id: 1, typeId: 1, sum: 1000 },
      { ...base, id: 2, typeId: 2, sum: 400 },
      { ...base, id: 3, ownerName: 'Анна', typeId: 2, sum: 200 },
    ])
    await signInAsStub(page)
    await page.goto('/debts/accounts')
    await expect(page.getByTestId('debts-mode-normal')).toBeChecked()
    await expect(page.getByTestId('debts-report-readonly')).toHaveText('Только просмотр')
    await expect(page.getByTestId('debts-add')).toHaveCount(0)
    await expect(page.getByTestId('debts-toggle-merge')).toHaveCount(0)
    await expect(page.getByTestId('debts-summary-net')).toHaveText('400 ₽')
    await page.getByTestId('debts-search').locator('input').fill('Иван')
    await expect(page.getByTestId('debts-summary-net')).toHaveText('600 ₽')
    await expect(page.getByTestId('debts-charts-filter-label')).toHaveText('По фильтру: «Иван»')
    const account = page.getByTestId('debt-account-Иван')
    await account.getByTestId('debt-account-toggle').click()
    await expect(account.getByTestId('debt-movement-out-1')).toHaveText('−1 000 ₽')
    await expect(account.getByTestId('debt-movement-in-2')).toHaveText('+400 ₽')
    for (const mode of ['accounting', 'rework', 'refined', 'normal']) {
      await page.getByTestId(`debts-mode-${mode}`).check()
      await expect(account.getByTestId('debt-movement-out-1')).toBeVisible()
      await expect(account.locator('[data-testid^="debt-movement-menu-"]')).toHaveCount(0)
      await expect(account.getByTestId('debt-account-outgoing')).toHaveCount(0)
      await expect(account.getByTestId('debt-account-forgive')).toHaveCount(0)
    }
    await page.getByTestId('debts-mode-accounting').check()
    await expect(account.locator('thead th')).toHaveCount(5)
    await expect(account.locator('tfoot')).toContainText('600,00 ₽')
    await page.getByTestId('debts-search').getByRole('button', { name: 'Очистить' }).click()
    await expect(page.getByTestId('debts-summary-net')).toHaveText('400,00 ₽')
    await expect(page.getByTestId('debts-charts-filter-label')).toHaveCount(0)
    await page.reload()
    await expect(page.getByTestId('debts-mode-accounting')).toBeChecked()
    expect(stub.created).toEqual([])
    expect(stub.updated).toEqual([])
    expect(stub.removed).toEqual([])
    expect(stub.restored).toEqual([])
    expect(stub.payments).toEqual([])
    expect(stub.ownerPayments).toEqual([])
    expect(stub.forgiven).toEqual([])
    expect(stub.merges).toEqual([])
    expect(stub.ownerRequests).toBe(0)
  })

  test('закрытые расчёты доступны с датами возвратов', async ({ page }) => {
    await stubApi(page, [{ ...debts[0]!, ownerName: 'Иван', sum: 1000, paySum: 1000, payComment: '2026.01.25 1000 Полностью вернул' }])
    await signInAsStub(page)
    await page.goto('/debts/accounts')
    await expect(page.getByTestId('debts-empty')).toBeVisible()
    await expect(page.getByTestId('debts-history-scope')).toContainText('Удалённые долги')
    await page.getByTestId('debts-toggle-paid').click()
    const account = page.getByTestId('debt-account-Иван')
    await account.getByTestId('debt-account-toggle').click()
    await expect(account.getByTestId('debt-account-balance')).toHaveText('Расчёты закрыты')
    await expect(account).toContainText('25.01.2026')
    await expect(account).toContainText('Полностью вернул')
  })

  test('неразбираемая старая история сохраняется целиком с неизвестной датой', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 })
    await stubApi(page, [{ ...debts[0]!, ownerName: 'Иван', paySum: 300, payComment: '2026.01.25 300 перевод\nдополнение к платежу' }])
    await signInAsStub(page)
    await page.goto('/debts/accounts')
    const account = page.getByTestId('debt-account-Иван')
    await account.getByTestId('debt-account-toggle').click()
    await expect(account).toContainText('дополнение к платежу')
    await expect(account).toContainText('Дата не сохранена')
    await expect(account.getByTestId('debt-account-balance')).toContainText('700')
  })

  test('оба отчёта помещаются на узком экране', async ({ page }) => {
    await stubApi(page)
    await signInAsStub(page)
    await page.goto('/debts/accounts')
    await page.getByTestId('debt-account-Пётр').getByTestId('debt-account-toggle').click()
    for (const width of [320, 390, 768, 1280]) {
      await page.setViewportSize({ width, height: 900 })
      for (const mode of ['accounting', 'normal', 'rework', 'refined']) {
        await page.getByTestId(`debts-mode-${mode}`).check()
        expect(await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth)).toBeLessThanOrEqual(1)
      }
    }
  })

  test('навигация в меню открывает отчёты и раздельно подсвечивает пункты Долгов', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 800 })
    await stubApi(page)
    await signInAsStub(page)
    await openDebts(page)

    const menu = page.getByTestId('nav-menu')
    const debtsLink = menu.getByTestId('nav-debts')
    const accountsLink = menu.getByTestId('nav-debt-accounts')

    await expect(debtsLink).toBeVisible()
    await expect(debtsLink).toContainText('Список')
    await expect(accountsLink).toBeVisible()
    await expect(accountsLink).toContainText('Отчёты')
    await expect(debtsLink.locator('.mud-nav-link')).toHaveClass(/active/)
    await expect(accountsLink.locator('.mud-nav-link')).not.toHaveClass(/active/)

    await accountsLink.click()

    await expect(page).toHaveURL(/\/debts\/accounts$/)
    await expect(accountsLink.locator('.mud-nav-link')).toHaveClass(/active/)
    await expect(debtsLink.locator('.mud-nav-link')).not.toHaveClass(/active/)
  })

  test('отчёт показывает чип чтения, компактные строки и все 4 графика без ошибок', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 800 })
    const consoleErrors: string[] = []
    const pageErrors: string[] = []
    page.on('console', message => {
      if (message.type() === 'error') {
        consoleErrors.push(message.text())
      }
    })
    page.on('pageerror', error => {
      pageErrors.push(error.message)
    })

    await stubApi(page)
    await signInAsStub(page)
    await page.goto('/debts/accounts')

    const readonlyChip = page.getByTestId('debts-report-readonly')
    await expect(readonlyChip).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(readonlyChip).toHaveClass(/mud-chip/)
    expect(await readonlyChip.evaluate(el => el.tagName !== 'BUTTON' && el.tagName !== 'A' && el.getAttribute('role') !== 'button')).toBe(true)
    await expect(page.getByTestId('debts-report-back')).toBeVisible()
    await expect(page.getByTestId('debts-toggle-paid')).toBeVisible()

    const account = page.getByTestId('debt-account-Пётр')
    await account.getByTestId('debt-account-toggle').click()

    const feedRow = account.locator('.feed-row').first()
    await expect(feedRow).toBeVisible()
    const rowBox = await feedRow.boundingBox()
    expect(rowBox?.height ?? Number.POSITIVE_INFINITY).toBeLessThanOrEqual(56)

    await expect(account.getByTestId('debt-account-chart-toggle')).toBeVisible()
    await expect(account.getByTestId('debt-account-chart')).toHaveCount(0)
    await account.getByTestId('debt-account-chart-toggle').click()
    await expect(account.getByTestId('debt-account-chart').locator('canvas')).toBeVisible()

    const charts = page.getByTestId('debts-charts-section')
    await expect(charts).toBeVisible()
    for (const tabId of ['debts-chart-tab-balance', 'debts-chart-tab-monthly', 'debts-chart-tab-owners', 'debts-chart-tab-cycles']) {
      const tab = charts.getByTestId(tabId)
      await expect(tab).toBeVisible()
      await tab.click()
      await expect(tab).toHaveAttribute('aria-pressed', 'true')
      await expect(charts.locator('canvas')).toBeVisible()
    }

    const chartsToggle = charts.getByTestId('debts-charts-toggle')
    await chartsToggle.click()
    await expect(chartsToggle).toHaveAttribute('aria-expanded', 'false')
    await expect(charts.locator('.charts-panel')).toHaveCount(0)
    await chartsToggle.click()
    await expect(chartsToggle).toHaveAttribute('aria-expanded', 'true')
    await expect(charts.locator('canvas')).toBeVisible()

    await page.goto('/debts/accounts?owner=%D0%9F%D1%91%D1%82%D1%80')
    const focusedAccount = page.getByTestId('debt-account-Пётр')
    await expect(focusedAccount.getByTestId('debt-account-chart-toggle')).toHaveAttribute('aria-expanded', 'true', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(focusedAccount.getByTestId('debt-account-chart').locator('canvas')).toBeVisible()

    expect(consoleErrors).toEqual([])
    expect(pageErrors).toEqual([])
  })

  test('при неразобранном старом комментарии временные графики предупреждают об оплатах без даты', async ({ page }) => {
    await stubApi(page, [{ ...debts[0]!, ownerName: 'Иван', paySum: 300, payComment: '2026.01.25 300 перевод\nдополнение к платежу' }])
    await signInAsStub(page)
    await page.goto('/debts/accounts')

    const charts = page.getByTestId('debts-charts-section')
    await expect(charts.getByTestId('debts-charts-undated-note')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })

    await charts.getByTestId('debts-chart-tab-monthly').click()
    await expect(charts.getByTestId('debts-charts-undated-note')).toBeVisible()

    await charts.getByTestId('debts-chart-tab-cycles').click()
    await expect(charts.getByTestId('debts-charts-undated-note')).toBeVisible()

    await charts.getByTestId('debts-chart-tab-owners').click()
    await expect(charts.getByTestId('debts-charts-undated-note')).toHaveCount(0)
  })
})

test.describe('общий возврат и история человека', () => {
  test('распределяет сумму по старым займам без встречного долга и взаимозачёта', async ({ page }) => {
    const base = { ...debts[0]!, ownerName: 'Иван', paySum: 0, payComment: null }
    const stub = await stubApi(page, [
      { ...base, id: 1, typeId: 1, sum: 100, date: '2026-01-20T00:00:00' },
      { ...base, id: 2, typeId: 1, sum: 500, paySum: 100, date: '2026-01-10T00:00:00', payComment: '2026.01.15 100 Первый взнос' },
      { ...base, id: 3, typeId: 1, sum: 300, date: '2026-01-10T00:00:00' },
      { ...base, id: 4, typeId: 2, sum: 1000 },
    ])
    await page.clock.setFixedTime(new Date(TODAY))
    await signInAsStub(page)
    await openDebts(page)
    const owner = page.getByTestId('debts-type-1').getByTestId('debts-owner-Иван')
    await owner.getByTestId('debts-owner-pay').click()
    await expect(page.locator(paymentSum)).toHaveValue('800')
    await page.locator(paymentSum).fill('550')
    await page.locator(paymentComment).fill('Общая сумма')
    await page.getByTestId('debts-payment-save').click()
    await expect.poll(() => stub.ownerPayments.length).toBe(1)
    expect(stub.ownerPayments[0]).toMatchObject({ id: 2, body: { sum: 550, comment: 'Общая сумма', ownerName: 'Иван', typeId: 1 } })
    expect(stub.created).toEqual([])
    expect(stub.payments).toEqual([])
    await expect(page.getByTestId('debts-card-2')).toHaveCount(0)
    await expect(page.getByTestId('debts-card-remainder-3')).toHaveText('150,00')
    await expect(page.getByTestId('debts-card-remainder-1')).toHaveText('100,00')
    await expect(page.getByTestId('debts-card-remainder-4')).toHaveText('1 000,00')
  })

  test('переплата не отправляется, допустимый остаток показан в форме', async ({ page }) => {
    const stub = await stubApi(page)
    await signInAsStub(page)
    await openDebts(page)
    await page.getByTestId('debts-owner-Пётр').getByTestId('debts-owner-pay').click()
    await page.locator(paymentSum).fill('751')
    await page.getByTestId('debts-payment-save').click()
    await expect(page.getByTestId('debts-payment')).toContainText('Максимальная сумма возврата: 750')
    expect(stub.ownerPayments).toEqual([])
    expect(stub.created).toEqual([])
  })

  test('при отказе сервера сохраняет сумму и комментарий', async ({ page }) => {
    await stubApi(page)
    await routeApi(page, { '/Debts': async ({ route, path }) => {
      if (path.endsWith('/PayOwner')) { await problem(route, 'Долги изменились, обновите страницу', 400) }
      else { await route.fallback() }
    } })
    await signInAsStub(page)
    await openDebts(page)
    await page.getByTestId('debts-owner-Пётр').getByTestId('debts-owner-pay').click()
    await page.locator(paymentSum).fill('200')
    await page.locator(paymentComment).fill('Не потерять')
    await page.getByTestId('debts-payment-save').click()
    await expect(page.getByTestId('debts-notice')).toContainText('Долги изменились')
    await expect(page.locator(paymentSum)).toHaveValue('200')
    await expect(page.locator(paymentComment)).toHaveValue('Не потерять')
    await page.getByTestId('debts-payment-cancel').click()
    await expect(page.getByTestId('debts-payment')).toHaveCount(0)
  })

  test('на телефоне общий возврат работает в раскрытой группе', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 })
    const stub = await stubApi(page)
    await signInAsStub(page)
    await page.goto('/debts')
    const owner = page.getByTestId('debts-owner-Пётр')
    await expect(owner).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await owner.getByTestId('debts-owner-toggle-Пётр').click()
    await owner.getByTestId('debts-owner-pay').click()
    await page.locator(paymentSum).fill('300')
    await page.getByTestId('debts-payment-save').click()
    await expect.poll(() => stub.ownerPayments.length).toBe(1)
    await expect(owner.getByTestId('debts-owner-remainder')).toHaveText('450,00')
    expect(await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth)).toBeLessThanOrEqual(1)
  })

  test('кнопка истории переводит на страницу отчётов с выбранным человеком и закрытыми займами', async ({ page }) => {
    const stub = await stubApi(page)
    await signInAsStub(page)
    await openDebts(page)
    await page.getByTestId('debts-owner-Пётр').getByTestId('debts-owner-history').click()
    await expect(page).toHaveURL(/\/debts\/accounts\?owner=/)
    const account = page.getByTestId('debt-account-Пётр')
    await expect(account).toBeVisible()
    await expect(account.getByTestId('debt-account-toggle')).toHaveAttribute('aria-expanded', 'true')
    await expect(page.getByTestId('debt-account-Анна')).toHaveCount(0)
    await expect(account).toContainText('25.01.2026')
    await expect(account.locator('[data-testid^="debt-movement-menu-"]')).toHaveCount(0)
    await page.setViewportSize({ width: 390, height: 844 })
    await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth)).toBeLessThanOrEqual(1)
    expect(stub.queries.some(query => query.withPaid === 'true')).toBe(true)
    await page.getByTestId('debts-report-back').click()
    await expect(page).toHaveURL(/\/debts$/)
    expect(stub.ownerPayments).toEqual([])
    expect(stub.created).toEqual([])
  })

  test('основной экран открывает отчёты и возвращается к займам', async ({ page }) => {
    await stubApi(page)
    await signInAsStub(page)
    await openDebts(page)
    await page.getByTestId('debts-view-accounts').click()
    await expect(page).toHaveURL(/\/debts\/accounts$/)
    await expect(page.getByTestId('debts-report-readonly')).toBeVisible()
    await page.getByTestId('debts-report-back').click()
    await expect(page).toHaveURL(/\/debts$/)
    await expect(page.getByTestId('debts-add')).toBeVisible()
  })

  test('на десктопе групповой возврат открывается в компактном поповере у кнопки и закрывается отменой', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 800 })
    await stubApi(page)
    await signInAsStub(page)
    await openDebts(page)

    const owner = page.getByTestId('debts-owner-Пётр')
    const payButton = owner.getByTestId('debts-owner-pay')
    await payButton.click()

    const popover = page.locator('.mud-popover.debt-owner__payment-popover.mud-popover-open')
    await expect(popover.getByTestId('debts-payment')).toBeVisible()

    await expect.poll(async () => {
      const popoverBox = await popover.boundingBox()
      const ownerBox = await owner.boundingBox()
      const buttonBox = await payButton.boundingBox()
      if (popoverBox === null || ownerBox === null || buttonBox === null) {
        return false
      }
      const rightDiff = Math.abs((popoverBox.x + popoverBox.width) - (buttonBox.x + buttonBox.width))
      return popoverBox.width <= 420 && popoverBox.width < ownerBox.width && rightDiff <= 16
    }).toBe(true)

    await popover.getByTestId('debts-payment-cancel').click()
    await expect(page.getByTestId('debts-payment')).toHaveCount(0)
  })

  test('на телефоне групповой возврат открывается в нижнем листе, закрывается крестиком и отправляется после повторного открытия', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 })
    const stub = await stubApi(page)
    await signInAsStub(page)
    await page.goto('/debts')

    const owner = page.getByTestId('debts-owner-Пётр')
    await expect(owner).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await owner.getByTestId('debts-owner-toggle-Пётр').click()
    await owner.getByTestId('debts-owner-pay').click()

    const dialog = page.getByTestId('debts-payment-dialog')
    await expect(dialog).toBeVisible()
    await dialog.locator('.mud-button-close').click()
    await expect(dialog).toHaveCount(0)

    await owner.getByTestId('debts-owner-pay').click()
    await expect(dialog).toBeVisible()
    await page.locator(paymentSum).fill('200')
    await page.getByTestId('debts-payment-save').click()

    await expect.poll(() => stub.ownerPayments.length).toBe(1)
    await expect(dialog).toHaveCount(0)
    await expect(owner.getByTestId('debts-owner-remainder')).toHaveText('550,00')
  })
})
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
    ownerPayments: [],
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

      if (path.includes('/ForgiveAccount/')) {
        const source = state.find(debt => debt.id === identifier(path))
        const body = route.request().postDataJSON() as PayRequest
        if (source !== undefined) {
          const balance = state.filter(debt => debt.ownerName === source.ownerName && !deleted.has(debt.id))
            .reduce((total, debt) => total + (debt.typeId === 1 ? 1 : -1) * (debt.sum - debt.paySum), 0)
          if (body.sum !== balance || balance <= 0) {
            await problem(route, 'Остаток изменился', 400)
            return
          }
          for (const debt of state.filter(debt => debt.ownerName === source.ownerName)) {
            deleted.add(debt.id)
          }
        }
        await empty(route)
        return
      }

      if (path.endsWith('/PayOwner')) {
        const id = identifier(path)
        const body = route.request().postDataJSON() as PayOwnerRequest
        const source = state.find(debt => debt.id === id)
        if (source === undefined || deleted.has(source.id)) {
          await problem(route, 'Долг не найден', 404)
          return
        }
        if (source.paySum >= source.sum || source.ownerName !== body.ownerName || source.typeId !== body.typeId) {
          await problem(route, 'Извините, но долги изменились, обновите страницу перед возвратом', 400)
          return
        }
        const entries = state.filter(debt => debt.ownerName === source.ownerName
          && debt.typeId === source.typeId && !deleted.has(debt.id) && debt.paySum < debt.sum)
          .sort((left, right) => left.date.localeCompare(right.date) || left.id - right.id)
        const available = entries.reduce((sum, debt) => sum + debt.sum - debt.paySum, 0)
        stub.ownerPayments.push({ id, body })
        if (body.sum <= 0 || body.sum > available) {
          await problem(route, `Максимальная сумма: ${available}`, 400)
          return
        }
        let remaining = body.sum
        for (const debt of entries) {
          const part = Math.min(remaining, debt.sum - debt.paySum)
          const line = `${body.date.slice(0, 10).replaceAll('-', '.')} ${part} ${body.comment ?? ''}`
          debt.paySum += part
          debt.payComment = debt.payComment ? debt.payComment + '\n' + line : line
          remaining -= part
          if (remaining === 0) { break }
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

  test('кнопка возврата к текстовой дате и знак рубля не съезжают и не перекрывают кнопки поля', async ({ page }) => {
    await stubApi(page)
    await page.clock.setFixedTime(new Date(TODAY))
    await openDebts(page)

    await page.getByTestId('debts-add').click()

    const dateField = page.getByTestId('debts-dialog-date')
    await dateField.locator('.mud-input-adornment-end button').click()

    const toggle = dateField.getByTestId('date-to-text')
    const adornment = dateField.locator('.mud-input-adornment-end button')
    await expect(toggle).toBeVisible()

    const toggleBox = await toggle.boundingBox()
    const adornmentBox = await adornment.boundingBox()
    expect(toggleBox).not.toBeNull()
    expect(adornmentBox).not.toBeNull()

    const toggleCenterY = toggleBox!.y + toggleBox!.height / 2
    const adornmentCenterY = adornmentBox!.y + adornmentBox!.height / 2
    expect(Math.abs(toggleCenterY - adornmentCenterY)).toBeLessThanOrEqual(2)
    expect(toggleBox!.x + toggleBox!.width).toBeLessThanOrEqual(adornmentBox!.x + 1)

    await toggle.click()
    await expect(dateField.getByTestId('date-text')).toBeVisible()

    const sumField = page.getByTestId('debts-dialog-sum')
    await fillField(page, dialogSum, '500')
    await sumField.locator('.mud-input-adornment-start button').click()

    const clearButton = sumField.locator('.mud-input-clear-button')
    const currency = sumField.locator('.debt-currency')
    await expect(clearButton).toBeVisible()

    const clearBox = await clearButton.boundingBox()
    const currencyBox = await currency.boundingBox()
    expect(clearBox).not.toBeNull()
    expect(currencyBox).not.toBeNull()
    expect(currencyBox!.x + currencyBox!.width).toBeLessThanOrEqual(clearBox!.x + 1)
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
