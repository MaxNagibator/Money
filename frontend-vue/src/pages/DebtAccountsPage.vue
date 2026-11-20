<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, useId, watch } from 'vue'
import { useRoute } from 'vue-router'
import { DebtAccountPanel, DebtChartsSection, debtAccounts, debtSummary, useDebts, useDebtViewMode, visibleDebtAccounts } from '@/features/debts'
import { systemClock } from '@/platform/clock'
import { formatRubles } from '@/platform/format'
import { MudAlert, MudButton, MudChip, MudPaper, MudProgressCircular, MudSelect, MudSelectItem, MudText, MudTextField } from '@/shared/mud'
import { mdArrowBack, mdVisibility, mdVisibilityOff } from '@/shared/icons'

const route = useRoute()
const state = useDebts(ref(true))
const today = ref(systemClock.today())

function refreshToday(): void {
  if (document.visibilityState === 'hidden') {
    return
  }

  const current = systemClock.today()
  if (today.value !== current) {
    today.value = current
  }
}

onMounted(() => {
  document.addEventListener('visibilitychange', refreshToday)
  window.addEventListener('focus', refreshToday)
})

onBeforeUnmount(() => {
  document.removeEventListener('visibilitychange', refreshToday)
  window.removeEventListener('focus', refreshToday)
})
const search = ref('')
const showPaid = ref(false)
const sort = ref<'name' | 'balance'>('name')
const mode = useDebtViewMode()
const modeId = useId()
const expandedNames = ref(new Set<string>())
const settledRevealedFor = ref<string | null>(null)
const allAccounts = computed(() => debtAccounts(state.debts.value))
const accounts = computed(() => visibleDebtAccounts(allAccounts.value, search.value, showPaid.value, sort.value))
const chartAccounts = computed(() => visibleDebtAccounts(allAccounts.value, search.value, true, sort.value))
const summary = computed(() => debtSummary(accounts.value))
const targetOwner = computed(() => typeof route.query.owner === 'string' ? route.query.owner : null)

watch(targetOwner, owner => {
  settledRevealedFor.value = null
  if (owner === null) {
    return
  }
  search.value = owner
  expandedNames.value = new Set([owner])
}, { immediate: true })

watch([allAccounts, targetOwner], ([list, owner]) => {
  if (owner === null || settledRevealedFor.value === owner || list.length === 0) {
    return
  }
  settledRevealedFor.value = owner
  if (list.some(account => account.name === owner && account.balance.isZero)) {
    showPaid.value = true
  }
}, { immediate: true })

function setAccountExpanded(name: string, value: boolean): void {
  const next = new Set(expandedNames.value)
  if (value) {
    next.add(name)
  } else {
    next.delete(name)
  }
  expandedNames.value = next
}
</script>

<template>
  <div class="debt-page-header">
    <MudText
      html-tag="h1"
      typo="h3"
    >
      Отчёты по долгам
    </MudText>
    <fieldset class="debt-view-modes">
      <legend class="sr-only">
        Режим просмотра долгов
      </legend>
      <label
        :for="modeId + '-accounting'"
        :class="{ active: mode === 'accounting' }"
      >
        <input
          :id="modeId + '-accounting'"
          v-model="mode"
          :name="modeId"
          type="radio"
          value="accounting"
          data-testid="debts-mode-accounting"
        >
        Бухгалтерский
      </label>
      <label
        :for="modeId + '-normal'"
        :class="{ active: mode === 'normal' }"
      >
        <input
          :id="modeId + '-normal'"
          v-model="mode"
          :name="modeId"
          type="radio"
          value="normal"
          data-testid="debts-mode-normal"
        >
        Обычный
      </label>
      <label
        :for="modeId + '-rework'"
        :class="{ active: mode === 'rework' }"
      >
        <input
          :id="modeId + '-rework'"
          v-model="mode"
          :name="modeId"
          type="radio"
          value="rework"
          data-testid="debts-mode-rework"
        >
        bob217
      </label>
      <label
        :for="modeId + '-refined'"
        :class="{ active: mode === 'refined' }"
      >
        <input
          :id="modeId + '-refined'"
          v-model="mode"
          :name="modeId"
          type="radio"
          value="refined"
          data-testid="debts-mode-refined"
        >
        bob217 reworked
      </label>
    </fieldset>
  </div>

  <div class="debt-page-actions">
    <MudButton
      data-testid="debts-report-back"
      size="small"
      :start-icon="mdArrowBack"
      to="/debts"
      variant="outlined"
    >
      К долгам
    </MudButton>
    <MudButton
      :aria-label="showPaid ? 'Скрыть закрытые счета' : 'Показать закрытые счета'"
      :aria-pressed="showPaid"
      :color="showPaid ? 'primary' : 'default'"
      data-testid="debts-toggle-paid"
      size="small"
      :start-icon="showPaid ? mdVisibilityOff : mdVisibility"
      :variant="showPaid ? 'filled' : 'outlined'"
      @click="showPaid = !showPaid"
    >
      {{ showPaid ? 'Скрыть закрытые' : 'Показать закрытые' }}
    </MudButton>
    <MudChip
      class="debt-readonly-chip"
      data-testid="debts-report-readonly"
      :icon="mdVisibility"
      size="small"
      text="Только просмотр"
      variant="outlined"
    />
  </div>

  <MudText
    class="debt-report-note"
    color="secondary"
    data-testid="debts-history-scope"
    typo="body2"
  >
    Удалённые долги и оплаты по ним в отчёт и историю не включаются.
  </MudText>

  <p
    v-if="mode === 'accounting'"
    class="debt-summary-caption"
  >
    Итоги по текущему списку
  </p>
  <div
    class="debt-summary"
    data-testid="debts-summary"
  >
    <MudPaper
      class="summary-card"
      :elevation="0"
    >
      <span>{{ mode === 'accounting' ? 'Нужно забрать' : 'Мне должны' }}</span>
      <strong
        class="summary-receivable"
        data-testid="debts-summary-receivable"
      >{{ formatRubles(summary.receivable, mode !== 'accounting') }}</strong>
    </MudPaper>
    <MudPaper
      class="summary-card"
      :elevation="0"
    >
      <span>{{ mode === 'accounting' ? 'Нужно отдать' : 'Я должен' }}</span>
      <strong
        class="summary-payable"
        data-testid="debts-summary-payable"
      >{{ formatRubles(summary.payable, mode !== 'accounting') }}</strong>
    </MudPaper>
    <MudPaper
      class="summary-card"
      :elevation="0"
    >
      <span>{{ mode === 'accounting' ? 'Сальдо' : 'Чистый баланс' }}</span>
      <strong data-testid="debts-summary-net">{{ formatRubles(summary.net, mode !== 'accounting') }}</strong>
    </MudPaper>
  </div>

  <DebtChartsSection
    :accounts="chartAccounts"
    :filter-label="search.trim()"
    :today="today"
  />

  <div class="debt-page-filters">
    <MudTextField
      v-model="search"
      clearable
      data-testid="debts-search"
      immediate
      label="Поиск по человеку и истории"
    />
    <MudSelect
      v-model="sort"
      label="Сортировка"
    >
      <MudSelectItem value="name">
        По имени
      </MudSelectItem>
      <MudSelectItem value="balance">
        По величине остатка
      </MudSelectItem>
    </MudSelect>
  </div>

  <MudProgressCircular
    v-if="state.isLoading.value"
    aria-label="Загрузка отчёта"
    indeterminate
  />
  <MudAlert
    v-else-if="state.loadError.value"
    role="alert"
    severity="error"
  >
    {{ state.loadError.value }}
  </MudAlert>
  <MudText
    v-else-if="accounts.length === 0"
    data-testid="debts-empty"
  >
    Нет открытых расчётов
  </MudText>
  <template v-else>
    <DebtAccountPanel
      v-for="account in accounts"
      :key="account.name"
      :account="account"
      :chart-initially-open="targetOwner === account.name"
      :expanded="expandedNames.has(account.name)"
      :mode="mode"
      :today="today"
      @update:expanded="setAccountExpanded(account.name, $event)"
    />
  </template>
