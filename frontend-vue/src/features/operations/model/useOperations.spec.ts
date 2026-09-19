import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import type { Ref } from 'vue'
import { computed, defineComponent, reactive, shallowRef } from 'vue'
import type { OperationDraft } from '@/api/adapters/operations'
import {
  createOperation,
  deleteOperation,
  fetchOperations,
  restoreOperation,
  updateOperation,
} from '@/api/adapters/operations'
import { ApiError } from '@/api/adapters/problem'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation, OperationsDay } from '@/domain/operation'
import { calculateDayTotal } from '@/domain/operation'
import type { OperationTypeId } from '@/domain/operation-type'
import type { OperationFilters } from './filters'
import { clearPinnedOperations } from './operation-pins'
import type { OperationsState } from './useOperations'
import { useOperations } from './useOperations'

vi.mock('@/api/adapters/operations', async importOriginal => {
  const actual = await importOriginal<typeof import('@/api/adapters/operations')>()

  return {
    ...actual,
    fetchOperations: vi.fn(),
    createOperation: vi.fn(),
    updateOperation: vi.fn(),
    deleteOperation: vi.fn(),
    restoreOperation: vi.fn(),
  }
})

const fetchOperationsMock = vi.mocked(fetchOperations)
const createOperationMock = vi.mocked(createOperation)
const updateOperationMock = vi.mocked(updateOperation)
const deleteOperationMock = vi.mocked(deleteOperation)
const restoreOperationMock = vi.mocked(restoreOperation)

const CATEGORIES: readonly Category[] = [
  { id: 1, parentId: null, name: 'Продукты', color: null, order: null, operationTypeId: 1, isDeleted: false },
  { id: 2, parentId: null, name: 'Зарплата', color: null, order: null, operationTypeId: 2, isDeleted: false },
]

const ROWS: readonly { id: number, categoryId: number, date: string, minorUnits: number }[] = [
  { id: 101, categoryId: 1, date: '2026-03-10', minorUnits: 15_000 },
  { id: 102, categoryId: 2, date: '2026-03-10', minorUnits: 50_000 },
  { id: 103, categoryId: 1, date: '2026-03-07', minorUnits: 2_500 },
]

const NARROW_FILTER: Partial<OperationFilters> = {
  from: date('2026-03-01'),
  to: date('2026-03-31'),
  categoryIds: [1],
  comment: 'кофе',
  place: 'Пятёрочка',
}

const CUT_BY_FILTER: readonly [string, Partial<OperationFilters>, Partial<OperationDraft>][] = [
  ['по категории', { categoryIds: [1] }, { categoryId: 2 }],
  ['по месту', { place: 'Пятёрочка' }, { place: 'Ашан' }],
  ['по комментарию', { comment: 'кофе' }, { comment: 'бензин' }],
  ['по периоду', { from: date('2026-03-01'), to: date('2026-03-31') }, { date: date('2026-02-20') }],
]

const OPEN_FILTERS: readonly [string, Partial<OperationFilters>][] = [
  ['пустые поля и пустой список категорий', { comment: '', place: '', categoryIds: [] }],
  ['пробелы вместо текста', { comment: '   ', place: '  ' }],
  ['период без границ', { from: null, to: null }],
]

const DRAFT: OperationDraft = {
  categoryId: 1,
  sum: Money.fromMinorUnits(1_000),
  comment: null,
  place: null,
  date: date('2026-03-10'),
}

beforeEach(() => {
  vi.clearAllMocks()
  clearPinnedOperations()

  showAllOperations()
  createOperationMock.mockResolvedValue(999)
  updateOperationMock.mockResolvedValue(undefined)
  deleteOperationMock.mockResolvedValue(undefined)
  restoreOperationMock.mockResolvedValue(undefined)
})

