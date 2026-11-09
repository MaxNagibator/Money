<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, useTemplateRef, watch } from 'vue'
import { mdClose, mdExpandLess, mdExpandMore } from '@/shared/icons'
import MudButton from './MudButton.vue'
import MudIcon from './MudIcon.vue'
import MudIconButton from './MudIconButton.vue'
import MudInputAdornment from './MudInputAdornment.vue'
import type { MudAdornment, MudColor, MudInputMode, MudInputSizing, MudInputType, MudMargin, MudSize, MudTypo, MudVariant } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  inputType?: MudInputType
  inputMode?: MudInputMode
  inputId?: string | undefined
  label?: string | undefined
  placeholder?: string | undefined
  variant?: MudVariant
  typo?: MudTypo
  margin?: MudMargin
  adornment?: MudAdornment
  adornmentText?: string | undefined
  adornmentIcon?: string | undefined
  adornmentColor?: MudColor
  adornmentAriaLabel?: string | undefined
  adornmentTestId?: string | undefined
  iconSize?: MudSize
  disabled?: boolean
  readonly?: boolean
  underline?: boolean
  immediate?: boolean
  error?: boolean
  errorId?: string | undefined
  helperId?: string | undefined
  required?: boolean
  shrinkLabel?: boolean
  pattern?: string | undefined
  lines?: number
  sizing?: MudInputSizing
  hideSpinButtons?: boolean
  clearable?: boolean
  clearIcon?: string
  subscribeToParentForm?: boolean
  numericUpIcon?: string
  numericDownIcon?: string
  onAdornmentClick?: ((event: MouseEvent) => void) | undefined
  onClearButtonClick?: ((event: MouseEvent) => void) | undefined
  onIncrement?: (() => void) | undefined
  onDecrement?: (() => void) | undefined
  onMouseWheel?: ((event: WheelEvent) => void) | undefined
}>(), {
  inputType: 'text',
  inputMode: 'text',
  label: '',
  variant: 'text',
  typo: 'subtitle1',
  margin: 'none',
  adornment: 'none',
  adornmentText: '',
  adornmentIcon: '',
  adornmentColor: 'default',
  iconSize: 'medium',
  disabled: false,
  readonly: false,
  underline: true,
  immediate: false,
  error: false,
  required: false,
  shrinkLabel: false,
  lines: 1,
  sizing: 'fixed',
  hideSpinButtons: true,
  clearable: false,
  clearIcon: mdClose,
  subscribeToParentForm: true,
  numericUpIcon: mdExpandLess,
  numericDownIcon: mdExpandMore,
})

const model = defineModel<string>({ default: '' })

const generatedId = useId()
const inputElement = useTemplateRef<HTMLInputElement | HTMLTextAreaElement>('inputElement')
const internalText = ref(model.value)

watch(model, value => {
  internalText.value = value
})

const inputElementId = computed(() => props.inputId ?? generatedId)

const shouldUseTextArea = computed(() => props.sizing !== 'fixed' || props.lines > 1)

const hasNativePlaceholder = computed(() => ['color', 'date', 'datetime-local', 'month', 'time', 'week'].includes(props.inputType))

const shrink = computed(() => hasNativePlaceholder.value
  || internalText.value.length > 0
  || props.adornment === 'start'
  || (props.placeholder ?? '').trim().length > 0
  || props.shrinkLabel)

const classes = computed(() => [
  'mud-input',
  `mud-input-${props.variant}`,
  {
    [`mud-input-${props.variant}-with-label`]: props.label.length > 0,
    [`mud-input-adorned-${props.adornment}`]: props.adornment !== 'none',
    [`mud-input-margin-${props.margin}`]: props.margin !== 'none',
    'mud-input-underline': props.underline && props.variant !== 'outlined',
    'mud-shrink': shrink.value,
    'mud-disabled': props.disabled,
    'mud-input-error': props.error,
    'mud-ltr': props.inputType === 'email' || props.inputType === 'tel',
  },
  `mud-typography-${props.typo}`,
  `mud-input-sizing-${props.sizing}`,
])

