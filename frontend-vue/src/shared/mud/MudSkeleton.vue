<script setup lang="ts">
import { computed } from 'vue'
import type { MudAnimation, MudSkeletonType } from './types'

const props = withDefaults(defineProps<{
  skeletonType?: MudSkeletonType
  animation?: MudAnimation
  width?: string
  height?: string
}>(), {
  skeletonType: 'text',
  animation: 'pulse',
})

const classes = computed(() => [
  'mud-skeleton',
  `mud-skeleton-${props.skeletonType}`,
  `mud-skeleton-${props.animation}`,
])

const styles = computed(() => ({
  height: props.height,
  width: props.width,
}))
</script>

<template>
  <span
    :class="classes"
    :style="styles"
  />
</template>
