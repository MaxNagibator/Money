<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, useAttrs, useId, useTemplateRef, watch, type ComponentPublicInstance } from 'vue'
import { mdArrowDropDown, mdArrowDropUp } from '@/shared/icons'
import MudInput from './MudInput.vue'
import MudInputControl from './MudInputControl.vue'
import MudList from './MudList.vue'
import MudListItem from './MudListItem.vue'
import MudPopover from './MudPopover.vue'
import type { MudAdornment, MudColor } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  adornment?: MudAdornment
  adornmentColor?: MudColor
  adornmentIcon?: string | undefined
  clearable?: boolean
  counter?: number | undefined
  error?: boolean
  errorText?: string | undefined
  label?: string | undefined
  resetValueOnEmptyText?: boolean
  searchFunc?: ((text: string) => readonly unknown[] | Promise<readonly unknown[]>) | undefined
  selectValueOnTab?: boolean
  toStringFunc?: ((value: unknown) => string | undefined) | undefined
}>(), {
  adornment: 'end',
  adornmentColor: 'default',
  adornmentIcon: '',
  clearable: false,
  error: false,
  errorText: '',
  label: '',
  resetValueOnEmptyText: false,
  selectValueOnTab: false,
})

const model = defineModel<unknown>()
const attrs = useAttrs()
const text = ref('')
const items = ref<unknown[]>([])
const open = ref(false)
const highlightedIndex = ref(-1)
const isFocused = ref(false)
const rootElement = useTemplateRef<HTMLElement>('rootElement')
const itemElements = new Map<number, HTMLElement>()
const inputId = `autocomplete-input-${useId()}`
const errorId = computed(() => (props.error ? `${inputId}-error` : undefined))
const elementId = `autocomplete-${useId()}`
let debounceTimer: ReturnType<typeof setTimeout> | undefined
let searchVersion = 0
let openingVersion: number | undefined

const stringifyValue = (value: unknown): string => {
  if (value === null || value === undefined) {
    return ''
  }

  if (props.toStringFunc) {
    return props.toStringFunc(value) ?? ''
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

watch(model, value => {
  const nextText = stringifyValue(value)
  if (text.value !== nextText) {
    text.value = nextText
  }
}, { immediate: true })

const currentIcon = computed(() => props.adornmentIcon || (open.value ? mdArrowDropUp : mdArrowDropDown))

const cancelSearch = (): void => {
  searchVersion += 1
  openingVersion = undefined
  clearTimeout(debounceTimer)
}

const firstEnabledItem = (): number => (items.value.length > 0 ? 0 : -1)

const runSearch = async (): Promise<void> => {
  const version = ++searchVersion
  openingVersion = version
  const searchFunc = props.searchFunc
  let result: readonly unknown[] = []

  if (searchFunc) {
    try {
      result = await searchFunc(text.value)
    } catch {
      result = []
    }
  }

  if (version !== searchVersion) {
    if (openingVersion === version) {
      openingVersion = undefined
    }
    return
  }

  items.value = [...result].slice(0, 10)
  highlightedIndex.value = firstEnabledItem()
  open.value = true
  openingVersion = undefined
}

const scheduleSearch = (): void => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    void runSearch()
  }, 100)
}

const openMenu = (): void => {
  if (!open.value && openingVersion === undefined) {
    void runSearch()
  }
}

const closeMenu = (): void => {
  cancelSearch()
  open.value = false
}

const toggleMenu = (): void => {
  if (open.value) {
    closeMenu()
  } else {
    openMenu()
  }
}

const setItemRef = (index: number, instance: Element | ComponentPublicInstance | null): void => {
  const root: unknown = instance instanceof Element ? instance : instance?.$el

  if (root instanceof HTMLElement) {
    itemElements.set(index, root)
  } else {
    itemElements.delete(index)
  }
}

const scrollToHighlighted = (): void => {
  void nextTick(() => itemElements.get(highlightedIndex.value)?.scrollIntoView({ block: 'nearest' }))
}

