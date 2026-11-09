<script setup lang="ts">
import { computed } from 'vue'
import type { MudTimelineOrientation, MudTimelinePosition } from './types'


const props = withDefaults(defineProps<{
  timelineOrientation?: MudTimelineOrientation
  timelinePosition?: MudTimelinePosition
  reverse?: boolean
}>(), {
  timelineOrientation: 'vertical',
  timelinePosition: 'alternate',
  reverse: false,
})

const classes = computed(() => [
  'mud-timeline',
  `mud-timeline-${props.timelineOrientation}`,
  `mud-timeline-position-${props.timelinePosition}`,
  {
    'mud-timeline-reverse': props.reverse && props.timelinePosition === 'alternate',
  },
  'mud-timeline-align-default',
  'mud-timeline-modifiers',
])
</script>

<template>
  <div :class="classes">
    <slot />
  </div>
</template>
