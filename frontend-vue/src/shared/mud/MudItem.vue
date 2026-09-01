<script setup lang="ts">
import { computed } from 'vue'
import type { MudBreakpoint } from './types'

const props = withDefaults(defineProps<{
  xs?: number
  sm?: number
  md?: number
  lg?: number
  xl?: number
  xxl?: number
}>(), {
  xs: 0,
  sm: 0,
  md: 0,
  lg: 0,
  xl: 0,
  xxl: 0,
})

const BREAKPOINTS: readonly MudBreakpoint[] = ['xs', 'sm', 'md', 'lg', 'xl', 'xxl']

const classes = computed(() => [
  'mud-grid-item',
  ...BREAKPOINTS.filter(name => props[name] !== 0).map(name => `mud-grid-item-${name}-${props[name]}`),
])
</script>

<template>
  <div :class="classes">
    <slot />
  </div>
</template>
