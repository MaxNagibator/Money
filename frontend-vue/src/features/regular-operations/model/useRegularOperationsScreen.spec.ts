import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { defineComponent } from 'vue'
import { fetchCategories } from '@/api/adapters/categories'
import { ApiError } from '@/api/adapters/problem'
import type { RegularOperation } from '@/api/adapters/regular-operations'
import {
  createRegularOperation,
  deleteRegularOperation,
  fetchRegularOperation,
  fetchRegularOperations,
  restoreRegularOperation,
  updateRegularOperation,
} from '@/api/adapters/regular-operations'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Clock } from '@/platform/clock'
import { SUM_UNPARSED } from '@/shared/ui/field-messages'
import type { RegularOperationsScreenState } from './useRegularOperationsScreen'
import { REGULAR_OPERATION_SAVED_NOTICE, useRegularOperationsScreen } from './useRegularOperationsScreen'

vi.mock('@/api/adapters/categories', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/categories')>(),
  fetchCategories: vi.fn(),
}))

vi.mock('@/api/adapters/regular-operations', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/regular-operations')>(),
  fetchRegularOperations: vi.fn(),
  fetchRegularOperation: vi.fn(),
  createRegularOperation: vi.fn(),
  updateRegularOperation: vi.fn(),
  deleteRegularOperation: vi.fn(),
  restoreRegularOperation: vi.fn(),
}))

const fetchCategoriesMock = vi.mocked(fetchCategories)
const fetchRegularOperationsMock = vi.mocked(fetchRegularOperations)
const fetchRegularOperationMock = vi.mocked(fetchRegularOperation)
const createRegularOperationMock = vi.mocked(createRegularOperation)
const updateRegularOperationMock = vi.mocked(updateRegularOperation)
const deleteRegularOperationMock = vi.mocked(deleteRegularOperation)
const restoreRegularOperationMock = vi.mocked(restoreRegularOperation)

const food = category(1, 'Еда', 1)
const salary = category(2, 'Зарплата', 2)

const TODAY = localDate('2026-08-23')

const clock: Clock = {
  now: () => new Date('2026-08-23T12:00:00'),
  today: () => TODAY,
}

let stored: RegularOperation[] = []

beforeEach(() => {
  vi.clearAllMocks()

  stored = [
    operation(1, '2026-09-10'),
    operation(2, null, { categoryId: salary.id }),
    operation(3, '2026-09-01'),
  ]

  fetchCategoriesMock.mockImplementation(() => Promise.resolve([food, salary]))
  fetchRegularOperationsMock.mockImplementation(() => Promise.resolve([...stored]))
  fetchRegularOperationMock.mockImplementation(id => Promise.resolve(stored.find(item => item.id === id) ?? null))

  createRegularOperationMock.mockImplementation(() => {
    stored = [...stored, operation(9, '2026-09-05', { name: 'Аренда' })]

    return Promise.resolve(9)
  })

  updateRegularOperationMock.mockImplementation((id, draft) => {
    stored = stored.map(item => item.id === id
      ? { ...item, name: draft.name, runTime: localDate('2026-09-20') }
      : item)

    return Promise.resolve()
  })

  deleteRegularOperationMock.mockResolvedValue(undefined)
  restoreRegularOperationMock.mockResolvedValue(undefined)
})

describe('загрузка экрана', () => {
  it('держит загрузку, пока не пришли операции и категории', async () => {
    const operations = defer<RegularOperation[]>()
    const categories = defer<Category[]>()

    fetchRegularOperationsMock.mockReturnValue(operations.promise)
    fetchCategoriesMock.mockReturnValue(categories.promise)

    const screen = setup()

    expect(screen.isLoading.value).toBe(true)
    expect(screen.isEmpty.value).toBe(false)

    operations.resolve([...stored])
    await flushPromises()

    expect(screen.isLoading.value).toBe(true)

    categories.resolve([food, salary])
    await flushPromises()

    expect(screen.isLoading.value).toBe(false)
    expect(screen.isEmpty.value).toBe(false)
  })

  it('список запрашивается один раз и приходит отсортированным', async () => {
    const screen = setup()

    await settle(screen)

    expect(screen.operations.value.map(item => item.id)).toEqual([3, 1, 2])
    expect(screen.operations.value[0]?.category).toEqual(food)
    expect(fetchRegularOperationsMock).toHaveBeenCalledTimes(1)
  })

  it('пустой ответ даёт пустое состояние без ошибки', async () => {
    stored = []

    const screen = setup()

    await settle(screen)

    expect(screen.isEmpty.value).toBe(true)
    expect(screen.loadError.value).toBeNull()
  })

  it('отказ загрузки отличим от пустого списка', async () => {
    fetchRegularOperationsMock.mockRejectedValue(new ApiError('Извините, произошла ошибка'))

    const screen = setup()

    await vi.waitFor(() => {
      expect(screen.loadError.value).toBe('Извините, произошла ошибка')
    })

    expect(screen.isEmpty.value).toBe(false)
    expect(screen.operations.value).toEqual([])
  })

  it('повтор перезапрашивает список', async () => {
    fetchRegularOperationsMock.mockRejectedValueOnce(new ApiError('Извините, произошла ошибка'))

    const screen = setup()

    await vi.waitFor(() => {
      expect(screen.loadError.value).toBe('Извините, произошла ошибка')
    })

    await screen.reload()
    await flushPromises()

    expect(screen.loadError.value).toBeNull()
    expect(screen.operations.value.map(item => item.id)).toEqual([3, 1, 2])
  })
})

