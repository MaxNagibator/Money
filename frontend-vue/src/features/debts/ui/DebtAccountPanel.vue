<script setup lang="ts">
import { computed } from 'vue'
import { formatLocalDate, formatRubles } from '@/platform/format'
import { mdExpandLess, mdExpandMore } from '@/shared/icons'
import { MudIconButton, MudPaper, MudText } from '@/shared/mud'
import { TAKE_TYPE_ID } from '../model/debt'
import type { DebtAccount, DebtMovement } from '../model/debt-accounts'
import type { DebtViewMode } from '../model/debt-presentation'
import { debtStatement, useAccountExpanded } from '../model/debt-presentation'
import DebtRefinedPanel from './DebtRefinedPanel.vue'
import DebtReworkPanel from './DebtReworkPanel.vue'

const props = withDefaults(
  defineProps<{ account: DebtAccount, mode?: DebtViewMode, expanded?: boolean | undefined, initiallyExpanded?: boolean }>(),
  { mode: 'normal', expanded: undefined, initiallyExpanded: false },
)
const emit = defineEmits<{ 'update:expanded': [value: boolean] }>()
const { isExpanded, toggle, setExpanded } = useAccountExpanded(() => props.expanded, props.initiallyExpanded, value => emit('update:expanded', value))
const rework = computed(() => props.mode === 'rework')
const refined = computed(() => props.mode === 'refined')
const accounting = computed(() => props.mode === 'accounting')
const statement = computed(() => debtStatement(props.account))
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
            <div class="feed-details">
              <div class="movement-comment">
                {{ row.movement.comment || movementLabel(row.movement) }}
              </div>
              <div class="feed-meta">
                {{ row.movement.date === null ? 'Дата не сохранена' : formatLocalDate(row.movement.date) }} · {{ movementLabel(row.movement) }}
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
.account-header { display: flex; align-items: center; gap: 12px; padding: 14px 18px; }
.account-name { flex: 1; min-width: 0; overflow-wrap: anywhere; }
.account-summary { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; justify-content: flex-end; }
.account-balance { padding: 4px 10px; border-radius: 20px; background: var(--mud-palette-divider); font-size: 0.875rem; font-variant-numeric: tabular-nums; }
.receivable { color: color-mix(in srgb, var(--mud-palette-success) 60%, var(--mud-palette-text-primary)); }
.payable { color: color-mix(in srgb, var(--mud-palette-warning) 60%, var(--mud-palette-text-primary)); }
.account-count, .account-hint, .feed-meta, .movement-comment small { color: var(--mud-palette-text-secondary); font-size: 0.75rem; }
.account-expanded { padding: 0 18px 12px; }
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
.feed-row { display: grid; grid-template-columns: minmax(0, 1fr) auto; align-items: center; gap: 12px; padding: 14px 0; border-bottom: 1px solid var(--mud-palette-divider); }
.feed-row:last-child { border-bottom: 0; }
.feed-details { min-width: 0; }
.feed-details > .movement-comment { font-size: var(--mud-typography-body1-size); line-height: var(--mud-typography-body1-lineheight); letter-spacing: var(--mud-typography-body1-letterspacing); }
.feed-meta { margin-top: 5px; line-height: 1.5; }
.feed-amount { font-size: var(--mud-typography-subtitle1-size); font-weight: var(--mud-typography-subtitle1-weight); line-height: var(--mud-typography-subtitle1-lineheight); }
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
  .account-header { display: grid; grid-template-columns: minmax(0, 1fr) 44px; gap: 6px; padding: 12px; }
  .account-name { grid-column: 1; grid-row: 1; }
  .account-header > :deep(.mud-icon-button) { grid-column: 2; grid-row: 1; }
  .account-summary { grid-column: 1 / -1; grid-row: 2; justify-content: flex-start; }
  .account-expanded { padding: 0 12px 8px; }
  .feed-row { gap: 6px; grid-template-columns: minmax(0, 1fr) auto; }
}
</style>
