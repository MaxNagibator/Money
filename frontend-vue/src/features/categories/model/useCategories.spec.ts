import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { defineComponent } from 'vue'
import type { CategoryDraft } from '@/api/adapters/categories'
import {
  deleteCategory,
  fetchCategories,
  fetchCategory,
  updateCategory,
} from '@/api/adapters/categories'
import type { Category } from '@/domain/category'
import type { CategoriesState } from './useCategories'
import { useCategories } from './useCategories'

vi.mock('@/api/adapters/categories', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/categories')>(),
  fetchCategories: vi.fn(),
  fetchCategory: vi.fn(),
  updateCategory: vi.fn(),
  deleteCategory: vi.fn(),
}))

const fetchCategoriesMock = vi.mocked(fetchCategories)
const fetchCategoryMock = vi.mocked(fetchCategory)
const updateCategoryMock = vi.mocked(updateCategory)
const deleteCategoryMock = vi.mocked(deleteCategory)

const removedOnServer = new Set<number>()

beforeEach(() => {
  vi.clearAllMocks()
  removedOnServer.clear()

  fetchCategoriesMock.mockImplementation(() => Promise.resolve(
    [category(1, 'Еда'), category(2, 'Кафе')].filter(item => !removedOnServer.has(item.id)),
  ))
  fetchCategoryMock.mockImplementation(id => Promise.resolve(category(id, 'Кафе у дома')))
  updateCategoryMock.mockResolvedValue(undefined)
  deleteCategoryMock.mockImplementation(id => {
    removedOnServer.add(id)

    return Promise.resolve()
  })
})

describe('пометка удаления в списке категорий', () => {
  it('сохранение соседней категории не стирает пометку удаления', async () => {
    const state = await consumer()

    await state.remove(1)
    await state.save(2, draft())

    expect(deleted(state)).toEqual([1])
    expect(state.categories.value.map(item => item.id)).toEqual([1, 2])
    expect(state.categories.value[1]?.name).toBe('Кафе у дома')
    expect(fetchCategoryMock).toHaveBeenCalledWith(2)
    expect(fetchCategoriesMock).toHaveBeenCalledTimes(1)
  })
})

async function consumer(): Promise<CategoriesState> {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false, staleTime: 30_000 } } })

  onTestFinished(() => {
    queryClient.clear()
  })

  let state!: CategoriesState

  const wrapper = mount(
    defineComponent({
      setup() {
        state = useCategories()

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

function deleted(state: CategoriesState): number[] {
  return state.categories.value.filter(item => item.isDeleted).map(item => item.id)
}

function draft(): CategoryDraft {
  return {
    name: 'Кафе у дома',
    color: null,
    order: null,
    parentId: 1,
    operationTypeId: 1,
  }
}

function category(id: number, name: string): Category {
  return {
    id,
    parentId: id === 1 ? null : 1,
    name,
    color: null,
    order: null,
    operationTypeId: 1,
    isDeleted: false,
  }
}