describe('сохранение', () => {
  it('созданная операция встаёт по ближайшему запуску, а не в начало списка', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(1)

    expect(screen.dialogTitle.value).toBe('Создать')
    expect(screen.form.value?.dateFrom).toBe(TODAY)

    await screen.submit({ ...screen.form.value!, name: 'Аренда', categoryId: food.id })
    await flushPromises()

    expect(createRegularOperationMock).toHaveBeenCalledWith(expect.objectContaining({
      name: 'Аренда',
      categoryId: food.id,
      timeTypeId: 3,
      timeValue: 1,
      dateFrom: TODAY,
      dateTo: null,
    }))

    expect(screen.operations.value.map(item => item.id)).toEqual([3, 9, 1, 2])
    expect(screen.form.value).toBeNull()
    expect(screen.notice.value).toEqual({ text: REGULAR_OPERATION_SAVED_NOTICE, type: 'success' })
  })

  it('изменение правит операцию на месте и перечитывает ближайший запуск', async () => {
    const screen = setup()

    await settle(screen)

    screen.startEdit(screen.operations.value[0]!)

    expect(screen.dialogTitle.value).toBe('Обновить')

    await screen.submit({ ...screen.form.value!, name: 'Аренда' })
    await flushPromises()

    expect(updateRegularOperationMock).toHaveBeenCalledWith(3, expect.objectContaining({ name: 'Аренда' }))
    expect(createRegularOperationMock).not.toHaveBeenCalled()
    expect(fetchRegularOperationMock).toHaveBeenCalledWith(3)
    expect(fetchRegularOperationsMock).toHaveBeenCalledTimes(1)

    const updated = screen.operations.value.find(item => item.id === 3)!

    expect(updated.name).toBe('Аренда')
    expect(updated.runTime).toBe('2026-09-20')
    expect(screen.operations.value.map(item => item.id)).toEqual([1, 3, 2])
  })

  it('отказ перечитывания не превращается в отказ сохранения', async () => {
    const screen = setup()

    await settle(screen)

    fetchRegularOperationMock.mockRejectedValue(new ApiError('Извините, произошла ошибка'))

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, name: 'Аренда' })
    await flushPromises()

    expect(screen.form.value).toBeNull()
    expect(screen.notice.value).toEqual({ text: REGULAR_OPERATION_SAVED_NOTICE, type: 'success' })
    expect(fetchRegularOperationsMock).toHaveBeenCalledTimes(2)
    expect(screen.operations.value.find(item => item.id === 3)?.name).toBe('Аренда')
  })

  it('отказ сохранения оставляет диалог открытым и показывает текст сервера', async () => {
    const screen = setup()

    await settle(screen)

    updateRegularOperationMock.mockRejectedValue(
      new ApiError('Извините, но значение интервала должно быть в диапазоне от 1 до 31 при Каждый месяц'),
    )

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, timeValue: 31 })
    await flushPromises()

    expect(screen.form.value).not.toBeNull()
    expect(screen.isSaving.value).toBe(false)
    expect(screen.notice.value).toEqual({
      text: 'Извините, но значение интервала должно быть в диапазоне от 1 до 31 при Каждый месяц',
      type: 'error',
    })
  })

  it('нераспознанная сумма прерывает сохранение своим сообщением', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(null)
    await screen.submit({ ...screen.form.value!, name: 'Аренда', categoryId: food.id, sum: null })

    expect(createRegularOperationMock).not.toHaveBeenCalled()
    expect(screen.form.value).not.toBeNull()
    expect(screen.isSaving.value).toBe(false)
    expect(screen.notice.value).toEqual({ text: SUM_UNPARSED, type: 'warning' })
  })

  it('незаполненная форма не доходит до запроса', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(null)
    await screen.submit(screen.form.value!)

    expect(createRegularOperationMock).not.toHaveBeenCalled()
    expect(screen.notice.value).toBeNull()
    expect(screen.form.value).not.toBeNull()
  })

  it('кэш мест сбрасывается только при изменённом непустом месте', async () => {
    const screen = setup()

    await settle(screen)

    const reset = vi.spyOn(screen.places, 'reset')

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, place: '' })

    expect(reset).not.toHaveBeenCalled()

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, place: 'Пятёрочка' })

    expect(reset).toHaveBeenCalledTimes(1)
  })
})

