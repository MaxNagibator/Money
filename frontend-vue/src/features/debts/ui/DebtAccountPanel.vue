<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { formatLocalDate, formatRubles } from '@/platform/format'
import { mdExpandLess, mdExpandMore, mdTrendingUp } from '@/shared/icons'
import { MudButton, MudIconButton, MudPaper, MudText } from '@/shared/mud'
import { TAKE_TYPE_ID } from '../model/debt'
import type { DebtAccount, DebtMovement } from '../model/debt-accounts'
import type { DebtViewMode } from '../model/debt-presentation'
import { debtStatement, useAccountExpanded } from '../model/debt-presentation'
import DebtBalanceTimelineChart from './DebtBalanceTimelineChart.vue'
import DebtRefinedPanel from './DebtRefinedPanel.vue'
import DebtReworkPanel from './DebtReworkPanel.vue'

const props = withDefaults(
  defineProps<{
    account: DebtAccount
    mode?: DebtViewMode
    expanded?: boolean | undefined
    initiallyExpanded?: boolean
    chartInitiallyOpen?: boolean | undefined
    today?: LocalDate | undefined
  }>(),
  { mode: 'normal', expanded: undefined, initiallyExpanded: false, chartInitiallyOpen: undefined, today: undefined },
)
const emit = defineEmits<{ 'update:expanded': [value: boolean] }>()
const { isExpanded, toggle, setExpanded } = useAccountExpanded(() => props.expanded, props.initiallyExpanded, value => emit('update:expanded', value))
const rework = computed(() => props.mode === 'rework')
const refined = computed(() => props.mode === 'refined')
const accounting = computed(() => props.mode === 'accounting')
const statement = computed(() => debtStatement(props.account))
const hasDatedMovements = computed(() =>
  props.account.movements.some(movement => !movement.source.isDeleted && movement.date !== null),
)
const defaultChartOpen = computed(() => props.chartInitiallyOpen ?? props.initiallyExpanded)
const isChartOpen = ref(defaultChartOpen.value)
watch(defaultChartOpen, value => {
  if (value) {
    isChartOpen.value = true
  }
})
const balanceText = computed(() => {
  const balance = props.account.balance
  if (balance.isZero) { return 'Расчёты закрыты' }
  const label = accounting.value
    ? (balance.isNegative ? 'Нужно отдать: ' : 'Нужно забрать: ')
    : (balance.isNegative ? 'Я должен: ' : 'Мне должны: ')
  return label + formatRubles(balance.isNegative ? balance.negate() : balance, !accounting.value)
})
function amountText(movement: DebtMovement): string {
  const magnitude = movement.amount.isNegative ? movement.amount.negate() : movement.amount
  return (movement.amount.isNegative ? '+' : '−') + formatRubles(magnitude, true)
}
function movementLabel(movement: DebtMovement): string {
  if (movement.kind === 'legacyPayment') { return 'Старые оплаты (суммарно)' }
  if (movement.kind === 'payment') { return movement.source.typeId === TAKE_TYPE_ID ? 'Вернули' : 'Вернул' }
  return movement.source.typeId === TAKE_TYPE_ID ? 'Дал в долг' : 'Взял в долг'
}
</script>

