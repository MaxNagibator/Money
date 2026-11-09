import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed } from 'vue'
import type { RegularOperation, RegularOperationDraft } from '@/api/adapters/regular-operations'
import {
  createRegularOperation,
  deleteRegularOperation,
  fetchRegularOperation,
  fetchRegularOperations,
  restoreRegularOperation,
  updateRegularOperation,
} from '@/api/adapters/regular-operations'
import type { Category } from '@/domain/category'
import { markDeleted } from '@/shared/query/deleted-mark'
import { refreshCachedItem } from '@/shared/query/refresh-one'
import type { RegularOperationView } from './regular-operation-list'
import { sortRegularOperations, withCategories } from './regular-operation-list'

export const REGULAR_OPERATIONS_QUERY_KEY = ['regular-operations']

export interface RegularOperationsState {
  operations: ComputedRef<RegularOperationView[]>
  isLoading: Ref<boolean>
  loadError: ComputedRef<string | null>
  save: (id: number | null, draft: RegularOperationDraft) => Promise<void>
  remove: (id: number) => Promise<void>
  restore: (id: number) => Promise<void>
}

export function useRegularOperations(
  categories: Ref<readonly Category[]> | ComputedRef<readonly Category[]>,
): RegularOperationsState {
  const queryClient = useQueryClient()

  const query = useQuery({
    queryKey: REGULAR_OPERATIONS_QUERY_KEY,
    queryFn: fetchRegularOperations,
  })

  const operations = computed(() => sortRegularOperations(withCategories(query.data.value ?? [], categories.value)))

  const loadError = computed(() => {
    const error = query.error.value

    return error === null ? null : error.message
  })

  const refresh = (id: number): Promise<void> =>
    refreshCachedItem<RegularOperation>(queryClient, REGULAR_OPERATIONS_QUERY_KEY, id, fetchRegularOperation)

  const saveMutation = useMutation({
    mutationFn: async ({ id, draft }: { id: number | null, draft: RegularOperationDraft }): Promise<number> => {
      if (id === null) {
        return await createRegularOperation(draft)
      }

      await updateRegularOperation(id, draft)

      return id
    },
    onSuccess: savedId => refresh(savedId),
  })

  // TODO: удалённая операция держится пометкой в кэше списка, GET удалённых не отдаёт и после перезагрузки восстановить её нечем – снять, когда список научится отдавать удалённые
  const removeMutation = useMutation({
    mutationFn: (id: number) => deleteRegularOperation(id),
    onSuccess: (_, id) => markDeleted<RegularOperation>(queryClient, REGULAR_OPERATIONS_QUERY_KEY, id, true),
  })

  const restoreMutation = useMutation({
    mutationFn: (id: number) => restoreRegularOperation(id),
    onSuccess: (_, id) => markDeleted<RegularOperation>(queryClient, REGULAR_OPERATIONS_QUERY_KEY, id, false),
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
  }
}
