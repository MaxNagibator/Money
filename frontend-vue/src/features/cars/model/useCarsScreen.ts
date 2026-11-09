import type { ComputedRef, Ref } from 'vue'
import { computed, ref, watch } from 'vue'
import { CAR_EVENT_SAVE_ERROR } from '@/api/adapters/car-events'
import { CAR_SAVE_ERROR } from '@/api/adapters/cars'
import type { CarEvent } from '@/domain/car'
import type { Clock } from '@/platform/clock'
import { systemClock } from '@/platform/clock'
import type { CarEventFormValue } from './car-event-form'
import { editCarEventForm, newCarEventForm, toCarEventDraft, validateCarEventForm } from './car-event-form'
import type { CarEventsState } from './useCarEvents'
import { useCarEvents } from './useCarEvents'
import type { CarsState } from './useCars'
import { useCars } from './useCars'

export const CAR_SAVED_NOTICE = 'Авто успешно сохранено!'
export const CAR_EVENT_SAVED_NOTICE = 'Успех!'
export const CAR_EVENT_DELETE_ERROR = 'Не удалось удалить событие'
export const CAR_EVENT_RESTORE_ERROR = 'Не удалось восстановить событие'

export interface CarsNotice {
  text: string
  type: 'success' | 'warning' | 'error'
}

export interface CarsScreenState {
  cars: CarsState
  events: CarEventsState
  isInitializing: ComputedRef<boolean>
  notice: Ref<CarsNotice | null>
  form: Ref<CarEventFormValue | null>
  dialogTitle: ComputedRef<string>
  isSaving: Ref<boolean>
  createCar: () => Promise<void>
  startCreateEvent: () => void
  startEditEvent: (event: CarEvent) => void
  cancelEvent: () => void
  submitEvent: (value: CarEventFormValue) => Promise<void>
  removeEvent: (event: CarEvent) => Promise<void>
  removeEditedEvent: () => Promise<void>
  restoreEvent: (event: CarEvent) => Promise<void>
}

export function useCarsScreen(clock: Clock = systemClock): CarsScreenState {
  const cars = useCars()
  const events = useCarEvents(cars.activeCarId)

  const notice = ref<CarsNotice | null>(null)
  const form = ref<CarEventFormValue | null>(null)
  const isSaving = ref(false)

  const isReady = ref(false)

  const isInitializing = computed(() => !isReady.value && (cars.isLoading.value || events.isLoading.value))

  watch(isInitializing, initializing => {
    isReady.value = isReady.value || !initializing
  }, { immediate: true })

  const dialogTitle = computed(() => form.value?.id === null ? 'Создать' : 'Обновить')

  async function removeById(id: number): Promise<void> {
    try {
      await events.remove(id)
    } catch (error) {
      notice.value = { text: messageOf(error, CAR_EVENT_DELETE_ERROR), type: 'error' }
    }
  }

  return {
    cars,
    events,
    isInitializing,
    notice,
    form,
    dialogTitle,
    isSaving,
    createCar: async () => {
      try {
        await cars.create()
        notice.value = { text: CAR_SAVED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, CAR_SAVE_ERROR), type: 'warning' }
      }
    },
    startCreateEvent: () => {
      form.value = newCarEventForm(clock.today())
    },
    startEditEvent: event => {
      form.value = editCarEventForm(event)
    },
    cancelEvent: () => {
      form.value = null
    },
    submitEvent: async value => {
      const carId = cars.activeCarId.value

      if (carId === null || Object.keys(validateCarEventForm(value)).length > 0) {
        return
      }

      isSaving.value = true

      try {
        await events.save(value.id, toCarEventDraft(carId, value))
        form.value = null
        notice.value = { text: CAR_EVENT_SAVED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, CAR_EVENT_SAVE_ERROR), type: 'error' }
      } finally {
        isSaving.value = false
      }
    },
    removeEvent: async event => {
      await removeById(event.id)
    },
    removeEditedEvent: async () => {
      const id = form.value?.id ?? null

      if (id === null || isSaving.value) {
        return
      }

      form.value = null

      await removeById(id)
    },
    restoreEvent: async event => {
      try {
        await events.restore(event.id)
      } catch (error) {
        notice.value = { text: messageOf(error, CAR_EVENT_RESTORE_ERROR), type: 'error' }
      }
    },
  }
}

function messageOf(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}
