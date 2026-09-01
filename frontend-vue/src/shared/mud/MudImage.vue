<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  fluid?: boolean
  src?: string
  alt?: string
  height?: number
  width?: number
}>()

const classes = computed(() => [
  'mud-image',
  {
    fluid: props.fluid,
  },
  'object-fill',
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
