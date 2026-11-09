import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { computed, defineComponent } from 'vue'
import type { RegularOperation } from '@/api/adapters/regular-operations'
import {
  deleteRegularOperation,
  fetchRegularOperations,
  restoreRegularOperation,
} from '@/api/adapters/regular-operations'
import type { Category } from '@/domain/category'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { RegularOperationsState } from './useRegularOperations'
import { useRegularOperations } from './useRegularOperations'

vi.mock('@/api/adapters/regular-operations', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/regular-operations')>(),
  fetchRegularOperations: vi.fn(),
  deleteRegularOperation: vi.fn(),
  restoreRegularOperation: vi.fn(),
}))

const fetchRegularOperationsMock = vi.mocked(fetchRegularOperations)
const deleteRegularOperationMock = vi.mocked(deleteRegularOperation)
const restoreRegularOperationMock = vi.mocked(restoreRegularOperation)

const food: Category = { id: 1, parentId: null, name: 'Еда', color: null, order: null, operationTypeId: 1, isDeleted: false }

beforeEach(() => {
  vi.clearAllMocks()

  fetchRegularOperationsMock.mockImplementation(() => Promise.resolve([operation(10), operation(11)]))
  deleteRegularOperationMock.mockResolvedValue(undefined)
  restoreRegularOperationMock.mockResolvedValue(undefined)
})

describe('пометка удаления в общем кэше', () => {
  it('удаление и восстановление видны второму потребителю списка', async () => {
    const queryClient = client()
    const first = await consumer(queryClient)
    const second = await consumer(queryClient)

    expect(fetchRegularOperationsMock).toHaveBeenCalledTimes(1)

    await first.remove(10)

    expect(deleted(first)).toEqual([10])
    expect(deleted(second)).toEqual([10])
    expect(second.operations.value.map(view => view.id)).toEqual([10, 11])

    await first.restore(10)

    expect(deleted(second)).toEqual([])
  })
})

function client(): QueryClient {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: 30_000 } } })

  onTestFinished(() => {
    queryClient.clear()
  })

  return queryClient
}

async function consumer(queryClient: QueryClient): Promise<RegularOperationsState> {
  let state!: RegularOperationsState

  const wrapper = mount(
    defineComponent({
      setup() {
        state = useRegularOperations(computed<readonly Category[]>(() => [food]))

        return () => null
      },
    }),
    { global: { plugins: [[VueQueryPlugin, { queryClient }]] } },
  )

  onTestFinished(() => {
    wrapper.unmount()
  })

  await vi.waitFor(() => {
    expect(state.isLoading.value).toBe(false)
  })

  return state
}

function deleted(state: RegularOperationsState): number[] {
  return state.operations.value.filter(view => view.isDeleted).map(view => view.id)
}

function operation(id: number): RegularOperation {
  return {
    id,
    name: `Операция ${id}`,
    categoryId: food.id,
    sum: Money.parseInput('100')!,
    comment: null,
    place: null,
    timeTypeId: 3,
    timeValue: 1,
    dateFrom: parseLocalDate('2026-08-23')!,
    dateTo: null,
    runTime: parseLocalDate(`2026-08-2${id === 10 ? 4 : 5}`),
    isDeleted: false,
  }
}
