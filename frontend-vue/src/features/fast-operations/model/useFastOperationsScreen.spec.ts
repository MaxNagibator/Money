import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { defineComponent } from 'vue'
import { fetchCategories } from '@/api/adapters/categories'
import type { FastOperation } from '@/api/adapters/fast-operations'
import {
  createFastOperation,
  deleteFastOperation,
  fetchFastOperation,
  fetchFastOperations,
  restoreFastOperation,
  updateFastOperation,
} from '@/api/adapters/fast-operations'
import { ApiError } from '@/api/adapters/problem'
import type { Category } from '@/domain/category'
import { Money } from '@/domain/money'
import { SUM_UNPARSED } from '@/shared/ui/field-messages'
import type { FastOperationsScreenState } from './useFastOperationsScreen'
import { FAST_OPERATION_SAVED_NOTICE, useFastOperationsScreen } from './useFastOperationsScreen'

vi.mock('@/api/adapters/categories', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/categories')>(),
  fetchCategories: vi.fn(),
}))

vi.mock('@/api/adapters/fast-operations', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/fast-operations')>(),
  fetchFastOperation: vi.fn(),
  fetchFastOperations: vi.fn(),
  createFastOperation: vi.fn(),
  updateFastOperation: vi.fn(),
  deleteFastOperation: vi.fn(),
  restoreFastOperation: vi.fn(),
}))

const fetchCategoriesMock = vi.mocked(fetchCategories)
const fetchFastOperationMock = vi.mocked(fetchFastOperation)
const fetchFastOperationsMock = vi.mocked(fetchFastOperations)
const createFastOperationMock = vi.mocked(createFastOperation)
const updateFastOperationMock = vi.mocked(updateFastOperation)
const deleteFastOperationMock = vi.mocked(deleteFastOperation)
const restoreFastOperationMock = vi.mocked(restoreFastOperation)

const food: Category = { id: 1, parentId: null, name: 'Еда', color: null, order: null, operationTypeId: 1, isDeleted: false }

let stored: FastOperation[] = []

beforeEach(() => {
  vi.clearAllMocks()

  stored = [operation(1, { order: 2 }), operation(2, { order: null }), operation(3, { order: 1 })]

  fetchCategoriesMock.mockImplementation(() => Promise.resolve([food]))
  fetchFastOperationMock.mockImplementation(id => Promise.resolve(stored.find(item => item.id === id) ?? null))
  fetchFastOperationsMock.mockImplementation(() => Promise.resolve([...stored]))
  createFastOperationMock.mockResolvedValue(9)
  updateFastOperationMock.mockResolvedValue(undefined)
  deleteFastOperationMock.mockResolvedValue(undefined)
  restoreFastOperationMock.mockResolvedValue(undefined)
})

describe('загрузка списка', () => {
  it('список запрашивается один раз и приходит отсортированным', async () => {
    const screen = setup()

    expect(screen.isLoading.value).toBe(true)

    await settle(screen)

    expect(fetchFastOperationsMock).toHaveBeenCalledTimes(1)
    expect(screen.operations.value.map(item => item.id)).toEqual([3, 1, 2])
    expect(screen.isEmpty.value).toBe(false)
  })

  it('отказ загрузки отличим от пустого списка', async () => {
    fetchFastOperationsMock.mockRejectedValue(new ApiError('Сервер недоступен'))

    const screen = setup()

    await vi.waitFor(() => {
      expect(screen.loadError.value).toBe('Сервер недоступен')
    })

    expect(screen.isEmpty.value).toBe(false)
  })

  it('пустой ответ даёт пустое состояние без ошибки', async () => {
    stored = []

    const screen = setup()

    await settle(screen)

    expect(screen.isEmpty.value).toBe(true)
    expect(screen.loadError.value).toBeNull()
  })
})

