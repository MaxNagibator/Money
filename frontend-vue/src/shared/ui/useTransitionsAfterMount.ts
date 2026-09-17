import { onMounted, onScopeDispose, readonly, ref, type Ref } from 'vue'

export function useTransitionsAfterMount(): Readonly<Ref<boolean>> {
  const isEnabled = ref(false)
  let frame: number | null = null

  onMounted(() => {
    frame = requestAnimationFrame(() => {
      frame = null
      isEnabled.value = true
    })
  })

  onScopeDispose(() => {
    if (frame !== null) {
      cancelAnimationFrame(frame)
      frame = null
    }
  })

  return readonly(isEnabled)
}
