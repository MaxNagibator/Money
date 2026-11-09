import type { Ref } from 'vue'
import { ref, watch } from 'vue'
import { parseDateInput } from '@/domain/date-input'
import type { LocalDate } from '@/domain/local-date'
import { formatLocalDate } from '@/platform/format'
import type { Clock } from '@/platform/clock'

export const DATE_PARSE_ERROR
  = 'Неверный формат даты. Используйте дд.мм.гггг, сегодня/завтра/вчера или с/з/в/пз/пв.'

export interface SmartDatePickerState {
  isCalendarVisible: Ref<boolean>
  dateText: Ref<string>
  hasError: Ref<boolean>
  commitText: () => boolean
  openCalendar: () => boolean
  switchToTextInput: () => void
  closeCalendar: () => void
  clear: () => void
  getDate: () => LocalDate | null
}

export interface SmartDatePickerOptions {
  blankKeepsValue?: boolean
}

export function useSmartDatePicker(
  model: Ref<LocalDate | null>,
  clock: Clock,
  options: SmartDatePickerOptions = {},
): SmartDatePickerState {
  const blankKeepsValue = options.blankKeepsValue ?? false
  const isCalendarVisible = ref(false)
  const dateText = ref('')
  const hasError = ref(false)

  watch(
    model,
    value => {
      dateText.value = value === null ? '' : formatLocalDate(value)
    },
    { immediate: true },
  )

  function commitText(): boolean {
    if (isCalendarVisible.value) {
      return true
    }

    const raw = dateText.value.trim()

    if (raw.length === 0) {
      hasError.value = false

      if (blankKeepsValue) {
        return false
      }

      dateText.value = ''
      model.value = null
      return true
    }

    const value = parseDateInput(raw, clock.today())

    if (value === null) {
      hasError.value = true
      return false
    }

    hasError.value = false
    model.value = value
    dateText.value = formatLocalDate(value)
    return true
  }

  function openCalendar(): boolean {
    if (dateText.value.trim().length > 0 && !commitText()) {
      return false
    }

    isCalendarVisible.value = true
    return true
  }

  function switchToTextInput(): void {
    hasError.value = false
    dateText.value = model.value === null ? '' : formatLocalDate(model.value)
    isCalendarVisible.value = false
  }

  function closeCalendar(): void {
    switchToTextInput()
  }

  function clear(): void {
    hasError.value = false
    dateText.value = ''

    if (!blankKeepsValue) {
      model.value = null
    }
  }

  function getDate(): LocalDate | null {
    return commitText() ? model.value : null
  }

  return {
    isCalendarVisible,
    dateText,
    hasError,
    commitText,
    openCalendar,
    switchToTextInput,
    closeCalendar,
    clear,
    getDate,
  }
}
