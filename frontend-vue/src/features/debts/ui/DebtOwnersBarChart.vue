<script setup lang="ts">
import type { ChartData, ChartOptions, Plugin, TooltipItem } from 'chart.js'
import { BarController, BarElement, CategoryScale, Chart, LinearScale, Tooltip } from 'chart.js'
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import { formatRubles } from '@/platform/format'
import { MudText } from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { DebtAccount } from '../model/debt-accounts'
import { buildOwnersBreakdown, formatAxisRubles } from '../model/debt-charts'
import { useDebtChartPalette } from '../model/useDebtChartPalette'

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip)

const props = defineProps<{
  accounts: readonly DebtAccount[]
}>()

const ROW_HEIGHT = 36
const BASE_HEIGHT = 56
const MIN_HEIGHT = 220

const palette = useDebtChartPalette()
const isNarrow = useNarrowViewport()

const breakdown = computed(() => buildOwnersBreakdown(props.accounts))
const isEmpty = computed(() => breakdown.value.labels.length === 0)
const containerStyle = computed(() => ({
  height: `${Math.max(MIN_HEIGHT, breakdown.value.labels.length * ROW_HEIGHT + BASE_HEIGHT)}px`,
}))

const zeroLinePlugin: Plugin<'bar'> = {
  id: 'debtOwnersZeroLine',
  beforeDatasetsDraw(chart) {
    const xScale = chart.scales.x
    if (xScale === undefined || xScale.min > 0 || xScale.max < 0) {
      return
    }

    const xPixel = xScale.getPixelForValue(0)
    const { top, bottom } = chart.chartArea
    const { ctx } = chart

    ctx.save()
    ctx.beginPath()
    ctx.setLineDash([6, 4])
    ctx.lineWidth = 1
    ctx.strokeStyle = palette.value.zeroLine
    ctx.moveTo(xPixel, top)
    ctx.lineTo(xPixel, bottom)
    ctx.stroke()
    ctx.restore()
  },
}

const chartData = computed<ChartData<'bar'>>(() => {
  const colors = palette.value

  return {
    labels: breakdown.value.labels,
    datasets: [{
      label: 'Текущий баланс',
      data: breakdown.value.values,
      backgroundColor: breakdown.value.directions.map(
        direction => (direction === 'receivable' ? colors.receivable : colors.payable),
      ),
      borderRadius: 4,
    }],
  }
})

const options = computed<ChartOptions<'bar'>>(() => {
  const colors = palette.value
  const narrow = isNarrow.value

  return {
    indexAxis: 'y',
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        ticks: {
          color: colors.mutedText,
          autoSkip: true,
          maxTicksLimit: narrow ? 4 : 7,
          callback(value) {
            return formatAxisRubles(value)
          },
        },
        grid: {
          color: colors.grid,
        },
      },
      y: {
        ticks: {
          color: colors.text,
          autoSkip: false,
        },
        grid: {
          display: false,
        },
      },
    },
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        callbacks: {
          label(item: TooltipItem<'bar'>) {
            const balance = breakdown.value.balances[item.dataIndex]
            const direction = breakdown.value.directions[item.dataIndex]
            if (balance === undefined || direction === undefined) {
              return ''
            }

            const magnitude = balance.isNegative ? balance.negate() : balance
            const prefix = direction === 'receivable' ? 'Мне должны: ' : 'Я должен: '
            return prefix + formatRubles(magnitude, true)
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
    Нет открытых счетов с ненулевым балансом
  </MudText>
  <div v-else>
    <div class="owners-legend">
      <span class="legend-item">
        <span
          class="legend-swatch"
          :style="{ backgroundColor: palette.receivable }"
        />
        Мне должны
      </span>
      <span class="legend-item">
        <span
          class="legend-swatch"
          :style="{ backgroundColor: palette.payable }"
        />
        Я должен
      </span>
    </div>
    <div
      aria-label="Текущие остатки долгов по людям"
      class="debt-chart-canvas"
      role="img"
      :style="containerStyle"
    >
      <Bar
        :data="chartData"
        :options="options"
        :plugins="[zeroLinePlugin]"
      />
    </div>
  </div>
</template>

<style scoped>
.owners-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
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
  width: 10px;
  height: 10px;
  border-radius: 2px;
}

.debt-chart-canvas {
  position: relative;
  width: 100%;
}
</style>