<template>
  <DebtReworkPanel
    v-if="rework"
    :account="account"
    :expanded="isExpanded"
    :initially-expanded="initiallyExpanded"
    @update:expanded="setExpanded($event)"
  />
  <DebtRefinedPanel
    v-else-if="refined"
    :account="account"
    :expanded="isExpanded"
    :initially-expanded="initiallyExpanded"
    @update:expanded="setExpanded($event)"
  />
  <MudPaper
    v-else
    class="debt-account"
    :data-testid="'debt-account-' + account.name"
    :elevation="1"
  >
    <header class="account-header">
      <MudText
        html-tag="h2"
        typo="h6"
        class="account-name"
      >
        {{ account.name }}
      </MudText>
      <div
        class="account-summary"
        data-testid="debt-account-summary"
      >
        <span
          class="account-balance"
          :class="{ receivable: !account.balance.isZero && !account.balance.isNegative, payable: account.balance.isNegative }"
          data-testid="debt-account-balance"
        >{{ balanceText }}</span>
        <span
          v-if="isExpanded && accounting"
          class="account-count"
        >Записей: {{ statement.count }}</span>
      </div>
      <MudIconButton
        :aria-expanded="isExpanded"
        :aria-label="isExpanded ? 'Свернуть историю' : 'Развернуть историю'"
        :icon="isExpanded ? mdExpandLess : mdExpandMore"
        data-testid="debt-account-toggle"
        @click="toggle"
      />
    </header>
    <div
      v-if="isExpanded"
      class="account-expanded"
    >
      <div
        v-if="hasDatedMovements"
        class="account-chart-block"
      >
        <MudButton
          :aria-expanded="isChartOpen"
          class="account-chart-toggle"
          data-testid="debt-account-chart-toggle"
          :end-icon="isChartOpen ? mdExpandLess : mdExpandMore"
          size="small"
          :start-icon="mdTrendingUp"
          variant="text"
          @click="isChartOpen = !isChartOpen"
        >
          График баланса
        </MudButton>
        <div
          v-if="isChartOpen"
          class="account-chart"
          data-testid="debt-account-chart"
        >
          <DebtBalanceTimelineChart
            :account="account"
            compact
            :today="today"
          />
        </div>
      </div>
      <template v-if="accounting">
        <p
          v-if="statement.hasUndatedPayments"
          class="account-hint"
        >
          Старые оплаты без даты сгруппированы с исходной записью. Сальдо рассчитано в порядке строк снизу вверх.
        </p>
        <div
          class="account-register"
          role="region"
          :aria-label="'Регистр расчётов: ' + account.name"
          tabindex="0"
        >
          <table>
            <caption class="sr-only">
              История расчётов: {{ account.name }}
            </caption>
            <colgroup>
              <col>
              <col>
              <col>
              <col>
              <col>
            </colgroup>
            <thead>
              <tr>
                <th
                  scope="col"
                  class="register-date"
                >
                  Дата
                </th>
                <th
                  scope="col"
                  class="register-comment"
                >
                  Основание / Комментарий
                </th>
                <th
                  scope="col"
                  class="numeric"
                >
                  Уход (Выдано)
                </th>
                <th
                  scope="col"
                  class="numeric"
                >
                  Приход (Получено)
                </th>
                <th
                  scope="col"
                  class="numeric"
                >
                  Сальдо
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="row in statement.rows"
                :key="row.movement.key"
                :class="{ deleted: row.movement.source.isDeleted }"
              >
                <td>{{ row.movement.date === null ? '–' : formatLocalDate(row.movement.date) }}</td>
                <td class="movement-comment">
                  <span>{{ row.movement.comment || movementLabel(row.movement) }}</span>
                  <small v-if="row.movement.kind === 'legacyPayment'">Старые оплаты (суммарно) · Дата не сохранена</small>
                </td>
                <td
                  class="numeric"
                  data-label="Уход"
                >
                  <span
                    v-if="!row.movement.amount.isNegative"
                    :data-testid="'debt-movement-out-' + row.movement.source.id"
                  >{{ formatRubles(row.movement.amount) }}</span><span v-else>–</span>
                </td>
                <td
                  class="numeric"
                  data-label="Приход"
                >
                  <span
                    v-if="row.movement.amount.isNegative"
                    :data-testid="'debt-movement-in-' + row.movement.source.id"
                  >{{ formatRubles(row.movement.amount.negate()) }}</span><span v-else>–</span>
                </td>
                <td
                  class="numeric"
                  data-label="Сальдо"
                  :data-testid="'debt-movement-balance-' + row.movement.key"
                >
                  {{ formatRubles(row.balance) }}
                </td>
              </tr>
            </tbody>
            <tfoot>
              <tr>
                <th
                  colspan="2"
                  scope="row"
                >
                  ИТОГО обороты <small>(записей: {{ statement.count }})</small>
                </th>
                <td
                  class="numeric"
                  data-label="Уход"
                >
                  {{ formatRubles(statement.outgoing) }}
                </td>
                <td
                  class="numeric"
                  data-label="Приход"
                >
                  {{ formatRubles(statement.incoming) }}
                </td>
                <td
                  class="numeric"
                  data-label="Сальдо"
                  :class="{ 'total-take': !statement.balance.isNegative, 'total-give': statement.balance.isNegative }"
                >
                  {{ formatRubles(statement.balance) }}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </template>
      <template v-else>
        <ul
          class="account-feed"
          :aria-label="'История расчётов: ' + account.name"
        >
          <li
            v-for="row in statement.rows"
            :key="row.movement.key"
            class="feed-row"
            :class="{ deleted: row.movement.source.isDeleted }"
          >
            <span
              aria-hidden="true"
              class="feed-indicator"
              :class="row.movement.kind === 'entry' ? 'feed-indicator--debt' : 'feed-indicator--payment'"
            />
            <div class="feed-details">
              <div class="movement-comment">
                {{ row.movement.comment || movementLabel(row.movement) }}
              </div>
              <div class="feed-meta">
                <span class="feed-date">{{ row.movement.date === null ? 'Дата не сохранена' : formatLocalDate(row.movement.date) }}</span><span
                  aria-hidden="true"
                  class="feed-meta-sep"
                > · </span><span class="feed-tag">{{ movementLabel(row.movement) }}</span>
              </div>
            </div>
            <span
              class="feed-amount numeric"
              :class="{ 'cash-incoming': !account.balance.isNegative && row.movement.amount.isNegative }"
              :data-testid="(row.movement.amount.isNegative ? 'debt-movement-in-' : 'debt-movement-out-') + row.movement.source.id"
            >{{ amountText(row.movement) }}</span>
          </li>
        </ul>
      </template>
    </div>
  </MudPaper>
