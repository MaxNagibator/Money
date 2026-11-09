import type { QueryClient } from '@tanstack/vue-query'
import { createOperation, updateOperation } from '@/api/adapters/operations'
import type { OperationFormValue } from './operation-form'
import { toOperationDraft } from './operation-form'
import { pinOperation, pinsEpoch } from './operation-pins'
import { OPERATIONS_QUERY_KEY } from './useOperations'

export async function saveOperation(queryClient: QueryClient, value: OperationFormValue): Promise<void> {
  const draft = toOperationDraft(value)
  const since = pinsEpoch()

  if (value.id === null) {
    pinOperation(await createOperation(draft), draft, null, since)
  } else {
    await updateOperation(value.id, draft)
    pinOperation(value.id, draft, null, since)
  }

  await queryClient.invalidateQueries({ queryKey: [OPERATIONS_QUERY_KEY] })
}
