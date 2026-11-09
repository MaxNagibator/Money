<script setup lang="ts">
import { computed, onScopeDispose, ref, watch } from 'vue'
import type { Debt } from '@/domain/debt'
import { formatLocalDate, formatMoney } from '@/platform/format'
import {
  mdCheckCircle,
  mdDelete,
  mdModeEdit,
  mdPayment,
  mdPending,
  mdRestore,
  mdWarningAmber,
} from '@/shared/icons'
import type { MudColor } from '@/shared/mud'
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
  MudProgressLinear,
  MudSimpleTable,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'
import { roleVariables } from '@/shared/ui/record-card'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import { useTransitionsAfterMount } from '@/shared/ui/useTransitionsAfterMount'
import type { DebtStatus } from '../model/debt'
import { DEBT_STATUS_TEXT, debtStatus, hasPaymentHistory, parsePaymentHistory, paymentProgress } from '../model/debt'
import type { PaymentFormValue } from '../model/payment-form'
import DebtPaymentForm from './DebtPaymentForm.vue'

const COLLAPSE_DELAY_MS = 1000

const STATUS_ICONS: Record<DebtStatus, string> = {
  paid: mdCheckCircle,
  inProgress: mdPending,
  notStarted: mdWarningAmber,
}

const STATUS_COLORS: Record<DebtStatus, MudColor> = {
  paid: 'success',
  inProgress: 'info',
  notStarted: 'secondary',
}

const props = defineProps<{
  debt: Debt
  isSelectable: boolean
  isSelected: boolean
  isPaymentOpen: boolean
  paymentValue: PaymentFormValue | null
  isPaying: boolean
}>()

const emit = defineEmits<{
  'toggle-selection': [debt: Debt]
  'edit': [debt: Debt]
  'remove': [debt: Debt]
  'restore': [debt: Debt]
  'open-payment': [debt: Debt]
  'cancel-payment': [debt: Debt]
  'submit-payment': [value: PaymentFormValue]
}>()

const isExpanded = ref(false)
const isSummaryExpanded = ref(true)
const areTransitionsEnabled = useTransitionsAfterMount()
const isNarrow = useNarrowViewport()

let collapseTimer: ReturnType<typeof setTimeout> | null = null

const remainder = computed(() => props.debt.sum.subtract(props.debt.paySum))
const comment = computed(() => {
  const value = props.debt.comment?.trim() ?? ''

  return value.length === 0 ? null : value
})
const progress = computed(() => paymentProgress(props.debt))
const status = computed(() => debtStatus(props.debt))
const isPaid = computed(() => status.value === 'paid' && !props.debt.isDeleted)
const toneClass = computed(() => {
  if (props.debt.isDeleted) {
    return ['debt-card--deleted', 'record-card', 'record-card--muted']
  }

  return isPaid.value ? ['debt-card--paid', 'record-card', 'record-card--faded'] : []
})
const toneStyle = computed(() => isPaid.value ? roleVariables(STATUS_COLORS.paid) : null)
const payments = computed(() => hasPaymentHistory(props.debt) ? parsePaymentHistory(props.debt.payComment) : [])
const actionSuffix = computed(() => `${props.debt.ownerName} на ${formatMoney(props.debt.sum)}`)

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
    emit('cancel-payment', props.debt)
    return
  }

  emit('open-payment', props.debt)
}
</script>

