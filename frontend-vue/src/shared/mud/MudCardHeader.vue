<script setup lang="ts">
import { computed, inject } from 'vue'

const contentPadding = inject<boolean>('mud-card-content-padding', true)

const classes = computed(() => [
  'mud-card-header',
  {
    'mud-card-header-padding': contentPadding,
  },
])
</script>

<template>
  <div :class="classes">
    <div
      v-if="$slots['card-header-avatar']"
      class="mud-card-header-avatar"
    >
      <slot name="card-header-avatar" />
    </div>
    <div
      v-if="$slots['card-header-content']"
      class="mud-card-header-content"
    >
      <slot name="card-header-content" />
    </div>
    <div
      v-if="$slots['card-header-actions']"
      class="mud-card-header-actions"
    >
      <slot name="card-header-actions" />
    </div>
    <slot />
  </div>
</template>