const inputClasses = computed(() => [
  'mud-input-slot',
  'mud-input-root',
  `mud-input-root-${props.variant}`,
  {
    [`mud-input-root-adorned-${props.adornment}`]: props.adornment !== 'none',
    [`mud-input-root-margin-${props.margin}`]: props.margin !== 'none',
  },
])

const adornmentClasses = computed(() => [
  {
    [`mud-input-adornment-${props.adornment}`]: props.adornment !== 'none',
    'mud-text': props.adornmentText.length > 0,
    'mud-input-root-filled-shrink': props.variant === 'filled',
  },
])

const clearButtonClasses = computed(() => [
  'mud-input-clear-button',
  {
    'me-n1': props.adornment === 'end' && !props.hideSpinButtons,
    'mud-icon-button-edge-end': props.adornment === 'end' && props.hideSpinButtons,
    'me-6': props.adornment !== 'end' && !props.hideSpinButtons,
    'mud-icon-button-edge-margin-end': props.adornment !== 'end' && props.hideSpinButtons,
  },
  'mud-no-activator',
])

const showClearButton = computed(() => !props.disabled
  && !(props.subscribeToParentForm && props.readonly)
  && props.clearable
  && internalText.value.trim().length > 0)

const buttonSize = computed<MudSize>(() => (props.margin === 'dense' ? 'small' : 'medium'))

const ariaDescribedBy = computed(() => {
  const errorId = props.error ? props.errorId : undefined
  const parts = [errorId, props.helperId].filter(part => part !== undefined)

  return parts.length > 0 ? parts.join(' ') : undefined
})

let metrics: { lineHeight: number, padding: number } | null = null
let scheduledFrame: number | null = null

const readMetrics = (element: HTMLTextAreaElement): { lineHeight: number, padding: number } => {
  if (metrics === null) {
    const style = getComputedStyle(element)

    metrics = {
      lineHeight: Number.parseFloat(style.lineHeight) || Number.parseFloat(style.fontSize) || 0,
      padding: (Number.parseFloat(style.paddingTop) || 0) + (Number.parseFloat(style.paddingBottom) || 0),
    }
  }

  return metrics
}

const adjustHeight = (): void => {
  const element = inputElement.value

  if (!(element instanceof HTMLTextAreaElement)) {
    return
  }

  const { lineHeight, padding } = readMetrics(element)

  element.style.overflowY = 'hidden'
  element.style.height = '0'
  element.style.height = `${Math.max(lineHeight * element.rows + padding, element.scrollHeight)}px`
}

const scheduleAdjustHeight = (): void => {
  if (!shouldUseTextArea.value || scheduledFrame !== null) {
    return
  }

  scheduledFrame = requestAnimationFrame(() => {
    scheduledFrame = null
    adjustHeight()
  })
}

const handleResize = (): void => {
  metrics = null
  scheduleAdjustHeight()
}

const setText = (text: string): void => {
  internalText.value = text
  model.value = text
}

const resetText = (text: string): void => {
  internalText.value = text

  if (inputElement.value !== null) {
    inputElement.value.value = text
  }
}

const handleInput = (event: Event): void => {
  if (!(event.target instanceof HTMLInputElement) && !(event.target instanceof HTMLTextAreaElement)) {
    return
  }

  scheduleAdjustHeight()

  if (props.immediate) {
    setText(event.target.value)
  }
}

const handleChange = (event: Event): void => {
  if (props.immediate) {
    return
  }

  if (event.target instanceof HTMLInputElement || event.target instanceof HTMLTextAreaElement) {
    setText(event.target.value)
  }
}

const handleClear = (event: MouseEvent): void => {
  setText('')
  inputElement.value?.focus()
  props.onClearButtonClick?.(event)
}