<template>
  <div
    v-if="isNarrow"
    :class="{ 'debt-row--deleted': debt.isDeleted, 'debt-row--paid': isPaid }"
    :data-testid="`debts-card-${debt.id}`"
    class="debt-row"
  >
    <MudCheckBox
      v-if="isSelectable"
      :data-testid="`debts-card-select-${debt.id}`"
      :model-value="isSelected"
      aria-label="Выбрать долг для прощения"
      color="tertiary"
      size="small"
      @click.stop="emit('toggle-selection', debt)"
    />

    <button
      :aria-label="`Изменить долг ${actionSuffix}`"
      :data-testid="`debts-card-edit-${debt.id}`"
      :disabled="debt.isDeleted"
      class="debt-row__main"
      type="button"
      @click="emit('edit', debt)"
    >
      <span class="debt-row__when">
        <span :data-testid="`debts-card-date-${debt.id}`">{{ formatLocalDate(debt.date) }}</span>
        <template v-if="comment !== null">
          <span>&nbsp;·&nbsp;</span>
          <span :data-testid="`debts-card-comment-${debt.id}`">{{ comment }}</span>
        </template>
      </span>

      <span
        :data-testid="`debts-card-remainder-${debt.id}`"
        class="debt-row__sum"
      >{{ formatMoney(remainder) }}</span>
    </button>

    <MudIconButton
      v-if="debt.isDeleted"
      :data-testid="`debts-card-restore-${debt.id}`"
      :icon="mdRestore"
      aria-label="Восстановить"
      color="success"
      size="small"
      title="Восстановить"
      @click.stop="emit('restore', debt)"
    />
  </div>

  <MudCard
    v-else
    :aria-expanded="isExpanded"
    :class="toneClass"
    :data-testid="`debts-card-${debt.id}`"
    :role="isExpanded || isSelectable ? undefined : 'button'"
    :style="toneStyle"
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
            :data-testid="`debts-card-select-${debt.id}`"
            :model-value="isSelected"
            aria-label="Выбрать долг для прощения"
            color="tertiary"
            size="small"
            @click.stop="emit('toggle-selection', debt)"
          />

          <span class="debt-card__field-label">Сумма</span>

          <MudText
            html-tag="span"
            typo="body1"
          >
            {{ formatMoney(debt.sum) }}
          </MudText>

          <MudSpacer />

          <span class="debt-card__field-label">Дата</span>

          <MudText
            color="info"
            html-tag="span"
            typo="body2"
          >
            {{ formatLocalDate(debt.date) }}
          </MudText>
        </MudStack>

        <MudStack
          align-items="center"
          row
          :spacing="1"
        >
          <span class="debt-card__field-label">Остаток</span>

          <MudText
            :data-testid="`debts-card-remainder-${debt.id}`"
            color="secondary"
            html-tag="span"
            typo="body1"
          >
            {{ formatMoney(remainder) }}
          </MudText>

          <MudSpacer />

          <span class="debt-card__field-label">Выплачено</span>

          <MudText
            color="tertiary"
            html-tag="span"
            typo="body2"
          >
            {{ formatMoney(debt.paySum) }}
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
            Дата: {{ formatLocalDate(debt.date) }}
          </MudText>

          <MudText
            v-if="comment !== null"
            :data-testid="`debts-card-comment-${debt.id}`"
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
            Сумма: {{ formatMoney(debt.sum) }}
          </MudText>

          <MudText
            color="secondary"
            html-tag="div"
            typo="subtitle2"
          >
            Остаток: {{ formatMoney(remainder) }}
          </MudText>

          <MudProgressLinear
            :aria-label="`Погашено ${progress}%`"
            :buffer-value="100"
            :value="progress"
            class="my-2"
            color="secondary"
          />

          <template v-if="payments.length > 0">
            <MudText
              color="tertiary"
              html-tag="div"
              typo="subtitle2"
            >
              История погашений:
            </MudText>

            <MudSimpleTable
              :data-testid="`debts-card-history-${debt.id}`"
              class="mx-n3"
              dense
              hover
            >
              <tbody>
                <tr
                  v-for="(payment, index) in payments"
                  :key="index"
                >
                  <td>{{ payment.date === null ? '' : formatLocalDate(payment.date) }}</td>
                  <td>{{ formatMoney(payment.sum) }}</td>
                  <td class="debt-card__history-comment">
                    {{ payment.comment }}
                  </td>
                </tr>
              </tbody>
            </MudSimpleTable>
          </template>
        </MudStack>
      </MudCollapse>
    </MudCardContent>

    <template v-if="isExpanded">
      <MudDivider />

      <MudCardActions class="px-1 py-2">
        <MudIconButton
          :data-testid="`debts-card-pay-${debt.id}`"
          :disabled="debt.isDeleted"
          :icon="mdPayment"
          aria-label="Оплатить"
          color="primary"
          size="small"
          title="Оплатить"
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
            @cancel="emit('cancel-payment', debt)"
            @submit="emit('submit-payment', $event)"
          />
        </MudPopover>

        <MudSpacer />

        <MudChip
          :color="STATUS_COLORS[status]"
          :data-testid="`debts-card-status-${debt.id}`"
          :icon="STATUS_ICONS[status]"
          :text="DEBT_STATUS_TEXT[status]"
          size="small"
          title="Статус"
          variant="filled"
        />

        <MudSpacer />

        <template v-if="debt.isDeleted">
          <MudIconButton
            :data-testid="`debts-card-restore-${debt.id}`"
            :icon="mdRestore"
            aria-label="Восстановить"
            color="success"
            size="small"
            title="Восстановить"
            @click.stop="emit('restore', debt)"
          />
        </template>

        <template v-else>
          <MudIconButton
            :data-testid="`debts-card-edit-${debt.id}`"
            :icon="mdModeEdit"
            aria-label="Редактировать"
            color="warning"
            size="small"
            title="Редактировать"
            @click.stop="emit('edit', debt)"
          />

          <MudIconButton
            :data-testid="`debts-card-delete-${debt.id}`"
            :icon="mdDelete"
            aria-label="Удалить"
            size="small"
            title="Удалить"
            @click.stop="emit('remove', debt)"
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
