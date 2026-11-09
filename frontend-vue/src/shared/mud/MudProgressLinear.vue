<script setup lang="ts">
import { computed } from 'vue'
import type { MudColor } from './types'

const props = withDefaults(defineProps<{
  color?: MudColor
  indeterminate?: boolean
  value?: number
  bufferValue?: number
}>(), {
  color: 'default',
})

const min = 0
const max = 100
const buffer = false

const value = computed(() => props.value ?? 0)
const bufferValue = computed(() => props.bufferValue ?? 0)

const getPercentage = (input: number) => {
  const total = Math.abs(max - min)
  if (total === 0) {
    return 0
  }

  const value = Math.max(0, Math.min(total, input - min))

  return value / total * 100
}

const valuePercent = computed(() => getPercentage(value.value))
const bufferPercent = computed(() => getPercentage(bufferValue.value))

const roundToEven = (input: number) => {
  const floor = Math.floor(input)
  const fraction = input - floor

  if (fraction < 0.5) {
    return floor
  }

  if (fraction > 0.5) {
    return floor + 1
  }

  return floor % 2 === 0 ? floor : floor + 1
}

const getStyledBarTransform = (input: number) =>
  `transform: translateX(-${roundToEven(100 - input)}%);`

const classes = computed(() => [
  'mud-progress-linear',
  {
    'mud-progress-indeterminate': props.indeterminate,
    'mud-progress-linear-buffer': buffer && !props.indeterminate,
  },
  'mud-progress-linear-small',
  `mud-progress-linear-color-${props.color}`,
  'mud-progress-linear-background',
  'horizontal',
  'mud-flip-x-rtl',
])

const ariaBusy = computed(() => props.indeterminate || value.value < max)
const ariaLive = computed(() => (props.indeterminate ? undefined : 'polite'))
const valueBarStyle = computed(() => getStyledBarTransform(valuePercent.value))
const bufferBarStyle = computed(() => getStyledBarTransform(bufferPercent.value))
</script>

<template>
  <div
    role="progressbar"
    :aria-busy="ariaBusy"
    :aria-valuenow="value"
    :aria-valuemin="min"
    :aria-valuemax="max"
    :aria-live="ariaLive"
    :class="classes"
  >
    <div class="mud-progress-linear-bars">
      <template v-if="indeterminate">
        <div class="mud-progress-linear-bar" />
        <div class="mud-progress-linear-bar" />
      </template>
      <template v-else-if="buffer">
        <div class="mud-progress-linear-bar" />
        <div
          class="mud-progress-linear-bar"
          :style="valueBarStyle"
        />
        <div
          class="mud-progress-linear-bar last"
          :style="bufferBarStyle"
        />
      </template>
      <template v-else>
        <div
          class="mud-progress-linear-bar"
          :style="valueBarStyle"
        />
      </template>
    </div>

    <div
      v-if="$slots.default"
      class="mud-progress-linear-content"
    >
      <slot />
    </div>
  </div>
</template>
