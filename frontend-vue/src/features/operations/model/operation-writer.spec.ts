import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { computed, defineComponent, reactive, shallowRef } from 'vue'
import { createOperation, deleteOperation, fetchOperations, updateOperation } from '@/api/adapters/operations'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import type { OperationFilters } from './filters'
import type { OperationFormValue } from './operation-form'
import { clearPinnedOperations } from './operation-pins'
import { saveOperation } from './operation-writer'
import type { OperationsState } from './useOperations'
import { useOperations } from './useOperations'

vi.mock('@/api/adapters/operations', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/operations')>(),
  fetchOperations: vi.fn(),
  createOperation: vi.fn(),
  updateOperation: vi.fn(),
  deleteOperation: vi.fn(),
}))

const fetchOperationsMock = vi.mocked(fetchOperations)
const createOperationMock = vi.mocked(createOperation)
const updateOperationMock = vi.mocked(updateOperation)
const deleteOperationMock = vi.mocked(deleteOperation)

const CATEGORIES: readonly Category[] = [
  { id: 1, parentId: null, name: 'Продукты', color: null, order: null, operationTypeId: 1, isDeleted: false },
]

const JANUARY_ROW = {
  id: 101,
  categoryId: 1,
  category: CATEGORIES[0] ?? null,
  sum: Money.fromMinorUnits(15_000),
  comment: null,
  place: null,
  date: date('2026-01-12'),
  createdTaskId: null,
  isDeleted: false,
}

beforeEach(() => {
  vi.clearAllMocks()
  clearPinnedOperations()

  fetchOperationsMock.mockResolvedValue([JANUARY_ROW])
  createOperationMock.mockResolvedValue(999)
  updateOperationMock.mockResolvedValue(undefined)
  deleteOperationMock.mockResolvedValue(undefined)
})

describe('удержание сохранённой операции', () => {
  it.each<[string | null, string | null, string, boolean]>([
    ['2026-01-01', '2026-01-31', '2026-01-20', true],
    ['2026-01-01', '2026-01-31', '2026-01-01', true],
    ['2026-01-01', '2026-01-31', '2026-01-31', true],
    ['2026-01-01', '2026-01-31', '2026-03-10', false],
    [null, '2026-01-31', '2025-12-01', true],
    [null, '2026-01-31', '2026-03-10', false],
    ['2026-01-01', null, '2026-03-10', true],
    ['2026-01-01', null, '2025-12-01', false],
    [null, null, '2026-03-10', true],
  ])('период %s .. %s, запись от %s: удерживается – %s', async (from, to, day, kept) => {
    const { state, queryClient } = setup(from, to)

    await settle(state)

    await saveOperation(queryClient, form(null, day))

    expect(operationIds(state).includes(999)).toBe(kept)
    expect(state.totals.value[1].toMinorUnits()).toBe(kept ? 17_500 : 15_000)
  })

  it('запись, сохранённая на другой странице, доживает до открытия списка', async () => {
    const away = setup('2026-01-01', '2026-01-31')

    await settle(away.state)
    await saveOperation(away.queryClient, form(null, '2026-01-20'))

    const list = setup('2026-01-01', '2026-01-31')

    await settle(list.state)

    expect(operationIds(list.state).includes(999)).toBe(true)
    expect(list.state.totals.value[1].toMinorUnits()).toBe(17_500)
  })

  it('сохранение, завершившееся после выхода из аккаунта, не удерживается', async () => {
    const { state, queryClient } = setup('2026-01-01', '2026-01-31')

    await settle(state)

    createOperationMock.mockImplementationOnce(() => {
      clearPinnedOperations()

      return Promise.resolve(999)
    })

    await saveOperation(queryClient, form(null, '2026-01-20'))

    expect(operationIds(state).includes(999)).toBe(false)
    expect(state.totals.value[1].toMinorUnits()).toBe(15_000)
  })

  it('удалённая запись не воскресает на другой странице', async () => {
    const away = setup('2026-01-01', '2026-01-31')

    await settle(away.state)
    await saveOperation(away.queryClient, form(null, '2026-01-20'))
    await away.state.remove(999)

    const list = setup('2026-01-01', '2026-01-31')

    await settle(list.state)

    expect(operationIds(list.state).includes(999)).toBe(false)
    expect(list.state.totals.value[1].toMinorUnits()).toBe(15_000)
  })

  it('удержанная запись внутри периода приходит с разрешённой категорией', async () => {
    const { state, queryClient } = setup('2026-01-01', '2026-01-31')

    await settle(state)

    await saveOperation(queryClient, form(null, '2026-01-20'))

    expect(findOperation(state, 999)?.category?.name).toBe('Продукты')
  })

  it('правка внутри периода показывает новую дату, не дожидаясь сервера', async () => {
    const { state, queryClient } = setup('2026-01-01', '2026-01-31')

    await settle(state)

    await saveOperation(queryClient, form(101, '2026-01-20'))

    expect(updateOperationMock).toHaveBeenCalledOnce()
    expect(findOperation(state, 101)?.date).toBe('2026-01-20')
  })

  it('правка, уносящая запись из периода, не подменяет ею ответ сервера', async () => {
    const { state, queryClient } = setup('2026-01-01', '2026-01-31')

    await settle(state)

    await saveOperation(queryClient, form(101, '2026-03-10'))

    expect(findOperation(state, 101)?.date).toBe('2026-01-12')
    expect(state.totals.value[1].toMinorUnits()).toBe(15_000)
  })
})

function form(id: number | null, day: string): OperationFormValue {
  return {
    id,
    operationTypeId: 1,
    categoryId: 1,
    sum: Money.fromMinorUnits(2_500),
    date: date(day),
    comment: '',
    place: '',
  }
}

function setup(from: string | null, to: string | null): { state: OperationsState, queryClient: QueryClient } {
  const filters = reactive<OperationFilters>({
    from: from === null ? null : date(from),
    to: to === null ? null : date(to),
    interval: null,
    comment: '',
    place: '',
    categoryIds: [],
    showEmptyDays: false,
  })

  const categories = shallowRef<Category[]>([...CATEGORIES])
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

  return { state, queryClient }
}

async function settle(state: OperationsState): Promise<void> {
  await vi.waitFor(() => {
    expect(state.isLoading.value).toBe(false)
  })
}

function date(value: string): LocalDate {
  return value as LocalDate
}

function operationIds(state: OperationsState): number[] {
  return state.days.value.flatMap(day => day.operations).map(operation => operation.id)
}

function findOperation(state: OperationsState, id: number): Operation | undefined {
  return state.days.value.flatMap(day => day.operations).find(operation => operation.id === id)
}