describe('сохранение', () => {
  it('нераспознанная сумма прерывает сохранение и оставляет кнопку разблокированной', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(1)
    await screen.submit({ ...screen.form.value!, name: 'Кофе', categoryId: food.id, sum: null })

    expect(createFastOperationMock).not.toHaveBeenCalled()
    expect(screen.isSaving.value).toBe(false)
    expect(screen.notice.value).toEqual({ text: SUM_UNPARSED, type: 'warning' })
    expect(screen.form.value).not.toBeNull()
  })

  it('незаполненные поля не доходят до запроса', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(1)
    await screen.submit({ ...screen.form.value!, name: '', categoryId: null, sum: Money.parseInput('10')! })

    expect(createFastOperationMock).not.toHaveBeenCalled()
    expect(screen.notice.value).toBeNull()
  })

  it('созданная операция встаёт на место по правилу сортировки', async () => {
    const screen = setup()

    await settle(screen)

    createFastOperationMock.mockImplementation(() => {
      stored = [...stored, operation(9, { order: null, name: 'Абонемент' })]

      return Promise.resolve(9)
    })

    screen.startCreate(1)
    await screen.submit({ ...screen.form.value!, name: 'Абонемент', categoryId: food.id, sum: Money.parseInput('10')! })
    await flushPromises()

    expect(screen.form.value).toBeNull()
    expect(screen.notice.value).toEqual({ text: FAST_OPERATION_SAVED_NOTICE, type: 'success' })
    await vi.waitFor(() => {
      expect(screen.operations.value.map(item => item.id)).toEqual([3, 1, 9, 2])
    })
  })

  it('изменение уходит по идентификатору формы', async () => {
    const screen = setup()

    await settle(screen)

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, name: 'Чай', categoryId: food.id, sum: Money.parseInput('10')! })

    expect(updateFastOperationMock).toHaveBeenCalledWith(3, expect.objectContaining({ name: 'Чай' }))
    expect(createFastOperationMock).not.toHaveBeenCalled()
  })

  it('отказ сохранения оставляет диалог открытым и показывает текст сервера', async () => {
    updateFastOperationMock.mockRejectedValue(new ApiError('Слишком длинное наименование'))

    const screen = setup()

    await settle(screen)

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, name: 'Чай', categoryId: food.id, sum: Money.parseInput('10')! })

    expect(screen.form.value).not.toBeNull()
    expect(screen.isSaving.value).toBe(false)
    expect(screen.notice.value).toEqual({ text: 'Слишком длинное наименование', type: 'error' })
  })

  it('кэш мест сбрасывается только при новом непустом месте', async () => {
    const screen = setup()

    await settle(screen)

    const reset = vi.spyOn(screen.places, 'reset')

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, name: 'Чай', categoryId: food.id, sum: Money.parseInput('10')!, place: '' })

    expect(reset).not.toHaveBeenCalled()

    screen.startEdit(screen.operations.value[0]!)
    await screen.submit({ ...screen.form.value!, name: 'Чай', categoryId: food.id, sum: Money.parseInput('10')!, place: 'Пекарня' })

    expect(reset).toHaveBeenCalledTimes(1)
  })
})

describe('удаление и восстановление', () => {
  it('удаление помечает карточку, не убирая её из списка, восстановление снимает пометку', async () => {
    const screen = setup()

    await settle(screen)

    await screen.remove(screen.operations.value[0]!)

    expect(deleteFastOperationMock.mock.calls[0]![0]).toBe(3)
    expect(screen.operations.value.map(item => item.id)).toEqual([3, 1, 2])
    expect(screen.operations.value[0]!.isDeleted).toBe(true)

    await screen.restore(screen.operations.value[0]!)

    expect(restoreFastOperationMock.mock.calls[0]![0]).toBe(3)
    expect(screen.operations.value[0]!.isDeleted).toBe(false)
  })

  it.each([
    ['из строки', async (screen: FastOperationsScreenState) => screen.remove(screen.operations.value[0]!)],
    ['из листа правки', async (screen: FastOperationsScreenState) => {
      screen.startEdit(screen.operations.value[0]!)
      await screen.removeEdited()
    }],
  ])('отказ удаления %s оставляет карточку неотмеченной, форму закрытой и показывает текст сервера', async (_, remove) => {
    deleteFastOperationMock.mockRejectedValue(new ApiError('Операция не найдена'))

    const screen = setup()

    await settle(screen)

    await remove(screen)

    expect(deleteFastOperationMock.mock.calls[0]![0]).toBe(3)
    expect(screen.form.value).toBeNull()
    expect(screen.operations.value[0]!.isDeleted).toBe(false)
    expect(screen.notice.value).toEqual({ text: 'Операция не найдена', type: 'error' })
  })

  it('удаление из листа правки закрывает форму и помечает карточку для восстановления', async () => {
    const screen = setup()

    await settle(screen)

    screen.startEdit(screen.operations.value[0]!)
    await screen.removeEdited()

    expect(screen.form.value).toBeNull()
    expect(deleteFastOperationMock.mock.calls[0]![0]).toBe(3)
    expect(screen.operations.value[0]!.isDeleted).toBe(true)
  })

  it('удаление из листа не срабатывает для новой формы и во время сохранения', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreate(null)
    await screen.removeEdited()

    screen.startEdit(screen.operations.value[0]!)
    screen.isSaving.value = true
    await screen.removeEdited()

    expect(deleteFastOperationMock).not.toHaveBeenCalled()
    expect(screen.form.value?.id).toBe(3)
  })
})

function setup(): FastOperationsScreenState {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  let screen!: FastOperationsScreenState

  const wrapper = mount(
    defineComponent({
      setup() {
        screen = useFastOperationsScreen()

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

async function settle(screen: FastOperationsScreenState): Promise<void> {
  await vi.waitFor(() => {
    expect(screen.isLoading.value).toBe(false)
  })
}

function operation(id: number, extra: Partial<FastOperation> = {}): FastOperation {
  return {
    id,
    name: `Шаблон ${id}`,
    order: null,
    categoryId: food.id,
    sum: Money.parseInput('100')!,
    comment: null,
    place: 'Кафе',
    isDeleted: false,
    ...extra,
  }
}
