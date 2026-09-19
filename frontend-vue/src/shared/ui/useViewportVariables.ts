import { tryOnScopeDispose, useEventListener } from '@vueuse/core'

export const VIEWPORT_HEIGHT_VARIABLE = '--viewport-height'
export const KEYBOARD_INSET_VARIABLE = '--keyboard-inset'

// TODO: iOS Safari не поддерживает `interactive-widget`, поэтому перекрытие клавиатурой считается скриптом – снять вместе с композаблом, когда Safari начнёт поддерживать `interactive-widget` либо отдаст `env(keyboard-inset-bottom)`
export function useViewportVariables(): void {
  const viewport = window.visualViewport

  if (!viewport) {
    return
  }

  const update = (): void => {
    const root = document.documentElement
    const inset = Math.max(0, root.clientHeight - (viewport.height + viewport.offsetTop))

    root.style.setProperty(VIEWPORT_HEIGHT_VARIABLE, `${viewport.height}px`)
    root.style.setProperty(KEYBOARD_INSET_VARIABLE, `${inset}px`)
  }

  useEventListener(viewport, 'resize', update)
  useEventListener(viewport, 'scroll', update)

  tryOnScopeDispose(() => {
    const root = document.documentElement

    root.style.removeProperty(VIEWPORT_HEIGHT_VARIABLE)
    root.style.removeProperty(KEYBOARD_INSET_VARIABLE)
  })

  update()
}
