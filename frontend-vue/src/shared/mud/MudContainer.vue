<script setup lang="ts">
import type { MudMaxWidth } from './types'
import { computed } from 'vue'


const props = withDefaults(defineProps<{
  maxWidth?: MudMaxWidth
  gutters?: boolean
}>(), {
  maxWidth: 'lg',
  gutters: true,
})

const classes = computed(() => [
  'mud-container',
  `mud-container-maxwidth-${props.maxWidth}`,
  {
    'mud-container--gutters': props.gutters,
  },
])
</script>

<template>
  <div :class="classes">
    <slot />
  </div>
</template>
