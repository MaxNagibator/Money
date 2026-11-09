<script setup lang="ts">
import { computed } from 'vue'
import MudInputLabel from './MudInputLabel.vue'
import type { MudMargin, MudVariant } from './types'

const props = withDefaults(defineProps<{
  margin?: MudMargin
  required?: boolean
  error?: boolean
  errorText?: string | undefined
  errorId?: string | undefined
  helperText?: string | undefined
  helperId?: string | undefined
  helperTextOnFocus?: boolean
  counterText?: string | undefined
  fullWidth?: boolean
  label?: string | undefined
  variant?: MudVariant
  disabled?: boolean
  forId?: string | undefined
}>(), {
  margin: 'none',
  required: false,
  error: false,
  errorText: '',
  helperText: '',
  helperTextOnFocus: false,
  counterText: '',
  fullWidth: false,
  label: '',
  variant: 'text',
  disabled: false,
  forId: '',
})

const classes = computed(() => [
  'mud-input-control',
  {
    'mud-input-required': props.required,
    [`mud-input-control-margin-${props.margin}`]: props.margin !== 'none',
    'mud-input-control-full-width': props.fullWidth,
    'mud-input-error': props.error,
    [`mud-input-${props.variant}-with-label`]: props.label.length > 0,
  },
])

const helperContainerClasses = computed(() => [
  'mud-input-control-helper-container',
  {
    'px-1': props.variant === 'filled',
    'px-2': props.variant === 'outlined',
  },
])

const helperClasses = computed(() => [
  'd-flex',
  'mud-input-helper-text',
  {
    'mud-input-helper-onfocus': props.helperTextOnFocus,
    'mud-input-error': props.error,
  },
])

const hasHelperContainer = computed(() => props.error || props.helperText.length > 0 || props.counterText.length > 0)
</script>

<template>
  <div :class="classes">
    <div class="mud-input-control-input-container">
      <slot name="input-content" />
      <MudInputLabel
        v-if="label.length > 0"
        class="mud-input-label-inputcontrol"
        :variant="variant"
        :disabled="disabled"
        :error="error"
        :margin="margin"
        :for-id="forId"
      >
        {{ label }}
      </MudInputLabel>
    </div>
    <div
      v-if="hasHelperContainer"
      :class="helperContainerClasses"
    >
      <div :class="helperClasses">
        <div
          v-if="error"
          :id="errorId"
          class="me-auto"
        >
          {{ errorText }}
        </div>
        <div
          v-else-if="helperText.length > 0"
          :id="helperId"
          class="me-auto"
        >
          {{ helperText }}
        </div>
        <div
          v-if="counterText.length > 0"
          class="ms-auto"
        >
          {{ counterText }}
        </div>
      </div>
    </div>
    <slot />
  </div>
</template>
