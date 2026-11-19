<script setup lang="ts">
import { computed } from 'vue'
import { UNNAMED_DEBT_OWNER } from '@/api/adapters/debts'
import type { Money } from '@/domain/money'
import { formatLocalDate, formatMoney, formatMoneyWhole } from '@/platform/format'
import { mdExpandLess, mdExpandMore } from '@/shared/icons'
import { MudIcon, MudText } from '@/shared/mud'
import { roleVariables } from '@/shared/ui/record-card'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import { TAKE_TYPE_ID } from '../model/debt'
import type { DebtAccount, DebtMovement } from '../model/debt-accounts'
import { debtStatement, useAccountExpanded } from '../model/debt-presentation'

const props = withDefaults(
  defineProps<{ account: DebtAccount, expanded?: boolean | undefined, initiallyExpanded?: boolean }>(),
  { expanded: undefined, initiallyExpanded: false },
)
const emit = defineEmits<{ 'update:expanded': [value: boolean] }>()

const isNarrow = useNarrowViewport()
const { isExpanded, toggle } = useAccountExpanded(() => props.expanded, props.initiallyExpanded, value => emit('update:expanded', value))

const title = computed(() => props.account.name.trim().length === 0 ? UNNAMED_DEBT_OWNER : props.account.name)
const statement = computed(() => debtStatement(props.account))
const turnover = computed(() => statement.value.outgoing.add(statement.value.incoming))
const magnitudeBalance = computed(() => props.account.balance.isNegative ? props.account.balance.negate() : props.account.balance)
const hasDeleted = computed(() => props.account.debts.some(debt => debt.isDeleted))
const tone = computed<'receivable' | 'payable' | 'settled'>(() => {
  if (props.account.balance.isZero) {
    return 'settled'
  }
  return props.account.balance.isNegative ? 'payable' : 'receivable'
})
const roleStyle = computed(() => {
  if (tone.value === 'settled') {
    return undefined
  }
  return roleVariables(tone.value === 'payable' ? 'warning' : 'success')
})
const balanceLabel = computed(() => {
  if (tone.value === 'settled') {
    return 'Расчёты закрыты'
  }
  return `${tone.value === 'payable' ? 'Я должен' : 'Мне должны'}: ${formatMoneyWhole(magnitudeBalance.value)} ₽`
})
const latestDate = computed(() => {
  const latest = statement.value.rows.find(row => !row.movement.source.isDeleted && row.movement.date !== null)
  return latest?.movement.date ? formatLocalDate(latest.movement.date) : null
})

function movementMagnitude(movement: DebtMovement): Money {
  return movement.amount.isNegative ? movement.amount.negate() : movement.amount
}

function kindLabel(movement: DebtMovement): string {
  if (movement.kind === 'legacyPayment') {
    return 'Старая оплата'
  }
  if (movement.kind === 'payment') {
    return movement.source.typeId === TAKE_TYPE_ID ? 'Вернули' : 'Вернул'
  }
  return movement.source.typeId === TAKE_TYPE_ID ? 'Дал' : 'Взял'
}

function movementComment(movement: DebtMovement): string | null {
  const value = movement.comment?.trim() ?? ''
  return value.length > 0 ? value : null
}
</script>

