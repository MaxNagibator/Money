<script setup lang="ts">
import { computed, ref } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import type { Operation } from '@/domain/operation'
import { OPERATION_TYPES } from '@/domain/operation-type'
import { useCategories } from '@/features/categories'
import { useFastOperations } from '@/features/fast-operations'
import type { FastOperation, OperationFormValue } from '@/features/operations'
import {
  fastOperationForm,
  newOperationForm,
  OperationDialog,
  OperationFiltersPanel,
  OperationsList,
  OperationsPeriodSummary,
  provideOperationSave,
  QuickOperationMenu,
  toOperationDraft,
  useOperationFiltersStore,
  useOperations,
} from '@/features/operations'
import { systemClock } from '@/platform/clock'
import type { MudColor } from '@/shared/mud'
import { MudAlert, MudButton, MudSnackbar, MudStack } from '@/shared/mud'

const categoriesState = useCategories()
const filtersState = useOperationFiltersStore()

filtersState.apply()

const operations = useOperations(filtersState.applied, categoriesState.categories)
const fastOperations = useFastOperations(categoriesState.categories)

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

function quickForm(date: LocalDate, operation: FastOperation): OperationFormValue {
  const category = categoriesState.categories.value.find(item => item.id === operation.categoryId) ?? null

  return fastOperationForm(operation, category, date)
}

async function save(value: OperationFormValue): Promise<void> {
  isSaving.value = true

  try {
    const draft = toOperationDraft(value)

    if (value.id === null) {
      await operations.create(draft)
    } else {
      await operations.update(value.id, draft)
    }
  } finally {
    isSaving.value = false
  }
}

provideOperationSave(save)

async function search(): Promise<void> {
  filtersState.apply()
  await operations.search()
}

function saved(): void {
  notice.value = 'Операция сохранена'
}

async function remove(operation: Operation): Promise<void> {
  try {
    await operations.remove(operation.id)
    notice.value = 'Операция удалена'
  } catch (error) {
    notice.value = error instanceof Error ? error.message : 'Не удалось удалить операцию'
  }
}

async function transfer(categoryId: number): Promise<void> {
  try {
    await operations.transfer(categoryId)
    notice.value = 'Операции перенесены'
  } catch (error) {
    notice.value = error instanceof Error ? error.message : 'Не удалось перенести операции'
  }
}

async function restore(operation: Operation): Promise<void> {
  try {
    await operations.restore(operation.id)
    notice.value = 'Операция восстановлена'
  } catch (error) {
    notice.value = error instanceof Error ? error.message : 'Не удалось восстановить операцию'
  }
}

function operationColor(color: string): MudColor {
  return color as MudColor
}
</script>

<template>
  <div class="operations-page">
    <OperationFiltersPanel
      :state="filtersState"
      :categories="categoriesState.categories.value"
      @search="search"
      @transfer="transfer"
    />

    <OperationsPeriodSummary
      :from="filtersState.applied.from"
      :to="filtersState.applied.to"
      :totals="operations.totals.value"
    />

    <div>
      <OperationDialog
        :categories="categoriesState.categories.value"
        :is-saving="isSaving"
        @notice="notice = $event"
        @submit="saved"
      >
        <template #default="{ isOpen, open }">
          <MudStack
            align-items="start"
            row
          >
            <MudButton
              v-for="type in OPERATION_TYPES"
              :key="type.id"
              :color="operationColor(type.color)"
              :data-testid="`operations-add-${type.id}`"
              :disabled="isOpen"
              :end-icon="type.icon"
              size="small"
              variant="filled"
              @click="open(newOperationForm(type.id, systemClock.today()))"
            >
              {{ type.addLabel }}
            </MudButton>

            <QuickOperationMenu
              :disabled="isOpen"
              :operations="fastOperations.operations.value"
              action-label="Быстрое добавление"
              color="info"
              label="Быстрое добавление"
              test-id="operations-quick"
              @select="open(quickForm(systemClock.today(), $event))"
            />
          </MudStack>
        </template>
      </OperationDialog>
    </div>

    <MudAlert
      v-if="categoriesState.loadError.value !== null"
      class="mb-4"
      data-testid="operations-categories-error"
      severity="warning"
      variant="text"
    >
      {{ categoriesState.loadError.value }}
    </MudAlert>

    <OperationsList
      :categories="categoriesState.categories.value"
      :days="operations.days.value"
      :fast-operations="fastOperations.operations.value"
      :is-saving="isSaving"
      :is-loading="operations.isLoading.value"
      :error-message="operations.errorMessage.value"
      @notice="notice = $event"
      @remove="remove"
      @restore="restore"
      @submit="saved"
    />

    <MudSnackbar
      v-model="noticeVisible"
      data-testid="operations-notice"
      :message="notice"
      :visible-state-duration="4000"
    />
  </div>
</template>
