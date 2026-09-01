<script setup lang="ts">
import type { ChartData } from 'chart.js'
import { BarController, BarElement, CategoryScale, Chart, Legend, LinearScale, Tooltip } from 'chart.js'
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import type { BarChartData } from '../model/bar-series'
import { buildBarOptions } from '../model/chart-options'
import { useChartPalette } from '../model/useChartPalette'

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Legend, Tooltip)

const props = defineProps<{ data: BarChartData }>()

const palette = useChartPalette()

const options = computed(() => buildBarOptions(palette.value))

const chartData = computed<ChartData<'bar'>>(() => ({
  labels: props.data.labels,
  datasets: props.data.series.map(series => ({
    label: series.label,
    backgroundColor: series.color,
    data: series.values,
  })),
}))
</script>

<template>
  <Bar
    :data="chartData"
    :options="options"
  />
</template>