<template>
  <article
    :class="['refined-card', `refined-card--${tone}`]"
    :data-testid="`debt-account-${account.name}`"
    :style="roleStyle"
  >
    <button
      :aria-expanded="isExpanded"
      :aria-label="`${isExpanded ? 'Свернуть' : 'Развернуть'} расчёты: ${title}`"
      class="refined-card__head"
      data-testid="debt-account-toggle"
      type="button"
      @click="toggle"
    >
      <div class="refined-card__identity">
        <MudText
          class="refined-card__title"
          html-tag="h2"
          typo="h6"
        >
          {{ title }}
        </MudText>
        <div
          class="refined-card__meta"
          data-testid="debt-account-summary"
        >
          <span>Движений: {{ statement.count }}</span>
          <span aria-hidden="true">·</span>
          <span>Оборот: {{ formatMoneyWhole(turnover) }} ₽</span>
          <template v-if="latestDate !== null">
            <span aria-hidden="true">·</span>
            <span>Посл.: {{ latestDate }}</span>
          </template>
        </div>
      </div>

      <div class="refined-card__right">
        <span
          :class="['refined-card__badge', `refined-card__badge--${tone}`]"
          data-testid="debt-account-balance"
        >{{ balanceLabel }}</span>
        <MudIcon
          :icon="isExpanded ? mdExpandLess : mdExpandMore"
          class="refined-card__chevron"
          size="small"
        />
      </div>
    </button>

    <div
      v-if="isExpanded"
      class="refined-card__body"
    >
      <p
        v-if="statement.hasUndatedPayments || hasDeleted"
        class="refined-card__notice"
      >
        <template v-if="statement.hasUndatedPayments">
          Оплаты без сохранённой даты отмечены отдельно.
        </template>
        <template v-if="hasDeleted">
          Удалённые записи показаны зачёркнутыми и не учитываются в итогах.
        </template>
      </p>

      <div :class="['refined-ledger', { 'refined-ledger--narrow': isNarrow }]">
        <div class="refined-ledger__heading refined-ledger__heading--out">
          <span>Уход</span>
          <small>дал / вернул</small>
        </div>
        <div class="refined-ledger__heading refined-ledger__heading--in">
          <span>Приход</span>
          <small>взял / вернули</small>
        </div>

        <div
          v-for="row in statement.rows"
          :key="row.movement.key"
          class="refined-ledger__row"
        >
          <div class="refined-ledger__cell refined-ledger__cell--out">
            <div
              v-if="!row.movement.amount.isNegative"
              :class="['refined-entry', 'refined-entry--out', { 'refined-entry--deleted': row.movement.source.isDeleted }]"
            >
              <div class="refined-entry__top">
                <div class="refined-entry__meta">
                  <span class="refined-entry__date">{{ row.movement.date === null ? 'Без даты' : formatLocalDate(row.movement.date) }}</span>
                  <span class="refined-entry__tag">{{ kindLabel(row.movement) }}</span>
                </div>
                <span
                  :data-testid="`debt-movement-out-${row.movement.source.id}`"
                  class="refined-entry__sum"
                >{{ formatMoney(movementMagnitude(row.movement)) }}</span>
              </div>
              <div
                v-if="movementComment(row.movement) !== null"
                class="refined-entry__comment"
              >
                {{ movementComment(row.movement) }}
              </div>
            </div>
          </div>

          <div class="refined-ledger__cell refined-ledger__cell--in">
            <div
              v-if="row.movement.amount.isNegative"
              :class="['refined-entry', 'refined-entry--in', { 'refined-entry--deleted': row.movement.source.isDeleted }]"
            >
              <div class="refined-entry__top">
                <span
                  :data-testid="`debt-movement-in-${row.movement.source.id}`"
                  class="refined-entry__sum"
                >{{ formatMoney(movementMagnitude(row.movement)) }}</span>
                <div class="refined-entry__meta">
                  <span class="refined-entry__tag">{{ kindLabel(row.movement) }}</span>
                  <span class="refined-entry__date">{{ row.movement.date === null ? 'Без даты' : formatLocalDate(row.movement.date) }}</span>
                </div>
              </div>
              <div
                v-if="movementComment(row.movement) !== null"
                class="refined-entry__comment"
              >
                {{ movementComment(row.movement) }}
              </div>
            </div>
          </div>
        </div>

        <div class="refined-ledger__foot refined-ledger__foot--out">
          <span>Итого уход</span>
          <strong class="refined-ledger__total">{{ formatMoney(statement.outgoing) }}</strong>
        </div>
        <div class="refined-ledger__foot refined-ledger__foot--in">
          <strong class="refined-ledger__total">{{ formatMoney(statement.incoming) }}</strong>
          <span>Итого приход</span>
        </div>
      </div>

      <footer class="refined-card__footer">
        <span>Итоговый баланс</span>
        <strong :class="`refined-card__footer-value--${tone}`">{{ balanceLabel }}</strong>
      </footer>
    </div>
  </article>
</template>

<style scoped>
.refined-card {
  position: relative;
  overflow: hidden;
  margin-bottom: 14px;
  border: 1px solid var(--mud-palette-lines-default);
  border-radius: 10px;
  background: var(--mud-palette-surface);
}

.refined-card::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 4px;
  background: var(--mud-palette-lines-default);
}

.refined-card--receivable::before,
.refined-card--payable::before {
  background: rgb(var(--record-card-role));
}

.refined-card--settled {
  opacity: 0.88;
}

.refined-card__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  width: 100%;
  padding: 12px 16px 12px 18px;
  border: 0;
  background: transparent;
  color: inherit;
  font-family: inherit;
  text-align: left;
  cursor: pointer;
}

.refined-card__head:hover {
  background: var(--mud-palette-action-default-hover);
}

.refined-card__identity {
  min-width: 0;
  flex: 1 1 auto;
}

.refined-card__title {
  overflow-wrap: anywhere;
  line-height: 1.25;
}

.refined-card__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 3px;
  color: var(--mud-palette-text-secondary);
  font-size: 0.75rem;
}

.refined-card__right {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 0 0 auto;
}

