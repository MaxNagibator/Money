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
import { formatLocalDate, formatLocalDateWithWeekday, formatMoney } from '@/platform/format'
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
  MudMenuItem,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
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

const isNarrow = useNarrowViewport()

const dateText = computed(() => (isNarrow.value ? formatLocalDateWithWeekday(props.day.date) : formatLocalDate(props.day.date)))

const totals = computed(() => OPERATION_TYPES.map(type => {
  const total = calculateDayTotal(props.day, type.id)

  return {
    type,
    color: type.color as MudColor,
    amount: formatMoney(total),
    isZero: total.isZero,
  }
}))

const visibleTotals = computed(() => (isNarrow.value ? totals.value.filter(total => !total.isZero) : totals.value))

const addLabel = computed(() => (isNarrow.value ? `Добавить за ${dateText.value}` : `Быстрое добавление за ${dateText.value}`))

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
      :class="isNarrow ? 'pa-1' : 'pa-2'"
      class="border-b-2 border-solid day-header"
      style="border-color: var(--mud-palette-divider)"
    >
      <MudItem :xs="12">
        <MudStack
          align-items="center"
          class="day-header-row"
          row
          :spacing="3"
          wrap="wrap"
        >
          <MudStack
            class="day-date"
            justify="start"
            row
            stretch-items="end"
          >
            <MudIcon
              v-if="!isNarrow"
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
              <div class="align-center d-flex day-actions flex-row gap-4">
                <template v-if="!isNarrow">
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
                </template>

                <QuickOperationMenu
                  :action-label="addLabel"
                  :color="isNarrow ? 'default' : 'info'"
                  :disabled="isOpen"
                  :operations="fastOperations"
                  :test-id="`operations-day-quick-${day.date}`"
                  @select="open(quickForm($event))"
                >
                  <template
                    v-if="isNarrow"
                    #items
                  >
                    <MudMenuItem
                      v-for="{ type, color } in totals"
                      :key="type.id"
                      :data-testid="`operations-day-add-${type.id}`"
                      @click="open(newOperationForm(type.id, day.date))"
                    >
                      <span class="align-center d-flex flex-row gap-2">
                        <MudIcon
                          :color="color"
                          :icon="type.icon"
                          size="small"
                        />
                        {{ type.shortAddLabel }}
                      </span>
                    </MudMenuItem>
                  </template>
                </QuickOperationMenu>
              </div>
            </template>
          </OperationDialog>

          <MudSpacer class="day-spacer" />

          <div class="day-totals">
            <div
              v-for="{ type, amount, color } in visibleTotals"
              :key="type.id"
              class="align-center d-flex flex-row gap-2 day-total"
            >
              <MudIcon
                v-if="!isNarrow"
                :color="color"
                :icon="type.icon"
                :title="type.name"
                size="small"
              />
              <MudText
                v-else
                :color="color"
                :title="type.name"
                class="day-total-text"
              >
                {{ type.sign }}
              </MudText>
              <MudText
                :data-testid="`operations-day-total-${type.id}`"
                class="day-total-text"
              >
                {{ amount }}
              </MudText>
            </div>
          </div>
        </MudStack>
      </MudItem>
    </MudCardHeader>

    <MudCardContent
      :class="[rowsClasses, isNarrow ? 'pa-0' : 'pa-2']"
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
.day-totals {
  display: contents;
}

@media (max-width: 599.98px) {
  .day-header {
    border-bottom-width: 1px !important;
    background-color: var(--mud-palette-background-grey);
  }

  .day-header-row {
    min-height: 44px;
    gap: 8px !important;
  }

  .day-date {
    flex: 0 0 auto;
  }

  .day-date :deep([data-testid="operations-day-date"]) {
    font-size: 0.8125rem;
    font-weight: 500;
    letter-spacing: 0.03em;
    text-transform: uppercase;
    color: var(--mud-palette-text-secondary);
  }

  .day-spacer {
    display: none;
  }

  .day-totals {
    display: flex;
    flex: 1 1 0;
    flex-wrap: wrap;
    justify-content: flex-end;
    padding-right: 8px;
    gap: 0 8px;
  }

  .day-total {
    gap: 2px !important;
  }

  .day-total-text {
    font-size: 0.9375rem;
    font-weight: 500;
    white-space: nowrap;
    font-variant-numeric: tabular-nums;
  }

  .day-header-row :deep(.mud-menu-icon-button-activator) {
    width: 44px;
    height: 44px;
  }

  .operation-row {
    padding: 0;
  }
}

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
