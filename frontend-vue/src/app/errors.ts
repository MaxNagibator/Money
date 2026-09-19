import type { ComputedRef } from 'vue'
import { computed, ref } from 'vue'

const UNKNOWN_REASON = 'Неизвестная ошибка'

const captured = ref<Error | null>(null)

export const renderError: ComputedRef<Error | null> = computed(() => captured.value)

export function handleAppError(reason: unknown, info: string): void {
  logAppError(reason, info)
  captureRenderError(reason)
}

export function logAppError(reason: unknown, info: string): void {
  console.error(info, reason)
}

export function captureRenderError(reason: unknown): void {
  captured.value = reason instanceof Error ? reason : new Error(describe(reason))
}

export function clearRenderError(): void {
  captured.value = null
}

function describe(reason: unknown): string {
  return typeof reason === 'string' && reason.length > 0 ? reason : UNKNOWN_REASON
}
