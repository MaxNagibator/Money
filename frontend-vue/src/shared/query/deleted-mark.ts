import type { QueryClient, QueryKey } from '@tanstack/vue-query'

export interface Deletable {
  id: number
  isDeleted: boolean
}

export async function markDeleted<T extends Deletable>(
  queryClient: QueryClient,
  queryKey: QueryKey,
  id: number,
  isDeleted: boolean,
): Promise<void> {
  const cached = queryClient.getQueryData<T[]>(queryKey)

  if (cached === undefined || !cached.some(item => item.id === id)) {
    await queryClient.invalidateQueries({ queryKey })
    return
  }

  queryClient.setQueryData<T[]>(queryKey, cached.map(item => item.id === id ? { ...item, isDeleted } : item))
}
