<script setup lang="ts">
import { computed, ref, useAttrs, useId, watch } from 'vue'
import { mdClose } from '@/shared/icons'
import MudInput from './MudInput.vue'
import MudInputControl from './MudInputControl.vue'
import type { MudAdornment, MudColor, MudInputMode, MudMargin, MudSize, MudTypo, MudVariant } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  label?: string | undefined
  placeholder?: string | undefined
  helperText?: string | undefined
  helperTextOnFocus?: boolean
  error?: boolean
  errorText?: string | undefined
  fullWidth?: boolean
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  immediate?: boolean
  underline?: boolean
  shrinkLabel?: boolean
  variant?: MudVariant
  typo?: MudTypo
  margin?: MudMargin
  inputMode?: MudInputMode
  inputId?: string | undefined
  pattern?: string | undefined
  min?: number | undefined
  max?: number | undefined
  step?: number | undefined
  hideSpinButtons?: boolean
  adornment?: MudAdornment
  adornmentText?: string | undefined
  adornmentIcon?: string | undefined
  adornmentColor?: MudColor
  adornmentAriaLabel?: string | undefined
  iconSize?: MudSize
  clearable?: boolean
  clearIcon?: string
  onAdornmentClick?: ((event: MouseEvent) => void) | undefined
  onClearButtonClick?: ((event: MouseEvent) => void) | undefined
}>(), {
  label: '',
  helperText: '',
  helperTextOnFocus: false,
  error: false,
  errorText: '',
  fullWidth: false,
  required: false,
  disabled: false,
  readonly: false,
  immediate: false,
  underline: true,
  shrinkLabel: false,
  variant: 'text',
  typo: 'subtitle1',
  margin: 'none',
  inputMode: 'numeric',
  hideSpinButtons: false,
  adornment: 'none',
  adornmentText: '',
  adornmentIcon: '',
  adornmentColor: 'default',
  iconSize: 'medium',
  clearable: false,
  clearIcon: mdClose,
})

const model = defineModel<number | null>({ default: null })

const formattedText = defineModel<string | undefined>('text', { default: undefined })

const isFormatted = computed(() => formattedText.value !== undefined)

const SPACES = /[\s\u00A0\u202F]/g

const DECIMAL_SEPARATOR = ','

const attrs = useAttrs()
const generatedId = useId()
const inputReference = ref<{ focus: () => void, blur: () => void, select: () => void, setText: (value: string) => void }>()

const isRootAttribute = (name: string): boolean => name === 'class' || name === 'style' || name.startsWith('data-')

const rootAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => isRootAttribute(name)),
))

const inputAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => !isRootAttribute(name)),
))

const parse = (value: string): number | null => {
  const trimmed = value.replace(SPACES, '').replaceAll(DECIMAL_SEPARATOR, '.')

  if (trimmed.length === 0) {
    return null
  }

  const parsed = Number(trimmed)

  return Number.isFinite(parsed) ? parsed : null
}

const constrain = (value: number): number => {
  if (props.max !== undefined && value > props.max) {
    return props.max
  }

  if (props.min !== undefined && value < props.min) {
    return props.min
  }

  return value
}

const format = (value: number | null): string => {
  if (value === null) {
    return ''
  }

  return isFormatted.value ? `${value}`.replace('.', DECIMAL_SEPARATOR) : `${value}`
}

const internalText = ref(format(model.value))

const text = computed({
  get: () => (formattedText.value === undefined ? internalText.value : formattedText.value),
  set: value => {
    if (formattedText.value !== undefined) {
      formattedText.value = value
      return
    }

    internalText.value = value
  },
})

watch(model, value => {
  if (formattedText.value !== undefined || parse(text.value) === value) {
    return
  }

  text.value = format(value)
})

const inputElementId = computed(() => props.inputId ?? generatedId)

const helperId = computed(() => (!props.error && props.helperText.length > 0
  ? `${inputElementId.value}-helper-text`
  : undefined))

