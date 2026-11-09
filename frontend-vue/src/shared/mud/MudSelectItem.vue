<script setup lang="ts">
import { computed, inject, onBeforeUnmount, onMounted, ref, toRef, useId, useSlots, watchEffect, type ComponentPublicInstance, type Ref } from 'vue'
import MudListItem from './MudListItem.vue'

const props = withDefaults(defineProps<{
  value?: unknown
  disabled?: boolean
}>(), {
  disabled: false,
})

interface MudSelectItemRegistration {
  id: string
  value: Ref<unknown>
  disabled: Readonly<Ref<boolean>>
  text: Readonly<Ref<string>>
  element: Readonly<Ref<HTMLElement | null>>
}

interface MudSelectContext {
  registerItem: (item: MudSelectItemRegistration, shadow: boolean) => () => void
  isSelected: (item: MudSelectItemRegistration) => boolean
  isHighlighted: (item: MudSelectItemRegistration) => boolean
  selectItem: (item: MudSelectItemRegistration) => void
}

const context = inject<MudSelectContext | undefined>('mud-select-context')
const isShadow = inject<boolean>('mud-select-shadow', false)
const slots = useSlots()
const itemId = `select-item-${useId()}`
const itemText = ref('')
const value = toRef(props, 'value')
const disabled = toRef(props, 'disabled')
const listItem = ref<ComponentPublicInstance | null>(null)
const element = computed<HTMLElement | null>(() => {
  const root: unknown = listItem.value?.$el

  return root instanceof HTMLElement ? root : null
})
const item: MudSelectItemRegistration = {
  id: itemId,
  value,
  disabled,
  text: itemText,
  element,
}
let unregister: (() => void) | undefined

const extractText = (valueToRead: unknown): string => {
  if (valueToRead === null || valueToRead === undefined || typeof valueToRead === 'boolean') {
    return ''
  }

  if (typeof valueToRead === 'string' || typeof valueToRead === 'number') {
    return String(valueToRead)
  }

  if (Array.isArray(valueToRead)) {
    return valueToRead.map(extractText).join('')
  }

  if (typeof valueToRead === 'object' && 'children' in valueToRead) {
    return extractText((valueToRead as { children?: unknown }).children)
  }

  return ''
}

watchEffect(() => {
  itemText.value = extractText(slots.default?.())
})

const selected = computed(() => context?.isSelected(item) ?? false)
const highlighted = computed(() => context?.isHighlighted(item) ?? false)

const classes = computed(() => [
  {
    'mud-list-item-disabled': disabled.value,
    'mud-selected-item mud-primary-text mud-primary-hover': selected.value || highlighted.value,
  },
])

const handleClick = (): void => {
  if (!disabled.value) {
    context?.selectItem(item)
  }
}

onMounted(() => {
  unregister = context?.registerItem(item, isShadow)
})

onBeforeUnmount(() => unregister?.())
</script>

<template>
  <MudListItem
    v-if="!isShadow"
    :id="itemId"
    ref="listItem"
    :class="classes"
    :tabindex="disabled ? -1 : 0"
    @click="handleClick"
  >
    <slot />
  </MudListItem>
</template>
