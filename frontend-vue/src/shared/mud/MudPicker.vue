<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, useId, useTemplateRef } from 'vue'
import { mdClose, mdEvent } from '@/shared/icons'
import MudOverlay from './MudOverlay.vue'
import MudPaper from './MudPaper.vue'
import MudPopover from './MudPopover.vue'
import MudTextField from './MudTextField.vue'
import type { MudAdornment, MudColor, MudOrigin, MudPickerVariant, MudVariant } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  label?: string | undefined
  helperText?: string | undefined
  error?: boolean
  errorText?: string | undefined
  editable?: boolean
  variant?: MudVariant
  adornment?: MudAdornment
  adornmentColor?: MudColor
  clearable?: boolean
  pickerVariant?: MudPickerVariant
  anchorOrigin?: MudOrigin
}>(), {
  label: '',
  helperText: '',
  error: false,
  errorText: '',
  editable: false,
  variant: 'text',
  adornment: 'end',
  adornmentColor: 'default',
  clearable: false,
  pickerVariant: 'inline',
  anchorOrigin: 'bottom-left',
})

const text = defineModel<string>({ default: '' })

const emit = defineEmits<{
  'picker-opened': []
  'picker-closed': []
  clear: [event: MouseEvent]
}>()

const pickerId = `picker-${useId()}`
const pickerElement = useTemplateRef<HTMLElement>('pickerElement')
const open = ref(false)

// TODO: вариант dialog рисуется как inline, свой MudPaper внутри mud-overlay-dialog – сделать, когда вариант понадобится хоть одному экрану
const isStatic = computed(() => props.pickerVariant === 'static')

const isInline = computed(() => props.pickerVariant === 'inline')

const pickerClasses = computed(() => [
  'mud-picker',
  {
    'mud-picker-inline': !isStatic.value,
    'mud-picker-static': isStatic.value,
    'mud-rounded': isStatic.value,
    'mud-elevation-0': !isInline.value,
    'mud-picker-input-text': props.editable && !isStatic.value,
    'mud-picker-input-button': !props.editable && !isStatic.value,
  },
])

const popoverClasses = ['mud-picker-popover', 'mud-elevation-8']

const paperClasses = computed(() => [
  'mud-picker',
  'mud-picker-paper',
  {
    'mud-picker-view': isInline.value,
    'mud-picker-popover-paper': isInline.value,
    'mud-picker-open': open.value && isInline.value,
    'mud-dialog': props.pickerVariant === 'dialog',
  },
])

const inputReadonly = computed(() => !props.editable)

const isInputControl = (event: MouseEvent): boolean => {
  if (!(event.target instanceof HTMLElement)) {
    return false
  }

  return event.target.closest('.mud-input-clear-button, .mud-input-adornment-icon-button') !== null
}

const openPicker = (): void => {
  if (open.value) {
    return
  }

  open.value = true
  emit('picker-opened')
}

const closePicker = (): void => {
  if (!open.value) {
    return
  }

  open.value = false
  emit('picker-closed')
}

const togglePicker = (event?: MouseEvent): void => {
  if (event !== undefined && isInputControl(event)) {
    if (event.target instanceof HTMLElement
      && event.target.closest('.mud-input-adornment-icon-button') !== null) {
      event.stopPropagation()
    } else {
      return
    }
  }

  if (open.value) {
    closePicker()
  } else {
    openPicker()
  }
}

const handlePickerClick = (event: MouseEvent): void => {
  if (!props.editable) {
    togglePicker(event)
  }
}

const handleKeydown = (event: KeyboardEvent): void => {
  if (event.key === 'Escape') {
    closePicker()
    return
  }

  if (open.value) {
    return
  }

  if (event.key === 'Enter' || event.key === ' ' || event.key === 'ArrowUp' || event.key === 'ArrowDown') {
    event.preventDefault()
    openPicker()
  }
}

const handleClear = (event: MouseEvent): void => {
  text.value = ''
  closePicker()
  emit('clear', event)
}

onMounted(() => {
  pickerElement.value?.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  pickerElement.value?.removeEventListener('keydown', handleKeydown)
})

defineExpose({
  open: openPicker,
  close: closePicker,
  toggle: () => togglePicker(),
})
</script>

<template>
  <div
    v-bind="$attrs"
    :id="pickerId"
    ref="pickerElement"
    :class="pickerClasses"
  >
    <slot
      v-if="!isStatic"
      name="input"
      :open="open"
      :toggle="togglePicker"
      :close="closePicker"
    >
      <MudTextField
        v-model="text"
        :label="label"
        :helper-text="helperText"
        :error="error"
        :error-text="errorText"
        :readonly="inputReadonly"
        :variant="variant"
        :adornment="adornment"
        :adornment-icon="mdEvent"
        :adornment-color="adornmentColor"
        :clearable="clearable && !inputReadonly"
        :clear-icon="mdClose"
        :on-adornment-click="togglePicker"
        :on-clear-button-click="handleClear"
        @click="handlePickerClick"
      />
    </slot>
    <MudPaper
      v-if="isStatic"
      :class="paperClasses"
    >
      <div class="mud-picker-container">
        <slot name="picker-content" />
      </div>
      <div
        v-if="$slots.actions"
        class="mud-picker-actions"
      >
        <slot name="actions" />
      </div>
    </MudPaper>
    <MudPopover
      v-else
      :open="open"
      fixed
      :paper="false"
      :anchor-origin="anchorOrigin"
      transform-origin="top-left"
      relative-width="ignore"
      :class="popoverClasses"
    >
      <div class="mud-picker-inline-paper">
        <MudPaper
          :class="paperClasses"
        >
          <div class="mud-picker-container">
            <slot name="picker-content" />
          </div>
          <div
            v-if="$slots.actions"
            class="mud-picker-actions"
          >
            <slot name="actions" />
          </div>
        </MudPaper>
      </div>
    </MudPopover>
    <MudOverlay
      v-if="isInline"
      v-model:visible="open"
      auto-close
      @closed="closePicker"
    />
  </div>
</template>
