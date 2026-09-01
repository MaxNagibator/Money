<script setup lang="ts">
import { nextTick, onBeforeUnmount, ref, useId, useTemplateRef, watch } from 'vue'
import { mdClose } from '@/shared/icons'
import MudIconButton from './MudIconButton.vue'
import MudOverlay from './MudOverlay.vue'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  closeOnEscapeKey?: boolean
}>(), {
  closeOnEscapeKey: false,
})

const visible = defineModel<boolean>('visible', { default: false })

defineSlots<{
  title?: () => unknown
  content?: () => unknown
  actions?: () => unknown
}>()

const tabbableSelector = [
  'a[href]:not([tabindex=\'-1\'])',
  'area[href]:not([tabindex=\'-1\'])',
  'button:not([disabled]):not([tabindex=\'-1\'])',
  'input:not([disabled]):not([tabindex=\'-1\']):not([type=\'hidden\'])',
  'select:not([disabled]):not([tabindex=\'-1\'])',
  'textarea:not([disabled]):not([tabindex=\'-1\'])',
  'iframe:not([tabindex=\'-1\'])',
  'details:not([tabindex=\'-1\'])',
  '[tabindex]:not([tabindex=\'-1\'])',
  '[contentEditable=true]:not([tabindex=\'-1\'])',
].join(',')

const bumperCount = 4
const bumperSkip = 2

const elementId = `dialog-${useId()}`
const dialogId = `_${useId()}`
const titleId = `${dialogId}_title`

const dialogElement = useTemplateRef<HTMLElement>('dialogElement')
const shiftDown = ref(false)
let previouslyFocused: HTMLElement | undefined

const close = (): void => {
  visible.value = false
}

const tabbables = (): HTMLElement[] => [...dialogElement.value?.querySelectorAll<HTMLElement>(tabbableSelector) ?? []]

const focusFirst = (): void => {
  const items = tabbables()
  const target = items.length <= bumperCount ? dialogElement.value : items[bumperSkip]
  target?.focus()
}

const focusLast = (): void => {
  const items = tabbables()
  const target = items.length <= bumperCount ? dialogElement.value : items[items.length - bumperSkip - 1]
  target?.focus()
}

const focusBumper = (): void => {
  if (shiftDown.value) {
    focusLast()
  } else {
    focusFirst()
  }
}

const trackTabDirection = (event: KeyboardEvent): void => {
  if (event.key === 'Tab') {
    shiftDown.value = event.shiftKey
  }
}

const handleKeydown = (event: KeyboardEvent): void => {
  trackTabDirection(event)

  if (props.closeOnEscapeKey && event.key === 'Escape' && visible.value) {
    event.stopPropagation()
    close()
  }
}

const restoreFocus = (): void => {
  const target = previouslyFocused
  previouslyFocused = undefined
  target?.focus()
}

watch(visible, async isVisible => {
  if (!isVisible) {
    document.removeEventListener('keydown', handleKeydown)
    document.removeEventListener('keyup', trackTabDirection)
    restoreFocus()
    return
  }

  previouslyFocused = document.activeElement instanceof HTMLElement ? document.activeElement : undefined
  shiftDown.value = false
  document.addEventListener('keydown', handleKeydown)
  document.addEventListener('keyup', trackTabDirection)
  await nextTick()
  dialogElement.value?.focus()
}, { immediate: true })

onBeforeUnmount(() => {
  document.removeEventListener('keydown', handleKeydown)
  document.removeEventListener('keyup', trackTabDirection)
  restoreFocus()
})
</script>

<template>
  <Teleport
    v-if="visible"
    to="body"
  >
    <div
      :id="elementId"
      class="mud-dialog-container mud-dialog-center"
    >
      <MudOverlay
        visible
        class="mud-overlay-dialog mud-skip-overlay-section mud-skip-overlay-positioning"
        dark-background
        lock-scroll
      />
      <div
        :id="dialogId"
        ref="dialogElement"
        v-bind="$attrs"
        role="dialog"
        :aria-labelledby="$slots.title ? titleId : undefined"
        class="mud-dialog mud-dialog-width-sm"
        tabindex="-1"
      >
        <div
          class="fixed pointer-events-none"
          tabindex="0"
          @focus="focusFirst"
        />
        <div
          class="fixed pointer-events-none"
          tabindex="0"
          @focus="focusBumper"
        />
        <div
          v-if="$slots.title"
          :id="titleId"
          class="mud-dialog-title"
        >
          <slot name="title" />
          <MudIconButton
            aria-label="Закрыть"
            class="mud-button-close"
            :icon="mdClose"
            @mousedown.prevent
            @click="close"
          />
        </div>
        <div class="mud-dialog-content">
          <slot name="content" />
        </div>
        <div
          v-if="$slots.actions"
          class="mud-dialog-actions"
        >
          <slot name="actions" />
        </div>
        <div
          class="fixed pointer-events-none"
          tabindex="0"
          @focus="focusBumper"
        />
        <div
          class="fixed pointer-events-none"
          tabindex="0"
          @focus="focusLast"
        />
      </div>
    </div>
  </Teleport>
</template>
