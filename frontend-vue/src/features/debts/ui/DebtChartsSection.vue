<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { mdExpandLess, mdExpandMore } from '@/shared/icons'
import { MudButton, MudIconButton, MudPaper, MudText } from '@/shared/mud'
import type { DebtAccount } from '../model/debt-accounts'
import { countUndatedPayments } from '../model/debt-charts'
import DebtBalanceTimelineChart from './DebtBalanceTimelineChart.vue'
import DebtCyclesTimelineChart from './DebtCyclesTimelineChart.vue'
import DebtMonthlyFlowChart from './DebtMonthlyFlowChart.vue'
import DebtOwnersBarChart from './DebtOwnersBarChart.vue'

type ChartTabId = 'balance' | 'monthly' | 'owners' | 'cycles'

interface ChartTab {
  id: ChartTabId
  label: string
  testId: string
}

const CHART_TABS: readonly ChartTab[] = [
  { id: 'balance', label: 'Баланс во времени', testId: 'debts-chart-tab-balance' },
  { id: 'monthly', label: 'Помесячно', testId: 'debts-chart-tab-monthly' },
  { id: 'owners', label: 'По людям', testId: 'debts-chart-tab-owners' },
  { id: 'cycles', label: 'Циклы долгов', testId: 'debts-chart-tab-cycles' },
]

const props = defineProps<{
  accounts: readonly DebtAccount[]
  today?: LocalDate | undefined
  filterLabel?: string | undefined
}>()

const activeTab = ref<ChartTabId>('balance')
const userToggled = ref(false)
const trimmedFilter = computed(() => props.filterLabel?.trim() ?? '')

const hasDatedMovements = computed(() =>
  props.accounts.some(account =>
    account.movements.some(movement => !movement.source.isDeleted && movement.date !== null),
  ),
)

const isExpanded = ref(hasDatedMovements.value)

watch(hasDatedMovements, value => {
  if (!userToggled.value && value) {
    isExpanded.value = true
  }
})

const undatedPaymentsCount = computed(() => countUndatedPayments(props.accounts))
const showUndatedNote = computed(() => activeTab.value !== 'owners' && undatedPaymentsCount.value > 0)

function toggle(): void {
  userToggled.value = true
  isExpanded.value = !isExpanded.value
}
</script>

<template>
  <MudPaper
    class="debts-charts-section"
    data-testid="debts-charts-section"
    :elevation="1"
  >
    <div class="charts-header">
      <div class="charts-title-group">
        <MudText
          html-tag="h2"
          typo="h6"
        >
          Графики истории
        </MudText>
        <span
          v-if="trimmedFilter.length > 0"
          class="charts-filter-label"
          data-testid="debts-charts-filter-label"
        >По фильтру: «{{ trimmedFilter }}»</span>
      </div>
      <MudIconButton
        :aria-expanded="isExpanded"
        :aria-label="isExpanded ? 'Свернуть графики' : 'Развернуть графики'"
        data-testid="debts-charts-toggle"
        :icon="isExpanded ? mdExpandLess : mdExpandMore"
        @click="toggle"
      />
    </div>

    <div
      v-if="isExpanded"
      class="charts-body"
    >
      <div
        aria-label="Выбор графика"
        class="debt-charts__tabs"
        role="group"
      >
        <MudButton
          v-for="tab in CHART_TABS"
          :key="tab.id"
          :aria-pressed="activeTab === tab.id"
          :color="activeTab === tab.id ? 'primary' : 'default'"
          :data-testid="tab.testId"
          size="small"
          :variant="activeTab === tab.id ? 'filled' : 'outlined'"
          @click="activeTab = tab.id"
        >
          {{ tab.label }}
        </MudButton>
      </div>

      <div class="charts-panel">
        <DebtBalanceTimelineChart
          v-if="activeTab === 'balance'"
          :accounts="accounts"
          :today="today"
        />
        <DebtMonthlyFlowChart
          v-else-if="activeTab === 'monthly'"
          :accounts="accounts"
        />
        <DebtOwnersBarChart
          v-else-if="activeTab === 'owners'"
          :accounts="accounts"
        />
        <DebtCyclesTimelineChart
          v-else
          :accounts="accounts"
          :today="today"
        />
      </div>

      <MudText
        v-if="showUndatedNote"
        class="charts-undated-note"
        color="secondary"
        data-testid="debts-charts-undated-note"
        html-tag="p"
        typo="caption"
      >
        Оплаты без даты ({{ undatedPaymentsCount }}) не включены во временные графики.
      </MudText>
    </div>
  </MudPaper>
</template>

<style scoped>
.debts-charts-section {
  margin-bottom: 20px;
}

.charts-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 18px;
}

.charts-title-group {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 4px 10px;
  min-width: 0;
}

.charts-filter-label {
  color: var(--mud-palette-text-secondary);
  font-size: 0.75rem;
  overflow-wrap: anywhere;
}

.charts-body {
  padding: 0 18px 16px;
}

.debt-charts__tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 14px;
}

.debt-charts__tabs :deep(.mud-button-label) {
  text-transform: none;
}

.charts-undated-note {
  margin: 10px 0 0;
}

@media (max-width: 599.98px) {
  .debts-charts-section {
    margin-bottom: 14px;
  }

  .charts-header {
    padding: 8px 12px;
  }

  .charts-body {
    padding: 0 12px 12px;
  }

  .debt-charts__tabs {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 6px;
  }
}
</style>
