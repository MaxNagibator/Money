<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { UNNAMED_DEBT_OWNER } from '@/api/adapters/debts'
import type { Debt } from '@/domain/debt'
import { formatLocalDate, formatMoney, formatMoneyWhole } from '@/platform/format'
import {
  mdArrowCircleUp,
  mdAttachMoney,
  mdCalendarToday,
  mdListAlt,
} from '@/shared/icons'
import {
  MudChip,
  MudGrid,
  MudItem,
  MudPaper,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'
import { pinLeavingItem, unpinItem } from '@/shared/ui/list-flow'
import { roleVariables } from '@/shared/ui/record-card'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { DebtOwnerGroup } from '../model/debt'
import {
  debtCountText,
  debtRoleColor,
  lastPaymentDate,
  NO_PAYMENT_MARK,
  nextPayableDebt,
  ownerPercent,
  ownerRemainder,
  ownerTotal,
  paidPercentText,
} from '../model/debt'
import type { PaymentFormValue } from '../model/payment-form'
import DebtCard from './DebtCard.vue'
import DebtPaymentForm from './DebtPaymentForm.vue'

const FULL_PROGRESS = 100

const props = defineProps<{
  owner: DebtOwnerGroup
  isSelectable: (debt: Debt) => boolean
  selectedIds: ReadonlySet<number>
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

const COLLAPSE_HINT = 'сначала завершите или отмените платёж'

const isNarrow = useNarrowViewport()
const isExpanded = ref(false)
const isCollapseBlocked = ref(false)

const title = computed(() => props.owner.name.trim().length === 0 ? UNNAMED_DEBT_OWNER : props.owner.name)
const total = computed(() => ownerTotal(props.owner))
const remainder = computed(() => ownerRemainder(props.owner))
const percent = computed(() => ownerPercent(props.owner))
const lastPayment = computed(() => lastPaymentDate(props.owner))

const roleStyle = computed(() => roleVariables(debtRoleColor(props.owner.debts[0]?.typeId ?? 0)))
const countText = computed(() => debtCountText(props.owner.debts.length))
const percentText = computed(() => paidPercentText(percent.value))
const progressWidth = computed(() => `${Math.min(Math.max(percent.value, 0), FULL_PROGRESS)}%`)
const payable = computed(() => nextPayableDebt(props.owner))
const payableDate = computed(() => payable.value === null ? null : formatLocalDate(payable.value.date))
const openPayment = computed(() => {
  const value = props.paymentValue

  if (value === null) {
    return null
  }

  return props.owner.debts.some(debt => debt.id === value.debtId) ? value : null
})
const isForgiving = computed(() => props.owner.debts.some(debt => props.isSelectable(debt)))
const collapseHint = computed(() => isCollapseBlocked.value ? COLLAPSE_HINT : '')

watch(isForgiving, isOpen => {
  if (isOpen) {
    isExpanded.value = true
  }
}, { immediate: true })

watch(openPayment, value => {
  if (value === null) {
    isCollapseBlocked.value = false
  }
})

function toggle(): void {
  if (isExpanded.value && openPayment.value !== null) {
    isCollapseBlocked.value = true
    return
  }

  isCollapseBlocked.value = false
  isExpanded.value = !isExpanded.value
}

function pay(): void {
  if (payable.value !== null) {
    emit('open-payment', payable.value)
  }
}

function cancelPayment(): void {
  const value = openPayment.value

  if (value === null) {
    return
  }

  const debt = props.owner.debts.find(candidate => candidate.id === value.debtId)

  if (debt !== undefined) {
    emit('cancel-payment', debt)
  }
}
</script>

<template>
  <div
    v-if="isNarrow"
    :data-testid="`debts-owner-${owner.name}`"
    :style="roleStyle"
    class="debt-book list-flow-host"
  >
    <button
      :aria-expanded="isExpanded"
      :aria-label="`${isExpanded ? 'Свернуть' : 'Развернуть'} долги: ${title}`"
      :data-testid="`debts-owner-toggle-${owner.name}`"
      class="debt-book__head"
      type="button"
      @click="toggle"
    >
      <span class="debt-book__who">{{ title }}</span>

      <span class="debt-book__money">
        <b
          class="debt-book__remainder"
          data-testid="debts-owner-remainder"
        >{{ formatMoney(remainder) }}</b>
        <span
          class="debt-book__of"
          data-testid="debts-owner-total"
        >из {{ formatMoney(total) }}</span>
      </span>
    </button>

    <div
      class="debt-book__meta"
      data-testid="debts-owner-meta"
    >
      <span>{{ countText }}</span>
      <template v-if="payableDate !== null">
        <span class="debt-book__dot">·</span>
        <span>{{ payableDate }}</span>
      </template>
      <template v-if="!isExpanded">
        <span class="debt-book__dot">·</span>
        <span>{{ percentText }}</span>
      </template>

      <span
        class="debt-book__hint"
        data-testid="debts-owner-collapse-hint"
        role="status"
      >{{ collapseHint }}</span>
    </div>

    <div
      :aria-label="`Погашено ${percent} %`"
      :aria-valuenow="percent"
      aria-valuemax="100"
      aria-valuemin="0"
      class="debt-book__bar"
      data-testid="debts-owner-progress"
      role="progressbar"
    >
      <i :style="{ width: progressWidth }" />
    </div>

    <template v-if="isExpanded">
      <TransitionGroup
        name="list-flow"
        @before-enter="unpinItem"
        @before-leave="pinLeavingItem"
      >
        <DebtCard
          v-for="debt in owner.debts"
          :key="debt.id"
          :debt="debt"
          :is-paying="isPaying"
          :is-payment-open="paymentValue !== null && paymentValue.debtId === debt.id"
          :is-selectable="isSelectable(debt)"
          :is-selected="selectedIds.has(debt.id)"
          :payment-value="paymentValue"
          @cancel-payment="emit('cancel-payment', $event)"
          @edit="emit('edit', $event)"
          @open-payment="emit('open-payment', $event)"
          @remove="emit('remove', $event)"
          @restore="emit('restore', $event)"
          @submit-payment="emit('submit-payment', $event)"
          @toggle-selection="emit('toggle-selection', $event)"
        />
      </TransitionGroup>

      <div
        v-if="owner.debts.length > 1"
        class="debt-book__sum"
      >
        <span class="debt-book__sum-text">Остаток по держателю</span>
        <span
          class="debt-book__sum-value"
          data-testid="debts-owner-sum"
        >{{ formatMoney(remainder) }}</span>
      </div>

      <div class="debt-book__foot">
        <DebtPaymentForm
          v-if="openPayment !== null"
          :is-saving="isPaying"
          :value="openPayment"
          @cancel="cancelPayment"
          @submit="emit('submit-payment', $event)"
        />

        <button
          v-else
          :disabled="payable === null"
          class="debt-book__pay"
          data-testid="debts-owner-pay"
          type="button"
          @click="pay"
        >
          Оплатить
        </button>
      </div>
    </template>
  </div>

  <div
    v-else
    :data-testid="`debts-owner-${owner.name}`"
  >
    <MudPaper
      class="pa-2"
      :elevation="1"
    >
      <MudStack
        align-items="center"
        row
        :spacing="1"
        wrap="wrap"
      >
        <MudText
          class="debt-owner__title"
          html-tag="h3"
          typo="h6"
        >
          {{ title }}
        </MudText>

        <MudSpacer />

        <MudChip
          :icon="mdListAlt"
          :text="String(owner.debts.length)"
          data-testid="debts-owner-count"
          size="small"
          title="Количество долгов"
          variant="filled"
        />

        <MudChip
          :icon="mdCalendarToday"
          :text="lastPayment === null ? NO_PAYMENT_MARK : formatLocalDate(lastPayment)"
          data-testid="debts-owner-last-payment"
          size="small"
          title="Последний платеж"
          variant="filled"
        />

        <MudChip
          :icon="mdAttachMoney"
          :text="`${formatMoneyWhole(total)}/${formatMoneyWhole(remainder)}`"
          data-testid="debts-owner-money"
          size="small"
          title="Всего/Остаток"
          variant="filled"
        />

        <MudChip
          :icon="mdArrowCircleUp"
          :text="`${percent} %`"
          data-testid="debts-owner-percent"
          size="small"
          title="Процент погашения"
          variant="filled"
        />
      </MudStack>
    </MudPaper>

    <MudGrid
      class="my-1 list-flow-host"
      :spacing="1"
    >
      <TransitionGroup
        name="list-flow"
        @before-enter="unpinItem"
        @before-leave="pinLeavingItem"
      >
        <MudItem
          v-for="debt in owner.debts"
          :key="debt.id"
          class="mb-3"
          :lg="3"
          :md="4"
          :xs="12"
        >
          <DebtCard
            :debt="debt"
            :is-paying="isPaying"
            :is-payment-open="paymentValue !== null && paymentValue.debtId === debt.id"
            :is-selectable="isSelectable(debt)"
            :is-selected="selectedIds.has(debt.id)"
            :payment-value="paymentValue"
            @cancel-payment="emit('cancel-payment', $event)"
            @edit="emit('edit', $event)"
            @open-payment="emit('open-payment', $event)"
            @remove="emit('remove', $event)"
            @restore="emit('restore', $event)"
            @submit-payment="emit('submit-payment', $event)"
            @toggle-selection="emit('toggle-selection', $event)"
          />
        </MudItem>
      </TransitionGroup>
    </MudGrid>
  </div>
</template>

<style scoped>
.debt-owner__title {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.debt-book {
  position: relative;
  overflow: hidden;
  margin-bottom: 12px;
  border: 1px solid var(--mud-palette-lines-default);
  border-radius: 6px;
}

.debt-book::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 4px;
  background: rgb(var(--record-card-role));
}

.debt-book__head {
  display: flex;
  align-items: baseline;
  gap: 10px;
  width: 100%;
  min-height: 48px;
  padding: 10px 12px 8px 16px;
  border: 0;
  background: none;
  color: inherit;
  font-family: inherit;
  text-align: left;
}

.debt-book__who {
  flex: 1 1 auto;
  min-width: 0;
  overflow: hidden;
  font-size: 1.1875rem;
  line-height: 1.2;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.debt-book__money {
  flex: 0 0 auto;
  min-width: var(--money-col-width, 9ch);
  text-align: right;
}

.debt-book__remainder {
  display: block;
  font-size: 1.1875rem;
  font-weight: 700;
  line-height: 1.15;
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum';
}

.debt-book__of {
  display: block;
  font-size: 0.6875rem;
  line-height: 1.3;
  color: var(--mud-palette-text-disabled);
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum';
}

.debt-book__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: center;
  padding: 0 12px 10px 16px;
  font-size: 0.75rem;
  line-height: 1.3;
  color: var(--mud-palette-text-secondary);
}

.debt-book__hint {
  flex: 1 1 100%;
  color: var(--mud-palette-warning);
}

.debt-book__hint:empty {
  display: none;
}

.debt-book__dot {
  color: var(--mud-palette-text-disabled);
}

.debt-book__bar {
  height: 3px;
  background: var(--mud-palette-action-default-hover);
}

.debt-book__bar > i {
  display: block;
  height: 100%;
  background: rgb(var(--record-card-role));
}

.debt-book__sum {
  display: flex;
  align-items: baseline;
  gap: 10px;
  padding: 8px 12px 8px 16px;
  border-top: 1px solid var(--mud-palette-lines-default);
  font-size: 0.8125rem;
  color: var(--mud-palette-text-secondary);
}

.debt-book__sum-text {
  flex: 1 1 auto;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.debt-book__sum-value {
  flex: 0 0 auto;
  min-width: var(--money-col-width, 9ch);
  text-align: right;
  font-weight: 700;
  color: var(--mud-palette-text-primary);
  font-variant-numeric: tabular-nums;
  font-feature-settings: 'tnum';
}

.debt-book__foot {
  padding: 8px 12px 10px 16px;
  border-top: 1px solid var(--mud-palette-lines-default);
}

.debt-book__pay {
  display: block;
  width: 100%;
  min-height: 40px;
  padding: 0 16px;
  border: 1px solid color-mix(in srgb, var(--record-card-role-edge) 85%, var(--mud-palette-text-primary));
  border-radius: 20px;
  background: rgba(var(--record-card-role), 0.12);
  color: var(--mud-palette-text-primary);
  font-family: inherit;
  font-size: 0.875rem;
  font-weight: 500;
  letter-spacing: 0.03em;
}

.debt-book__pay:active:not(:disabled) {
  background: rgba(var(--record-card-role), 0.22);
}

.debt-book__pay:disabled {
  border-color: var(--mud-palette-lines-default);
  background: var(--mud-palette-action-disabled-background);
  color: var(--mud-palette-action-disabled);
}
</style>
