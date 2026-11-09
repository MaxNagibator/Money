<script setup lang="ts">
import { computed } from 'vue'
import MudIconButton from './MudIconButton.vue'
import type { MudColor, MudSize, MudVariant } from './types'

const props = withDefaults(defineProps<{
  icon?: string
  toggledIcon?: string
  color?: MudColor
  size?: MudSize
  variant?: MudVariant
  toggledVariant?: MudVariant
}>(), {
  color: 'default',
  size: 'medium',
  variant: 'text',
})

const toggled = defineModel<boolean>('toggled', { default: false })

const icon = computed(() => (toggled.value ? (props.toggledIcon ?? props.icon) : props.icon) ?? '')
const variant = computed(() => toggled.value ? (props.toggledVariant ?? props.variant) : props.variant)

const toggle = () => {
  toggled.value = !toggled.value
}
</script>

<template>
  <MudIconButton
    :icon="icon"
    :size="size"
    :color="color"
    :variant="variant"
    :aria-pressed="toggled"
    @click="toggle"
  />
</template>
