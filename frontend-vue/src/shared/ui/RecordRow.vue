<script setup lang="ts">
import { computed } from 'vue'
import type { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import { operationTypeById } from '@/domain/operation-type'
import { formatMoney } from '@/platform/format'
import { mdRestore } from '@/shared/icons'
import { MudIconButton } from '@/shared/mud'
import { recordRowName, recordSumLabel } from './record-card'

const {
  operationTypeId = null,
  sum = null,
  sumTestId = '',
  editLabel,
  restoreLabel,
  deleted = false,
  deletedMark = 'удалена',
} = defineProps<{
  operationTypeId?: OperationTypeId | null
  sum?: Money | null
  editLabel: string
  restoreLabel: string
  editTestId: string
  restoreTestId: string
  sumTestId?: string
  deleted?: boolean
  deletedMark?: string
}>()

const emit = defineEmits<{
  edit: []
  restore: []
}>()

defineSlots<{
  title: () => unknown
  meta?: () => unknown
}>()

const type = computed(() => (operationTypeId === null ? null : operationTypeById(operationTypeId)))

const isIncome = computed(() => type.value?.sign === '+')

const sumText = computed(() => {
  if (sum === null) {
    return null
  }

  const text = formatMoney(sum)

  return isIncome.value && !sum.isNegative ? `+${text}` : text
})

const sumLabel = computed(() => (sum === null ? null : recordSumLabel(operationTypeId, sum)))

const accessibleName = computed(() => recordRowName(editLabel, sumLabel.value, deleted ? deletedMark : null))

const restoreName = computed(() => recordRowName(restoreLabel, sumLabel.value))

const stripeStyle = computed(() => {
  if (sum === null) {
    return {}
  }

  return {
    '--record-row-stripe': type.value === null
      ? 'var(--mud-palette-text-disabled)'
      : `var(--mud-palette-${type.value.color})`,
  }
})
</script>

<template>
  <div
    :class="{ 'record-row--deleted': deleted }"
    class="record-row"
  >
    <button
      :aria-label="accessibleName"
      :data-testid="editTestId"
      :disabled="deleted"
      :style="stripeStyle"
      class="record-row-edit"
      type="button"
      @click="emit('edit')"
    >
      <span class="record-row-text">
        <span class="record-row-line record-row-title">
          <slot name="title" />
        </span>

        <span
          v-if="$slots.meta"
          class="record-row-line record-row-meta"
        >
          <slot name="meta" />
        </span>
      </span>

      <span
        v-if="sumText !== null"
        :class="{ 'record-row-sum--income': isIncome }"
        :data-testid="sumTestId"
        class="record-row-line record-row-sum"
      >{{ sumText }}</span>
    </button>

    <MudIconButton
      v-if="deleted"
      :aria-label="restoreName"
      :data-testid="restoreTestId"
      :icon="mdRestore"
      class="record-row-restore"
      size="small"
      title="Восстановить"
      @click="emit('restore')"
    />
  </div>
</template>

<style scoped>
.record-row {
  display: flex;
  align-items: center;
}

.record-row + .record-row {
  border-top: 1px solid var(--mud-palette-divider);
}

.record-row-edit {
  display: flex;
  align-items: flex-start;
  flex: 1 1 auto;
  gap: 8px;
  min-width: 0;
  min-height: 48px;
  padding: 6px 12px 6px 11px;
  border: 0;
  border-left: 3px solid var(--record-row-stripe, transparent);
  background: none;
  color: inherit;
  font-family: inherit;
  text-align: left;
  cursor: pointer;
}

.record-row-edit:disabled {
  color: inherit;
  cursor: default;
}

.record-row-edit:focus-visible {
  outline: 2px solid var(--mud-palette-primary);
  outline-offset: -2px;
}

.record-row-text {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-width: 0;
  gap: 2px;
}

.record-row-line {
  overflow: hidden;
  max-width: 100%;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.record-row-title {
  font-size: 0.9375rem;
  line-height: 1.33;
}

.record-row-meta {
  font-size: 0.78125rem;
  line-height: 1.45;
  color: var(--mud-palette-text-secondary);
}

.record-row-meta :deep(.mud-icon-root) {
  vertical-align: text-bottom;
}

.record-row-sum {
  flex: 0 0 auto;
  padding-top: 1px;
  font-size: 0.9375rem;
  line-height: 1.33;
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum';
}

.record-row-sum--income {
  color: var(--mud-palette-success);
}

.mud-icon-button.record-row-restore,
.mud-icon-button.record-row-restore :deep(.mud-icon-root) {
  color: var(--mud-palette-info);
}

.record-row--deleted .record-row-edit {
  opacity: 0.55;
  filter: grayscale(0.4);
}

.record-row--deleted .record-row-title,
.record-row--deleted .record-row-sum {
  text-decoration: line-through;
  text-decoration-thickness: 1px;
  text-decoration-color: var(--mud-palette-error);
}
</style>
