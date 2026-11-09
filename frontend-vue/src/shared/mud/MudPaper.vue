<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  elevation?: number
  square?: boolean
  outlined?: boolean
  height?: string
  width?: string
  maxHeight?: string
  maxWidth?: string
  minHeight?: string
  minWidth?: string
}>(), {
  elevation: 1,
})

const classes = computed(() => [
  'mud-paper',
  {
    'mud-paper-outlined': props.outlined,
    'mud-paper-square': props.square,
    [`mud-elevation-${props.elevation}`]: !props.outlined,
  },
])

const styles = computed(() => ({
  height: props.height,
  width: props.width,
  'max-height': props.maxHeight,
  'max-width': props.maxWidth,
  'min-height': props.minHeight,
  'min-width': props.minWidth,
}))
</script>

<template>
  <div
    :class="classes"
    :style="styles"
  >
    <slot />
  </div>
</template>
