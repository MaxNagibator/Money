import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed, ref } from 'vue'
import type { CategoryDraft } from '@/api/adapters/categories'
import {
  CATEGORIES_QUERY_KEY,
  createCategory,
  deleteCategory,
  fetchCategories,
  fetchCategory,
  restoreCategory,
  updateCategory,
} from '@/api/adapters/categories'
import type { Category } from '@/domain/category'
import { refreshCachedItem } from '@/shared/query/refresh-one'

export interface CategoriesState {
  categories: ComputedRef<Category[]>
  isLoading: Ref<boolean>
  loadError: ComputedRef<string | null>
  save: (id: number | null, draft: CategoryDraft) => Promise<void>
  remove: (id: number) => Promise<void>
  restore: (id: number) => Promise<void>
}

export function useCategories(): CategoriesState {
  const queryClient = useQueryClient()
  const deletedIds = ref(new Set<number>())

  const query = useQuery({
    queryKey: CATEGORIES_QUERY_KEY,
    queryFn: fetchCategories,
  })

  const categories = computed(() => (query.data.value ?? []).map(category => ({
    ...category,
    isDeleted: deletedIds.value.has(category.id),
  })))

  const loadError = computed(() => {
    const error = query.error.value

    return error === null ? null : error.message
  })

  const refresh = (id: number): Promise<void> =>
    refreshCachedItem<Category>(queryClient, CATEGORIES_QUERY_KEY, id, fetchCategory)

  const saveMutation = useMutation({
    mutationFn: async ({ id, draft }: { id: number | null, draft: CategoryDraft }): Promise<number> => {
      if (id === null) {
        return await createCategory(draft)
      }

      await updateCategory(id, draft)

      return id
    },
    onSuccess: savedId => refresh(savedId),
  })

  // TODO: удалённая категория держится пометкой в памяти экрана, GET удалённых не отдаёт и после перезагрузки восстановить её нечем – снять, когда список научится отдавать удалённые
  const removeMutation = useMutation({
    mutationFn: deleteCategory,
    onSuccess: (_, id) => {
      deletedIds.value = new Set(deletedIds.value).add(id)
    },
  })

  const restoreMutation = useMutation({
    mutationFn: restoreCategory,
    onSuccess: (_, id) => {
      const next = new Set(deletedIds.value)

      next.delete(id)
      deletedIds.value = next
    },
  })

  return {
    categories,
    isLoading: query.isPending,
    loadError,
    save: async (id, draft) => {
      await saveMutation.mutateAsync({ id, draft })
    },
    remove: async id => {
      await removeMutation.mutateAsync(id)
    },
    restore: async id => {
      await restoreMutation.mutateAsync(id)
    },
  }
}
