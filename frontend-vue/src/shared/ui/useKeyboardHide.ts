import { useEventListener } from '@vueuse/core'
import type { Ref } from 'vue'
import { readonly, ref } from 'vue'

export const KEYBOARD_VIEWPORT_RATIO = 0.25

const SILENT_INPUT_TYPES = new Set([
  'button',
  'checkbox',
  'color',
  'file',
  'hidden',
  'image',
  'radio',
  'range',
  'reset',
  'submit',
])

export function useKeyboardHide(): Readonly<Ref<boolean>> {
  const isHidden = ref(false)
  const viewport = window.visualViewport
  let hasFieldFocus = raisesKeyboard(document.activeElement)

  const update = (): void => {
    isHidden.value = hasFieldFocus && (!viewport || isViewportShrunk(viewport))
  }

  useEventListener(document, 'focusin', (event: FocusEvent) => {
    hasFieldFocus = raisesKeyboard(event.target)
    update()
  })

  useEventListener(document, 'focusout', (event: FocusEvent) => {
    hasFieldFocus = raisesKeyboard(event.relatedTarget)
    update()
  })

  if (viewport) {
    useEventListener(viewport, 'resize', update)
  }

  update()

  return readonly(isHidden)
}

// TODO: iOS Safari держит `screen.height` портретным при любом повороте, поэтому в альбомной ориентации вьюпорт читается сжатым всегда и признак вырождается в один фокус – снять, когда Safari начнёт поддерживать `interactive-widget` либо отдаст `navigator.virtualKeyboard`
function isViewportShrunk(viewport: VisualViewport): boolean {
  const height = viewport.height * viewport.scale
  const screenHeight = window.screen.height

  return screenHeight - height > screenHeight * KEYBOARD_VIEWPORT_RATIO
}

function raisesKeyboard(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) {
    return false
  }

  if (target instanceof HTMLTextAreaElement) {
    return !target.readOnly
  }

  if (target instanceof HTMLInputElement) {
    return !target.readOnly && !SILENT_INPUT_TYPES.has(target.type)
  }

  if (target.isContentEditable) {
    return true
  }

  const editable = target.getAttribute('contenteditable')

  return editable !== null && editable !== 'false'
}
