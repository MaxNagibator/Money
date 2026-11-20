<script setup lang="ts">
import type { ChartData, ChartOptions, Plugin, TooltipItem } from 'chart.js'
import {
  Chart,
  Filler,
  Legend,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Tooltip,
} from 'chart.js'
import { computed } from 'vue'
import { Line } from 'vue-chartjs'
import type { LocalDate } from '@/domain/local-date'
import { addDays } from '@/domain/local-date'
import { systemClock } from '@/platform/clock'
import { formatLocalDate, formatLocalDateDayMonth, formatRubles } from '@/platform/format'
import { MudText } from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { DebtAccount } from '../model/debt-accounts'
import type { OwnerTimelineMarkerKind } from '../model/debt-charts'
import { buildOwnerTimeline, buildPortfolioTimeline, diffDays, formatAxisRubles } from '../model/debt-charts'
import { useDebtChartPalette } from '../model/useDebtChartPalette'

Chart.register(
  LinearScale,
  PointElement,
  LineElement,
  LineController,
  Filler,
  Legend,
  Tooltip,
)

const props = defineProps<{
  accounts?: readonly DebtAccount[] | undefined
  account?: DebtAccount | undefined
  today?: LocalDate | undefined
  compact?: boolean | undefined
}>()

const palette = useDebtChartPalette()
const isNarrow = useNarrowViewport()

const effectiveToday = computed(() => props.today ?? systemClock.today())
const isSingleOwner = computed(() => props.account !== undefined)

const ownerTimeline = computed(() =>
  props.account === undefined ? null : buildOwnerTimeline(props.account, effectiveToday.value),
)

const portfolioTimeline = computed(() =>
  props.account === undefined ? buildPortfolioTimeline(props.accounts ?? [], effectiveToday.value) : null,
)

const undatedPaymentsCount = computed(() => ownerTimeline.value?.undatedPaymentsCount ?? 0)

const axisSpan = computed(() => {
  const points = ownerTimeline.value !== null
    ? ownerTimeline.value.points
    : (portfolioTimeline.value?.points ?? [])
  const first = points[0]
  const last = points.at(-1)
  if (first === undefined || last === undefined) {
    return { minDate: null, totalDays: 0 }
  }

  return {
    minDate: first.date,
    totalDays: Math.max(1, diffDays(first.date, last.date)),
  }
})

const isEmpty = computed(() => {
  if (ownerTimeline.value !== null) {
    return ownerTimeline.value.points.length === 0
  }

  return (portfolioTimeline.value?.points.length ?? 0) === 0
})

const ariaLabel = computed(() => {
  if (props.account !== undefined) {
    return `Динамика баланса по счёту ${props.account.name}`
  }

  return 'Динамика портфеля долгов во времени'
})

const containerStyle = computed(() => {
  if (props.compact) {
    return { height: isNarrow.value ? '180px' : '210px' }
  }

  return { height: isNarrow.value ? '240px' : '290px' }
})

const MARKER_LABELS: Record<OwnerTimelineMarkerKind, string> = {
  start: 'Начало расчётов',
  reborrowed: 'Повторный долг',
  settled: 'Полностью погашено',
  payment: 'Погашение',
  debt: 'Новый долг',
  today: 'На сегодня',
}

const zeroLinePlugin: Plugin<'line'> = {
  id: 'debtBalanceZeroLine',
  beforeDatasetsDraw(chart) {
    const yScale = chart.scales.y
    if (yScale === undefined || yScale.min > 0 || yScale.max < 0) {
      return
    }

    const yPixel = yScale.getPixelForValue(0)
    const { left, right } = chart.chartArea
    const { ctx } = chart

    ctx.save()
    ctx.beginPath()
    ctx.setLineDash([6, 4])
    ctx.lineWidth = 1
    ctx.strokeStyle = palette.value.zeroLine
    ctx.moveTo(left, yPixel)
    ctx.lineTo(right, yPixel)
    ctx.stroke()
    ctx.restore()
  },
}

const chartData = computed<ChartData<'line'>>(() => {
  const colors = palette.value
  const minDate = axisSpan.value.minDate

  if (ownerTimeline.value !== null) {
    const points = ownerTimeline.value.points

    return {
      datasets: [{
        label: 'Баланс',
        data: points.map(point => ({
          x: minDate === null ? 0 : diffDays(minDate, point.date),
          y: point.balanceAfter.toDecimal().toNumber(),
        })),
        stepped: 'before',
        borderColor: colors.net,
        backgroundColor: colors.net,
        borderWidth: 2,
        fill: false,
        pointRadius: points.map(point => markerRadius(point.markerKind)),
        pointHoverRadius: points.map(point => (point.markerKind === 'today' ? 4 : 7)),
        pointBackgroundColor: points.map(point => markerColor(point.markerKind, colors)),
        pointBorderColor: '#FFFFFF',
        pointBorderWidth: 1.5,
      }],
    }
  }

  const points = portfolioTimeline.value?.points ?? []
  const pointRadius = points.length > 30 ? 0 : (isNarrow.value ? 2 : 3)

  return {
    datasets: [
      {
        label: 'Мне должны',
        data: points.map(point => ({
          x: minDate === null ? 0 : diffDays(minDate, point.date),
          y: point.receivable.toDecimal().toNumber(),
        })),
        stepped: 'before',
        borderColor: colors.receivable,
        backgroundColor: colors.receivableFill,
        borderWidth: 2,
        fill: 'origin',
        pointRadius,
        pointHitRadius: 10,
        pointHoverRadius: 5,
      },
      {
        label: 'Я должен',
        data: points.map(point => ({
          x: minDate === null ? 0 : diffDays(minDate, point.date),
          y: point.payable.toDecimal().toNumber(),
        })),
        stepped: 'before',
        borderColor: colors.payable,
        backgroundColor: colors.payableFill,
        borderWidth: 2,
        fill: 'origin',
        pointRadius,
        pointHitRadius: 10,
        pointHoverRadius: 5,
      },
      {
        label: 'Чистый баланс',
        data: points.map(point => ({
          x: minDate === null ? 0 : diffDays(minDate, point.date),
          y: point.net.toDecimal().toNumber(),
        })),
        stepped: 'before',
        borderColor: colors.net,
        backgroundColor: colors.net,
        borderWidth: 2,
        borderDash: [5, 3],
        fill: false,
        pointRadius,
        pointHitRadius: 10,
        pointHoverRadius: 5,
      },
    ],
  }
})

