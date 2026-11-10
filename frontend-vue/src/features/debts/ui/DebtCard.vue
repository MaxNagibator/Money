<script setup lang="ts">
import { computed, onScopeDispose, ref, watch } from 'vue'
import type { DebtMovement } from '@/domain/debt'
import { formatLocalDate, formatMoney } from '@/platform/format'
import {
  mdDelete,
  mdInput,
  mdModeEdit,
  mdOutput,
  mdPayment,
  mdRestore,
} from '@/shared/icons'
import {
  MudCard,
  MudCardActions,
  MudCardContent,
  MudCheckBox,
  MudChip,
  MudCollapse,
  MudDivider,
  MudIconButton,
  MudPopover,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import { useTransitionsAfterMount } from '@/shared/ui/useTransitionsAfterMount'
import type { PaymentFormValue } from '../model/payment-form'
import DebtPaymentForm from './DebtPaymentForm.vue'

const COLLAPSE_DELAY_MS = 1000

const props = defineProps<{
  movement: DebtMovement
  ledger?: boolean
  isSelectable: boolean
  isSelected: boolean
  isPaymentOpen: boolean
  paymentValue: PaymentFormValue | null
  isPaying: boolean
}>()

const emit = defineEmits<{
  'toggle-selection': [movement: DebtMovement]
  'edit': [movement: DebtMovement]
  'remove': [movement: DebtMovement]
  'restore': [movement: DebtMovement]
  'open-payment': [movement: DebtMovement]
  'cancel-payment': [movement: DebtMovement]
  'submit-payment': [value: PaymentFormValue]
}>()

const isExpanded = ref(false)
const isSummaryExpanded = ref(true)
const areTransitionsEnabled = useTransitionsAfterMount()
const isNarrow = useNarrowViewport()

let collapseTimer: ReturnType<typeof setTimeout> | null = null

const direction = computed(() => props.movement.typeId === 1 ? 1 : -1)
const movementSum = computed(() => props.movement.sum.multiply(direction.value))
const remainder = computed(() => movementSum.value)
const comment = computed(() => {
  const value = props.movement.comment?.trim() ?? ''

  return value.length === 0 ? null : value
})
const toneClass = computed(() => {
  if (props.movement.isDeleted) {
    return ['debt-card--deleted', 'record-card', 'record-card--muted']
  }

  return []
})
const actionSuffix = computed(() => `${props.movement.ownerName} на ${formatMoney(movementSum.value)}`)

watch(() => props.isPaymentOpen, isOpen => {
  if (isOpen) {
    stopCollapse()
  }
})

onScopeDispose(stopCollapse)

function expand(): void {
  isExpanded.value = true
}

function stopCollapse(): void {
  if (collapseTimer === null) {
    return
  }

  clearTimeout(collapseTimer)
  collapseTimer = null
}

function scheduleCollapse(): void {
  stopCollapse()

  if (!isExpanded.value || props.isPaymentOpen) {
    return
  }

  collapseTimer = setTimeout(() => {
    collapseTimer = null
    isExpanded.value = false
  }, COLLAPSE_DELAY_MS)
}

function onFocusOut(event: FocusEvent): void {
  const card = event.currentTarget
  const next = event.relatedTarget

  if (!(next instanceof Node)) {
    return
  }

  if (card instanceof HTMLElement && card.contains(next)) {
    return
  }

  scheduleCollapse()
}

function togglePayment(): void {
  if (props.isPaymentOpen) {
    emit('cancel-payment', props.movement)
    return
  }

  emit('open-payment', props.movement)
}
</script>

<template>
  <div
    v-if="isNarrow || ledger"
    :class="[
      { 'debt-row--deleted': movement.isDeleted },
      `debt-row--type-${movement.typeId}`,
    ]"
    :data-testid="`debts-card-${movement.id}`"
    class="debt-row"
  >
    <MudCheckBox
      v-if="isSelectable"
      :data-testid="`debts-card-select-${movement.id}`"
      :model-value="isSelected"
      aria-label="Выбрать долг для прощения"
      color="tertiary"
      size="small"
      @click.stop="emit('toggle-selection', movement)"
    />

    <button
      :aria-label="`Изменить долг ${actionSuffix}`"
      :data-testid="`debts-card-edit-${movement.id}`"
      :disabled="movement.isDeleted"
      class="debt-row__main"
      type="button"
      @click="emit('edit', movement)"
    >
      <span class="debt-row__when">
        <span :data-testid="`debts-card-date-${movement.id}`">{{ formatLocalDate(movement.date) }}</span>
        <template v-if="comment !== null">
          <span>&nbsp;·&nbsp;</span>
          <span :data-testid="`debts-card-comment-${movement.id}`">{{ comment }}</span>
        </template>
      </span>

      <span
        :data-testid="`debts-card-remainder-${movement.id}`"
        class="debt-row__sum"
      >{{ formatMoney(movement.sum) }}</span>
    </button>

    <MudIconButton
      v-if="movement.isDeleted"
      :data-testid="`debts-card-restore-${movement.id}`"
      :icon="mdRestore"
      aria-label="Восстановить"
      color="success"
      size="small"
      title="Восстановить"
      @click.stop="emit('restore', movement)"
    />
  </div>

  <MudCard
    v-else
    :aria-expanded="isExpanded"
    :class="toneClass"
    :data-testid="`debts-card-${movement.id}`"
    :role="isExpanded || isSelectable ? undefined : 'button'"
    :tabindex="isExpanded || isSelectable ? undefined : 0"
    class="debt-card"
    outlined
    @click="expand"
    @focusin="stopCollapse"
    @focusout="onFocusOut"
    @keydown.enter.self="expand"
    @keydown.space.self.prevent="expand"
    @mouseenter="stopCollapse"
    @mouseleave="scheduleCollapse"
  >
    <MudCardContent class="px-3 py-2">
      <MudCollapse
        :class="{ 'debt-card__summary-static': !areTransitionsEnabled }"
        :expanded="isSummaryExpanded"
      >
        <MudStack
          align-items="center"
          row
          :spacing="1"
        >
          <MudCheckBox
            v-if="isSelectable"
            :data-testid="`debts-card-select-${movement.id}`"
            :model-value="isSelected"
            aria-label="Выбрать долг для прощения"
            color="tertiary"
            size="small"
            @click.stop="emit('toggle-selection', movement)"
          />

          <span class="debt-card__field-label">Движение</span>

          <MudText
            html-tag="span"
            typo="body1"
          >
            {{ formatMoney(movementSum) }}
          </MudText>

          <MudSpacer />

          <span class="debt-card__field-label">Дата</span>

          <MudText
            color="info"
            html-tag="span"
            typo="body2"
          >
            {{ formatLocalDate(movement.date) }}
          </MudText>
        </MudStack>

        <MudStack
          align-items="center"
          row
          :spacing="1"
        >
          <span class="debt-card__field-label">Влияние</span>

          <MudText
            :data-testid="`debts-card-remainder-${movement.id}`"
            color="secondary"
            html-tag="span"
            typo="body1"
          >
            {{ formatMoney(remainder) }}
          </MudText>
        </MudStack>
      </MudCollapse>

      <MudCollapse
        :expanded="isExpanded"
        @animation-end="isSummaryExpanded = !isExpanded"
      >
        <MudStack :spacing="2">
          <MudText
            color="info"
            html-tag="div"
            typo="body2"
          >
            Дата: {{ formatLocalDate(movement.date) }}
          </MudText>

          <MudText
            v-if="comment !== null"
            :data-testid="`debts-card-comment-${movement.id}`"
            html-tag="div"
            typo="body2"
          >
            Комментарий: {{ comment }}
          </MudText>

          <MudDivider class="mt-1 mb-1" />

          <MudText
            html-tag="div"
            typo="subtitle2"
          >
            Движение: {{ formatMoney(movementSum) }}
          </MudText>

          <MudText
            color="secondary"
            html-tag="div"
            typo="subtitle2"
          >
            Влияние на баланс: {{ formatMoney(remainder) }}
          </MudText>
        </MudStack>
      </MudCollapse>
    </MudCardContent>

    <template v-if="isExpanded">
      <MudDivider />

      <MudCardActions class="px-1 py-2">
        <MudIconButton
          :data-testid="`debts-card-pay-${movement.id}`"
          :disabled="movement.isDeleted"
          :icon="mdPayment"
          aria-label="Добавить возврат"
          color="primary"
          size="small"
          title="Добавить возврат"
          @click.stop="togglePayment"
        />

        <MudPopover
          :open="isPaymentOpen"
          anchor-origin="bottom-left"
          transform-origin="top-left"
        >
          <DebtPaymentForm
            :is-saving="isPaying"
            :value="paymentValue"
            @cancel="emit('cancel-payment', movement)"
            @submit="emit('submit-payment', $event)"
          />
        </MudPopover>

        <MudSpacer />

        <MudChip
          :color="movement.typeId === 1 ? 'success' : 'warning'"
          :data-testid="`debts-card-status-${movement.id}`"
          :icon="movement.typeId === 1 ? mdInput : mdOutput"
          :text="movement.typeId === 1 ? 'Увеличение' : 'Уменьшение'"
          size="small"
          title="Направление движения"
          variant="filled"
        />

        <MudSpacer />

        <template v-if="movement.isDeleted">
          <MudIconButton
            :data-testid="`debts-card-restore-${movement.id}`"
            :icon="mdRestore"
            aria-label="Восстановить"
            color="success"
            size="small"
            title="Восстановить"
            @click.stop="emit('restore', movement)"
          />
        </template>

        <template v-else>
          <MudIconButton
            :data-testid="`debts-card-edit-${movement.id}`"
            :icon="mdModeEdit"
            aria-label="Редактировать"
            color="warning"
            size="small"
            title="Редактировать"
            @click.stop="emit('edit', movement)"
          />

          <MudIconButton
            :data-testid="`debts-card-delete-${movement.id}`"
            :icon="mdDelete"
            aria-label="Удалить"
            size="small"
            title="Удалить"
            @click.stop="emit('remove', movement)"
          />
        </template>
      </MudCardActions>
    </template>
  </MudCard>
