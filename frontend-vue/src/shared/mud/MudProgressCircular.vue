<script setup lang="ts">
import { computed } from 'vue'
import type { MudColor, MudSize } from './types'

const props = withDefaults(defineProps<{
  color?: MudColor
  size?: MudSize
  indeterminate?: boolean
}>(), {
  color: 'default',
  size: 'medium',
})

const value = 0
const min = 0
const max = 100
const strokeWidth = 3
const magicNumber = 126
const viewBox = '22.5 22.5 43 43'

const svgValue = computed(() => {
  const minValue = Math.min(Math.max(min, value), max)
  const fraction = (minValue - min) / (max - min)

  return Math.round(magicNumber - magicNumber * fraction)
})

const classes = computed(() => [
  'mud-progress-circular',
  `mud-${props.color}-text`,
  `mud-progress-${props.size}`,
  {
    'mud-progress-indeterminate': props.indeterminate,
    'mud-progress-static': !props.indeterminate,
  },
])

const svgClasses = computed(() => [
  'mud-progress-circular-circle',
  {
    'mud-progress-indeterminate': props.indeterminate,
    'mud-progress-static': !props.indeterminate,
  },
])

const ariaBusy = computed(() => props.indeterminate || value < max)
const ariaLive = computed(() => (props.indeterminate ? undefined : 'polite'))
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
    <template v-if="indeterminate">
      <svg
        class="mud-progress-circular-svg"
        :viewBox="viewBox"
      >
        <circle
          :class="svgClasses"
          cx="44"
          cy="44"
          r="20"
          fill="none"
          :stroke-width="strokeWidth"
        />
      </svg>

      <div class="mud-progress-circular-percentage mud-progress-circular-indeterminate-child">
        <slot />
      </div>
    </template>
    <template v-else>
      <svg
        class="mud-progress-circular-svg"
        :viewBox="viewBox"
      >
        <circle
          :class="svgClasses"
          cx="44"
          cy="44"
          r="20"
          fill="none"
          :stroke-width="strokeWidth"
          :style="`stroke-dasharray: ${magicNumber}; stroke-dashoffset: ${svgValue};`"
        />
      </svg>

      <div class="mud-progress-circular-percentage">
        <slot />
      </div>
    </template>
  </div>
</template>
