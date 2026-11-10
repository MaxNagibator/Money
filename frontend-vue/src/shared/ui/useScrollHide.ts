import { useScroll, useTimeoutFn } from '@vueuse/core'
import type { MaybeRefOrGetter, Ref } from 'vue'
import { readonly, ref, toValue, watch } from 'vue'

const SCROLL_HIDE_THRESHOLD = 24
const SCROLL_HIDE_DELAY = 150

export function useScrollHide(isEnabled: MaybeRefOrGetter<boolean> = true): Readonly<Ref<boolean>> {
  const { y: scrollTop } = useScroll(window)

  const isHidden = ref(false)
  const isScrollingDown = ref(false)

  let directionAnchor = scrollTop.value

  const { start: scheduleHide, stop: cancelHide } = useTimeoutFn(() => {
    isHidden.value = true
  }, SCROLL_HIDE_DELAY, { immediate: false })

  watch(scrollTop, value => {
    if (!toValue(isEnabled)) {
      directionAnchor = value
      return
    }

    const shift = value - directionAnchor

    if (Math.abs(shift) < SCROLL_HIDE_THRESHOLD) {
      return
    }

    directionAnchor = value
    isScrollingDown.value = shift > 0
  })

  watch(() => toValue(isEnabled), enabled => {
    if (!enabled) {
      directionAnchor = scrollTop.value
      isScrollingDown.value = false
    }
  })

  watch(() => toValue(isEnabled) && isScrollingDown.value, shouldHide => {
    if (shouldHide) {
      scheduleHide()
      return
    }

    cancelHide()
    isHidden.value = false
  })

  return readonly(isHidden)
}
