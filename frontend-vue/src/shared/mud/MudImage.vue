<script setup lang="ts">
import { computed } from 'vue'
import type { MudObjectFit } from './types'

const props = withDefaults(defineProps<{
  fluid?: boolean
  src?: string
  alt?: string
  height?: number
  width?: number
  objectFit?: MudObjectFit
}>(), {
  objectFit: 'fill',
})

const classes = computed(() => [
  'mud-image',
  {
    fluid: props.fluid,
  },
  `object-${props.objectFit}`,
  'object-center',
])
</script>

<template>
  <img
    :src="src"
    :alt="alt"
    :class="classes"
    :width="width"
    :height="height"
  >
</template>
