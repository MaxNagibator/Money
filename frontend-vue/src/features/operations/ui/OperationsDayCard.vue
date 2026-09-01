<script setup lang="ts">
import { computed } from 'vue'
import type { FastOperation } from '@/api/adapters/fast-operations'
import type { Category } from '@/domain/category'
import type { Operation, OperationsDay } from '@/domain/operation'
import { calculateDayTotal } from '@/domain/operation'
import { OPERATION_TYPES } from '@/domain/operation-type'
import {
  alternateRowClass,
  moneyColumnStyle,
  operationsListClasses,
  shouldShowDivider,
  useDisplaySettings,
} from '@/features/settings'
import { formatLocalDate, formatMoney } from '@/platform/format'
import { mdAdd, mdCalendarToday } from '@/shared/icons'
import type { MudColor } from '@/shared/mud'
import {
  MudCard,
  MudCardContent,
  MudCardHeader,
  MudDivider,
  MudIcon,
  MudIconButton,
  MudItem,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'
import { fastOperationForm, newOperationForm } from '../model/operation-form'
import type { OperationFormValue } from '../model/operation-form'
import OperationDialog from './OperationDialog.vue'
import OperationItem from './OperationItem.vue'
import QuickOperationMenu from './QuickOperationMenu.vue'

const props = defineProps<{
  day: OperationsDay
  fastOperations: readonly FastOperation[]
  categories: readonly Category[]
  isSaving: boolean
}>()

const emit = defineEmits<{
  submit: [value: OperationFormValue]
  notice: [text: string]
  remove: [operation: Operation]
  restore: [operation: Operation]
}>()

function quickForm(operation: FastOperation): OperationFormValue {
  const category = props.categories.find(item => item.id === operation.categoryId) ?? null

  return fastOperationForm(operation, category, props.day.date)
}

const { settings } = useDisplaySettings()

const dateText = computed(() => formatLocalDate(props.day.date))

const totals = computed(() => OPERATION_TYPES.map(type => ({
  type,
  color: type.color as MudColor,
  amount: formatMoney(calculateDayTotal(props.day, type.id)),
})))

const rowsClasses = computed(() => operationsListClasses(settings))

const rowsStyle = computed(() => moneyColumnStyle(settings))
</script>

<template>
  <MudCard
    :data-date="day.date"
    class="mt-2"
    data-testid="operations-day"
    :elevation="3"
  >
    <MudCardHeader
      class="border-b-2 border-solid pa-2"
      style="border-color: var(--mud-palette-divider)"
    >
      <MudItem :xs="12">
        <MudStack
          align-items="center"
          row
          :spacing="3"
          wrap="wrap"
        >
          <MudStack
            justify="start"
            row
            stretch-items="end"
          >
            <MudIcon
              :icon="mdCalendarToday"
              size="small"
            />
            <MudText data-testid="operations-day-date">
              {{ dateText }}
            </MudText>
          </MudStack>

          <OperationDialog
            :categories="categories"
            :is-saving="isSaving"
            @notice="emit('notice', $event)"
            @submit="emit('submit', $event)"
          >
            <template #default="{ isOpen, open }">
              <div class="align-center d-flex flex-row gap-4">
                <MudIconButton
                  v-for="{ type, color } in totals"
                  :key="type.id"
                  :aria-label="`${type.addLabel} за ${dateText}`"
                  :color="color"
                  :data-testid="`operations-day-add-${type.id}`"
                  :disabled="isOpen"
                  :icon="mdAdd"
                  :title="`${type.addLabel} за ${dateText}`"
                  size="small"
                  @click="open(newOperationForm(type.id, day.date))"
                />

                <QuickOperationMenu
                  :action-label="`Быстрое добавление за ${dateText}`"
                  :disabled="isOpen"
                  :operations="fastOperations"
                  :test-id="`operations-day-quick-${day.date}`"
                  color="info"
                  @select="open(quickForm($event))"
                />
              </div>
            </template>
          </OperationDialog>

          <MudSpacer />

          <div
            v-for="{ type, amount, color } in totals"
            :key="type.id"
            class="align-center d-flex flex-row gap-2"
          >
            <MudIcon
              :color="color"
              :icon="type.icon"
              :title="type.name"
              size="small"
            />
            <MudText :data-testid="`operations-day-total-${type.id}`">
              {{ amount }}
            </MudText>
          </div>
        </MudStack>
      </MudItem>
    </MudCardHeader>

    <MudCardContent
      :class="[rowsClasses, 'pa-2']"
      :style="rowsStyle"
    >
      <MudStack :spacing="0">
        <template
          v-for="(operation, index) in day.operations"
          :key="operation.id"
        >
          <div
            :class="alternateRowClass(settings.alternateRowStyle, index)"
            class="operation-row"
          >
            <OperationItem
              :categories="categories"
              :is-saving="isSaving"
              :operation="operation"
              @notice="emit('notice', $event)"
              @remove="emit('remove', $event)"
              @restore="emit('restore', $event)"
              @submit="emit('submit', $event)"
            />
          </div>

          <MudDivider
            v-if="shouldShowDivider(settings, index, day.operations.length)"
            flex-item
          />
        </template>
      </MudStack>
    </MudCardContent>
  </MudCard>
</template>

<style scoped>
.operation-row {
  padding: 2px 4px;
  transition: background-color 0.15s ease-in-out;
  border-radius: 4px;
}

.operation-row :deep(.mud-grid-item) {
  min-width: 0;
}

.operations-alt-bg .operation-row-alt {
  background-color: var(--mud-palette-action-default-hover);
}

.operations-alt-border .operation-row {
  border-left: 3px solid transparent;
  border-radius: 0 4px 4px 0;
}

.operations-alt-border .operation-row-alt {
  border-left-color: var(--mud-palette-primary);
}

.operations-money-align :deep(.operation-sum) {
  display: inline-block;
  min-width: var(--money-col-width, 9ch);
  padding-right: 8px;
  text-align: right;
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum';
}
</style>
