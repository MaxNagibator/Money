<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { systemClock } from '@/platform/clock'
import { mdCalendarMonth, mdEdit } from '@/shared/icons'
import type { MudAdornment } from '@/shared/mud'
import { MudDatePicker, MudIconButton, MudTextField } from '@/shared/mud'
import { DATE_PARSE_ERROR, useSmartDatePicker } from '@/shared/ui/useSmartDatePicker'

const props = withDefaults(defineProps<{
  label?: string
  helperText?: string
  adornment?: MudAdornment
  blankKeepsValue?: boolean
  clearable?: boolean
  error?: boolean
  errorText?: string
}>(), {
  label: 'Дата',
  helperText: '',
  adornment: 'end',
  blankKeepsValue: false,
  clearable: true,
  error: false,
  errorText: '',
})

const model = defineModel<LocalDate | null>({ default: null })
const state = useSmartDatePicker(model, systemClock, { blankKeepsValue: props.blankKeepsValue })
const textField = ref<{ focus: () => void }>()
const calendar = ref<{ open: () => void }>()

watch(state.isCalendarVisible, async isVisible => {
  if (!isVisible) {
    return
  }

  await nextTick()
  calendar.value?.open()
})

const hasError = computed(() => state.hasError.value || props.error)

const errorText = computed(() => state.hasError.value ? DATE_PARSE_ERROR : props.errorText)

function onCalendarClosed(): void {
  state.closeCalendar()
}

defineExpose({
  commitText: state.commitText,
  focusInput: () => textField.value?.focus(),
  getDate: state.getDate,
})
</script>

<template>
  <MudTextField
    v-if="!state.isCalendarVisible.value"
    ref="textField"
    v-model="state.dateText.value"
    data-testid="date-text"
    :label="label"
    :error="hasError"
    :error-text="errorText"
    :helper-text="helperText"
    :adornment="adornment"
    adornment-color="tertiary"
    :adornment-icon="mdCalendarMonth"
    :clearable="clearable"
    immediate
    :on-adornment-click="() => state.openCalendar()"
    :on-clear-button-click="() => state.clear()"
    @blur="state.commitText()"
    @keydown.enter="state.commitText()"
  />
  <div
    v-else
    style="position: relative;"
  >
    <MudDatePicker
      ref="calendar"
      v-model="model"
      data-testid="date-calendar"
      :label="label"
      :helper-text="helperText"
      :adornment="adornment"
      adornment-color="tertiary"
      color="tertiary"
      :error="error"
      :error-text="errorText"
      date-format="dd.MM.yyyy"
      mask="dd.MM.yyyy"
      editable
      @picker-closed="onCalendarClosed"
    />
    <MudIconButton
      data-testid="date-to-text"
      :icon="mdEdit"
      color="tertiary"
      size="small"
      title="Текстовый ввод"
      style="position: absolute; right: 24px; top: 65%; transform: translateY(-50%); z-index: 1000;"
      @click="state.switchToTextInput()"
    />
  </div>
</template>
