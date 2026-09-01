import type { Ref } from 'vue'
import { ref, watch } from 'vue'
import { formatTimeOfDay, parseTimeInput } from '@/domain/time-input'

export const TIME_PARSE_ERROR = 'Неверный формат времени. Используйте 24-часовые ЧЧ:ММ, например 18:30.'

export interface SmartTimePickerState {
  isClockVisible: Ref<boolean>
  timeText: Ref<string>
  hasError: Ref<boolean>
  commitText: () => boolean
  openClock: () => boolean
  switchToTextInput: () => void
  closeClock: () => void
  getMinutes: () => number | null
}

export function useSmartTimePicker(model: Ref<string>): SmartTimePickerState {
  const isClockVisible = ref(false)
  const timeText = ref('')
  const hasError = ref(false)

  watch(
    model,
    value => {
      if (value === timeText.value) {
        return
      }

      timeText.value = value
      hasError.value = false
    },
    { immediate: true },
  )

  function commitText(): boolean {
    if (isClockVisible.value) {
      return true
    }

    const raw = timeText.value.trim()

    if (raw.length === 0) {
      hasError.value = false
      timeText.value = ''
      model.value = ''
      return true
    }

    const minutes = parseTimeInput(raw)

    if (minutes === null) {
      timeText.value = raw
      hasError.value = true
      model.value = raw
      return false
    }

    hasError.value = false
    timeText.value = formatTimeOfDay(minutes)
    model.value = timeText.value
    return true
  }

  function openClock(): boolean {
    if (!commitText()) {
      return false
    }

    isClockVisible.value = true
    return true
  }

  function switchToTextInput(): void {
    hasError.value = false
    timeText.value = model.value
    isClockVisible.value = false
  }

  function closeClock(): void {
    switchToTextInput()
  }

  function getMinutes(): number | null {
    return commitText() ? parseTimeInput(model.value) : null
  }

  return {
    isClockVisible,
    timeText,
    hasError,
    commitText,
    openClock,
    switchToTextInput,
    closeClock,
    getMinutes,
  }
}
