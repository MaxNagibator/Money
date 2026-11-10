<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { UNNAMED_DEBT_OWNER } from '@/api/adapters/debts'
import type { DebtMovement } from '@/domain/debt'
import { formatLocalDate, formatMoney, formatMoneyWhole } from '@/platform/format'
import {
  mdAttachMoney,
  mdSwapHoriz,
} from '@/shared/icons'
import {
  MudChip,
  MudIcon,
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
  debtRoleColor,
  nextPayableDebt,
  ownerRemainder,
  ownerTotal,
  signedBalance,
} from '../model/debt'
import type { PaymentFormValue } from '../model/payment-form'
import DebtCard from './DebtCard.vue'
import DebtPaymentForm from './DebtPaymentForm.vue'

const props = defineProps<{
  owner: DebtOwnerGroup
  isSelectable: (movement: DebtMovement) => boolean
  selectedIds: ReadonlySet<number>
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

const COLLAPSE_HINT = 'сначала завершите или отмените платёж'

const isNarrow = useNarrowViewport()
const isExpanded = ref(false)
const isCollapseBlocked = ref(false)

const title = computed(() => props.owner.name.trim().length === 0 ? UNNAMED_DEBT_OWNER : props.owner.name)
const total = computed(() => ownerTotal(props.owner))
const balance = computed(() => signedBalance(props.owner.debts))
const remainder = computed(() => ownerRemainder(props.owner))

const roleStyle = computed(() => roleVariables(debtRoleColor(signedBalance(props.owner.debts).isNegative ? 2 : 1)))
const balanceColor = computed(() => balance.value.isNegative ? 'error' as const : balance.value.isZero ? 'default' as const : 'success' as const)
const outgoingMovements = computed(() => props.owner.debts.filter(movement => movement.typeId === 1))
const incomingMovements = computed(() => props.owner.debts.filter(movement => movement.typeId === 2))
const payable = computed(() => nextPayableDebt(props.owner))
const payableDate = computed(() => payable.value === null ? null : formatLocalDate(payable.value.date))
const openPayment = computed(() => {
  const value = props.paymentValue

  if (value === null) {
    return null
  }

  return props.owner.debts.some(movement => movement.id === value.debtId) ? value : null
})
const isForgiving = computed(() => props.owner.debts.some(movement => props.isSelectable(movement)))
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
    // Идентификатор задаёт направление, сумма — встречное движение по всему счёту.
    emit('open-payment', {
      ...payable.value,
      sum: remainder.value,
    })
  }
}

