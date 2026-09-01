import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef } from 'vue'
import { computed, ref } from 'vue'
import { fetchDebtOwners, mergeDebtOwners } from '@/api/adapters/debts'
import type { DebtOwner } from '@/domain/debt'
import { DEBTS_QUERY_KEY } from './useDebts'

export const DEBT_OWNERS_QUERY_KEY = 'debt-owners'

export interface DebtOwnersState {
  owners: ComputedRef<DebtOwner[]>
  isLoading: ComputedRef<boolean>
  loadError: ComputedRef<string | null>
  load: () => void
  merge: (fromUserId: number, toUserId: number) => Promise<void>
}

export function useDebtOwners(): DebtOwnersState {
  const queryClient = useQueryClient()
  const isRequested = ref(false)

  const query = useQuery({
    queryKey: [DEBT_OWNERS_QUERY_KEY],
    queryFn: fetchDebtOwners,
    enabled: isRequested,
    staleTime: Number.POSITIVE_INFINITY,
  })

  const mergeMutation = useMutation({
    mutationFn: ({ fromUserId, toUserId }: { fromUserId: number, toUserId: number }) => mergeDebtOwners(fromUserId, toUserId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [DEBT_OWNERS_QUERY_KEY] })
      await queryClient.invalidateQueries({ queryKey: [DEBTS_QUERY_KEY] })
    },
  })

  return {
    owners: computed(() => query.data.value ?? []),
    isLoading: computed(() => query.isFetching.value),
    loadError: computed(() => {
      const error = query.error.value

      return error === null ? null : error.message
    }),
    load: () => {
      if (!isRequested.value) {
        isRequested.value = true
        return
      }

      void queryClient.invalidateQueries({ queryKey: [DEBT_OWNERS_QUERY_KEY] })
    },
    merge: async (fromUserId, toUserId) => {
      await mergeMutation.mutateAsync({ fromUserId, toUserId })
    },
  }
}
