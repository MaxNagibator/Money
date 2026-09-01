<script setup lang="ts">
import { computed, inject, onBeforeUnmount, onMounted, type ComputedRef, type InjectionKey } from 'vue'
import type { MudColor, MudSize } from './types'
import MudButton from './MudButton.vue'
import { mudToggleGroupContextKey } from './MudToggleGroup.vue'

interface MudToggleGroupContext {
  color: Readonly<ComputedRef<MudColor>>
  size: Readonly<ComputedRef<MudSize>>
  vertical: Readonly<ComputedRef<boolean>>
  delimiters: Readonly<ComputedRef<boolean>>
  ripple: Readonly<ComputedRef<boolean>>
  disabled: Readonly<ComputedRef<boolean>>
  registerItem: (item: unknown) => () => void
  isSelected: (value: unknown) => boolean
  toggleItem: (value: unknown) => void
}

const props = defineProps<{
  value?: unknown
}>()

const context = inject(mudToggleGroupContextKey as InjectionKey<MudToggleGroupContext>)
const item = {}
let unregister: (() => void) | undefined

onMounted(() => {
  if (context) {
    unregister = context.registerItem(item)
  }
})

onBeforeUnmount(() => unregister?.())

const selected = computed(() => context?.isSelected(props.value) ?? false)
const disabled = computed(() => context?.disabled.value ?? false)
const size = computed(() => context?.size.value ?? 'medium')
const color = computed(() => context?.color.value ?? 'primary')
const ripple = computed(() => context?.ripple.value ?? true)
const classes = computed(() => [
  'mud-toggle-item',
  {
    'mud-toggle-item-selected': selected.value,
    'mud-toggle-item-vertical': context?.vertical.value ?? false,
    'mud-toggle-item-delimiter': context?.delimiters.value ?? true,
    [`mud-toggle-item-size-${size.value}`]: true,
    'mud-ripple': ripple.value,
  },
  'mud-typography-input',
])
const variant = computed(() => selected.value ? 'filled' : 'outlined')
const displayValue = computed(() => {
  if (typeof props.value === 'string' || typeof props.value === 'number') {
    return String(props.value)
  }
  return ''
})

const toggle = () => {
  if (!disabled.value) {
    context?.toggleItem(props.value)
  }
}
</script>

<template>
  <MudButton
    :class="classes"
    :color="color"
    :disabled="disabled"
    :ripple="ripple"
    :size="size"
    :variant="variant"
    role="checkbox"
    :aria-checked="selected ? 'true' : 'false'"
    @click="toggle"
  >
    <div
      v-if="$slots.default"
      class="mud-toggle-item-content"
    >
      <slot :selected="selected" />
    </div>
    <template v-else>
      {{ displayValue }}
    </template>
  </MudButton>
</template>