function cancelPayment(): void {
  const value = openPayment.value

  if (value === null) {
    return
  }

  const movement = props.owner.debts.find(candidate => candidate.id === value.debtId)

  if (movement !== undefined) {
    emit('cancel-payment', movement)
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
        <span class="debt-book__metric">
          <small>Баланс</small>
          <b
            :class="`debt-book__remainder debt-book__remainder--${balanceColor}`"
            data-testid="debts-owner-remainder"
          >{{ formatMoney(balance) }}</b>
        </span>
        <span class="debt-book__metric">
          <small>Движения / оборот</small>
          <span class="debt-book__combined">
            <MudIcon :icon="mdSwapHoriz" size="small" />
            <b data-testid="debts-owner-total">{{ owner.debts.length }} / {{ formatMoneyWhole(total) }}</b>
          </span>
        </span>
      </span>
    </button>

    <div
      class="debt-book__meta"
      data-testid="debts-owner-meta"
    >
      <template v-if="payableDate !== null">
        <span>{{ payableDate }}</span>
      </template>
      <span
        class="debt-book__hint"
        data-testid="debts-owner-collapse-hint"
        role="status"
      >{{ collapseHint }}</span>
    </div>

    <template v-if="isExpanded">
      <div class="debt-ledger">
        <div class="debt-ledger__heading">
          Уход
        </div>
        <div class="debt-ledger__heading">
          Приход
        </div>
        <div
          v-for="(movements, column) in [outgoingMovements, incomingMovements]"
          :key="column"
          :data-testid="column === 0 ? 'debts-owner-outgoing' : 'debts-owner-incoming'"
          class="debt-ledger__column"
        >
          <TransitionGroup
            name="list-flow"
            @before-enter="unpinItem"
            @before-leave="pinLeavingItem"
          >
            <DebtCard
              v-for="movement in movements"
              :key="movement.id"
              ledger
              :movement="movement"
              :is-paying="isPaying"
              :is-payment-open="paymentValue !== null && paymentValue.debtId === movement.id"
              :is-selectable="isSelectable(movement)"
              :is-selected="selectedIds.has(movement.id)"
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
        </div>
      </div>

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
          Закрыть расчёт
        </button>
      </div>
    </template>
  </div>

  <div
    v-else
    :data-testid="`debts-owner-${owner.name}`"
  >
    <MudPaper
      :aria-expanded="isExpanded"
      :data-testid="`debts-owner-toggle-${owner.name}`"
      class="pa-2 debt-owner__head"
      :elevation="1"
      role="button"
      tabindex="0"
      @click="toggle"
      @keydown.enter="toggle"
      @keydown.space.prevent="toggle"
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
          :color="balanceColor"
          :icon="mdAttachMoney"
          :text="formatMoneyWhole(balance)"
          data-testid="debts-owner-money"
          size="small"
          title="Баланс"
          variant="filled"
        />

        <MudChip
          :icon="mdSwapHoriz"
          :text="`${owner.debts.length} / ${formatMoneyWhole(total)}`"
          data-testid="debts-owner-turnover"
          size="small"
          title="Количество движений / оборот"
          variant="filled"
        />
      </MudStack>
    </MudPaper>

    <div
      v-if="isExpanded"
      class="debt-ledger debt-ledger--wide list-flow-host"
    >
      <div class="debt-ledger__heading">
        Уход
      </div>
      <div class="debt-ledger__heading">
        Приход
      </div>
      <div
        v-for="(movements, column) in [outgoingMovements, incomingMovements]"
        :key="column"
        :data-testid="column === 0 ? 'debts-owner-outgoing' : 'debts-owner-incoming'"
        class="debt-ledger__column"
      >
        <TransitionGroup
          name="list-flow"
          @before-enter="unpinItem"
          @before-leave="pinLeavingItem"
        >
          <DebtCard
            v-for="movement in movements"
            :key="movement.id"
            ledger
            :movement="movement"
            :is-paying="isPaying"
            :is-payment-open="paymentValue !== null && paymentValue.debtId === movement.id"
            :is-selectable="isSelectable(movement)"
            :is-selected="selectedIds.has(movement.id)"
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
      </div>
    </div>
  </div>
</template>

<style scoped>
.debt-owner__title {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.debt-owner__head {
  cursor: pointer;
}

.debt-ledger {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  border-top: 1px solid var(--mud-palette-lines-default);
}

.debt-ledger--wide {
  margin: 4px 0 16px;
  border: 1px solid var(--mud-palette-lines-default);
  border-radius: 0 0 6px 6px;
}

.debt-ledger__column {
  min-width: 0;
  padding: 4px 0;
}

.debt-ledger__heading {
  padding: 7px 12px;
  border-bottom: 1px solid var(--mud-palette-lines-default);
  color: var(--mud-palette-text-secondary);
  font-size: 0.75rem;
  font-weight: 500;
  text-align: center;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.debt-ledger__heading + .debt-ledger__heading {
  border-left: 1px solid var(--mud-palette-lines-default);
}

.debt-ledger__column + .debt-ledger__column {
  border-left: 1px solid var(--mud-palette-lines-default);
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
  display: flex;
  flex: 0 0 auto;
  gap: 14px;
  text-align: right;
}

.debt-book__metric {
  display: grid;
  gap: 1px;
}

.debt-book__metric small {
  color: var(--mud-palette-text-disabled);
  font-size: 0.625rem;
  font-weight: 400;
  line-height: 1;
}

.debt-book__combined {
  display: flex;
  gap: 3px;
  align-items: center;
}

.debt-book__remainder--success {
  color: var(--mud-palette-success);
}

.debt-book__remainder--error {
  color: var(--mud-palette-error);
}

.debt-book__remainder--default {
  color: var(--mud-palette-text-primary);
}

.debt-book__remainder {
  display: block;
  font-size: 1.1875rem;
  font-weight: 700;
  line-height: 1.15;
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
