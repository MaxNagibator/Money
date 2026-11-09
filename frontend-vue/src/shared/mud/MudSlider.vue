<script setup lang="ts">
import { computed } from 'vue'
import type { MudColor, MudSize } from './types'

const props = withDefaults(defineProps<{
  min?: number
  max?: number
  step?: number
  disabled?: boolean
  color?: MudColor
  size?: MudSize
  tickMarks?: boolean
  valueLabel?: boolean
}>(), {
  min: 0,
  max: 100,
  step: 1,
  disabled: false,
  color: 'primary',
  size: 'small',
  tickMarks: false,
  valueLabel: false,
})

const model = defineModel<number>({ default: 0 })

const classes = computed(() => [
  'mud-slider',
  `mud-slider-${props.size}`,
  `mud-slider-${props.color}`,
])

const tickMarkCount = computed(() => props.tickMarks
  ? 1 + Math.trunc((props.max - props.min) / props.step)
  : 0)

const position = computed(() => {
  const result = 100 * (model.value - props.min) / (props.max - props.min)
  const clamped = Math.min(Math.max(0, result), 100)

  return Math.round(clamped * 100) / 100
})

const valueLabelPositionStyle = computed(() => ({
  left: `${position.value}%`,
}))
</script>

<template>
  <div :class="classes">
    <div class="mud-slider-container">
      <div
        v-if="tickMarks"
        class="mud-slider-inner-container"
      >
        <div class="mud-slider-tickmarks">
          <div
            v-for="index in tickMarkCount"
            :key="index"
            class="d-flex flex-column relative"
          >
            <span class="mud-slider-track-tick" />
          </div>
        </div>
      </div>

      <input
        v-model.number="model"
        type="range"
        class="mud-slider-input"
        aria-label="Ползунок"
        :min="min"
        :max="max"
        :step="step"
        :disabled="disabled"
      >
      <div
        v-if="valueLabel"
        class="mud-slider-value-label"
        :style="valueLabelPositionStyle"
      >
        {{ model }}
      </div>
    </div>
  </div>
</template>
