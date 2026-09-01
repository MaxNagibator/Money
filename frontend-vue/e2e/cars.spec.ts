import type { Page } from '@playwright/test'
import { expect, test } from './support/test'
import { created, empty, json, optionalIdentifier, routeApi } from './support/api'
import type { CarDto, StoredCarEvent } from './support/fixtures'
import { carEvents, cars } from './support/fixtures'
import { signInAsStub } from './support/session'

interface SaveRequest {
  carId: number
  typeId: number
  title: string | null
  comment: string | null
  mileage: number | null
  date: string
}

interface CarsStub {
  createdCars: string[]
  created: SaveRequest[]
  updated: { id: number, body: SaveRequest }[]
  removed: number[]
  restored: number[]
  eventQueries: number[]
}

const FIRST_RENDER_TIMEOUT = 30_000

const dialogTitleInput = '[data-testid="cars-dialog-title-input"] textarea'
const dialogComment = '[data-testid="cars-dialog-comment"] textarea'
const dialogMileage = '[data-testid="cars-dialog-mileage"] input'
const dialogDate = '[data-testid="cars-dialog-date"] [data-testid="date-text"] input'
const addName = '[data-testid="cars-add-name"] textarea'

async function stubApi(page: Page, list: CarDto[] = cars): Promise<CarsStub> {
  const carState = list.map(car => ({ ...car }))
  const eventState = carEvents.map(event => ({ ...event }))
  const stub: CarsStub = { createdCars: [], created: [], updated: [], removed: [], restored: [], eventQueries: [] }
  let lastCarId = 99
  let lastEventId = 100

  await routeApi(page, {
    '/Cars': async ({ route, method }) => {
      if (method === 'POST') {
        const body = route.request().postDataJSON() as { name: string }

        lastCarId += 1
        stub.createdCars.push(body.name)
        carState.push({ id: lastCarId, name: body.name })
        await created(route, lastCarId)
        return
      }

      await json(route, carState)
    },
    '/CarEvents': async ({ route, method, path }) => {
      const id = optionalIdentifier(path)

      if (id !== null && path.endsWith('/Restore')) {
        setDeleted(eventState, id, false)
        stub.restored.push(id)
        await empty(route)
        return
      }

      if (id !== null && method === 'DELETE') {
        setDeleted(eventState, id, true)
        stub.removed.push(id)
        await empty(route)
        return
      }

      if (id !== null && method === 'PUT') {
        const body = route.request().postDataJSON() as SaveRequest
        const stored = eventState.find(event => event.id === id)

        stub.updated.push({ id, body })

        if (stored !== undefined) {
          Object.assign(stored, { ...body, id, isDeleted: stored.isDeleted })
        }

        await empty(route)
        return
      }

      if (method === 'POST') {
        const body = route.request().postDataJSON() as SaveRequest

        lastEventId += 1
        stub.created.push(body)
        eventState.push({ ...body, id: lastEventId, isDeleted: false })
        await created(route, lastEventId)
        return
      }

      if (id === null) {
        await json(route, [])
        return
      }

      stub.eventQueries.push(id)

      await json(route, eventState.filter(event => event.carId === id && !event.isDeleted))
    },
  })

  return stub
}

function setDeleted(state: StoredCarEvent[], id: number, isDeleted: boolean): void {
  const stored = state.find(event => event.id === id)

  if (stored !== undefined) {
    stored.isDeleted = isDeleted
  }
}

