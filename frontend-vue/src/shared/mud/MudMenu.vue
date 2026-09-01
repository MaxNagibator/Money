<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, provide, ref, shallowRef, toRef, useId, useSlots, useTemplateRef, watch, type Ref } from 'vue'
import MudButton from './MudButton.vue'
import MudIconButton from './MudIconButton.vue'
import MudList from './MudList.vue'
import MudPopover from './MudPopover.vue'
import type { MudColor, MudOrigin, MudSize, MudVariant } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  anchorOrigin?: MudOrigin
  ariaLabel?: string | undefined
  color?: MudColor
  dense?: boolean
  disabled?: boolean
  endIcon?: string | undefined
  fullWidth?: boolean
  icon?: string | undefined
  label?: string | undefined
  maxHeight?: number | undefined
  size?: MudSize
  transformOrigin?: MudOrigin
  variant?: MudVariant
}>(), {
  anchorOrigin: 'bottom-left',
  color: 'default',
  dense: false,
  disabled: false,
  fullWidth: false,
  size: 'medium',
  transformOrigin: 'top-left',
  variant: 'text',
})

interface MudMenuItemRegistration {
  element: HTMLElement
  disabled: Readonly<Ref<boolean>>
}

const slots = useSlots()
const open = defineModel<boolean>('open', { default: false })
const registrations = shallowRef<MudMenuItemRegistration[]>([])
const focusedIndex = ref(-1)
const rootElement = useTemplateRef<HTMLElement>('rootElement')
const wrapperElement = useTemplateRef<HTMLElement>('wrapperElement')
const lastInteractionWasKeyboard = ref(false)
const keyboardActivationWindow = 50
let lastKeyboardActivation = 0
const menuId = `menu-${useId()}`
const dense = toRef(props, 'dense')
const disabled = toRef(props, 'disabled')

const registerItem = (item: MudMenuItemRegistration): (() => void) => {
  registrations.value = [...registrations.value, item]

  return () => {
    registrations.value = registrations.value.filter(current => current !== item)
  }
}

const activatorHidden = computed(() => slots.activator === undefined
  && (props.label ?? '').trim().length === 0
  && (props.icon ?? '').trim().length === 0)

const classes = computed(() => [
  'mud-menu',
  {
    'mud-menu-button-hidden': activatorHidden.value,
  },
])

const activatorClasses = computed(() => [
  'mud-menu-activator',
  {
    'mud-disabled': props.disabled,
  },
])

const enabledItems = computed(() => registrations.value.filter(item => !item.disabled.value))

const focusItem = (index: number): void => {
  const item = enabledItems.value[index]
  if (!item) {
    return
  }

  focusedIndex.value = registrations.value.indexOf(item)
  item.element.focus()
}

const moveFocus = (direction: -1 | 1): void => {
  const items = enabledItems.value
  if (items.length === 0) {
    return
  }

  const currentItem = registrations.value[focusedIndex.value]
  const currentIndex = currentItem ? items.indexOf(currentItem) : -1
  const nextIndex = currentIndex < 0
    ? direction > 0 ? 0 : items.length - 1
    : (currentIndex + direction + items.length) % items.length

  focusItem(nextIndex)
}

const focusActivator = (): void => {
  const activator = rootElement.value?.querySelector('button, a, [role="button"]')
  if (activator instanceof HTMLElement) {
    activator.focus()
  }
}

const close = (): void => {
  open.value = false
  focusedIndex.value = -1
  lastInteractionWasKeyboard.value = false
}

const closeAll = (): void => {
  close()
  focusActivator()
}

provide('mud-menu-context', {
  dense,
  disabled,
  registerItem,
  close,
  closeAll,
})

const toggleMenu = (event?: Event): void => {
  if (disabled.value) {
    return
  }

  if (event instanceof MouseEvent && Date.now() - lastKeyboardActivation < keyboardActivationWindow) {
    return
  }

  open.value = !open.value
}

const handleKeydown = (event: KeyboardEvent): void => {
  if (event.key === 'ArrowDown') {
    event.preventDefault()
    event.stopPropagation()
    moveFocus(1)
  } else if (event.key === 'ArrowUp') {
    event.preventDefault()
    event.stopPropagation()
    moveFocus(-1)
  } else if (event.key === 'Escape') {
    event.preventDefault()
    event.stopPropagation()
    closeAll()
  } else if (event.key === 'Tab') {
    event.preventDefault()
    closeAll()
  }
}

const trackKeyboardInteraction = (event: KeyboardEvent): void => {
  lastInteractionWasKeyboard.value = true

  if (!open.value || focusedIndex.value !== -1 || enabledItems.value.length === 0) {
    return
  }

  if (event.key === 'ArrowDown') {
    focusItem(0)
  } else if (event.key === 'ArrowUp') {
    focusItem(enabledItems.value.length - 1)
  }
}

const handleActivatorKeydown = (event: KeyboardEvent): void => {
  trackKeyboardInteraction(event)

  if (event.key === 'Enter' || event.key === ' ') {
    lastKeyboardActivation = Date.now()
    toggleMenu()
  }
}

const handleOutsidePointerdown = (event: PointerEvent): void => {
  if (!open.value || !(event.target instanceof Node) || rootElement.value?.contains(event.target)) {
    return
  }

  close()
}

watch(open, async value => {
  if (value) {
    document.addEventListener('pointerdown', handleOutsidePointerdown)
    await nextTick()
    wrapperElement.value?.focus({ preventScroll: true })

    if (lastInteractionWasKeyboard.value && enabledItems.value.length > 0) {
      focusItem(0)
    }
  } else {
    document.removeEventListener('pointerdown', handleOutsidePointerdown)
  }
})

onBeforeUnmount(() => document.removeEventListener('pointerdown', handleOutsidePointerdown))
</script>

<template>
  <div
    :id="menuId"
    ref="rootElement"
    :class="classes"
    v-bind="$attrs"
  >
    <div
      v-if="$slots.activator"
      :aria-expanded="open"
      :class="activatorClasses"
      aria-haspopup="true"
      role="button"
      tabindex="0"
      @keydown="handleActivatorKeydown"
    >
      <slot
        name="activator"
        :toggle="toggleMenu"
        :open="open"
      />
    </div>
    <MudIconButton
      v-else-if="icon"
      class="mud-menu-icon-button-activator"
      :aria-label="ariaLabel"
      :color="color"
      :disabled="disabled"
      :icon="icon"
      :size="size"
      :variant="variant"
      @click="toggleMenu"
      @keydown="trackKeyboardInteraction"
    />
    <MudButton
      v-else-if="!activatorHidden"
      :aria-label="ariaLabel"
      class="mud-menu-button-activator"
      :color="color"
      :disabled="disabled"
      :end-icon="endIcon"
      :full-width="fullWidth"
      :size="size"
      :variant="variant"
      @click="toggleMenu"
      @keydown="trackKeyboardInteraction"
    >
      {{ label }}
    </MudButton>

    <MudPopover
      :open="open"
      :anchor-origin="anchorOrigin"
      :transform-origin="transformOrigin"
      :max-height="maxHeight"
      :duration="dense ? 0 : 251"
    >
      <div
        ref="wrapperElement"
        data-testid="menu-wrapper"
        role="menu"
        tabindex="-1"
        @keydown="handleKeydown"
        @pointerdown.stop
      >
        <MudList
          class="mud-menu-list"
          :dense="dense"
        >
          <slot />
        </MudList>
      </div>
    </MudPopover>
  </div>
</template>
