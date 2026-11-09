<script setup lang="ts">
import { computed } from 'vue'
import MudToolBar from './MudToolBar.vue'
import type { MudColor } from './types'

const props = withDefaults(defineProps<{
  color?: MudColor
}>(), {
  color: 'default',
})

const classes = computed(() => [
  'mud-picker-toolbar',
  `mud-theme-${props.color}`,
])
</script>

<template>
  <MudToolBar
    :class="classes"
  >
    <slot />
  </MudToolBar>
</template>