.refined-card__badge {
  padding: 5px 11px;
  border-radius: 999px;
  background: var(--mud-palette-action-default-hover);
  font-size: 0.8125rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.refined-card__badge--receivable {
  background: color-mix(in srgb, var(--mud-palette-success) 16%, transparent);
  color: color-mix(in srgb, var(--mud-palette-success) 68%, var(--mud-palette-text-primary));
}

.refined-card__badge--payable {
  background: color-mix(in srgb, var(--mud-palette-warning) 16%, transparent);
  color: color-mix(in srgb, var(--mud-palette-warning) 68%, var(--mud-palette-text-primary));
}

.refined-card__badge--settled {
  color: var(--mud-palette-text-secondary);
}

.refined-card__chevron {
  color: var(--mud-palette-text-secondary);
}

.refined-card__body {
  border-top: 1px solid var(--mud-palette-lines-default);
}

.refined-card__notice {
  margin: 0;
  padding: 8px 16px 8px 18px;
  border-bottom: 1px solid var(--mud-palette-lines-default);
  color: var(--mud-palette-text-secondary);
  font-size: 0.75rem;
}

.refined-ledger {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
}

.refined-ledger__heading {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 8px;
  padding: 8px 14px 8px 18px;
  border-bottom: 1px solid var(--mud-palette-lines-default);
  background: color-mix(in srgb, var(--mud-palette-text-primary) 3%, transparent);
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.refined-ledger__heading small {
  color: var(--mud-palette-text-secondary);
  font-size: 0.6875rem;
  font-weight: 400;
  letter-spacing: 0;
  text-transform: none;
}

.refined-ledger__heading--in {
  flex-direction: row-reverse;
  padding: 8px 16px 8px 14px;
  border-left: 1px solid var(--mud-palette-lines-default);
}

.refined-ledger__row {
  display: grid;
  grid-column: 1 / -1;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
}

.refined-ledger__row:nth-child(even) {
  background: color-mix(in srgb, var(--mud-palette-text-primary) 2%, transparent);
}

.refined-ledger__cell {
  min-width: 0;
  border-bottom: 1px solid var(--mud-palette-lines-default);
}

.refined-ledger__cell--in {
  border-left: 1px solid var(--mud-palette-lines-default);
}

.refined-entry {
  display: grid;
  gap: 4px;
  padding: 9px 14px 9px 18px;
}

.refined-entry--in {
  padding: 9px 16px 9px 14px;
}

.refined-entry__top {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 10px;
}

.refined-entry__meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 6px;
  min-width: 0;
}

.refined-entry__date {
  color: var(--mud-palette-text-secondary);
  font-size: 0.75rem;
  white-space: nowrap;
}

.refined-entry__tag {
  padding: 1px 6px;
  border-radius: 4px;
  background: var(--mud-palette-action-default-hover);
  color: var(--mud-palette-text-secondary);
  font-size: 0.6875rem;
  font-weight: 500;
  white-space: nowrap;
}

.refined-entry__sum {
  flex: 0 0 auto;
  font-size: 0.9375rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.refined-entry__comment {
  color: var(--mud-palette-text-primary);
  font-size: 0.8125rem;
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.refined-entry--in .refined-entry__comment {
  text-align: right;
}

.refined-entry--deleted {
  opacity: 0.55;
  text-decoration: line-through;
  text-decoration-color: var(--mud-palette-error);
}

.refined-ledger__foot {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 8px;
  padding: 9px 14px 9px 18px;
  background: color-mix(in srgb, var(--mud-palette-text-primary) 4%, transparent);
  color: var(--mud-palette-text-secondary);
  font-size: 0.75rem;
}

.refined-ledger__foot--in {
  padding: 9px 16px 9px 14px;
  border-left: 1px solid var(--mud-palette-lines-default);
}

.refined-ledger__total {
  color: var(--mud-palette-text-primary);
  font-size: 0.875rem;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.refined-card__footer {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 10px;
  padding: 9px 16px 10px 18px;
  border-top: 1px solid var(--mud-palette-lines-default);
  font-size: 0.8125rem;
  color: var(--mud-palette-text-secondary);
}

.refined-card__footer-value--receivable {
  color: color-mix(in srgb, var(--mud-palette-success) 68%, var(--mud-palette-text-primary));
}

.refined-card__footer-value--payable {
  color: color-mix(in srgb, var(--mud-palette-warning) 68%, var(--mud-palette-text-primary));
}

.refined-card__footer-value--settled {
  color: var(--mud-palette-text-secondary);
}

.refined-ledger--narrow .refined-ledger__heading {
  padding: 7px 10px 7px 12px;
}

.refined-ledger--narrow .refined-ledger__heading--in {
  padding: 7px 10px;
}

.refined-ledger--narrow .refined-ledger__heading small {
  display: none;
}

.refined-ledger--narrow .refined-entry {
  padding: 8px 10px 8px 12px;
}

.refined-ledger--narrow .refined-entry--in {
  padding: 8px 10px;
}

.refined-ledger--narrow .refined-entry__top {
  flex-direction: column;
  align-items: flex-start;
  gap: 3px;
}

.refined-ledger--narrow .refined-entry--in .refined-entry__top {
  flex-direction: column-reverse;
  align-items: flex-end;
}

.refined-ledger--narrow .refined-entry--in .refined-entry__meta {
  justify-content: flex-end;
}

.refined-ledger--narrow .refined-entry__comment {
  font-size: 0.75rem;
}

.refined-ledger--narrow .refined-ledger__foot {
  flex-direction: column;
  align-items: flex-end;
  gap: 2px;
  padding: 8px 10px 8px 12px;
}

.refined-ledger--narrow .refined-ledger__foot--in {
  flex-direction: column-reverse;
  align-items: flex-start;
  padding: 8px 10px;
}

@media (max-width: 599.98px) {
  .refined-card__head {
    flex-wrap: wrap;
    align-items: flex-start;
    padding: 10px 12px 10px 16px;
  }

  .refined-card__right {
    width: 100%;
    justify-content: space-between;
  }
}
</style>