watch(internalText, () => {
  if (shouldUseTextArea.value) {
    void nextTick(scheduleAdjustHeight)
  }
})

onMounted(() => {
  if (!shouldUseTextArea.value) {
    return
  }

  adjustHeight()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)

  if (scheduledFrame !== null) {
    cancelAnimationFrame(scheduledFrame)
  }
})

defineExpose({
  focus: () => inputElement.value?.focus(),
  blur: () => inputElement.value?.blur(),
  select: () => inputElement.value?.select(),
  setText: resetText,
  element: inputElement,
})
</script>

<template>
  <div :class="classes">
    <MudInputAdornment
      v-if="adornment === 'start'"
      :class="adornmentClasses"
      :icon="adornmentIcon"
      :color="adornmentColor"
      :size="iconSize"
      :text="adornmentText"
      placement="start"
      :aria-label="adornmentAriaLabel"
      :test-id="adornmentTestId"
      :on-adornment-click="onAdornmentClick"
    />

    <textarea
      v-if="shouldUseTextArea"
      :id="inputElementId"
      ref="inputElement"
      v-bind="$attrs"
      :class="inputClasses"
      :rows="lines"
      :value="internalText"
      :placeholder="placeholder"
      :disabled="disabled"
      :readonly="readonly"
      :inputmode="inputMode"
      :aria-describedby="ariaDescribedBy"
      :aria-invalid="error ? 'true' : 'false'"
      :required="required"
      :aria-required="required ? 'true' : 'false'"
      @input="handleInput"
      @change="handleChange"
      @wheel="onMouseWheel?.($event)"
    />
    <input
      v-else
      :id="inputElementId"
      ref="inputElement"
      v-bind="$attrs"
      :class="inputClasses"
      :type="inputType"
      :value="internalText"
      :placeholder="placeholder"
      :disabled="disabled"
      :readonly="readonly"
      :inputmode="inputType === 'number' ? undefined : inputMode"
      :pattern="pattern"
      :aria-describedby="ariaDescribedBy"
      :aria-invalid="error ? 'true' : 'false'"
      :required="required"
      :aria-required="required ? 'true' : 'false'"
      @input="handleInput"
      @change="handleChange"
      @wheel="onMouseWheel?.($event)"
    >

    <MudIconButton
      v-if="showClearButton"
      :class="clearButtonClasses"
      color="default"
      :icon="clearIcon"
      size="small"
      aria-label="Очистить"
      tabindex="-1"
      @click="handleClear"
      @mousedown.stop
    />

    <MudInputAdornment
      v-if="adornment === 'end'"
      :class="adornmentClasses"
      :icon="adornmentIcon"
      :color="adornmentColor"
      :size="iconSize"
      :text="adornmentText"
      placement="end"
      :aria-label="adornmentAriaLabel"
      :test-id="adornmentTestId"
      :on-adornment-click="onAdornmentClick"
    />

    <fieldset
      v-if="variant === 'outlined'"
      class="mud-input-outlined-border"
    >
      <legend v-if="label.length > 0">
        {{ label }}
      </legend>
    </fieldset>

    <div
      v-if="!hideSpinButtons"
      class="mud-input-numeric-spin"
    >
      <MudButton
        variant="text"
        :disabled="disabled || readonly"
        tabindex="-1"
        aria-label="Увеличить"
        @click="onIncrement?.()"
        @mousedown.prevent
      >
        <MudIcon
          :icon="numericUpIcon"
          :size="buttonSize"
        />
      </MudButton>

      <MudButton
        variant="text"
        :disabled="disabled || readonly"
        tabindex="-1"
        aria-label="Уменьшить"
        @click="onDecrement?.()"
        @mousedown.prevent
      >
        <MudIcon
          :icon="numericDownIcon"
          :size="buttonSize"
        />
      </MudButton>
    </div>
  </div>
</template>
