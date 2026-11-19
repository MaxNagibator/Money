<script setup lang="ts">
import type { ChartData, ChartOptions, TooltipItem } from 'chart.js'
import { BarController, BarElement, CategoryScale, Chart, LinearScale, Tooltip } from 'chart.js'
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import type { LocalDate } from '@/domain/local-date'
import { addDays } from '@/domain/local-date'
import { systemClock } from '@/platform/clock'
import { formatLocalDate, formatLocalDateDayMonth, formatRubles } from '@/platform/format'
import { MudText } from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { DebtAccount } from '../model/debt-accounts'
import type { DebtCycle } from '../model/debt-charts'
import { buildDebtCycles, countUndatedPayments } from '../model/debt-charts'
import { useDebtChartPalette } from '../model/useDebtChartPalette'

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip)

const props = defineProps<{
  accounts: readonly DebtAccount[]
  today?: LocalDate | undefined
}>()

const ROW_HEIGHT = 42
const BASE_HEIGHT = 64
const MIN_HEIGHT = 220
const SHORT_SPAN_MAX_DAYS = 180

const palette = useDebtChartPalette()
const isNarrow = useNarrowViewport()

const effectiveToday = computed(() => props.today ?? systemClock.today())
const cyclesResult = computed(() => buildDebtCycles(props.accounts, effectiveToday.value))
const undatedByOwner = computed(() => {
  const counts = new Map<string, number>()

  for (const account of props.accounts) {
    const count = countUndatedPayments([account])
    if (count > 0) {
      counts.set(account.name, (counts.get(account.name) ?? 0) + count)
    }
  }

  return counts
})
const isEmpty = computed(() => cyclesResult.value.cycles.length === 0)
const containerStyle = computed(() => ({
  height: `${Math.max(MIN_HEIGHT, cyclesResult.value.owners.length * ROW_HEIGHT + BASE_HEIGHT)}px`,
}))

const cycleSlots = computed<(DebtCycle | null)[][]>(() => {
  const { owners, cycles } = cyclesResult.value
  const byOwner = new Map<string, DebtCycle[]>()

  for (const cycle of cycles) {
    const list = byOwner.get(cycle.ownerName) ?? []
    list.push(cycle)
    byOwner.set(cycle.ownerName, list)
  }

  let maxSlots = 0
  for (const list of byOwner.values()) {
    if (list.length > maxSlots) {
      maxSlots = list.length
    }
  }

  const slots: (DebtCycle | null)[][] = []
  for (let slot = 0; slot < maxSlots; slot += 1) {
    slots.push(owners.map(ownerName => byOwner.get(ownerName)?.[slot] ?? null))
  }

  return slots
})

const chartData = computed<ChartData<'bar'>>(() => {
  const colors = palette.value
  const { owners } = cyclesResult.value

  return {
    labels: owners,
    datasets: cycleSlots.value.map((slotCycles, slotIndex) => ({
      label: `Цикл ${slotIndex + 1}`,
      grouped: false,
      barThickness: 18,
      borderRadius: 4,
      borderSkipped: false,
      data: slotCycles.map(cycle => (cycle === null ? null : [cycle.startOffsetDays, cycle.endOffsetDays])),
      backgroundColor: slotCycles.map(cycle => {
        if (cycle === null) {
          return 'transparent'
        }

        if (cycle.direction === 'receivable') {
          return cycle.status === 'active' ? colors.cycleReceivableActive : colors.cycleReceivableClosed
        }

        return cycle.status === 'active' ? colors.cyclePayableActive : colors.cyclePayableClosed
      }),
      borderColor: slotCycles.map(cycle => {
        if (cycle === null) {
          return 'transparent'
        }

        return cycle.direction === 'receivable' ? colors.receivable : colors.payable
      }),
      borderWidth: slotCycles.map(cycle => (cycle === null ? 0 : (cycle.status === 'closed' ? 1.5 : 1))),
    })),
  }
})

