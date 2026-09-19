<script setup lang="ts">
import { computed } from 'vue'
import MudToolBar from './MudToolBar.vue'

const props = withDefaults(defineProps<{
  elevation?: number
  bottom?: boolean
}>(), {
  elevation: 4,
  bottom: false,
})

const classes = computed(() => [
  'mud-appbar',
  props.bottom ? 'mud-appbar-fixed-bottom' : 'mud-appbar-fixed-top',
  `mud-elevation-${props.elevation}`,
])
</script>

<template>
  <component
    :is="bottom ? 'footer' : 'header'"
    :class="classes"
  >
    <MudToolBar class="mud-toolbar-appbar">
      <slot />
    </MudToolBar>
  </component>
</template>
