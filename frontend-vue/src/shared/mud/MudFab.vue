<script setup lang="ts">
import { computed } from 'vue'
import MudIcon from './MudIcon.vue'
import type { MudColor, MudSize } from './types'

const props = withDefaults(defineProps<{
  color?: MudColor
  size?: MudSize
  startIcon?: string
  disabled?: boolean
}>(), {
  color: 'default',
  size: 'large',
})

const classes = computed(() => [
  'mud-button-root mud-fab',
  `mud-fab-${props.color}`,
  `mud-fab-size-${props.size}`,
  {
    'mud-ripple': !props.disabled,
  },
])
</script>

<template>
  <button
    type="button"
    :disabled="disabled"
    :class="classes"
  >
    <span class="mud-fab-label">
      <MudIcon
        v-if="startIcon?.trim()"
        :icon="startIcon ?? ''"
        color="inherit"
        size="medium"
        :disabled="disabled"
      />
    </span>
  </button>
</template>