describe('категории диалога', () => {
  it('кнопка типа оставляет в диалоге только категории этого типа', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(2)

    expect(screen.dialogCategories.value.map(item => item.id)).toEqual([salary.id])
  })

  it('изменение оставляет категории типа текущей категории', async () => {
    const screen = setup()

    await settle(screen)

    screen.startEdit(screen.operations.value.find(item => item.id === 2)!)

    expect(screen.dialogCategories.value.map(item => item.id)).toEqual([salary.id])
  })

  it('выбранная в диалоге категория не сужает список типов', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(null)
    screen.form.value = { ...screen.form.value!, categoryId: food.id }

    expect(screen.dialogCategories.value.map(item => item.id)).toEqual([food.id, salary.id])
  })
})

describe('удаление и восстановление', () => {
  it('удаление помечает карточку, не убирая её из списка, восстановление снимает пометку', async () => {
    const screen = setup()

    await settle(screen)

    await screen.remove(screen.operations.value[0]!)

    expect(deleteRegularOperationMock).toHaveBeenCalledWith(3)
    expect(screen.operations.value.map(item => item.id)).toEqual([3, 1, 2])
    expect(screen.operations.value[0]?.isDeleted).toBe(true)
    expect(fetchRegularOperationsMock).toHaveBeenCalledTimes(1)

    await screen.restore(screen.operations.value[0]!)

    expect(restoreRegularOperationMock).toHaveBeenCalledWith(3)
    expect(screen.operations.value[0]?.isDeleted).toBe(false)
  })

  it('отказ удаления оставляет карточку неотмеченной и показывает текст сервера', async () => {
    deleteRegularOperationMock.mockRejectedValue(new ApiError('Регулярная операция не найдена'))

    const screen = setup()

    await settle(screen)
    await screen.remove(screen.operations.value[0]!)

    expect(screen.operations.value[0]?.isDeleted).toBe(false)
    expect(screen.notice.value).toEqual({ text: 'Регулярная операция не найдена', type: 'error' })
  })
})

function setup(): RegularOperationsScreenState {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  let screen!: RegularOperationsScreenState

  const wrapper = mount(
    defineComponent({
      setup() {
        screen = useRegularOperationsScreen(clock)

        return () => null
      },
    }),
    { global: { plugins: [[VueQueryPlugin, { queryClient }]] } },
  )

  onTestFinished(() => {
    wrapper.unmount()
    queryClient.clear()
  })

  return screen
}

async function settle(screen: RegularOperationsScreenState): Promise<void> {
  await vi.waitFor(() => {
    expect(screen.isLoading.value).toBe(false)
  })
}

function defer<T>(): { promise: Promise<T>, resolve: (value: T) => void } {
  let resolve!: (value: T) => void
  const promise = new Promise<T>(done => {
    resolve = done
  })

  return { promise, resolve }
}

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function category(id: number, name: string, operationTypeId: 1 | 2): Category {
  return { id, parentId: null, name, color: null, order: null, operationTypeId, isDeleted: false }
}

function operation(id: number, runTime: string | null, extra: Partial<RegularOperation> = {}): RegularOperation {
  return {
    id,
    name: `Операция ${id}`,
    categoryId: food.id,
    sum: Money.parseInput('100')!,
    comment: null,
    place: null,
    timeTypeId: 3,
    timeValue: 1,
    dateFrom: localDate('2026-08-23'),
    dateTo: null,
    runTime: runTime === null ? null : localDate(runTime),
    isDeleted: false,
    ...extra,
  }
}
