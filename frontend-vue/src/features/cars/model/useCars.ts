import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref, WritableComputedRef } from 'vue'
import { computed, ref } from 'vue'
import { createCar, fetchCars } from '@/api/adapters/cars'
import type { Car } from '@/domain/car'

export const CARS_QUERY_KEY = ['cars']

export interface CarsState {
  cars: ComputedRef<Car[]>
  isLoading: ComputedRef<boolean>
  loadError: ComputedRef<string | null>
  isEmpty: ComputedRef<boolean>
  activeCarId: WritableComputedRef<number | null>
  activeCar: ComputedRef<Car | null>
  isAddOpen: Ref<boolean>
  newCarName: Ref<string>
  canCreate: ComputedRef<boolean>
  isCreating: ComputedRef<boolean>
  toggleAdd: () => void
  create: () => Promise<void>
}

export function useCars(): CarsState {
  const queryClient = useQueryClient()

  const selectedCarId = ref<number | null>(null)
  const isAddOpen = ref(false)
  const newCarName = ref('')

  const query = useQuery({
    queryKey: CARS_QUERY_KEY,
    queryFn: fetchCars,
  })

  const cars = computed(() => query.data.value ?? [])

  const isLoading = computed(() => query.isPending.value)

  const loadError = computed(() => {
    const error = query.error.value

    return error === null ? null : error.message
  })

  const isEmpty = computed(() => !isLoading.value && loadError.value === null && cars.value.length === 0)

  const activeCarId = computed<number | null>({
    get: () => selectedCarId.value ?? cars.value[0]?.id ?? null,
    set: value => {
      selectedCarId.value = value
    },
  })

  const activeCar = computed(() => cars.value.find(car => car.id === activeCarId.value) ?? null)

  const canCreate = computed(() => newCarName.value.trim().length > 0)

  const createMutation = useMutation({
    mutationFn: (name: string) => createCar(name),
    onSuccess: async id => {
      selectedCarId.value = id
      await queryClient.invalidateQueries({ queryKey: CARS_QUERY_KEY })
    },
  })

  return {
    cars,
    isLoading,
    loadError,
    isEmpty,
    activeCarId,
    activeCar,
    isAddOpen,
    newCarName,
    canCreate,
    isCreating: computed(() => createMutation.isPending.value),
    toggleAdd: () => {
      isAddOpen.value = !isAddOpen.value
    },
    create: async () => {
      if (!canCreate.value) {
        return
      }

      await createMutation.mutateAsync(newCarName.value)
      newCarName.value = ''
      isAddOpen.value = false
    },
  }
}
