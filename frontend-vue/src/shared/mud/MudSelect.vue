<script setup lang="ts">
import { computed, defineComponent, h, nextTick, onBeforeUnmount, provide, ref, shallowRef, useAttrs, useId, useTemplateRef, watch, type Ref } from 'vue'
import { mdArrowDropDown, mdArrowDropUp } from '@/shared/icons'
import MudInput from './MudInput.vue'
import MudInputControl from './MudInputControl.vue'
import MudList from './MudList.vue'
import MudPopover from './MudPopover.vue'
import type { MudAdornment, MudColor } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  adornment?: MudAdornment
  adornmentColor?: MudColor
  adornmentIcon?: string | undefined
  clearable?: boolean
  helperText?: string | undefined
  label?: string | undefined
  error?: boolean
  errorText?: string | undefined
}>(), {
  adornment: 'end',
  adornmentColor: 'default',
  adornmentIcon: '',
  clearable: false,
  helperText: '',
  label: '',
  error: false,
  errorText: '',
})

interface MudSelectItemRegistration {
  id: string
  value: Ref<unknown>
  disabled: Readonly<Ref<boolean>>
  text: Readonly<Ref<string>>
  element: Readonly<Ref<HTMLElement | null>>
}

interface MudSelectContext {
  items: Readonly<Ref<MudSelectItemRegistration[]>>
  registerItem: (item: MudSelectItemRegistration, shadow: boolean) => () => void
  isSelected: (item: MudSelectItemRegistration) => boolean
  isHighlighted: (item: MudSelectItemRegistration) => boolean
  selectItem: (item: MudSelectItemRegistration) => void
}

const model = defineModel<unknown>()
const attrs = useAttrs()
const open = ref(false)
const items = shallowRef<MudSelectItemRegistration[]>([])
const shadowItems = shallowRef<MudSelectItemRegistration[]>([])
const highlightedIndex = ref(-1)
const inputText = ref('')
const rootElement = useTemplateRef<HTMLElement>('rootElement')
const inputId = `select-input-${useId()}`
const elementId = `select-${useId()}`
const errorId = computed(() => (props.error ? `${inputId}-error` : undefined))

const sameValue = (left: unknown, right: unknown): boolean => Object.is(left, right)

const registerItem = (item: MudSelectItemRegistration, shadow: boolean): (() => void) => {
  const collection = shadow ? shadowItems : items
  collection.value = [...collection.value, item]

  return () => {
    collection.value = collection.value.filter(current => current !== item)
  }
}

const findItem = (collection: readonly MudSelectItemRegistration[]): MudSelectItemRegistration | undefined =>
  collection.find(item => sameValue(item.value.value, model.value))

const selectedItem = computed(() => findItem(items.value) ?? findItem(shadowItems.value))

const itemState = computed<readonly [unknown, string][]>(() => [
  ...items.value,
  ...shadowItems.value,
].map(item => [item.value.value, item.text.value] as [unknown, string]))

const stringifyValue = (value: unknown): string => {
  if (value === null || value === undefined) {
    return ''
  }

  if (typeof value === 'string') {
    return value
  }

  if (typeof value === 'number' || typeof value === 'bigint') {
    return value.toString()
  }

  if (typeof value === 'boolean') {
    return value ? 'True' : 'False'
  }

  return ''
}

const syncInputText = (): void => {
  const nextText = selectedItem.value?.text.value ?? stringifyValue(model.value)
  if (inputText.value !== nextText) {
    inputText.value = nextText
  }
}

watch([model, itemState], syncInputText, { immediate: true })

const isSelected = (item: MudSelectItemRegistration): boolean => sameValue(item.value.value, model.value)

watch(items, value => {
  if (open.value && highlightedIndex.value < 0) {
    highlightedIndex.value = value.findIndex(item => isSelected(item))
  }
})

const isHighlighted = (item: MudSelectItemRegistration): boolean =>
  items.value[highlightedIndex.value] === item

const highlightedItem = (): MudSelectItemRegistration | undefined => items.value[highlightedIndex.value]

const scrollToItem = (item: MudSelectItemRegistration): void => {
  void nextTick(() => item.element.value?.scrollIntoView({ block: 'nearest' }))
}

const applyItem = (item: MudSelectItemRegistration): void => {
  model.value = item.value.value
  inputText.value = item.text.value
  highlightedIndex.value = items.value.indexOf(item)
  scrollToItem(item)
}

const selectItem = (item: MudSelectItemRegistration): void => {
  if (item.disabled.value) {
    return
  }

  const index = items.value.indexOf(item)
  model.value = item.value.value
  inputText.value = item.text.value
  highlightedIndex.value = index
  open.value = false
}

const context: MudSelectContext = {
  items,
  registerItem,
  isSelected,
  isHighlighted,
  selectItem,
}

provide('mud-select-context', context)

const MudSelectShadowItems = defineComponent({
  name: 'MudSelectShadowItems',
  setup(_, { slots }) {
    provide('mud-select-shadow', true)
    return () => h('div', { class: 'd-none' }, slots.default?.())
  },
})

const currentIcon = computed(() => props.adornmentIcon || (open.value ? mdArrowDropUp : mdArrowDropDown))

const enabledIndices = computed<number[]>(() => items.value
  .map((item, index) => item.disabled.value ? -1 : index)
  .filter(index => index >= 0))

