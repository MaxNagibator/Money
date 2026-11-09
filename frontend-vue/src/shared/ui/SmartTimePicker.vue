<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { formatTimeOfDay, parseTimeInput } from '@/domain/time-input'
import { mdEdit, mdSchedule } from '@/shared/icons'
import { MudIconButton, MudTextField, MudTimePicker } from '@/shared/mud'
import { TIME_PARSE_ERROR, useSmartTimePicker } from '@/shared/ui/useSmartTimePicker'

const props = withDefaults(defineProps<{
  label?: string
  hint?: string
  disabled?: boolean
  testId?: string
  error?: boolean
  errorText?: string
}>(), {
  label: 'Время',
  hint: '',
  disabled: false,
  testId: '',
  error: false,
  errorText: '',
})

const model = defineModel<string>({ default: '' })
const state = useSmartTimePicker(model)
const textField = ref<{ focus: () => void }>()
const clock = ref<{ open: () => void }>()

const clockValue = computed({
  get: () => (model.value.length === 0 ? null : parseTimeInput(model.value)),
  set: (value: number | null) => {
    model.value = value === null ? '' : formatTimeOfDay(value)
  },
})

watch(state.isClockVisible, async isVisible => {
  if (!isVisible) {
    return
  }

  await nextTick()
  clock.value?.open()
})

const hasError = computed(() => state.hasError.value || props.error)

const errorText = computed(() => state.hasError.value ? TIME_PARSE_ERROR : props.errorText)

function onClockClosed(): void {
  state.closeClock()
}

defineExpose({
  commitText: state.commitText,
  focusInput: () => textField.value?.focus(),
  getMinutes: state.getMinutes,
})
</script>

<template>
  <MudTextField
    v-if="!state.isClockVisible.value"
    ref="textField"
    v-model="state.timeText.value"
    :data-testid="testId.length > 0 ? testId : undefined"
    :label="label"
    :helper-text="hint"
    :disabled="disabled"
    :error="hasError"
    :error-text="errorText"
    adornment="end"
    adornment-color="tertiary"
    :adornment-icon="mdSchedule"
    input-mode="numeric"
    immediate
    :on-adornment-click="() => state.openClock()"
    @blur="state.commitText()"
    @keydown.enter="state.commitText()"
  />
  <div
    v-else
    style="position: relative;"
  >
    <MudTimePicker
      ref="clock"
      v-model="clockValue"
      data-testid="time-clock"
      :label="label"
      :helper-text="hint"
      :disabled="disabled"
      :error="error"
      :error-text="errorText"
      @picker-closed="onClockClosed"
    />
    <MudIconButton
      data-testid="time-to-text"
      :icon="mdEdit"
      color="tertiary"
      size="small"
      title="Текстовый ввод"
      style="position: absolute; right: 24px; top: 65%; transform: translateY(-50%); z-index: 1000;"
      @click="state.switchToTextInput()"
    />
  </div>
</template>
