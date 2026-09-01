<script setup lang="ts">
import { computed } from 'vue'
import MudToolBar from './MudToolBar.vue'

const props = withDefaults(defineProps<{
  elevation?: number
}>(), {
  elevation: 4,
})

const classes = computed(() => [
  'mud-appbar',
  'mud-appbar-fixed-top',
  `mud-elevation-${props.elevation}`,
])
</script>

<template>
  <header :class="classes">
    <MudToolBar class="mud-toolbar-appbar">
      <slot />
    </MudToolBar>
  </header>
</template>