const moveHighlight = (direction: -1 | 1): void => {
  if (enabledIndices.value.length === 0) {
    return
  }

  const currentPosition = enabledIndices.value.indexOf(highlightedIndex.value)
  const nextPosition = currentPosition < 0
    ? 0
    : Math.min(Math.max(currentPosition + direction, 0), enabledIndices.value.length - 1)
  const nextIndex = enabledIndices.value[nextPosition]
  if (nextIndex === undefined) {
    return
  }

  const nextItem: MudSelectItemRegistration | undefined = items.value[nextIndex]
  if (nextItem) {
    applyItem(nextItem)
  } else {
    highlightedIndex.value = nextIndex
  }
}

const searchCollection = (): readonly MudSelectItemRegistration[] => (open.value ? items.value : shadowItems.value)

const findItemBySearch = (
  collection: readonly MudSelectItemRegistration[],
  character: string,
): MudSelectItemRegistration | undefined => {
  const search = character.toLocaleLowerCase()
  const matches = collection.filter(item =>
    !item.disabled.value && item.text.value.toLocaleLowerCase().startsWith(search))

  if (matches.length === 0) {
    return highlightedItem()
  }

  const active = highlightedItem()
  const activeIndex = active === undefined ? -1 : matches.indexOf(active)

  return activeIndex < 0 ? matches[0] : matches[(activeIndex + 1) % matches.length]
}

const selectFirstItem = (character?: string): void => {
  const collection = searchCollection()
  if (collection.length === 0) {
    return
  }

  if (character !== undefined) {
    const found = findItemBySearch(collection, character)
    if (found) {
      applyItem(found)
      return
    }
  }

  const first = collection.find(item => !item.disabled.value)
  if (first) {
    applyItem(first)
  }
}

const selectLastItem = (): void => {
  const last = [...items.value].reverse().find(item => !item.disabled.value)
  if (last) {
    applyItem(last)
  }
}

const openMenu = (): void => {
  if (open.value) {
    return
  }

  open.value = true
  highlightedIndex.value = items.value.findIndex(item => isSelected(item))
}

const closeMenu = (): void => {
  open.value = false
  highlightedIndex.value = items.value.findIndex(item => isSelected(item))
}

const toggleMenu = (): void => {
  if (open.value) {
    closeMenu()
  } else {
    openMenu()
  }
}

const handleMouseDown = (event: MouseEvent): void => {
  if (event.button === 0) {
    toggleMenu()
  }
}

const handleAdornmentClick = (): void => toggleMenu()

const selectHighlighted = (): void => {
  const item = items.value[highlightedIndex.value]
  if (item) {
    selectItem(item)
  }
}

const handleKeydown = (event: KeyboardEvent): void => {
  if (event.key === 'ArrowDown') {
    event.preventDefault()
    if (event.altKey || !open.value) {
      openMenu()
    } else {
      moveHighlight(1)
    }
  } else if (event.key === 'ArrowUp') {
    event.preventDefault()
    if (event.altKey) {
      closeMenu()
    } else if (!open.value) {
      openMenu()
    } else {
      moveHighlight(-1)
    }
  } else if (event.key === 'Enter') {
    event.preventDefault()
    if (!open.value) {
      openMenu()
    } else {
      selectHighlighted()
    }
  } else if (event.key === 'Escape') {
    if (open.value) {
      event.preventDefault()
      event.stopPropagation()
      closeMenu()
    }
  } else if (event.key === ' ') {
    event.preventDefault()
    toggleMenu()
  } else if (event.key === 'Home') {
    event.preventDefault()
    selectFirstItem()
  } else if (event.key === 'End') {
    event.preventDefault()
    selectLastItem()
  } else if (event.key === 'Tab') {
    closeMenu()
  } else if (event.key.length === 1 && !event.ctrlKey && !event.shiftKey && !event.altKey && !event.metaKey) {
    selectFirstItem(event.key)
  }
}

const clearSelection = (): void => {
  model.value = null
  inputText.value = ''
  closeMenu()
}

const handleOutsidePointerdown = (event: PointerEvent): void => {
  if (!open.value || !(event.target instanceof Node) || rootElement.value?.contains(event.target)) {
    return
  }

  closeMenu()
}

watch(open, value => {
  if (value) {
    document.addEventListener('pointerdown', handleOutsidePointerdown)
    void nextTick(syncInputText)
  } else {
    document.removeEventListener('pointerdown', handleOutsidePointerdown)
  }
})

onBeforeUnmount(() => document.removeEventListener('pointerdown', handleOutsidePointerdown))
</script>

<template>
  <div
    :id="elementId"
    ref="rootElement"
    class="mud-select"
  >
    <MudInputControl
      class="mud-select"
      :for-id="inputId"
      :helper-text="helperText"
      :label="label"
      :error="error"
      :error-id="errorId"
      :error-text="errorText"
      @mousedown="handleMouseDown"
    >
      <template #input-content>
        <MudInput
          v-bind="attrs"
          v-model="inputText"
          class="mud-select-input"
          :label="label"
          :adornment="adornment"
          :adornment-color="adornmentColor"
          :adornment-icon="currentIcon"
          :clearable="clearable"
          :subscribe-to-parent-form="false"
          :input-id="inputId"
          :error="error"
          :error-id="errorId"
          readonly
          :on-adornment-click="handleAdornmentClick"
          :on-clear-button-click="clearSelection"
          @keydown="handleKeydown"
        />
        <MudPopover
          :open="open"
          anchor-origin="bottom-left"
          transform-origin="top-left"
          relative-width="relative"
          :max-height="300"
        >
          <MudList
            @pointerdown.stop
          >
            <slot />
          </MudList>
        </MudPopover>
      </template>
    </MudInputControl>
  </div>
  <MudSelectShadowItems>
    <slot />
  </MudSelectShadowItems>
</template>
