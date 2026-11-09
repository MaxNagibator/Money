import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed, shallowRef } from 'vue'
import type { CarEventDraft } from '@/api/adapters/car-events'
import {
  createCarEvent,
  deleteCarEvent,
  fetchCarEvents,
  restoreCarEvent,
  updateCarEvent,
} from '@/api/adapters/car-events'
import type { CarEvent } from '@/domain/car'
import { maxMileage, sortCarEvents } from '@/domain/car'

export const CAR_EVENTS_QUERY_KEY = 'car-events'

export interface CarEventsState {
  events: ComputedRef<CarEvent[]>
  isLoading: ComputedRef<boolean>
  loadError: ComputedRef<string | null>
  topMileage: ComputedRef<number>
  save: (id: number | null, draft: CarEventDraft) => Promise<void>
  remove: (id: number) => Promise<void>
  restore: (id: number) => Promise<void>
}

export function useCarEvents(carId: Ref<number | null> | ComputedRef<number | null>): CarEventsState {
  const queryClient = useQueryClient()
  const tombstones = shallowRef(new Map<number, CarEvent>())

  const query = useQuery({
    queryKey: computed(() => [CAR_EVENTS_QUERY_KEY, carId.value]),
    queryFn: () => fetchCarEvents(carId.value!),
    enabled: computed(() => carId.value !== null),
    staleTime: Number.POSITIVE_INFINITY,
  })

  const events = computed<CarEvent[]>(() => {
    const active = carId.value
    const buried = tombstones.value

    if (active === null) {
      return []
    }

    const loaded = (query.data.value ?? []).map(event => ({ ...event, isDeleted: buried.has(event.id) }))
    const loadedIds = new Set(loaded.map(event => event.id))

    const kept = [...buried.values()]
      .filter(event => event.carId === active && !loadedIds.has(event.id))
      .map(event => ({ ...event, isDeleted: true }))

    return sortCarEvents([...loaded, ...kept])
  })

  const invalidate = (id: number) => queryClient.invalidateQueries({ queryKey: [CAR_EVENTS_QUERY_KEY, id] })

  const saveMutation = useMutation({
    mutationFn: async ({ id, draft }: { id: number | null, draft: CarEventDraft }) => {
      if (id === null) {
        await createCarEvent(draft)
        return
      }

      await updateCarEvent(id, draft)
    },
    onSuccess: (_, { draft }) => invalidate(draft.carId),
  })

  const removeMutation = useMutation({
    mutationFn: (removed: CarEvent) => deleteCarEvent(removed.id),
    onSuccess: (_, removed) => {
      tombstones.value = new Map(tombstones.value).set(removed.id, { ...removed, isDeleted: true })
    },
  })

  const restoreMutation = useMutation({
    mutationFn: (id: number) => restoreCarEvent(id),
    onSuccess: (_, id) => {
      const next = new Map(tombstones.value)
      const restored = next.get(id)

      next.delete(id)
      tombstones.value = next

      return restored === undefined ? undefined : invalidate(restored.carId)
    },
  })

  return {
    events,
    isLoading: computed(() => carId.value !== null && query.isPending.value),
    loadError: computed(() => {
      const error = query.error.value

      return error === null ? null : error.message
    }),
    topMileage: computed(() => maxMileage(events.value)),
    save: async (id, draft) => {
      await saveMutation.mutateAsync({ id, draft })
    },
    remove: async id => {
      const removed = events.value.find(event => event.id === id)

      if (removed === undefined) {
        return
      }

      await removeMutation.mutateAsync(removed)
    },
    restore: async id => {
      await restoreMutation.mutateAsync(id)
    },
  }
}
