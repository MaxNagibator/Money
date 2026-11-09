<script setup lang="ts">
import { computed } from 'vue'
import type { MudColor, MudSize } from './types'

const props = withDefaults(defineProps<{
  icon?: string
  color?: MudColor | undefined
  size?: MudSize
}>(), {
  color: 'default',
  size: 'small',
})

const classes = [
  'mud-timeline-item',
  'mud-timeline-item-default',
]

const dotClasses = computed(() => [
  'mud-timeline-item-dot',
  `mud-timeline-dot-size-${props.size}`,
  'mud-elevation-1',
])

const dotInnerClasses = computed(() => [
  'mud-timeline-item-dot-inner',
  `mud-timeline-dot-${props.color}`,
])
</script>

<template>
  <div :class="classes">
    <div class="mud-timeline-item-content">
      <slot v-if="$slots.default" />
      <slot
        v-else
        name="ItemContent"
      />
    </div>
    <div class="mud-timeline-item-divider">
      <div :class="dotClasses">
        <div :class="dotInnerClasses" />
      </div>
    </div>
  </div>
</template>
