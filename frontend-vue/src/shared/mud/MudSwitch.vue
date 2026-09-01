<script setup lang="ts">
import { computed, useAttrs, useId } from 'vue'
import MudInputControl from './MudInputControl.vue'
import MudText from './MudText.vue'
import type { MudColor, MudSize } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  color?: MudColor
  size?: MudSize
  disabled?: boolean
  label?: string
}>(), {
  color: 'default',
  size: 'medium',
  disabled: false,
  label: '',
})

const model = defineModel<boolean | null>({ default: false })
const attrs = useAttrs()
const elementId = `switch-${useId()}`
const inputId = computed(() => typeof attrs.id === 'string' ? attrs.id : `${elementId}-input`)

const isRootAttribute = (name: string): boolean => name === 'class' || name === 'style' || name.startsWith('data-')

const rootAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => isRootAttribute(name)),
))

const inputAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => !isRootAttribute(name)),
))

const labelClasses = computed(() => [
  'mud-switch',
  {
    'mud-disabled': props.disabled,
  },
  'mud-input-content-placement-end',
])

const switchClasses = computed(() => [
  'mud-button-root mud-icon-button mud-switch-base',
  {
    'mud-ripple mud-ripple-switch': !props.disabled,
    [`mud-${props.color}-text hover:mud-${props.color}-hover`]: !props.disabled && model.value === true,
    'mud-default-text hover:mud-default-hover': !props.disabled && model.value === false,
    'mud-switch-disabled': props.disabled,
    'mud-checked': model.value === true,
  },
  `mud-switch-base-${props.size}`,
])

const trackClasses = computed(() => [
  'mud-switch-track',
  model.value === true ? `mud-${props.color}` : 'mud-default',
])

const thumbClasses = computed(() => [
  `mud-switch-thumb-${props.size}`,
  'd-flex align-center justify-center',
])

const spanClasses = computed(() => [
  'mud-switch-span',
  `mud-switch-span-${props.size}`,
])

const ariaCheckedState = computed(() => {
  if (model.value === null) {
    return 'mixed'
  }

  return model.value ? 'true' : 'false'
})

const handleChange = (event: Event): void => {
  if (event.target instanceof HTMLInputElement && !props.disabled) {
    model.value = event.target.checked
  }
}

const handleKeydown = (event: KeyboardEvent): void => {
  if (event.key === 'ArrowUp' || event.key === 'ArrowDown' || event.key === ' ') {
    event.preventDefault()
  }

  if (props.disabled) {
    return
  }

  if (event.key === 'ArrowLeft' || event.key === 'Delete') {
    model.value = false
  } else if (event.key === 'ArrowRight' || event.key === 'Enter' || event.key === 'NumpadEnter') {
    model.value = true
  } else if (event.key === ' ') {
    model.value = model.value === null ? null : !model.value
  }
}
</script>

<template>
  <MudInputControl
    v-bind="rootAttrs"
    class="mud-input-control-boolean-input"
  >
    <template #input-content>
      <label
        :id="elementId"
        :for="inputId"
        :style="attrs.style"
        :class="labelClasses"
      >
        <span :class="spanClasses">
          <span :class="switchClasses">
            <span class="mud-switch-button">
              <input
                v-bind="inputAttrs"
                :id="inputId"
                :aria-readonly="false"
                role="switch"
                :aria-checked="ariaCheckedState"
                type="checkbox"
                class="mud-switch-input"
                :checked="model === true"
                :tabindex="disabled ? -1 : 0"
                :disabled="disabled"
                @keydown="handleKeydown"
                @change="handleChange"
              >
              <span :class="thumbClasses" />
            </span>
          </span>
          <span :class="trackClasses" />
        </span>
        <MudText
          v-if="label.length > 0"
          html-tag="span"
          :class="labelClasses"
          aria-hidden="false"
          color="inherit"
        >
          {{ label }}
        </MudText>
        <MudText
          v-if="$slots.default"
          html-tag="span"
          :class="labelClasses"
          aria-hidden="false"
          color="inherit"
        >
          <slot />
        </MudText>
      </label>
    </template>
  </MudInputControl>
</template>
