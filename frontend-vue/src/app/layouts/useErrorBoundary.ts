import type { ComputedRef } from 'vue'
import { onErrorCaptured, watch } from 'vue'
import { useRoute } from 'vue-router'
import { captureRenderError, clearRenderError, renderError } from '@/app/errors'

export interface ErrorBoundaryState {
  error: ComputedRef<Error | null>
  recover: () => void
}

export function useErrorBoundary(): ErrorBoundaryState {
  const route = useRoute()

  onErrorCaptured(reason => {
    captureRenderError(reason)
  })

  watch(() => route.fullPath, () => {
    clearRenderError()
  })

  return {
    error: renderError,
    recover: clearRenderError,
  }
}
