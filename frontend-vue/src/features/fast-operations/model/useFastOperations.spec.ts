import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { computed, defineComponent } from 'vue'
import type { FastOperation, FastOperationDraft } from '@/api/adapters/fast-operations'
import {
  deleteFastOperation,
  fetchFastOperation,
  fetchFastOperations,
  restoreFastOperation,
  updateFastOperation,
} from '@/api/adapters/fast-operations'
import type { Category } from '@/domain/category'
import { Money } from '@/domain/money'
import type { FastOperationsState } from './useFastOperations'
import { useFastOperations } from './useFastOperations'

vi.mock('@/api/adapters/fast-operations', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/fast-operations')>(),
  fetchFastOperations: vi.fn(),
  fetchFastOperation: vi.fn(),
  updateFastOperation: vi.fn(),
  deleteFastOperation: vi.fn(),
  restoreFastOperation: vi.fn(),
}))

const fetchFastOperationsMock = vi.mocked(fetchFastOperations)
const fetchFastOperationMock = vi.mocked(fetchFastOperation)
const updateFastOperationMock = vi.mocked(updateFastOperation)
const deleteFastOperationMock = vi.mocked(deleteFastOperation)
const restoreFastOperationMock = vi.mocked(restoreFastOperation)

const food: Category = { id: 1, parentId: null, name: 'Еда', color: null, order: null, operationTypeId: 1, isDeleted: false }

const removedOnServer = new Set<number>()

beforeEach(() => {
  vi.clearAllMocks()
  removedOnServer.clear()

  fetchFastOperationsMock.mockImplementation(() => Promise.resolve(
    [operation(10), operation(11)].filter(item => !removedOnServer.has(item.id)),
  ))
  fetchFastOperationMock.mockImplementation(id => Promise.resolve({ ...operation(id), name: 'Кофе большой' }))
  updateFastOperationMock.mockResolvedValue(undefined)
  deleteFastOperationMock.mockImplementation(id => {
    removedOnServer.add(id)

    return Promise.resolve()
  })
  restoreFastOperationMock.mockImplementation(id => {
    removedOnServer.delete(id)

    return Promise.resolve()
  })
})

describe('пометка удаления в общем кэше', () => {
  it('удаление на одном экране видно второму потребителю списка', async () => {
    const queryClient = client()
    const templates = await consumer(queryClient)
    const quickAdd = await consumer(queryClient)

    expect(fetchFastOperationsMock).toHaveBeenCalledTimes(1)

    await templates.remove(10)

    expect(deleted(templates)).toEqual([10])
    expect(deleted(quickAdd)).toEqual([10])
    expect(quickAdd.operations.value.map(view => view.id)).toEqual([10, 11])
  })

  it('восстановление снимает пометку у обоих потребителей', async () => {
    const queryClient = client()
    const templates = await consumer(queryClient)
    const quickAdd = await consumer(queryClient)

    await templates.remove(10)
    await templates.restore(10)

    expect(deleted(templates)).toEqual([])
    expect(deleted(quickAdd)).toEqual([])
    expect(restoreFastOperationMock.mock.calls[0]![0]).toBe(10)
  })

  it('сохранение соседнего шаблона не стирает пометку удаления', async () => {
    const queryClient = client()
    const templates = await consumer(queryClient)

    await templates.remove(10)
    await templates.save(11, draft())

    expect(deleted(templates)).toEqual([10])
    expect(templates.operations.value.map(view => view.id)).toEqual([10, 11])
    expect(templates.operations.value[1]?.name).toBe('Кофе большой')
    expect(fetchFastOperationMock).toHaveBeenCalledWith(11)
    expect(fetchFastOperationsMock).toHaveBeenCalledTimes(1)
  })

  it('удаление во время сохранения соседа переживает обновление кэша', async () => {
    const queryClient = client()
    const templates = await consumer(queryClient)

    let release!: (fresh: FastOperation) => void

    fetchFastOperationMock.mockImplementation(() => new Promise<FastOperation>(resolve => {
      release = resolve
    }))

    const saving = templates.save(11, draft())

    await vi.waitFor(() => {
      expect(fetchFastOperationMock).toHaveBeenCalledWith(11)
    })

    await templates.remove(10)

    release({ ...operation(11), name: 'Кофе большой' })
    await saving

    expect(deleted(templates)).toEqual([10])
    expect(templates.operations.value.map(view => view.id)).toEqual([10, 11])
    expect(templates.operations.value[1]?.name).toBe('Кофе большой')
  })
})

function client(): QueryClient {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: 30_000 } } })

  onTestFinished(() => {
    queryClient.clear()
  })

  return queryClient
}

async function consumer(queryClient: QueryClient): Promise<FastOperationsState> {
  let state!: FastOperationsState

  const wrapper = mount(
    defineComponent({
      setup() {
        state = useFastOperations(computed<readonly Category[]>(() => [food]))

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

function deleted(state: FastOperationsState): number[] {
  return state.operations.value.filter(view => view.isDeleted).map(view => view.id)
}

function draft(): FastOperationDraft {
  return {
    name: 'Кофе большой',
    order: 11,
    categoryId: food.id,
    sum: Money.parseInput('300')!,
    comment: null,
    place: null,
  }
}

function operation(id: number): FastOperation {
  return {
    id,
    name: `Шаблон ${id}`,
    order: id,
    categoryId: food.id,
    sum: Money.parseInput('100')!,
    comment: null,
    place: null,
    isDeleted: false,
  }
}
