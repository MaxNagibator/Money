import { useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed, ref, shallowRef } from 'vue'
import { CATEGORIES_QUERY_KEY, fetchCategories } from '@/api/adapters/categories'
import { REGULAR_OPERATION_SAVE_ERROR } from '@/api/adapters/regular-operations'
import type { Category } from '@/domain/category'
import type { OperationTypeId } from '@/domain/operation-type'
import type { Clock } from '@/platform/clock'
import { systemClock } from '@/platform/clock'
import { SUM_UNPARSED } from '@/shared/ui/field-messages'
import type { RegularOperationFormValue } from './regular-operation-form'
import {
  categoryOptions,
  editRegularOperationForm,
  newRegularOperationForm,
  shouldResetPlaces,
  toRegularOperationDraft,
  validateRegularOperationForm,
} from './regular-operation-form'
import type { RegularOperationView } from './regular-operation-list'
import type { PlaceSuggestionsState } from '@/shared/ui/usePlaceSuggestions'
import { usePlaceSuggestions } from '@/shared/ui/usePlaceSuggestions'
import type { RegularOperationsState } from './useRegularOperations'
import { useRegularOperations } from './useRegularOperations'

export const REGULAR_OPERATION_SAVED_NOTICE = 'Успех!'
export const REGULAR_OPERATION_DELETE_ERROR = 'Не удалось удалить регулярную операцию'
export const REGULAR_OPERATION_RESTORE_ERROR = 'Не удалось восстановить регулярную операцию'

export interface RegularOperationsNotice {
  text: string
  type: 'success' | 'warning' | 'error'
}

export interface RegularOperationsScreenState {
  operations: ComputedRef<RegularOperationView[]>
  isLoading: ComputedRef<boolean>
  loadError: ComputedRef<string | null>
  isEmpty: ComputedRef<boolean>
  notice: Ref<RegularOperationsNotice | null>
  form: Ref<RegularOperationFormValue | null>
  dialogTitle: ComputedRef<string>
  dialogCategories: ComputedRef<Category[]>
  isSaving: Ref<boolean>
  places: PlaceSuggestionsState
  reload: () => Promise<void>
  startCreate: (operationTypeId: OperationTypeId | null) => void
  startEdit: (view: RegularOperationView) => void
  cancel: () => void
  submit: (value: RegularOperationFormValue) => Promise<void>
  remove: (view: RegularOperationView) => Promise<void>
  restore: (view: RegularOperationView) => Promise<void>
}

export function useRegularOperationsScreen(clock: Clock = systemClock): RegularOperationsScreenState {
  const queryClient = useQueryClient()

  const categoriesQuery = useQuery({
    queryKey: CATEGORIES_QUERY_KEY,
    queryFn: fetchCategories,
  })

  const categories = computed<readonly Category[]>(() => categoriesQuery.data.value ?? [])
  const regular: RegularOperationsState = useRegularOperations(categories)
  const places = usePlaceSuggestions()

  const notice = ref<RegularOperationsNotice | null>(null)
  const form = shallowRef<RegularOperationFormValue | null>(null)
  const isSaving = ref(false)

  const requiredTypeId = ref<OperationTypeId | null>(null)
  const openedCategoryId = ref<number | null>(null)
  const openedPlace = ref<string | null>(null)

  const isLoading = computed(() => regular.isLoading.value || categoriesQuery.isPending.value)
  const loadError = computed(() => regular.loadError.value ?? categoriesQuery.error.value?.message ?? null)
  const isEmpty = computed(() => !isLoading.value && loadError.value === null && regular.operations.value.length === 0)
  const dialogTitle = computed(() => form.value?.id === null ? 'Создать' : 'Обновить')

  const dialogCategories = computed(
    () => categoryOptions(categories.value, requiredTypeId.value, openedCategoryId.value),
  )

  return {
    operations: regular.operations,
    isLoading,
    loadError,
    isEmpty,
    notice,
    form,
    dialogTitle,
    dialogCategories,
    isSaving,
    places,
    reload: async () => {
      await queryClient.refetchQueries({ type: 'active' })
    },
    startCreate: operationTypeId => {
      requiredTypeId.value = operationTypeId
      openedCategoryId.value = null
      openedPlace.value = null
      form.value = newRegularOperationForm(clock.today())
    },
    startEdit: view => {
      requiredTypeId.value = null
      openedCategoryId.value = view.category === null ? null : view.category.id
      openedPlace.value = view.place
      form.value = editRegularOperationForm(view)
    },
    cancel: () => {
      form.value = null
    },
    submit: async value => {
      if (isSaving.value || Object.keys(validateRegularOperationForm(value)).length > 0) {
        return
      }

      if (value.sum === null) {
        notice.value = { text: SUM_UNPARSED, type: 'warning' }
        return
      }

      const previousPlace = openedPlace.value

      isSaving.value = true

      try {
        await regular.save(value.id, toRegularOperationDraft(value))

        if (shouldResetPlaces(previousPlace, value.place)) {
          places.reset()
        }

        form.value = null
        notice.value = { text: REGULAR_OPERATION_SAVED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, REGULAR_OPERATION_SAVE_ERROR), type: 'error' }
      } finally {
        isSaving.value = false
      }
    },
    remove: async view => {
      try {
        await regular.remove(view.id)
      } catch (error) {
        notice.value = { text: messageOf(error, REGULAR_OPERATION_DELETE_ERROR), type: 'error' }
      }
    },
    restore: async view => {
      try {
        await regular.restore(view.id)
      } catch (error) {
        notice.value = { text: messageOf(error, REGULAR_OPERATION_RESTORE_ERROR), type: 'error' }
      }
    },
  }
}

function messageOf(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}
