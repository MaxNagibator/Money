<script setup lang="ts">
import { computed } from 'vue'
import type { MudJustify } from './types'

const props = withDefaults(defineProps<{
  spacing?: number
  justify?: MudJustify
}>(), {
  spacing: 6,
  justify: 'start',
})

const classes = computed(() => ['mud-grid', `mud-grid-spacing-xs-${props.spacing}`, `justify-${props.justify}`])
</script>

<template>
  <div :class="classes">
    <slot />
  </div>
</template>
