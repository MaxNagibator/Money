<script setup lang="ts">
import { computed } from 'vue'
import { UNNAMED_DEBT_OWNER } from '@/api/adapters/debts'
import { Money } from '@/domain/money'
import { formatLocalDate, formatMoney, formatMoneyWhole } from '@/platform/format'
import { mdAttachMoney, mdSwapHoriz } from '@/shared/icons'
import { MudChip, MudIcon, MudPaper, MudSpacer, MudStack, MudText } from '@/shared/mud'
import { roleVariables } from '@/shared/ui/record-card'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { DebtAccount, DebtMovement } from '../model/debt-accounts'
import { debtRoleColor } from '../model/debt'
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
const remainder = computed(() => props.account.balance.isNegative ? props.account.balance.negate() : props.account.balance)
const roleStyle = computed(() => roleVariables(debtRoleColor(props.account.balance.isNegative ? 2 : 1)))
const balanceColor = computed(() => props.account.balance.isNegative ? 'warning' as const : props.account.balance.isZero ? 'default' as const : 'success' as const)
const latestDate = computed(() => {
  const latest = statement.value.rows.find(row => !row.movement.source.isDeleted && row.movement.date !== null)
  return latest?.movement.date ? formatLocalDate(latest.movement.date) : null
})

function movementMagnitude(movement: DebtMovement): Money {
  return movement.amount.isNegative ? movement.amount.negate() : movement.amount
}

function movementText(movement: DebtMovement): string | null {
  const value = movement.comment?.trim() ?? ''
  if (value.length > 0) {
    return value
  }
  return movement.kind === 'legacyPayment' ? 'Старые оплаты' : null
}
</script>

