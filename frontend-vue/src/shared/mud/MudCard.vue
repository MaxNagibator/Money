<script setup lang="ts">
import { provide } from 'vue'
import MudPaper from './MudPaper.vue'

withDefaults(defineProps<{
  elevation?: number
  outlined?: boolean
}>(), {
  elevation: 1,
})

provide('mud-card-content-padding', true)
</script>

<template>
  <MudPaper
    class="mud-card"
    :elevation="elevation"
    :outlined="outlined"
  >
    <slot />
  </MudPaper>
</template>
