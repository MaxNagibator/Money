<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  dense?: boolean
  hover?: boolean
}>()

const classes = computed(() => [
  'mud-table mud-simple-table',
  {
    'mud-table-dense': props.dense,
    'mud-table-hover': props.hover,
  },
  'mud-elevation-1',
])
</script>

<template>
  <div :class="classes">
    <div class="mud-table-container">
      <table>
        <slot />
      </table>
    </div>
  </div>
</template>
