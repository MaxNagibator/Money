<script setup lang="ts">
import { computed } from 'vue'
import type { FastOperation } from '@/api/adapters/fast-operations'
import type { Category } from '@/domain/category'
import type { Operation, OperationsDay } from '@/domain/operation'
import { mdArrowCircleUp, mdInbox } from '@/shared/icons'
import {
  MudAlert,
  MudFab,
  MudIcon,
  MudPaper,
  MudProgressLinear,
  MudScrollToTop,
  MudText,
} from '@/shared/mud'
import VirtualList from '@/shared/ui/VirtualList.vue'
import type { OperationFormValue } from '../model/operation-form'
import OperationsDayCard from './OperationsDayCard.vue'

const props = defineProps<{
  days: readonly OperationsDay[]
  fastOperations: readonly FastOperation[]
  categories: readonly Category[]
  isSaving: boolean
  isLoading: boolean
  errorMessage: string | null
}>()

const emit = defineEmits<{
  submit: [value: OperationFormValue]
  notice: [text: string]
  remove: [operation: Operation]
  restore: [operation: Operation]
}>()

const DAY_CARD_ESTIMATE = 220

const items = computed(() => props.days)

function dayKey(day: OperationsDay): string {
  return day.date
}
</script>

<template>
  <div
    class="operations-list"
    data-testid="operations-list"
  >
    <MudProgressLinear
      v-if="isLoading"
      aria-label="Загрузка операций"
      class="my-4"
      color="tertiary"
      data-testid="operations-list-loading"
      indeterminate
    />

    <MudAlert
      v-else-if="errorMessage !== null"
      data-testid="operations-list-error"
      role="alert"
      severity="error"
      variant="text"
    >
      {{ errorMessage }}
    </MudAlert>

    <MudPaper
      v-else-if="items.length === 0"
      class="pa-6 my-4 d-flex flex-column align-center"
      data-testid="operations-list-empty"
      :elevation="0"
      role="status"
    >
      <MudIcon
        :icon="mdInbox"
        color="default"
        size="large"
      />
      <MudText
        class="mt-2"
        color="secondary"
        typo="body1"
      >
        Нет операций за выбранный период
      </MudText>
    </MudPaper>

    <template v-else>
      <VirtualList
        data-testid="operations-list-days"
        :estimate="DAY_CARD_ESTIMATE"
        :item-key="dayKey"
        :items="items"
      >
        <template #default="{ item }">
          <OperationsDayCard
            :categories="categories"
            :day="item"
            :fast-operations="fastOperations"
            :is-saving="isSaving"
            @notice="emit('notice', $event)"
            @remove="emit('remove', $event)"
            @restore="emit('restore', $event)"
            @submit="emit('submit', $event)"
          />
        </template>
      </VirtualList>

      <MudScrollToTop
        aria-label="Вернуться к началу списка"
        data-testid="operations-list-top"
      >
        <MudFab
          :start-icon="mdArrowCircleUp"
          color="tertiary"
          size="small"
          title="Вернуться к началу списка"
        />
      </MudScrollToTop>
    </template>
  </div>
</template>
