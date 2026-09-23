<script lang="ts">
import type { ComputedRef, InjectionKey, Ref } from 'vue'
import type { MudColor, MudSelectionMode, MudSize } from './types'


export interface MudToggleItemRegistration {
  value: () => unknown
}

export interface MudToggleGroupContext {
  color: Readonly<ComputedRef<MudColor>>
  size: Readonly<ComputedRef<MudSize>>
  vertical: Readonly<ComputedRef<boolean>>
  delimiters: Readonly<ComputedRef<boolean>>
  ripple: Readonly<ComputedRef<boolean>>
  disabled: Readonly<ComputedRef<boolean>>
  exclusive: Readonly<ComputedRef<boolean>>
  items: Readonly<Ref<MudToggleItemRegistration[]>>
  registerItem: (item: MudToggleItemRegistration) => () => void
  isSelected: (value: unknown) => boolean
  isTabStop: (item: MudToggleItemRegistration) => boolean
  toggleItem: (value: unknown) => void
  moveSelection: (event: KeyboardEvent) => void
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
const items = shallowRef<MudToggleItemRegistration[]>([])
const size = computed(() => 'medium' as const)
const vertical = computed(() => false)
const delimiters = computed(() => true)
const ripple = computed(() => true)
const disabled = computed(() => false)
const exclusive = computed(() => props.selectionMode !== 'multi-selection')

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

const registerItem = (item: MudToggleItemRegistration) => {
  items.value = [...items.value, item]

  return () => {
    items.value = items.value.filter(registeredItem => registeredItem !== item)
  }
}

const isSelected = (value: unknown) => Object.is(model.value, value)

const tabStop = computed(() => items.value.find(item => isSelected(item.value())) ?? items.value[0])

const isTabStop = (item: MudToggleItemRegistration) => tabStop.value === item

const toggleItem = (value: unknown) => {
  if (props.selectionMode === 'toggle-selection' && isSelected(value)) {
    model.value = null
  } else {
    model.value = value
  }
}

const arrowSteps: Record<string, number> = {
  ArrowRight: 1,
  ArrowDown: 1,
  ArrowLeft: -1,
  ArrowUp: -1,
}

const moveSelection = (event: KeyboardEvent) => {
  const step = arrowSteps[event.key]

  const origin = event.currentTarget
  const container = origin instanceof HTMLElement ? origin.parentElement : null

  if (!exclusive.value || step === undefined || container === null) {
    return
  }

  const radios = Array.from(container.querySelectorAll<HTMLButtonElement>(':scope > [role="radio"]'))
  const current = radios.findIndex(radio => radio === origin)

  if (current < 0) {
    return
  }

  event.preventDefault()

  for (let offset = 1; offset < radios.length; offset++) {
    const next = radios[(((current + step * offset) % radios.length) + radios.length) % radios.length]

    if (next !== undefined && !next.disabled) {
      next.focus()
      next.click()
      return
    }
  }
}

const context: MudToggleGroupContext = {
  color: computed(() => props.color),
  size,
  vertical,
  delimiters,
  ripple,
  disabled,
  exclusive,
  items,
  registerItem,
  isSelected,
  isTabStop,
  toggleItem,
  moveSelection,
}

provide(mudToggleGroupContextKey, context)
</script>

<template>
  <div
    :class="classes"
    :style="styles"
    :role="exclusive ? 'radiogroup' : 'group'"
  >
    <slot />
  </div>
</template>