describe('данные экрана операций', () => {
  it('отдаёт операции, сгруппированные по дням от новых к старым', async () => {
    const { state } = setup()

    await settle(state)

    expect(state.days.value.map(day => day.date)).toEqual(['2026-03-10', '2026-03-07'])
    expect(state.days.value[0]?.operations.map(operation => operation.id)).toEqual([101, 102])
    expect(state.days.value[0]?.operations[0]?.category?.name).toBe('Продукты')
    expect(state.errorMessage.value).toBeNull()
    expect(state.isLoading.value).toBe(false)
  })

  it('дополняет пустые дни без повторного запроса', async () => {
    const { state, filters } = setup()

    await settle(state)

    filters.showEmptyDays = true
    await flushPromises()

    expect(state.days.value.map(day => day.date)).toEqual([
      '2026-03-10',
      '2026-03-09',
      '2026-03-08',
      '2026-03-07',
    ])

    filters.showEmptyDays = false
    await flushPromises()

    expect(state.days.value).toHaveLength(2)
    expect(fetchOperationsMock).toHaveBeenCalledTimes(1)
  })

  it('считает итоги периода по типам без локально удалённой операции', async () => {
    const { state } = setup()

    await settle(state)

    expect(state.totals.value[1].toMinorUnits()).toBe(17_500)
    expect(state.totals.value[2].toMinorUnits()).toBe(50_000)

    await state.remove(101)

    expect(state.totals.value[1].toMinorUnits()).toBe(2_500)
    expect(state.totals.value[2].toMinorUnits()).toBe(50_000)
  })

  it('держит удалённую операцию в списке и снимает метку при восстановлении', async () => {
    const { state } = setup()

    await settle(state)
    await state.remove(101)

    expect(findOperation(state, 101)?.isDeleted).toBe(true)
    expect(deleteOperationMock).toHaveBeenCalledWith(101)
    expect(fetchOperationsMock).toHaveBeenCalledTimes(1)

    await state.restore(101)

    expect(findOperation(state, 101)?.isDeleted).toBe(false)
    expect(restoreOperationMock).toHaveBeenCalledWith(101)
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))
  })

  it('оставляет удалённую операцию с кнопкой восстановления после перезапроса', async () => {
    const { state } = setup()

    await settle(state)
    await state.remove(101)

    hideOperation(101)
    await state.create(DRAFT)
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    const removed = findOperation(state, 101)

    expect(removed?.isDeleted).toBe(true)
    expect(removed?.category?.name).toBe('Продукты')
    expect(removed?.sum.toMinorUnits()).toBe(15_000)
    expect(operationIds(state, '2026-03-10')).toEqual([102, 999, 101])
    expect(dayTotal(state, '2026-03-10', 1).toMinorUnits()).toBe(1_000)
    expect(state.totals.value[1].toMinorUnits()).toBe(3_500)

    showAllOperations()
    await state.restore(101)
    await vi.waitFor(() => expect(state.totals.value[1].toMinorUnits()).toBe(18_500))

    expect(findOperation(state, 101)?.isDeleted).toBe(false)
    expect(operationIds(state, '2026-03-10')).toEqual([101, 102, 999])
  })

  it('забывает удалённые операции при смене применённого фильтра', async () => {
    const { state, filters } = setup()

    await settle(state)
    await state.remove(101)

    hideOperation(101)
    filters.comment = 'кофе'
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    expect(findOperation(state, 101)).toBeUndefined()
    expect(state.totals.value[1].toMinorUnits()).toBe(2_500)
  })

  it('пробрасывает ошибку мутации и не помечает операцию удалённой', async () => {
    const { state } = setup()

    await settle(state)

    deleteOperationMock.mockRejectedValue(new ApiError('Не удалось удалить операцию'))

    await expect(state.remove(101)).rejects.toThrow(ApiError)
    await expect(state.remove(101)).rejects.toThrow('Не удалось удалить операцию')

    expect(findOperation(state, 101)?.isDeleted).toBe(false)
  })

  it('перезапрашивает операции при смене фильтра, но не из-за лишних пробелов', async () => {
    const { state, filters } = setup()

    await settle(state)

    filters.comment = 'кофе'
    filters.categoryIds = [2, 1]
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    expect(fetchOperationsMock).toHaveBeenLastCalledWith(
      expect.objectContaining({ comment: 'кофе', categoryIds: [1, 2] }),
      expect.anything(),
    )

    filters.comment = '  кофе  '
    filters.place = '   '
    await flushPromises()

    expect(fetchOperationsMock).toHaveBeenCalledTimes(2)
  })

  it('поиск перечитывает список, даже когда фильтр не менялся', async () => {
    const { state } = setup()

    await settle(state)

    await state.search()

    expect(fetchOperationsMock).toHaveBeenCalledTimes(2)

    await state.search()

    expect(fetchOperationsMock).toHaveBeenCalledTimes(3)
  })

  it('оставляет в списке созданную операцию с датой вне периода', async () => {
    const { state } = setup()

    await settle(state)

    createOperationMock.mockResolvedValue(777)
    await state.create({ ...DRAFT, date: date('2026-02-20') })
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    expect(state.days.value.map(day => day.date)).toEqual(['2026-03-10', '2026-03-07', '2026-02-20'])
    expect(operationIds(state, '2026-02-20')).toEqual([777])
    expect(findOperation(state, 777)?.category?.name).toBe('Продукты')
    expect(state.totals.value[1].toMinorUnits()).toBe(18_500)
  })

  it('переносит операцию в день новой даты, когда правка увела её из периода', async () => {
    const { state } = setup()

    await settle(state)

    hideOperation(101)
    await state.update(101, { ...DRAFT, date: date('2026-02-20') })
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    expect(operationIds(state, '2026-03-10')).toEqual([102])
    expect(operationIds(state, '2026-02-20')).toEqual([101])
    expect(findOperation(state, 101)?.sum.toMinorUnits()).toBe(1_000)
  })

  it('забывает созданное вне периода по явному поиску', async () => {
    const { state } = setup()

    await settle(state)

    createOperationMock.mockResolvedValue(777)
    await state.create({ ...DRAFT, date: date('2026-02-20') })
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    await state.search()

    expect(state.days.value.map(day => day.date)).toEqual(['2026-03-10', '2026-03-07'])
    expect(findOperation(state, 777)).toBeUndefined()
  })

  it('забывает созданное вне периода при смене применённого фильтра', async () => {
    const { state, filters } = setup()

    await settle(state)

    createOperationMock.mockResolvedValue(777)
    await state.create({ ...DRAFT, date: date('2026-02-20') })
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    filters.comment = 'кофе'
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(3))

    expect(findOperation(state, 777)).toBeUndefined()
  })

  it('подставляет категории, приехавшие позже операций', async () => {
    const { state, categories } = setup({}, [])

    await settle(state)

    expect(state.totals.value[1].toMinorUnits()).toBe(0)

    categories.value = [...CATEGORIES]
    await flushPromises()

    expect(findOperation(state, 101)?.category?.name).toBe('Продукты')
    expect(state.totals.value[1].toMinorUnits()).toBe(17_500)
    expect(fetchOperationsMock).toHaveBeenCalledTimes(1)
  })

  it.each(CUT_BY_FILTER)('не удерживает запись, отсечённую %s', async (_reason, applied, changed) => {
    const { state } = setup(applied)

    await settle(state)

    createOperationMock.mockResolvedValue(777)
    await state.create({ ...DRAFT, ...changed })
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    expect(findOperation(state, 777)).toBeUndefined()
    expect(state.totals.value[1].toMinorUnits()).toBe(17_500)
    expect(state.totals.value[2].toMinorUnits()).toBe(50_000)
  })

  it('удерживает запись, прошедшую применённый фильтр целиком', async () => {
    const { state } = setup(NARROW_FILTER)

    await settle(state)

    createOperationMock.mockResolvedValue(777)
    await state.create({ ...DRAFT, comment: 'Утренний КОФЕ', place: 'ПЯТЁРОЧКА на углу' })
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    expect(findOperation(state, 777)?.sum.toMinorUnits()).toBe(1_000)
    expect(state.totals.value[1].toMinorUnits()).toBe(18_500)
  })

  it.each(OPEN_FILTERS)('удерживает запись, когда фильтр не ограничивает: %s', async (_reason, applied) => {
    const { state } = setup(applied)

    await settle(state)

    createOperationMock.mockResolvedValue(777)
    await state.create({ ...DRAFT, comment: null, place: null, date: date('2024-01-05') })
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    expect(findOperation(state, 777)?.sum.toMinorUnits()).toBe(1_000)
    expect(state.totals.value[1].toMinorUnits()).toBe(18_500)
  })

  it('перечитывает список после создания и изменения операции', async () => {
    const { state } = setup()

    await settle(state)

    await state.create(DRAFT)

    expect(createOperationMock).toHaveBeenCalledWith(DRAFT)
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(2))

    await state.update(101, DRAFT)

    expect(updateOperationMock).toHaveBeenCalledWith(101, DRAFT)
    await vi.waitFor(() => expect(fetchOperationsMock).toHaveBeenCalledTimes(3))
  })
})