const options = computed<ChartOptions<'bar'>>(() => {
  const colors = palette.value
  const narrow = isNarrow.value
  const { minDate, totalDays } = cyclesResult.value

  return {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        type: 'linear',
        min: 0,
        max: totalDays,
        ticks: {
          color: colors.mutedText,
          autoSkip: true,
          autoSkipPadding: 20,
          includeBounds: false,
          maxTicksLimit: narrow ? 4 : 6,
          callback(value) {
            if (minDate === null) {
              return String(value)
            }

            const numeric = typeof value === 'number' ? value : Number(value)
            if (!Number.isFinite(numeric)) {
              return String(value)
            }

            const date = addDays(minDate, Math.round(numeric))
            return narrow || totalDays <= SHORT_SPAN_MAX_DAYS
              ? formatLocalDateDayMonth(date)
              : formatLocalDate(date)
          },
        },
        grid: {
          color: colors.grid,
        },
      },
      y: {
        type: 'category',
        ticks: {
          color: colors.text,
          autoSkip: false,
        },
        grid: {
          color: colors.grid,
        },
      },
    },
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        callbacks: {
          title(items) {
            const first = items[0]
            if (first === undefined) {
              return ''
            }

            const cycle = cycleSlots.value[first.datasetIndex]?.[first.dataIndex]
            return cycle?.ownerName ?? ''
          },
          label(item: TooltipItem<'bar'>) {
            const cycle = cycleSlots.value[item.datasetIndex]?.[item.dataIndex]
            if (cycle === undefined || cycle === null) {
              return ''
            }

            const directionLabel = cycle.direction === 'receivable' ? 'Мне должны' : 'Я должен'
            const statusLabel = cycle.status === 'active' ? 'Активный цикл' : 'Закрытый цикл'
            const lines = [
              `${directionLabel} (${statusLabel})`,
              `Период: с ${formatLocalDate(cycle.startDate)} по ${formatLocalDate(cycle.endDate)}`,
              `Длительность: ${cycle.durationDays} дн.`,
              `Пиковый остаток: ${formatRubles(cycle.peakAmount, true)}`,
              `Операций: ${cycle.eventsCount}`,
            ]

            const undatedCount = undatedByOwner.value.get(cycle.ownerName) ?? 0
            if (undatedCount > 0) {
              lines.push(`Есть оплаты без даты (${undatedCount}): фактический остаток может отличаться`)
            }

            return lines
          },
        },
      },
    },
  }
})
</script>

<template>
  <MudText
    v-if="isEmpty"
    align="center"
    class="py-6"
    color="secondary"
    html-tag="div"
  >
    Нет датированных циклов долгов
  </MudText>
  <div v-else>
    <div class="cycles-legend">
      <span class="legend-item">
        <span
          class="legend-swatch"
          :style="{ backgroundColor: palette.cycleReceivableActive, borderColor: palette.receivable }"
        />
        Мне должны (открыт)
      </span>
      <span class="legend-item">
        <span
          class="legend-swatch"
          :style="{ backgroundColor: palette.cycleReceivableClosed, borderColor: palette.receivable }"
        />
        Мне должны (закрыт)
      </span>
      <span class="legend-item">
        <span
          class="legend-swatch"
          :style="{ backgroundColor: palette.cyclePayableActive, borderColor: palette.payable }"
        />
        Я должен (открыт)
      </span>
      <span class="legend-item">
        <span
          class="legend-swatch"
          :style="{ backgroundColor: palette.cyclePayableClosed, borderColor: palette.payable }"
        />
        Я должен (закрыт)
      </span>
    </div>
    <div
      aria-label="Таймлайн циклов долгов по людям"
      class="debt-chart-canvas"
      role="img"
      :style="containerStyle"
    >
      <Bar
        :data="chartData"
        :options="options"
      />
    </div>
  </div>
</template>

<style scoped>
.cycles-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 12px 16px;
  justify-content: center;
  margin-bottom: 8px;
  font-size: 0.75rem;
  color: var(--mud-palette-text-secondary);
}

.legend-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.legend-swatch {
  width: 12px;
  height: 10px;
  border: 1.5px solid transparent;
  border-radius: 2px;
}

.debt-chart-canvas {
  position: relative;
  width: 100%;
}
</style>
