<script setup lang="ts">
import { provide, toRef } from 'vue'

const props = withDefaults(defineProps<{
  dense?: boolean
}>(), {
  dense: false,
})

provide('mud-list-context', {
  dense: toRef(props, 'dense'),
})
</script>

<template>
  <div class="mud-list">
    <slot />
  </div>
</template>
