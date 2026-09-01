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
</script>

<template>
  <MudStack
    :class="{ 'operation-deleted': operation.isDeleted }"
    :data-operation-id="operation.id"
    data-testid="operation-item"
    row
    :spacing="0"
    wrap="wrap"
  >
    <MudItem
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