const errorId = computed(() => (props.error ? `${inputElementId.value}-error` : undefined))

const isNumberMode = computed(() => props.inputMode === 'numeric' || props.inputMode === 'decimal')

const inputType = computed(() => (props.pattern === undefined && isNumberMode.value && !isFormatted.value ? 'number' : 'text'))

const classes = computed(() => [
  'mud-input-input-control',
  'mud-input-number-control',
  props.hideSpinButtons ? 'mud-input-nospin' : 'mud-input-showspin',
])

const showClearButton = computed(() => props.clearable && !props.readonly)

const handleTextChange = (value: string): void => {
  const parsed = parse(value)
  const constrained = parsed === null ? null : constrain(parsed)

  if (constrained === parsed) {
    text.value = value
  } else {
    text.value = format(constrained)
    inputReference.value?.setText(text.value)
  }

  model.value = constrained
}

const change = (factor: number): void => {
  const current = formattedText.value === undefined ? model.value : parse(text.value)
  const next = constrain((current ?? 0) + (props.step ?? 1) * factor)

  text.value = format(next)
  model.value = next
  inputReference.value?.setText(text.value)
}

const handleKeyDown = (event: KeyboardEvent): void => {
  if (props.disabled || props.readonly) {
    return
  }

  if (event.key === 'ArrowUp') {
    event.preventDefault()
    change(1)
  } else if (event.key === 'ArrowDown') {
    event.preventDefault()
    change(-1)
  }
}

const handleWheel = (event: WheelEvent): void => {
  if (!event.shiftKey || props.disabled || props.readonly) {
    return
  }

  if (event.deltaY < 0) {
    change(1)
  } else if (event.deltaY > 0) {
    change(-1)
  }
}

defineExpose({
  focus: () => inputReference.value?.focus(),
  blur: () => inputReference.value?.blur(),
  select: () => inputReference.value?.select(),
  increment: () => change(1),
  decrement: () => change(-1),
})
</script>

<template>
  <MudInputControl
    v-bind="rootAttrs"
    :class="classes"
    :label="label"
    :variant="variant"
    :helper-id="helperId"
    :helper-text="helperText"
    :helper-text-on-focus="helperTextOnFocus"
    :full-width="fullWidth"
    :error="error"
    :error-id="errorId"
    :error-text="errorText"
    :disabled="disabled"
    :margin="margin"
    :required="required"
    :for-id="inputElementId"
  >
    <template #input-content>
      <MudInput
        ref="inputReference"
        v-bind="inputAttrs"
        :model-value="text"
        :label="label"
        :input-type="inputType"
        :input-mode="inputMode"
        :input-id="inputElementId"
        :variant="variant"
        :typo="typo"
        :placeholder="placeholder"
        :disabled="disabled"
        :underline="underline"
        :readonly="readonly"
        :adornment="adornment"
        :adornment-text="adornmentText"
        :adornment-icon="adornmentIcon"
        :adornment-color="adornmentColor"
        :adornment-aria-label="adornmentAriaLabel"
        :icon-size="iconSize"
        :on-adornment-click="onAdornmentClick"
        :error="error"
        :error-id="errorId"
        :helper-id="helperId"
        :immediate="immediate"
        :margin="margin"
        :hide-spin-buttons="hideSpinButtons"
        :min="min"
        :max="max"
        :step="step ?? 'any'"
        :pattern="pattern"
        :shrink-label="shrinkLabel"
        :required="required"
        :clearable="showClearButton"
        :clear-icon="clearIcon"
        :on-clear-button-click="onClearButtonClick"
        :on-increment="() => change(1)"
        :on-decrement="() => change(-1)"
        :on-mouse-wheel="handleWheel"
        @update:model-value="handleTextChange"
        @keydown="handleKeyDown"
      />
    </template>
  </MudInputControl>
</template>