</template>

<style scoped>
.debt-account { margin-bottom: 12px; }
.account-header { display: flex; align-items: center; gap: 12px; padding: 10px 14px; }
.account-name { flex: 1; min-width: 0; overflow-wrap: anywhere; }
.account-summary { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; justify-content: flex-end; }
.account-balance { padding: 2px 10px; border-radius: 20px; background: var(--mud-palette-divider); font-size: 0.8125rem; font-variant-numeric: tabular-nums; }
.receivable { color: color-mix(in srgb, var(--mud-palette-success) 60%, var(--mud-palette-text-primary)); }
.payable { color: color-mix(in srgb, var(--mud-palette-warning) 60%, var(--mud-palette-text-primary)); }
.account-count, .account-hint, .feed-meta, .movement-comment small { color: var(--mud-palette-text-secondary); font-size: 0.75rem; }
.account-expanded { padding: 0 14px 12px; }
.account-chart-block { margin-bottom: 10px; }
.account-chart-toggle :deep(.mud-button-label) { text-transform: none; }
.account-chart { margin-top: 8px; padding: 10px 12px; border: 1px solid var(--mud-palette-divider); border-radius: 8px; }
.account-actions { display: flex; align-items: center; flex-wrap: wrap; gap: 8px; padding-bottom: 8px; }
.account-actions :deep(.mud-button-label) { text-transform: none; }
.account-quick-action { background: rgba(255, 255, 255, 0.07); border: 1px solid var(--mud-palette-divider); border-radius: 8px; padding: 6px 12px; min-height: 40px; color: var(--mud-palette-text-primary); }
.account-quick-action:hover { background: color-mix(in srgb, var(--mud-palette-text-primary) 12%, transparent); }
.account-more { margin-left: auto; }
.account-hint { margin: 8px 0; line-height: 1.5; }
.account-register { position: relative; container-type: inline-size; }
table { width: 100%; border-collapse: collapse; font-family: var(--mud-typography-body2-family); font-size: var(--mud-typography-body2-size); line-height: var(--mud-typography-body2-lineheight); }
th { text-align: left; color: var(--mud-palette-text-secondary); font-weight: 500; }
th, td { padding: 8px 10px; border-bottom: 1px solid var(--mud-palette-divider); vertical-align: top; }
.register-date { width: 100px; white-space: nowrap; }
.register-comment { width: 100%; }
.register-actions { box-sizing: border-box; width: 48px; min-width: 48px; max-width: 48px; padding: 0; vertical-align: middle; }
.numeric { text-align: right; font-variant-numeric: tabular-nums; white-space: nowrap; }
.movement-comment { min-width: 0; white-space: pre-wrap; overflow-wrap: anywhere; }
.movement-comment small { display: block; margin-top: 4px; }
tfoot th, tfoot td { padding-top: 12px; border-bottom: 0; font-weight: 600; }
tfoot th small { font-weight: 400; white-space: nowrap; }
.total-take { color: var(--mud-palette-success); }
.total-give { color: var(--mud-palette-warning); }
.account-feed { margin: 0; padding: 0; list-style: none; }
.feed-row { display: grid; grid-template-columns: 3px minmax(76px, auto) minmax(0, 1fr) minmax(108px, auto) minmax(96px, auto); align-items: center; gap: 10px; min-height: 36px; padding: 7px 0; border-bottom: 1px solid var(--mud-palette-divider); }
.feed-row:last-child { border-bottom: 0; }
.feed-indicator { grid-column: 1; grid-row: 1; width: 3px; height: 18px; border-radius: 2px; }
.feed-indicator--payment { background: color-mix(in srgb, var(--mud-palette-success) 65%, transparent); }
.feed-indicator--debt { background: color-mix(in srgb, var(--mud-palette-warning) 60%, transparent); }
.feed-details { display: contents; }
.feed-details > .movement-comment { grid-column: 3; grid-row: 1; font-size: var(--mud-typography-body2-size); line-height: 1.35; }
.feed-meta { display: contents; }
.feed-date { grid-column: 2; grid-row: 1; min-width: 72px; color: var(--mud-palette-text-secondary); font-size: 0.75rem; font-variant-numeric: tabular-nums; white-space: nowrap; }
.feed-meta-sep { display: none; }
.feed-tag { grid-column: 4; grid-row: 1; justify-self: end; padding: 2px 8px; border-radius: 999px; background: var(--mud-palette-action-default-hover); color: var(--mud-palette-text-secondary); font-size: 0.75rem; line-height: 1.3; white-space: nowrap; }
.feed-amount { grid-column: 5; grid-row: 1; font-size: 0.9375rem; font-weight: 500; line-height: 1.35; }
.cash-incoming { color: color-mix(in srgb, var(--mud-palette-success) 60%, var(--mud-palette-text-primary)); }
.deleted { opacity: 0.55; text-decoration: line-through; }
.sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); }
@container (max-width: 800px) {
  table, tbody, tfoot { display: block; }
  colgroup { display: none; }
  thead { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); }
  tbody tr, tfoot tr { position: relative; display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 8px; padding: 12px 0; border-bottom: 1px solid var(--mud-palette-divider); }
  th, td { min-width: 0; padding: 0; border: 0; }
  tbody td:first-child { grid-column: 1 / -1; padding-right: 48px; color: var(--mud-palette-text-secondary); font-size: var(--mud-typography-caption-size); }
  tbody td.movement-comment { grid-column: 1 / -1; padding-right: 48px; }
  tbody td.register-actions { position: absolute; top: 4px; right: 0; }
  .numeric { white-space: normal; overflow-wrap: anywhere; }
  .numeric::before { content: attr(data-label); display: block; margin-bottom: 4px; color: var(--mud-palette-text-secondary); font-size: var(--mud-typography-caption-size); font-weight: var(--mud-typography-caption-weight); }
  tfoot th { grid-column: 1 / -1; }
  tfoot th, tfoot td { padding: 0; }
  tfoot .register-actions { display: none; }
  tfoot tr { border-bottom: 0; }
}
@media (max-width: 599.98px) {
  .account-header { display: grid; grid-template-columns: minmax(0, 1fr) 44px; gap: 6px; padding: 10px 12px; }
  .account-name { grid-column: 1; grid-row: 1; }
  .account-header > :deep(.mud-icon-button) { grid-column: 2; grid-row: 1; }
  .account-summary { grid-column: 1 / -1; grid-row: 2; justify-content: flex-start; }
  .account-expanded { padding: 0 12px 8px; }
  .feed-row { grid-template-columns: 3px minmax(0, 1fr) auto; grid-template-rows: auto auto; gap: 2px 8px; min-height: 0; padding: 8px 0; }
  .feed-indicator { grid-column: 1; grid-row: 1 / 3; height: auto; align-self: stretch; margin: 2px 0; }
  .feed-details > .movement-comment { grid-column: 2; grid-row: 1; }
  .feed-amount { grid-column: 3; grid-row: 1; }
  .feed-meta { display: block; grid-column: 2 / -1; grid-row: 2; margin-top: 0; line-height: 1.25; }
  .feed-meta-sep { display: inline; }
  .feed-date { min-width: 0; color: inherit; font-size: inherit; }
  .feed-tag { padding: 0; border-radius: 0; background: transparent; color: inherit; font-size: inherit; line-height: inherit; }
}
</style>
