<script setup lang="ts">
import type { ChartData } from 'chart.js'
import { ArcElement, Chart, Legend, PieController, Tooltip } from 'chart.js'
import { computed } from 'vue'
import { Pie } from 'vue-chartjs'
import type { PieSlice } from '../model/category-sums'
import { buildPieOptions } from '../model/chart-options'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import { useChartPalette } from '../model/useChartPalette'

Chart.register(ArcElement, PieController, Legend, Tooltip)

const props = defineProps<{ slices: readonly PieSlice[] }>()

const palette = useChartPalette()
const isNarrow = useNarrowViewport()

const options = computed(() => buildPieOptions(palette.value, isNarrow.value))

const chartData = computed<ChartData<'pie'>>(() => ({
  labels: props.slices.map(slice => slice.label),
  datasets: [{
    data: props.slices.map(slice => slice.value),
    backgroundColor: props.slices.map(slice => slice.color),
  }],
}))
</script>

<template>
  <Pie
    :data="chartData"
    :options="options"
  />
</template>
