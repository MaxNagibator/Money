<script setup lang="ts">
import { computed, inject } from 'vue'

const contentPadding = inject<boolean>('mud-card-content-padding', true)

const classes = computed(() => [
  'mud-card-actions',
  {
    'mud-card-actions-padding': contentPadding,
  },
])
</script>

<template>
  <div :class="classes">
    <slot />
  </div>
</template>