function setup(
  overrides: Partial<OperationFilters> = {},
  initialCategories: readonly Category[] = CATEGORIES,
): { state: OperationsState, filters: OperationFilters, categories: Ref<Category[]> } {
  const filters = reactive<OperationFilters>({
    from: null,
    to: null,
    interval: null,
    comment: '',
    place: '',
    categoryIds: [],
    showEmptyDays: false,
    ...overrides,
  })

  const categories = shallowRef<Category[]>([...initialCategories])
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: 30_000 } } })
  let state!: OperationsState

  const wrapper = mount(
    defineComponent({
      setup() {
        state = useOperations(filters, computed(() => categories.value))

        return () => null
      },
    }),
    { global: { plugins: [[VueQueryPlugin, { queryClient }]] } },
  )

  onTestFinished(() => {
    wrapper.unmount()
  })

  return { state, filters, categories }
}

async function settle(state: OperationsState): Promise<void> {
  await vi.waitFor(() => {
    expect(state.isLoading.value).toBe(false)
  })
}

function findOperation(state: OperationsState, id: number): Operation | undefined {
  return state.days.value
    .flatMap(day => day.operations)
    .find(operation => operation.id === id)
}

function operationIds(state: OperationsState, day: string): number[] {
  return findDay(state, day).operations.map(operation => operation.id)
}

function dayTotal(state: OperationsState, day: string, operationTypeId: OperationTypeId): Money {
  return calculateDayTotal(findDay(state, day), operationTypeId)
}

function findDay(state: OperationsState, day: string): OperationsDay {
  const found = state.days.value.find(candidate => candidate.date === day)

  if (found === undefined) {
    throw new Error(`День ${day} не найден в списке`)
  }

  return found
}

function hideOperation(id: number): void {
  fetchOperationsMock.mockImplementation((_, known) => Promise.resolve(
    buildOperations(known).filter(operation => operation.id !== id),
  ))
}

function showAllOperations(): void {
  fetchOperationsMock.mockImplementation((_, known) => Promise.resolve(buildOperations(known)))
}

function buildOperations(categories: readonly Category[]): Operation[] {
  const known = new Map(categories.map(category => [category.id, category]))

  return ROWS.map(row => ({
    id: row.id,
    categoryId: row.categoryId,
    category: known.get(row.categoryId) ?? null,
    sum: Money.fromMinorUnits(row.minorUnits),
    comment: null,
    place: null,
    date: date(row.date),
    createdTaskId: null,
    isDeleted: false,
  }))
}

function date(value: string): LocalDate {
  return value as LocalDate
}