</template>

<style scoped>
.debt-card {
  height: 100%;
  box-shadow: 0 2px 4px rgb(0 0 0 / 10%);
  transition: transform 0.2s;
}

.debt-card__summary-static {
  transition: none;
}

.debt-card[role='button'] {
  cursor: pointer;
}

.debt-card__field-label {
  display: none;
}

.debt-row {
  display: flex;
  align-items: center;
  border-top: 1px solid var(--mud-palette-lines-default);
}

.debt-row__main {
  display: flex;
  align-items: baseline;
  flex: 1 1 auto;
  gap: 10px;
  min-width: 0;
  min-height: 44px;
  padding: 7px 12px 7px 16px;
  border: 0;
  background: none;
  color: inherit;
  font-family: inherit;
  font-size: 0.8125rem;
  text-align: left;
}

.debt-row__main:disabled {
  color: inherit;
}

.debt-row__when {
  flex: 1 1 auto;
  min-width: 0;
  overflow: hidden;
  color: var(--mud-palette-text-secondary);
  text-overflow: ellipsis;
  white-space: nowrap;
}

.debt-row__sum {
  flex: 0 0 auto;
  min-width: var(--money-col-width, 9ch);
  text-align: right;
  font-weight: 500;
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum';
}

.debt-row--paid .debt-row__sum {
  color: var(--mud-palette-text-disabled);
}

.debt-row--deleted {
  opacity: 0.55;
  filter: grayscale(0.4);
}

.debt-row--deleted .debt-row__when,
.debt-row--deleted .debt-row__sum {
  text-decoration: line-through;
  text-decoration-thickness: 1px;
  text-decoration-color: var(--mud-palette-error);
}

.debt-card__history-comment {
  max-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>

<style scoped src="@/shared/ui/record-card.css"></style>
