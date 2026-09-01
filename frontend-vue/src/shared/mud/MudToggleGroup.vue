<script lang="ts">
import type { ComputedRef, InjectionKey, Ref } from 'vue'
import type { MudColor, MudSelectionMode, MudSize } from './types'


export interface MudToggleGroupContext {
  color: Readonly<ComputedRef<MudColor>>
  size: Readonly<ComputedRef<MudSize>>
  vertical: Readonly<ComputedRef<boolean>>
  delimiters: Readonly<ComputedRef<boolean>>
  ripple: Readonly<ComputedRef<boolean>>
  disabled: Readonly<ComputedRef<boolean>>
  items: Readonly<Ref<unknown[]>>
  registerItem: (item: unknown) => () => void
  isSelected: (value: unknown) => boolean
  toggleItem: (value: unknown) => void
}

export const mudToggleGroupContextKey: InjectionKey<MudToggleGroupContext> = Symbol('mud-toggle-group')
</script>

<script setup lang="ts">
import { computed, provide, shallowRef } from 'vue'

const props = withDefaults(defineProps<{
  color?: MudColor
  selectionMode?: MudSelectionMode
}>(), {
  color: 'primary',
  selectionMode: 'single-selection',
})

const model = defineModel<unknown>()
const items = shallowRef<unknown[]>([])
const size = computed(() => 'medium' as const)
const vertical = computed(() => false)
const delimiters = computed(() => true)
const ripple = computed(() => true)
const disabled = computed(() => false)

const classes = computed(() => [
  'mud-toggle-group',
  'mud-toggle-group-horizontal',
  'mud-toggle-group-size-medium',
  `mud-toggle-group-${props.color}`,
  'mud-toggle-group-outlined',
])

const styles = computed(() => ({
  'grid-template-columns': `repeat(${items.value.length}, minmax(0, 1fr))`,
}))

const registerItem = (item: unknown) => {
  items.value = [...items.value, item]

  return () => {
    items.value = items.value.filter(registeredItem => registeredItem !== item)
  }
}

const isSelected = (value: unknown) => Object.is(model.value, value)

const toggleItem = (value: unknown) => {
  if (props.selectionMode === 'toggle-selection' && isSelected(value)) {
    model.value = null
  } else {
    model.value = value
  }
}

const context: MudToggleGroupContext = {
  color: computed(() => props.color),
  size,
  vertical,
  delimiters,
  ripple,
  disabled,
  items,
  registerItem,
  isSelected,
  toggleItem,
}

provide(mudToggleGroupContextKey, context)
</script>

<template>
  <div
    :class="classes"
    :style="styles"
    role="group"
  >
    <slot />
  </div>
</template>