const moveHighlight = (direction: -1 | 1): void => {
  if (items.value.length === 0) {
    return
  }

  const nextIndex = highlightedIndex.value < 0
    ? 0
    : Math.min(Math.max(highlightedIndex.value + direction, 0), items.value.length - 1)
  highlightedIndex.value = nextIndex
  scrollToHighlighted()
}

const selectItem = (item: unknown, index: number): void => {
  if (item === undefined) {
    return
  }

  model.value = item
  text.value = stringifyValue(item)
  highlightedIndex.value = index
  closeMenu()
}

const selectHighlighted = (): void => {
  const index = highlightedIndex.value
  const item = items.value[index]
  if (item !== undefined) {
    selectItem(item, index)
  }
}

const handleKeydown = (event: KeyboardEvent): void => {
  if (event.key === 'Tab') {
    if (open.value && props.selectValueOnTab) {
      selectHighlighted()
    }
    closeMenu()
  } else if (event.key === 'ArrowDown') {
    event.preventDefault()
    if (open.value) {
      moveHighlight(1)
    } else {
      openMenu()
    }
  } else if (event.key === 'ArrowUp') {
    event.preventDefault()
    if (event.altKey) {
      closeMenu()
    } else if (open.value) {
      moveHighlight(-1)
    } else {
      openMenu()
    }
  } else if (event.key === 'Enter') {
    event.preventDefault()
    if (open.value) {
      selectHighlighted()
    } else {
      openMenu()
    }
  } else if (event.key === 'Escape') {
    if (open.value) {
      event.preventDefault()
      event.stopPropagation()
      closeMenu()
    }
  } else if (event.key === 'Backspace' && event.ctrlKey && event.shiftKey) {
    clearValue()
  }
}

const handleMouseDown = (event: MouseEvent): void => {
  if (event.button === 0 && !open.value) {
    openMenu()
  }
}

const handleFocus = (): void => {
  isFocused.value = true
  if (!open.value) {
    openMenu()
  }
}

const handleBlur = (): void => {
  isFocused.value = false
}

const clearValue = (): void => {
  cancelSearch()
  model.value = null
  text.value = ''
  open.value = false
}

watch(text, value => {
  if (props.resetValueOnEmptyText && value.trim().length === 0) {
    model.value = null
  }

  if (isFocused.value || open.value) {
    scheduleSearch()
  }
})

const handleOutsidePointerdown = (event: PointerEvent): void => {
  if (!open.value || !(event.target instanceof Node) || rootElement.value?.contains(event.target)) {
    return
  }

  closeMenu()
}

watch(open, value => {
  if (value) {
    document.addEventListener('pointerdown', handleOutsidePointerdown)
  } else {
    document.removeEventListener('pointerdown', handleOutsidePointerdown)
  }
})

onBeforeUnmount(() => {
  cancelSearch()
  document.removeEventListener('pointerdown', handleOutsidePointerdown)
})
</script>

<template>
  <div
    :id="elementId"
    ref="rootElement"
    class="mud-select mud-autocomplete"
  >
    <MudInputControl
      class="mud-select"
      :for-id="inputId"
      :label="label"
      :error="error"
      :error-id="errorId"
      :error-text="errorText"
      @mousedown="handleMouseDown"
    >
      <template #input-content>
        <MudInput
          v-bind="attrs"
          v-model="text"
          autocomplete="off"
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
          :on-adornment-click="toggleMenu"
          :on-clear-button-click="clearValue"
          :immediate="true"
          @blur="handleBlur"
          @focus="handleFocus"
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
            v-if="items.length > 0"
            @pointerdown.stop
          >
            <MudListItem
              v-for="(item, index) in items"
              :key="index"
              :ref="element => setItemRef(index, element)"
              :class="{
                'mud-selected-item mud-primary-text mud-primary-hover': index === highlightedIndex,
              }"
              :tabindex="index === highlightedIndex ? 0 : -1"
              @click="selectItem(item, index)"
            >
              {{ stringifyValue(item) }}
            </MudListItem>
          </MudList>
        </MudPopover>
      </template>
    </MudInputControl>
  </div>
</template>
