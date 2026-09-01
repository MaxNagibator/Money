<script setup lang="ts">
import { computed } from 'vue'
import type { MudMargin, MudVariant } from './types'

const props = withDefaults(defineProps<{
  disabled?: boolean
  error?: boolean
  variant?: MudVariant
  margin?: MudMargin
  forId?: string | undefined
}>(), {
  disabled: false,
  error: false,
  variant: 'text',
  margin: 'none',
  forId: '',
})

const classes = computed(() => [
  'mud-input-label',
  'mud-input-label-animated',
  `mud-input-label-${props.variant}`,
  {
    [`mud-input-label-margin-${props.margin}`]: props.margin !== 'none',
    'mud-disabled': props.disabled,
    'mud-input-error': props.error,
  },
])
</script>

<template>
  <label
    :class="classes"
    :for="forId"
  >
    <slot />
  </label>
</template>