</template>
<style scoped>
.debt-page-header { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 16px; margin-bottom: 20px; }
.debt-view-modes { display: inline-flex; flex-wrap: wrap; padding: 4px; gap: 4px; margin: 0; border: 1px solid var(--mud-palette-divider); border-radius: 10px; }
.debt-view-modes label { position: relative; padding: 9px 14px; border-radius: 7px; cursor: pointer; font-size: 0.875rem; }
.debt-view-modes input { position: absolute; opacity: 0; width: 1px; height: 1px; }
.debt-view-modes label:focus-within { outline: 2px solid var(--mud-palette-primary); outline-offset: 2px; }
.debt-view-modes .active { background: var(--mud-palette-primary); color: var(--mud-palette-primary-text); }
.debt-page-actions { display: flex; align-items: center; flex-wrap: wrap; gap: 8px; margin-bottom: 22px; }
.debt-page-actions :deep(.mud-button-label) { text-transform: none; }
.debt-page-actions :deep(.debt-readonly-chip) { margin-left: auto; pointer-events: none; color: var(--mud-palette-text-secondary); border-color: var(--mud-palette-divider); }
.debt-report-note { margin: -12px 0 18px; }
.debt-summary-caption { margin-bottom: 8px; color: var(--mud-palette-text-secondary); font-size: 0.75rem; }
.debt-summary { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px; margin-bottom: 20px; }
.summary-card { display: flex; flex-direction: column; gap: 7px; padding: 14px 16px; border: 1px solid var(--mud-palette-divider); border-radius: 10px; }
.summary-card span { color: var(--mud-palette-text-secondary); font-size: 0.8125rem; }
.summary-card strong { font-size: 1.25rem; font-weight: 500; font-variant-numeric: tabular-nums; overflow-wrap: anywhere; }
.summary-receivable { color: color-mix(in srgb, var(--mud-palette-success) 60%, var(--mud-palette-text-primary)); }
.summary-payable { color: color-mix(in srgb, var(--mud-palette-warning) 60%, var(--mud-palette-text-primary)); }
.debt-page-filters { display: grid; grid-template-columns: minmax(0, 1fr) 220px; align-items: start; gap: 18px; margin-bottom: 22px; }
.sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); }
@media (max-width: 599.98px) {
  .debt-page-header { gap: 12px; margin-bottom: 16px; }
  .debt-page-header :deep(h1) { font-size: 1.75rem; }
  .debt-view-modes { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); width: 100%; }
  .debt-view-modes label { text-align: center; }
  .debt-page-actions { gap: 6px; margin-bottom: 16px; }
  .debt-page-actions :deep(.debt-readonly-chip) { margin-left: 0; }
  .debt-report-note { margin: -8px 0 14px; }
  .debt-summary { gap: 6px; margin-bottom: 14px; }
  .summary-card { padding: 10px 8px; gap: 6px; }
  .summary-card span { font-size: 0.6875rem; }
  .summary-card strong { font-size: 0.9375rem; }
  .debt-page-filters { grid-template-columns: minmax(0, 1fr); gap: 8px; margin-bottom: 16px; }
}
</style>
