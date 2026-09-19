<script setup lang="ts">
import { computed, onMounted, ref, useTemplateRef } from 'vue'
import { useCategories } from '@/features/categories'
import type { OperationFormValue } from '@/features/operations'
import {
  matchesOperationFilter,
  newOperationForm,
  OperationDialog,
  provideOperationSave,
  toOperationFilter,
  useOperationFiltersStore,
  useOperationSave,
} from '@/features/operations'
import { systemClock } from '@/platform/clock'
import { MudSnackbar } from '@/shared/mud'

const EXPENSE_TYPE_ID = 1
const SAVED_NOTICE = 'Операция сохранена'
const OUTSIDE_FILTER_NOTICE = 'Операция сохранена. Она не подходит под условия показа, поэтому на экране её нет.'

interface OperationDialogInstance {
  open: (value: OperationFormValue) => void
}

const categoriesState = useCategories()
const filtersState = useOperationFiltersStore()
const save = useOperationSave()

const isSaving = ref(false)
const notice = ref('')

const noticeVisible = computed({
  get: () => notice.value !== '',
  set: (visible: boolean) => {
    if (!visible) {
      notice.value = ''
    }
  },
})

provideOperationSave(async value => {
  isSaving.value = true

  try {
    await save(value)
  } finally {
    isSaving.value = false
  }
})

function reportSaved(value: OperationFormValue): void {
  const shown = matchesOperationFilter(value, toOperationFilter(filtersState.applied))

  notice.value = shown ? SAVED_NOTICE : OUTSIDE_FILTER_NOTICE
}

const dialog = useTemplateRef<OperationDialogInstance>('dialog')

onMounted(() => {
  dialog.value?.open(newOperationForm(EXPENSE_TYPE_ID, systemClock.today()))
})
</script>

<template>
  <OperationDialog
    ref="dialog"
    :categories="categoriesState.categories.value"
    :is-saving="isSaving"
    @notice="notice = $event"
    @submit="reportSaved"
  />

  <MudSnackbar
    v-model="noticeVisible"
    data-testid="bottom-nav-notice"
    :message="notice"
    :visible-state-duration="4000"
  />
</template>
