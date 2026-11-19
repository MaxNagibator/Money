import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed, shallowRef } from 'vue'
import type { DebtDraft, DebtForgiveDraft, DebtPaymentDraft } from '@/api/adapters/debts'
import {
  createDebt,
  deleteDebt,
  fetchDebts,
  forgiveDebts,
  payDebt,
  restoreDebt,
  updateDebt,
} from '@/api/adapters/debts'
import type { DebtMovement } from '@/domain/debt'

export const DEBTS_QUERY_KEY = 'debts'

export interface DebtsState {
  debts: ComputedRef<DebtMovement[]>
  isLoading: ComputedRef<boolean>
  loadError: ComputedRef<string | null>
  save: (id: number | null, draft: DebtDraft) => Promise<void>
  remove: (id: number) => Promise<void>
  restore: (id: number) => Promise<void>
  pay: (id: number, draft: DebtPaymentDraft) => Promise<void>
  forgive: (draft: DebtForgiveDraft) => Promise<void>
}

export function useDebts(withPaid: Ref<boolean>): DebtsState {
  const queryClient = useQueryClient()

  // TODO: удалённый долг держится надгробием в памяти вкладки, после перезагрузки восстановить его нечем; снять, когда GET /Debts научится отдавать удалённые по флагу
  const tombstones = shallowRef(new Map<number, DebtMovement>())

  const queryKey = computed(() => [DEBTS_QUERY_KEY, withPaid.value])

  const query = useQuery({
    queryKey,
    queryFn: () => fetchDebts(withPaid.value),
  })

  const debts = computed<DebtMovement[]>(() => {
    const buried = tombstones.value
    const loaded = (query.data.value ?? []).map(movement => ({ ...movement, isDeleted: buried.has(movement.id) }))
    const loadedIds = new Set(loaded.map(movement => movement.id))

    const kept = [...buried.values()]
      .filter(movement => !loadedIds.has(movement.id))
      .map(movement => ({ ...movement, isDeleted: true }))

    return [...loaded, ...kept]
  })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: [DEBTS_QUERY_KEY] })

  const saveMutation = useMutation({
    mutationFn: async ({ id, draft }: { id: number | null, draft: DebtDraft }) => {
      if (id === null) {
        await createDebt(draft)
        return
      }

      await updateDebt(id, draft)
    },
    onSuccess: invalidate,
  })

  const removeMutation = useMutation({
    mutationFn: (id: number) => deleteDebt(id),
    onSuccess: (_, id) => {
      const removed = debts.value.find(movement => movement.id === id)

      if (removed === undefined) {
        return
      }

      tombstones.value = new Map(tombstones.value).set(id, { ...removed, isDeleted: true })
    },
  })

  const restoreMutation = useMutation({
    mutationFn: (id: number) => restoreDebt(id),
    onSuccess: (_, id) => {
      const next = new Map(tombstones.value)

      next.delete(id)
      tombstones.value = next

      return invalidate()
    },
  })

  const payMutation = useMutation({
    mutationFn: ({ id, draft }: { id: number, draft: DebtPaymentDraft }) => payDebt(id, draft),
    onSuccess: invalidate,
  })

  const forgiveMutation = useMutation({
    mutationFn: (draft: DebtForgiveDraft) => forgiveDebts(draft),
    onSuccess: invalidate,
  })

  return {
    debts,
    isLoading: computed(() => query.isPending.value),
    loadError: computed(() => {
      const error = query.error.value

      return error === null ? null : error.message
    }),
    save: async (id, draft) => {
      await saveMutation.mutateAsync({ id, draft })
    },
    remove: async id => {
      await removeMutation.mutateAsync(id)
    },
    restore: async id => {
      await restoreMutation.mutateAsync(id)
    },
    pay: async (id, draft) => {
      await payMutation.mutateAsync({ id, draft })
    },
    forgive: async draft => {
      await forgiveMutation.mutateAsync(draft)
    },
  }
}
