<script setup lang="ts">
import type { ChartData, ChartOptions, TooltipItem } from 'chart.js'
import { BarController, BarElement, CategoryScale, Chart, Legend, LinearScale, Tooltip } from 'chart.js'
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import { formatRubles } from '@/platform/format'
import { MudText } from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { DebtAccount } from '../model/debt-accounts'
import { buildMonthlyFlows, formatAxisRubles } from '../model/debt-charts'
import { useDebtChartPalette } from '../model/useDebtChartPalette'

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Legend, Tooltip)

const props = defineProps<{
  accounts: readonly DebtAccount[]
}>()

const palette = useDebtChartPalette()
const isNarrow = useNarrowViewport()

const flows = computed(() => buildMonthlyFlows(props.accounts))
const isEmpty = computed(() => flows.value.months.length === 0)
const containerStyle = computed(() => ({
  height: isNarrow.value ? '240px' : '290px',
}))

const chartData = computed<ChartData<'bar'>>(() => {
  const colors = palette.value

  return {
    labels: flows.value.months.map(month => month.label),
    datasets: [
      {
        label: 'Выдано и взято',
        backgroundColor: colors.issued,
        data: flows.value.months.map(month => month.issued.toDecimal().toNumber()),
        borderRadius: 4,
      },
      {
        label: 'Погашения',
        backgroundColor: colors.repaid,
        data: flows.value.months.map(month => month.repaid.toDecimal().toNumber()),
        borderRadius: 4,
      },
    ],
  }
})

const options = computed<ChartOptions<'bar'>>(() => {
  const colors = palette.value
  const narrow = isNarrow.value

  return {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        stacked: false,
        ticks: {
          color: colors.mutedText,
          autoSkip: true,
          maxTicksLimit: narrow ? 5 : 12,
          maxRotation: narrow ? 45 : 0,
        },
        grid: {
          color: colors.grid,
        },
      },
      y: {
        stacked: false,
        beginAtZero: true,
        ticks: {
          color: colors.mutedText,
          callback(value) {
            return formatAxisRubles(value)
          },
        },
        grid: {
          color: colors.grid,
        },
      },
    },
    plugins: {
      legend: {
        display: true,
        position: narrow ? 'bottom' : 'top',
        labels: {
          color: colors.text,
          boxWidth: narrow ? 12 : 16,
        },
      },
      tooltip: {
        callbacks: {
          title(items) {
            const index = items[0]?.dataIndex
            if (index === undefined) {
              return ''
            }

            return flows.value.months[index]?.fullLabel ?? ''
          },
          label(item: TooltipItem<'bar'>) {
            const bucket = flows.value.months[item.dataIndex]
            if (bucket === undefined) {
              return ''
            }

            const amount = item.datasetIndex === 0 ? bucket.issued : bucket.repaid
            const label = item.dataset.label ?? ''
            return `${label}: ${formatRubles(amount, true)}`
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
    Нет датированных операций по месяцам
  </MudText>
  <div
    v-else
    aria-label="Помесячная динамика новых долгов и погашений"
    class="debt-chart-canvas"
    role="img"
    :style="containerStyle"
  >
    <Bar
      :data="chartData"
      :options="options"
    />
  </div>
</template>

<style scoped>
.debt-chart-canvas {
  position: relative;
  width: 100%;
}
</style>