<template>
  <div
    v-if="isNarrow"
    :data-testid="`debt-account-${account.name}`"
    :style="roleStyle"
    class="debt-book"
  >
    <button
      :aria-expanded="isExpanded"
      :aria-label="`${isExpanded ? 'Свернуть' : 'Развернуть'} долги: ${title}`"
      data-testid="debt-account-toggle"
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
            data-testid="debt-account-balance"
          >{{ formatMoney(account.balance) }}</b>
        </span>
        <span class="debt-book__metric">
          <small>Движения / оборот</small>
          <span class="debt-book__combined">
            <MudIcon
              :icon="mdSwapHoriz"
              size="small"
            />
            <b data-testid="debt-account-turnover">{{ statement.count }} / {{ formatMoneyWhole(turnover) }}</b>
          </span>
        </span>
      </span>
    </button>

    <div
      v-if="latestDate !== null"
      class="debt-book__meta"
      data-testid="debt-account-summary"
    >
      <span>{{ latestDate }}</span>
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
          v-for="row in statement.rows"
          :key="row.movement.key"
          class="debt-ledger__row"
        >
          <div class="debt-ledger__cell debt-ledger__cell--outgoing">
            <div
              v-if="!row.movement.amount.isNegative"
              :class="{ 'debt-row--deleted': row.movement.source.isDeleted }"
              class="debt-row"
            >
              <div class="debt-row__main">
                <span class="debt-row__when">
                  <span>{{ row.movement.date === null ? 'Без даты' : formatLocalDate(row.movement.date) }}</span>
                  <template v-if="movementText(row.movement) !== null">
                    <span>&nbsp;·&nbsp;</span>
                    <span>{{ movementText(row.movement) }}</span>
                  </template>
                </span>
                <span
                  :data-testid="`debt-movement-out-${row.movement.source.id}`"
                  class="debt-row__sum"
                >{{ formatMoney(movementMagnitude(row.movement)) }}</span>
              </div>
            </div>
          </div>

          <div class="debt-ledger__cell debt-ledger__cell--incoming">
            <div
              v-if="row.movement.amount.isNegative"
              :class="{ 'debt-row--deleted': row.movement.source.isDeleted }"
              class="debt-row"
            >
              <div class="debt-row__main debt-row__main--incoming">
                <span class="debt-row__when">
                  <span>{{ row.movement.date === null ? 'Без даты' : formatLocalDate(row.movement.date) }}</span>
                  <template v-if="movementText(row.movement) !== null">
                    <span>&nbsp;·&nbsp;</span>
                    <span>{{ movementText(row.movement) }}</span>
                  </template>
                </span>
                <span
                  :data-testid="`debt-movement-in-${row.movement.source.id}`"
                  class="debt-row__sum"
                >{{ formatMoney(movementMagnitude(row.movement)) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div
        v-if="statement.rows.length > 1"
        class="debt-book__sum"
      >
        <span class="debt-book__sum-text">Остаток по держателю</span>
        <span
          class="debt-book__sum-value"
          data-testid="debt-account-sum"
        >{{ formatMoney(remainder) }}</span>
      </div>
    </template>
  </div>

  <div
    v-else
    :data-testid="`debt-account-${account.name}`"
  >
    <MudPaper
      :aria-expanded="isExpanded"
      data-testid="debt-account-toggle"
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
          html-tag="h2"
          typo="h6"
        >
          {{ title }}
        </MudText>

        <MudSpacer />

        <MudChip
          :color="balanceColor"
          :icon="mdAttachMoney"
          :text="formatMoneyWhole(account.balance)"
          data-testid="debt-account-balance"
          size="small"
          title="Баланс"
          variant="filled"
        />

        <MudChip
          :icon="mdSwapHoriz"
          :text="`${statement.count} / ${formatMoneyWhole(turnover)}`"
          data-testid="debt-account-turnover"
          size="small"
          title="Количество движений / оборот"
          variant="filled"
        />
      </MudStack>
    </MudPaper>

    <div
      v-if="isExpanded"
      class="debt-ledger debt-ledger--wide"
    >
      <div class="debt-ledger__heading">
        Уход
      </div>
      <div class="debt-ledger__heading">
        Приход
      </div>

      <div
        v-for="row in statement.rows"
        :key="row.movement.key"
        class="debt-ledger__row"
      >
        <div class="debt-ledger__cell debt-ledger__cell--outgoing">
          <div
            v-if="!row.movement.amount.isNegative"
            :class="{ 'debt-row--deleted': row.movement.source.isDeleted }"
            class="debt-row"
          >
            <div class="debt-row__main">
              <span class="debt-row__when">
                <span>{{ row.movement.date === null ? 'Без даты' : formatLocalDate(row.movement.date) }}</span>
                <template v-if="movementText(row.movement) !== null">
                  <span>&nbsp;·&nbsp;</span>
                  <span>{{ movementText(row.movement) }}</span>
                </template>
              </span>
              <span
                :data-testid="`debt-movement-out-${row.movement.source.id}`"
                class="debt-row__sum"
              >{{ formatMoney(movementMagnitude(row.movement)) }}</span>
            </div>
          </div>
        </div>

        <div class="debt-ledger__cell debt-ledger__cell--incoming">
          <div
            v-if="row.movement.amount.isNegative"
            :class="{ 'debt-row--deleted': row.movement.source.isDeleted }"
            class="debt-row"
          >
            <div class="debt-row__main debt-row__main--incoming">
              <span class="debt-row__when">
                <span>{{ row.movement.date === null ? 'Без даты' : formatLocalDate(row.movement.date) }}</span>
                <template v-if="movementText(row.movement) !== null">
                  <span>&nbsp;·&nbsp;</span>
                  <span>{{ movementText(row.movement) }}</span>
                </template>
              </span>
              <span
                :data-testid="`debt-movement-in-${row.movement.source.id}`"
                class="debt-row__sum"
              >{{ formatMoney(movementMagnitude(row.movement)) }}</span>
            </div>
          </div>
        </div>
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
  margin-bottom: 8px;
}

.debt-ledger {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  border-top: 1px solid var(--mud-palette-lines-default);
}

.debt-ledger--wide {
  margin: -4px 0 16px;
  border: 1px solid var(--mud-palette-lines-default);
  border-radius: 0 0 6px 6px;
}

.debt-ledger__row {
  display: grid;
  grid-column: 1 / -1;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
}

.debt-ledger__cell {
  min-width: 0;
  border-bottom: 1px solid var(--mud-palette-lines-default);
}

.debt-ledger__cell--incoming {
  border-left: 1px solid var(--mud-palette-lines-default);
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

.debt-row {
  display: flex;
  align-items: center;
}

.debt-row__main {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: baseline;
  flex: 1 1 auto;
  gap: 10px;
  min-width: 0;
  min-height: 44px;
  padding: 7px 12px 7px 16px;
  font-size: 0.8125rem;
}

.debt-row__main--incoming {
  grid-template-columns: auto minmax(0, 1fr);
}

.debt-row__main--incoming .debt-row__when {
  grid-column: 2;
  text-align: right;
}

.debt-row__main--incoming .debt-row__sum {
  grid-column: 1;
  grid-row: 1;
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

.debt-book__remainder--warning {
  color: var(--mud-palette-warning);
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
</style>
