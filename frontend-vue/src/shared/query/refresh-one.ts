import type { QueryClient, QueryKey } from '@tanstack/vue-query'

export interface Identified {
  id: number
}

export async function refreshCachedItem<T extends Identified>(
  queryClient: QueryClient,
  queryKey: QueryKey,
  id: number,
  fetchOne: (id: number) => Promise<T | null>,
): Promise<void> {
  if (queryClient.getQueryData<T[]>(queryKey) === undefined) {
    await queryClient.invalidateQueries({ queryKey })
    return
  }

  // TODO: отказ GET одной записи уводит на перезапрос списка и стирает клиентские пометки удаления – снять, когда список научится отдавать удалённые
  const fresh = await fetchOne(id).catch(() => null)

  if (fresh === null) {
    await queryClient.invalidateQueries({ queryKey })
    return
  }

  queryClient.setQueryData<T[]>(queryKey, current =>
    current === undefined ? undefined : replaceOrAppend(current, fresh))
}

function replaceOrAppend<T extends Identified>(items: readonly T[], fresh: T): T[] {
  const isKnown = items.some(item => item.id === fresh.id)

  return isKnown
    ? items.map(item => item.id === fresh.id ? fresh : item)
    : [...items, fresh]
}
