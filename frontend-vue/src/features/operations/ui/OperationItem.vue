<script setup lang="ts">
import { computed } from 'vue'
import type { Category } from '@/domain/category'
import type { Operation } from '@/domain/operation'
import type { OperationType } from '@/domain/operation-type'
import { operationTypeById } from '@/domain/operation-type'
import { formatMoney } from '@/platform/format'
import {
  mdCalendarToday,
  mdDelete,
  mdError,
  mdModeEdit,
  mdRestore,
} from '@/shared/icons'
import type { MudColor } from '@/shared/mud'
import {
  MudIcon,
  MudIconButton,
  MudItem,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'
import { recordRowName, recordSumLabel } from '@/shared/ui/record-card'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import { editOperationForm } from '../model/operation-form'
import type { OperationFormValue } from '../model/operation-form'
import OperationDialog from './OperationDialog.vue'

const UNRESOLVED_CATEGORY = 'Категория недоступна'

const props = defineProps<{
  operation: Operation
  categories: readonly Category[]
  isSaving: boolean
}>()

const emit = defineEmits<{
  submit: [value: OperationFormValue]
  notice: [text: string]
  remove: [operation: Operation]
  restore: [operation: Operation]
}>()

const visual = computed<OperationType | null>(() => {
  const operationTypeId = props.operation.category?.operationTypeId

  return operationTypeId === undefined ? null : operationTypeById(operationTypeId)
})

const visualColor = computed<MudColor>(() => {
  if (visual.value === null) {
    return 'default'
  }

  return visual.value.color as MudColor
})

const sumText = computed(() => formatMoney(props.operation.sum))

const categoryName = computed(() => props.operation.category?.name ?? UNRESOLVED_CATEGORY)

const actionSuffix = computed(() => `${categoryName.value} на ${sumText.value}`)

const isNarrow = useNarrowViewport()

const narrowSumLabel = computed(() => recordSumLabel(props.operation.category?.operationTypeId ?? null, props.operation.sum))

const narrowEditName = computed(() => recordRowName(
  `Изменить операцию ${categoryName.value}`,
  narrowSumLabel.value,
  props.operation.isDeleted ? 'удалена' : null,
))

const narrowRestoreName = computed(() => recordRowName(`Восстановить операцию ${categoryName.value}`, narrowSumLabel.value))

const isIncome = computed(() => visual.value?.sign === '+')

const narrowSum = computed(() => (isIncome.value && !props.operation.sum.isNegative ? `+${sumText.value}` : sumText.value))

const stripeStyle = computed(() => ({
  '--operation-stripe': visual.value === null
    ? 'var(--mud-palette-text-disabled)'
    : `var(--mud-palette-${visual.value.color})`,
}))

const hasPlace = computed(() => (props.operation.place ?? '').length > 0)

const hasComment = computed(() => (props.operation.comment ?? '').length > 0)

const hasMeta = computed(() => hasPlace.value || hasComment.value || props.operation.createdTaskId !== null)
</script>

<template>
  <div
    v-if="isNarrow"
    :class="{ 'operation-deleted': operation.isDeleted }"
    :data-operation-id="operation.id"
    class="operation-narrow-row"
    data-testid="operation-item"
  >
    <OperationDialog
      :categories="categories"
      :is-saving="isSaving"
      removable
      @notice="emit('notice', $event)"
      @remove="emit('remove', operation)"
      @submit="emit('submit', $event)"
    >
      <template #default="{ isOpen, open }">
        <button
          :aria-label="narrowEditName"
          :disabled="operation.isDeleted || isOpen"
          :style="stripeStyle"
          class="operation-narrow"
          data-testid="operation-item-edit"
          type="button"
          @click="open(editOperationForm(operation))"
        >
          <span class="operation-narrow-text">
            <span
              class="large-text operation-narrow-title"
              data-testid="operation-item-category"
            >
              <MudIcon
                v-if="visual === null"
                :icon="mdError"
                :title="UNRESOLVED_CATEGORY"
                color="warning"
                size="small"
              />
              {{ categoryName }}
            </span>

            <span
              v-if="hasMeta"
              class="large-text operation-narrow-meta"
            >
              <MudIcon
                v-if="operation.createdTaskId !== null"
                :icon="mdCalendarToday"
                color="tertiary"
                data-testid="operation-item-task"
                size="small"
                title="Создано регулярной задачей"
              />
              <span
                v-if="hasPlace"
                data-testid="operation-item-place"
              >{{ operation.place }}</span>
              <span v-if="hasPlace && hasComment">&nbsp;·&nbsp;</span>
              <span
                v-if="hasComment"
                data-testid="operation-item-comment"
              >{{ operation.comment }}</span>
            </span>
          </span>

          <span
            :class="{ 'operation-narrow-sum-income': isIncome }"
            class="large-text operation-narrow-sum operation-sum"
            data-testid="operation-item-sum"
          >{{ narrowSum }}</span>
        </button>
      </template>
    </OperationDialog>

    <MudIconButton
      v-if="operation.isDeleted"
      :aria-label="narrowRestoreName"
      :icon="mdRestore"
      class="restore-btn"
      data-testid="operation-item-restore"
      size="small"
      title="Восстановить"
      @click="emit('restore', operation)"
    />
  </div>

  <MudStack
    v-else
    :class="{ 'operation-deleted': operation.isDeleted }"
    :data-operation-id="operation.id"
    data-testid="operation-item"
    row
    :spacing="0"
    wrap="wrap"
  >
    <MudItem
      class="operation-cell operation-cell-sum"
      :md="2"
      :xs="4"
    >
      <div class="align-center d-flex flex-row gap-0 operation-sum-row">
        <MudIcon
          v-if="visual !== null"
          :color="visualColor"
          :icon="visual.icon"
          :title="visual.name"
          size="small"
        />
        <MudIcon
          v-else
          :icon="mdError"
          :title="UNRESOLVED_CATEGORY"
          color="warning"
          size="small"
        />
        <OperationDialog
          :categories="categories"
          :is-saving="isSaving"
          @notice="emit('notice', $event)"
          @submit="emit('submit', $event)"
        >
          <template #default="{ isOpen, open }">
            <MudIconButton
              :aria-label="`Изменить операцию ${actionSuffix}`"
              :disabled="operation.isDeleted || isOpen"
              :icon="mdModeEdit"
              data-testid="operation-item-edit"
              size="small"
              @click="open(editOperationForm(operation))"
            />
          </template>
        </OperationDialog>
        <MudText
          class="large-text operation-sum"
          data-testid="operation-item-sum"
          :title="sumText"
        >
          {{ sumText }}
        </MudText>
      </div>
    </MudItem>

    <MudItem
      class="operation-cell operation-cell-category"
      :md="3"
      :xs="4"
    >
      <MudText
        class="large-text"
        data-testid="operation-item-category"
        :title="categoryName"
      >
        {{ categoryName }}
      </MudText>
    </MudItem>

    <MudItem
      class="operation-cell operation-cell-place"
      :md="3"
      :xs="4"
    >
      <MudText
        class="large-text"
        data-testid="operation-item-place"
        :title="operation.place ?? ''"
      >
        {{ operation.place }}
      </MudText>
    </MudItem>

    <MudItem
      class="operation-cell operation-cell-comment"
      :md="3"
      :xs="11"
    >
      <MudText
        class="large-text"
        data-testid="operation-item-comment"
        :title="operation.comment ?? ''"
      >
        {{ operation.comment }}
      </MudText>
    </MudItem>

    <MudItem
      class="operation-cell operation-cell-actions"
      :md="1"
      :xs="1"
    >
      <div class="align-center d-flex flex-row gap-0">
        <MudSpacer />
        <MudIcon
          v-if="operation.createdTaskId !== null"
          :icon="mdCalendarToday"
          color="tertiary"
          data-testid="operation-item-task"
          size="small"
          title="Создано регулярной задачей"
        />
        <MudIconButton
          v-if="operation.isDeleted"
          :aria-label="`Восстановить операцию ${actionSuffix}`"
          :icon="mdRestore"
          class="restore-btn"
          data-testid="operation-item-restore"
          size="small"
          title="Восстановить"
          @click="emit('restore', operation)"
        />
        <MudIconButton
          v-else
          :aria-label="`Удалить операцию ${actionSuffix}`"
          :icon="mdDelete"
          class="trash-btn"
          data-testid="operation-item-remove"
          size="small"
          title="Удалить"
          @click="emit('remove', operation)"
        />
      </div>
    </MudItem>
  </MudStack>
</template>

<style scoped>
.large-text {
  overflow: hidden;
  max-width: 100%;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.operation-sum-row {
  overflow: hidden;
  min-width: 0;
}

.operation-narrow-row {
  display: flex;
  align-items: center;
}

.operation-narrow {
  display: flex;
  align-items: flex-start;
  flex: 1 1 auto;
  gap: 8px;
  min-width: 0;
  min-height: 48px;
  padding: 6px 0 6px 11px;
  border: 0;
  border-left: 3px solid var(--operation-stripe, transparent);
  background: none;
  color: inherit;
  font-family: inherit;
  text-align: left;
}

.operation-narrow:disabled {
  color: inherit;
}

.operation-narrow-text {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-width: 0;
  gap: 2px;
}

.operation-narrow-title {
  font-size: 0.9375rem;
  line-height: 1.33;
}

.operation-narrow-meta {
  font-size: 0.78125rem;
  line-height: 1.45;
  color: var(--mud-palette-text-secondary);
}

.operation-narrow-meta :deep(.mud-icon-root) {
  vertical-align: text-bottom;
}

.operation-narrow-sum {
  flex: 0 0 auto;
  padding-top: 1px;
  font-size: 0.9375rem;
  line-height: 1.33;
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum';
}

.operation-narrow-sum-income {
  color: var(--mud-palette-success);
}

.mud-icon-button.trash-btn {
  transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out;
}

.mud-icon-button.trash-btn:hover,
.mud-icon-button.trash-btn:hover :deep(.mud-icon-root) {
  color: var(--mud-palette-error);
}

.mud-icon-button.restore-btn {
  transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out;
}

.mud-icon-button.restore-btn,
.mud-icon-button.restore-btn :deep(.mud-icon-root) {
  color: var(--mud-palette-info);
}

.mud-icon-button.restore-btn:hover,
.mud-icon-button.restore-btn:hover :deep(.mud-icon-root) {
  color: var(--mud-palette-success);
}

.operation-deleted {
  transition: opacity 0.2s ease-in-out, filter 0.2s ease-in-out;
  opacity: 0.55;
  filter: grayscale(0.4);
}

.operation-deleted .large-text {
  text-decoration: line-through;
  text-decoration-thickness: 1px;
  text-decoration-color: var(--mud-palette-error);
}
</style>
