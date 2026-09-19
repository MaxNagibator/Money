import { useMediaQuery } from '@vueuse/core'
import type { Ref } from 'vue'

export const NARROW_VIEWPORT = '(max-width: 599.98px)'

export function useNarrowViewport(): Ref<boolean> {
  return useMediaQuery(NARROW_VIEWPORT)
}
