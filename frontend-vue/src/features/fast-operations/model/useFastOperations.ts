import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed } from 'vue'
import type { FastOperation, FastOperationDraft } from '@/api/adapters/fast-operations'
import {
  createFastOperation,
  deleteFastOperation,
  fetchFastOperation,
  fetchFastOperations,
  restoreFastOperation,
  updateFastOperation,
} from '@/api/adapters/fast-operations'
import type { Category } from '@/domain/category'
import { markDeleted } from '@/shared/query/deleted-mark'
import { refreshCachedItem } from '@/shared/query/refresh-one'
import type { FastOperationView } from './fast-operation-list'
import { sortFastOperations, withCategories } from './fast-operation-list'

export const FAST_OPERATIONS_QUERY_KEY = ['fast-operations']

export interface FastOperationsState {
  operations: ComputedRef<FastOperationView[]>
  isLoading: Ref<boolean>
  loadError: ComputedRef<string | null>
  save: (id: number | null, draft: FastOperationDraft) => Promise<void>
  remove: (id: number) => Promise<void>
  restore: (id: number) => Promise<void>
  reload: () => Promise<void>
}

export function useFastOperations(
  categories: Ref<readonly Category[]> | ComputedRef<readonly Category[]>,
): FastOperationsState {
  const queryClient = useQueryClient()

  const query = useQuery({
    queryKey: FAST_OPERATIONS_QUERY_KEY,
    queryFn: fetchFastOperations,
  })

  const operations = computed(() => sortFastOperations(withCategories(query.data.value ?? [], categories.value)))

  const loadError = computed(() => {
    const error = query.error.value

    return error === null ? null : error.message
  })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: FAST_OPERATIONS_QUERY_KEY })

  const refresh = (id: number): Promise<void> =>
    refreshCachedItem<FastOperation>(queryClient, FAST_OPERATIONS_QUERY_KEY, id, fetchFastOperation)

  const saveMutation = useMutation({
    mutationFn: async ({ id, draft }: { id: number | null, draft: FastOperationDraft }): Promise<number> => {
      if (id === null) {
        return await createFastOperation(draft)
      }

      await updateFastOperation(id, draft)

      return id
    },
    onSuccess: savedId => refresh(savedId),
  })

  // TODO: удалённый шаблон держится пометкой в кэше списка, GET удалённых не отдаёт и после перезагрузки восстановить его нечем – снять, когда список научится отдавать удалённые
  const removeMutation = useMutation({
    mutationFn: deleteFastOperation,
    onSuccess: (_, id) => markDeleted<FastOperation>(queryClient, FAST_OPERATIONS_QUERY_KEY, id, true),
  })

  const restoreMutation = useMutation({
    mutationFn: restoreFastOperation,
    onSuccess: (_, id) => markDeleted<FastOperation>(queryClient, FAST_OPERATIONS_QUERY_KEY, id, false),
  })

  return {
    operations,
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
    reload: async () => {
      await invalidate()
    },
  }
}
