import { useQuery } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed, ref, shallowRef } from 'vue'
import { CATEGORIES_QUERY_KEY, fetchCategories } from '@/api/adapters/categories'
import type { Category } from '@/domain/category'
import type { OperationTypeId } from '@/domain/operation-type'
import { SUM_UNPARSED } from '@/shared/ui/field-messages'
import type { FastOperationFormValue } from './fast-operation-form'
import {
  editFastOperationForm,
  newFastOperationForm,
  shouldResetPlaces,
  toFastOperationDraft,
  validateFastOperationForm,
} from './fast-operation-form'
import type { FastOperationView } from './fast-operation-list'
import type { PlaceSuggestionsState } from '@/shared/ui/usePlaceSuggestions'
import { usePlaceSuggestions } from '@/shared/ui/usePlaceSuggestions'
import type { FastOperationsState } from './useFastOperations'
import { useFastOperations } from './useFastOperations'

export const FAST_OPERATION_SAVED_NOTICE = 'Успех!'
export const FAST_OPERATION_SAVE_ERROR = 'Ошибка. Пожалуйста, попробуйте еще раз.'
export const FAST_OPERATION_DELETE_ERROR = 'Не удалось удалить быструю операцию'
export const FAST_OPERATION_RESTORE_ERROR = 'Не удалось восстановить быструю операцию'

export interface FastOperationsNotice {
  text: string
  type: 'success' | 'warning' | 'error'
}

export interface FastOperationsScreenState {
  operations: ComputedRef<FastOperationView[]>
  categories: ComputedRef<readonly Category[]>
  isLoading: ComputedRef<boolean>
  loadError: ComputedRef<string | null>
  isEmpty: ComputedRef<boolean>
  notice: Ref<FastOperationsNotice | null>
  form: Ref<FastOperationFormValue | null>
  dialogTitle: ComputedRef<string>
  isSaving: Ref<boolean>
  places: PlaceSuggestionsState
  reload: () => Promise<void>
  startCreate: (operationTypeId: OperationTypeId | null) => void
  startEdit: (view: FastOperationView) => void
  cancel: () => void
  submit: (value: FastOperationFormValue) => Promise<void>
  remove: (view: FastOperationView) => Promise<void>
  removeEdited: () => Promise<void>
  restore: (view: FastOperationView) => Promise<void>
}

export function useFastOperationsScreen(): FastOperationsScreenState {
  const categoriesQuery = useQuery({
    queryKey: CATEGORIES_QUERY_KEY,
    queryFn: fetchCategories,
  })

  const categories = computed<readonly Category[]>(() => categoriesQuery.data.value ?? [])
  const fast: FastOperationsState = useFastOperations(categories)
  const places = usePlaceSuggestions()

  const notice = ref<FastOperationsNotice | null>(null)
  const form = shallowRef<FastOperationFormValue | null>(null)
  const isSaving = ref(false)

  const isLoading = computed(() => fast.isLoading.value || categoriesQuery.isPending.value)
  const loadError = computed(() => fast.loadError.value ?? categoriesQuery.error.value?.message ?? null)
  const isEmpty = computed(() => !isLoading.value && loadError.value === null && fast.operations.value.length === 0)
  const dialogTitle = computed(() => form.value?.id === null ? 'Создать' : 'Обновить')

  async function removeById(id: number): Promise<void> {
    try {
      await fast.remove(id)
    } catch (error) {
      notice.value = { text: messageOf(error, FAST_OPERATION_DELETE_ERROR), type: 'error' }
    }
  }

  return {
    operations: fast.operations,
    categories,
    isLoading,
    loadError,
    isEmpty,
    notice,
    form,
    dialogTitle,
    isSaving,
    places,
    reload: fast.reload,
    startCreate: operationTypeId => {
      form.value = newFastOperationForm(operationTypeId)
    },
    startEdit: view => {
      form.value = editFastOperationForm(view)
    },
    cancel: () => {
      form.value = null
    },
    submit: async value => {
      if (Object.keys(validateFastOperationForm(value)).length > 0) {
        return
      }

      if (value.sum === null) {
        notice.value = { text: SUM_UNPARSED, type: 'warning' }
        return
      }

      const previousPlace = form.value?.place ?? null

      isSaving.value = true

      try {
        await fast.save(value.id, toFastOperationDraft(value))

        if (shouldResetPlaces(previousPlace, value.place)) {
          places.reset()
        }

        form.value = null
        notice.value = { text: FAST_OPERATION_SAVED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, FAST_OPERATION_SAVE_ERROR), type: 'error' }
      } finally {
        isSaving.value = false
      }
    },
    remove: async view => {
      await removeById(view.id)
    },
    removeEdited: async () => {
      const id = form.value?.id ?? null

      if (id === null || isSaving.value) {
        return
      }

      form.value = null

      await removeById(id)
    },
    restore: async view => {
      try {
        await fast.restore(view.id)
      } catch (error) {
        notice.value = { text: messageOf(error, FAST_OPERATION_RESTORE_ERROR), type: 'error' }
      }
    },
  }
}

function messageOf(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}