const options = computed<ChartOptions<'line'>>(() => {
  const colors = palette.value
  const narrow = isNarrow.value
  const single = isSingleOwner.value
  const { minDate, totalDays } = axisSpan.value

  return {
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
      mode: 'index',
      intersect: false,
    },
    scales: {
      x: {
        type: 'linear',
        min: 0,
        max: totalDays,
        ticks: {
          color: colors.mutedText,
          precision: 0,
          autoSkip: true,
          autoSkipPadding: 20,
          includeBounds: false,
          maxTicksLimit: narrow ? 4 : 8,
          maxRotation: narrow ? 45 : 0,
          callback(value) {
            if (minDate === null) {
              return String(value)
            }

            const numeric = typeof value === 'number' ? value : Number(value)
            if (!Number.isFinite(numeric)) {
              return String(value)
            }

            return formatTickDate(addDays(minDate, Math.round(numeric)), narrow)
          },
        },
        grid: {
          color: colors.grid,
        },
      },
      y: {
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
        display: !single,
        position: narrow ? 'bottom' : 'top',
        labels: {
          color: colors.text,
          boxWidth: narrow ? 12 : 18,
        },
      },
      tooltip: {
        callbacks: {
          title(items) {
            const index = items[0]?.dataIndex
            if (index === undefined) {
              return ''
            }

            if (ownerTimeline.value !== null) {
              const point = ownerTimeline.value.points[index]
              return point === undefined ? '' : formatLocalDate(point.date)
            }

            const point = portfolioTimeline.value?.points[index]
            return point === undefined ? '' : formatLocalDate(point.date)
          },
          label(item: TooltipItem<'line'>) {
            if (ownerTimeline.value !== null) {
              const point = ownerTimeline.value.points[item.dataIndex]
              if (point === undefined) {
                return ''
              }

              const lines = [
                MARKER_LABELS[point.markerKind],
                `Остаток: ${formatRubles(point.balanceAfter, true)}`,
              ]

              if (!point.delta.isZero) {
                const sign = point.delta.isNegative ? '' : '+'
                lines.splice(1, 0, `Изменение: ${sign}${formatRubles(point.delta, true)}`)
              }

              if (point.comments.length > 0) {
                lines.push(...point.comments)
              }

              return lines
            }

            const point = portfolioTimeline.value?.points[item.dataIndex]
            if (point === undefined) {
              return ''
            }

            if (item.datasetIndex === 0) {
              return `Мне должны: ${formatRubles(point.receivable, true)}`
            }
            if (item.datasetIndex === 1) {
              return `Я должен: ${formatRubles(point.payable, true)}`
            }

            return `Чистый баланс: ${formatRubles(point.net, true)}`
          },
        },
      },
    },
  }
})

function formatTickDate(date: LocalDate, narrow: boolean): string {
  return narrow ? formatLocalDateDayMonth(date) : formatLocalDate(date)
}

function markerRadius(kind: OwnerTimelineMarkerKind): number {
  switch (kind) {
    case 'settled':
    case 'start':
    case 'reborrowed':
      return 6
    case 'today':
      return 2
    case 'payment':
    case 'debt':
      return 3.5
  }
}

function markerColor(
  kind: OwnerTimelineMarkerKind,
  colors: ReturnType<typeof useDebtChartPalette>['value'],
): string {
  switch (kind) {
    case 'settled':
    case 'payment':
      return colors.receivable
    case 'reborrowed':
    case 'debt':
      return colors.payable
    case 'start':
      return colors.net
    case 'today':
      return colors.mutedText
  }
}
</script>

<template>
  <MudText
    v-if="isEmpty"
    align="center"
    class="py-6"
    color="secondary"
    html-tag="div"
  >
    Нет датированных операций для построения графика
  </MudText>
  <div v-else>
    <div
      :aria-label="ariaLabel"
      class="debt-chart-canvas"
      role="img"
      :style="containerStyle"
    >
      <Line
        :data="chartData"
        :options="options"
        :plugins="[zeroLinePlugin]"
      />
    </div>
    <p
      v-if="isSingleOwner && undatedPaymentsCount > 0"
      class="debt-chart-undated-note"
      data-testid="debt-account-chart-undated-note"
    >
      Оплаты без даты ({{ undatedPaymentsCount }}) не включены в график: остаток по графику может отличаться от остатка карточки.
    </p>
  </div>
</template>

<style scoped>
.debt-chart-canvas {
  position: relative;
  width: 100%;
}

.debt-chart-undated-note {
  margin: 8px 0 0;
  color: var(--mud-palette-text-secondary);
  font-size: 0.75rem;
}
</style>