async function openCars(page: Page): Promise<void> {
  await page.goto('/cars')
  await expect(page.getByTestId('cars-tab-1')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
}

async function fillDate(page: Page, value: string): Promise<void> {
  await page.locator(dialogDate).fill(value)
  await page.locator(dialogDate).blur()
}

async function fillMileage(page: Page, value: string): Promise<void> {
  await page.locator(dialogMileage).fill(value)
  await page.locator(dialogMileage).blur()
}

test.describe('машины и события', () => {
  test.beforeEach(async ({ page }) => {
    await signInAsStub(page)
  })

  test('создаёт событие из диалога и уходит запросом с полями формы', async ({ page }) => {
    const stub = await stubApi(page)

    await openCars(page)
    await page.getByTestId('cars-event-add').click()

    await expect(page.getByTestId('cars-dialog-title')).toHaveText('Создать')

    await page.locator(dialogTitleInput).fill('Замена масла')
    await page.getByTestId('cars-dialog-type-3').click()
    await fillDate(page, '12.02.2026')
    await fillMileage(page, '120000')
    await page.locator(dialogComment).fill('у дилера')

    await page.getByTestId('cars-dialog-save').click()

    await expect(page.getByTestId('cars-notice')).toContainText('Успех!')
    expect(stub.created).toEqual([{
      carId: 1,
      typeId: 3,
      title: 'Замена масла',
      comment: 'у дилера',
      mileage: 120000,
      date: '2026-02-12T00:00:00',
    }])

    await expect(page.getByTestId('cars-dialog')).toHaveCount(0)
    await expect(page.getByTestId('cars-event-title-101')).toHaveText('Замена масла')
    await expect(page.getByTestId('cars-event-date-101')).toContainText('12.02.2026')
    await expect(page.getByTestId('cars-event-comment-101')).toHaveText('у дилера')
  })

  test('редактирование открывается заполненным и уходит по идентификатору события', async ({ page }) => {
    const stub = await stubApi(page)

    await openCars(page)
    await page.getByTestId('cars-event-edit-11').click()

    await expect(page.getByTestId('cars-dialog-title')).toHaveText('Обновить')
    await expect(page.locator(dialogTitleInput)).toHaveValue('Страховка ОСАГО')
    await expect(page.locator(dialogDate)).toHaveValue('20.01.2026')
    await expect(page.locator(dialogMileage)).toHaveValue('98\u00A0000')
    await expect(page.getByTestId('cars-dialog-type-2')).toHaveAttribute('aria-checked', 'true')

    await page.locator(dialogTitleInput).fill('Страховка КАСКО')
    await page.locator(dialogComment).fill('на год')
    await page.getByTestId('cars-dialog-save').click()

    await expect(page.getByTestId('cars-dialog')).toHaveCount(0)
    expect(stub.created).toHaveLength(0)
    expect(stub.updated).toEqual([{
      id: 11,
      body: {
        carId: 1,
        typeId: 2,
        title: 'Страховка КАСКО',
        comment: 'на год',
        mileage: 98000,
        date: '2026-01-20T00:00:00',
      },
    }])

    await expect(page.getByTestId('cars-event-title-11')).toHaveText('Страховка КАСКО')
    await expect(page.getByTestId('cars-event-comment-11')).toHaveText('на год')
  })

  test('без типа и без даты форма остаётся открытой с видимым сообщением', async ({ page }) => {
    const stub = await stubApi(page)

    await openCars(page)
    await page.getByTestId('cars-event-add').click()

    await page.locator(dialogTitleInput).fill('Мойка')
    await page.getByTestId('cars-dialog-save').click()

    await expect(page.getByTestId('cars-dialog-type-error')).toHaveText('Выбери меня')
    await expect(page.getByTestId('cars-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)

    await page.getByTestId('cars-dialog-type-99').click()
    await expect(page.getByTestId('cars-dialog-type-error')).toHaveCount(0)

    await fillDate(page, '')
    await page.getByTestId('cars-dialog-save').click()

    await expect(page.getByTestId('cars-dialog-date')).toContainText('Заполни меня')
    await expect(page.getByTestId('cars-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('нераспознанная дата не сохраняет событие', async ({ page }) => {
    const stub = await stubApi(page)

    await openCars(page)
    await page.getByTestId('cars-event-add').click()

    await page.getByTestId('cars-dialog-type-99').click()
    await page.locator(dialogDate).fill('абв')
    await page.getByTestId('cars-dialog-save').click()

    await expect(page.getByTestId('cars-dialog-date')).toContainText('Неверный формат даты.')
    await expect(page.getByTestId('cars-dialog')).toBeVisible()
    expect(stub.created).toHaveLength(0)
  })

  test('удаление и восстановление уходят на сервер и меняют карточку', async ({ page }) => {
    const stub = await stubApi(page)

    await openCars(page)

    await page.getByTestId('cars-event-delete-12').click()

    await expect(page.getByTestId('cars-event-restore-12')).toBeVisible()
    await expect(page.getByTestId('cars-event-delete-12')).toHaveCount(0)
    await expect(page.getByTestId('cars-event-edit-12')).toHaveCount(0)
    await expect(page.getByTestId('cars-event-card-12')).toHaveClass(/car-event-card--deleted/)
    await expect(page.getByTestId('cars-event-card-11')).toBeVisible()
    expect(stub.removed).toEqual([12])

    await page.getByTestId('cars-event-restore-12').click()

    await expect(page.getByTestId('cars-event-delete-12')).toBeVisible()
    await expect(page.getByTestId('cars-event-restore-12')).toHaveCount(0)
    await expect(page.getByTestId('cars-event-card-12')).not.toHaveClass(/car-event-card--deleted/)
    expect(stub.restored).toEqual([12])
  })

  test('перезагрузка возвращает первую вкладку и оставляет только состояние сервера', async ({ page }) => {
    await stubApi(page)

    await openCars(page)

    await page.getByTestId('cars-tab-2').click()
    await expect(page.getByTestId('cars-event-card-21')).toBeVisible()

    await page.getByTestId('cars-event-delete-21').click()
    await expect(page.getByTestId('cars-event-restore-21')).toBeVisible()

    await page.reload()

    await expect(page.getByTestId('cars-event-card-11')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('cars-event-card-12')).toBeVisible()

    await page.getByTestId('cars-tab-2').click()

    await expect(page.getByTestId('cars-events-empty')).toBeVisible()
    await expect(page.getByTestId('cars-event-card-21')).toHaveCount(0)
  })

  test('без авто виден призыв добавить первое, события не запрашиваются', async ({ page }) => {
    const stub = await stubApi(page, [])

    await page.goto('/cars')

    await expect(page.getByTestId('cars-empty')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('cars-empty')).toContainText('Машинок пока нет')
    expect(stub.eventQueries).toHaveLength(0)
  })

  test('созданное авто становится активной вкладкой с пустым списком событий', async ({ page }) => {
    const stub = await stubApi(page, [])

    await page.goto('/cars')
    await expect(page.getByTestId('cars-empty')).toBeVisible({ timeout: FIRST_RENDER_TIMEOUT })

    await page.getByTestId('cars-add-toggle').click()
    await page.locator(addName).fill('Ласточка')
    await page.getByTestId('cars-add-submit').click()

    await expect(page.getByTestId('cars-notice')).toContainText('Авто успешно сохранено!')
    expect(stub.createdCars).toEqual(['Ласточка'])

    await expect(page.getByTestId('cars-tab-100')).toHaveAttribute('aria-selected', 'true')
    await expect(page.getByTestId('cars-empty')).toHaveCount(0)
    await expect(page.getByTestId('cars-events-empty')).toContainText('Событий пока нет')
    await expect(page.getByTestId('cars-add-name')).toHaveCount(0)
    expect(stub.eventQueries).toEqual([100])
  })

  test('отказ загрузки авто показывает сообщение сервера вместо списка', async ({ page }) => {
    await routeApi(page, {
      '/Cars': ({ route }) => route.fulfill({
        status: 500,
        contentType: 'application/problem+json',
        body: JSON.stringify({ detail: 'Сервер недоступен' }),
      }),
    })

    await page.goto('/cars')

    await expect(page.getByTestId('cars-error')).toContainText('Сервер недоступен', { timeout: FIRST_RENDER_TIMEOUT })
    await expect(page.getByTestId('cars-event-add')).toHaveCount(0)
    await expect(page.getByTestId('cars-empty')).toHaveCount(0)
  })
})
