<script setup lang="ts">
import { computed, ref, useAttrs, useId } from 'vue'
import { mdClose } from '@/shared/icons'
import MudInput from './MudInput.vue'
import MudInputControl from './MudInputControl.vue'
import type { MudAdornment, MudColor, MudInputMode, MudInputSizing, MudInputType, MudMargin, MudSize, MudTypo, MudVariant } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  label?: string | undefined
  placeholder?: string | undefined
  helperText?: string | undefined
  helperTextOnFocus?: boolean
  counter?: number | undefined
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
  inputType?: MudInputType
  inputMode?: MudInputMode
  inputId?: string | undefined
  pattern?: string | undefined
  lines?: number
  sizing?: MudInputSizing
  adornment?: MudAdornment
  adornmentText?: string | undefined
  adornmentIcon?: string | undefined
  adornmentColor?: MudColor
  adornmentAriaLabel?: string | undefined
  adornmentTestId?: string | undefined
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
  inputType: 'text',
  inputMode: 'text',
  lines: 1,
  sizing: 'fixed',
  adornment: 'none',
  adornmentText: '',
  adornmentIcon: '',
  adornmentColor: 'default',
  iconSize: 'medium',
  clearable: false,
  clearIcon: mdClose,
})

const model = defineModel<string>({ default: '' })

const attrs = useAttrs()
const generatedId = useId()
const inputReference = ref<{ focus: () => void, blur: () => void, select: () => void }>()
const isFocused = ref(false)

const isRootAttribute = (name: string): boolean => name === 'class' || name === 'style' || name.startsWith('data-')

const rootAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => isRootAttribute(name)),
))

const inputAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => !isRootAttribute(name)),
))

const inputElementId = computed(() => props.inputId ?? generatedId)

const helperId = computed(() => (!props.error && props.helperText.length > 0
  ? `${inputElementId.value}-helper-text`
  : undefined))

const errorId = computed(() => (props.error ? `${inputElementId.value}-error` : undefined))

const classes = computed(() => [
  'mud-input-input-control',
  `mud-input-sizing-${props.sizing}`,
])

const counterText = computed(() => {
  if (props.counter === undefined) {
    return ''
  }

  return props.counter === 0
    ? `${model.value.length}`
    : `${model.value.length} / ${props.counter}`
})

const showClearButton = computed(() => props.clearable && !props.readonly && !props.disabled)

const handleContainerClick = (): void => {
  if (!isFocused.value) {
    inputReference.value?.focus()
  }
}

defineExpose({
  focus: () => inputReference.value?.focus(),
  blur: () => inputReference.value?.blur(),
  select: () => inputReference.value?.select(),
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
    :counter-text="counterText"
    :full-width="fullWidth"
    :error="error"
    :error-id="errorId"
    :error-text="errorText"
    :disabled="disabled"
    :margin="margin"
    :required="required"
    :for-id="inputElementId"
    @click="handleContainerClick"
    @focusin="isFocused = true"
    @focusout="isFocused = false"
  >
    <template #input-content>
      <MudInput
        ref="inputReference"
        v-bind="inputAttrs"
        v-model="model"
        :label="label"
        :input-type="inputType"
        :input-mode="inputMode"
        :input-id="inputElementId"
        :lines="lines"
        :sizing="sizing"
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
        :adornment-test-id="adornmentTestId"
        :icon-size="iconSize"
        :on-adornment-click="onAdornmentClick"
        :error="error"
        :error-id="errorId"
        :helper-id="helperId"
        :immediate="immediate"
        :margin="margin"
        hide-spin-buttons
        :clearable="showClearButton"
        :clear-icon="clearIcon"
        :on-clear-button-click="onClearButtonClick"
        :pattern="pattern"
        :shrink-label="shrinkLabel"
        :required="required"
      />
    </template>
  </MudInputControl>
</template>
