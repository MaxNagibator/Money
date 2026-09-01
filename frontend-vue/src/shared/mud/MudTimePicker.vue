<script setup lang="ts">
import { computed, onBeforeUnmount, reactive, ref, useTemplateRef, watch } from 'vue'
import { formatTimeOfDay } from '@/domain/time-input'
import { mdAccessTime } from '@/shared/icons'
import MudButton from './MudButton.vue'
import MudPicker from './MudPicker.vue'
import MudPickerContent from './MudPickerContent.vue'
import MudPickerToolbar from './MudPickerToolbar.vue'
import MudText from './MudText.vue'
import MudTextField from './MudTextField.vue'
import './vendor/MudTimePicker.min.js'

type MudTimePickerView = 'hours' | 'minutes'

interface MudTimePickerInterop {
  initPointerEvents: (element: HTMLElement, target: MudTimePickerPointerTarget) => void
  destroyPointerEvents: (element: HTMLElement) => void
}

interface MudTimePickerPointerTarget {
  invokeMethodAsync: (method: string, value: number, pointerMoving?: boolean) => Promise<void>
}


withDefaults(defineProps<{
  label?: string | undefined
  helperText?: string | undefined
  error?: boolean
  errorText?: string | undefined
  disabled?: boolean
}>(), {
  label: '',
  helperText: '',
  error: false,
  errorText: '',
  disabled: false,
})

const model = defineModel<number | null>({ default: null })

const pickerReference = ref<{ open: () => void, close: () => void }>()
const clockElement = useTemplateRef<HTMLElement>('clockElement')
const inputText = ref('')
const currentView = ref<MudTimePickerView>('hours')
const timeIntermediate = ref<number | null>(null)
const pointerMoving = ref(false)
const timeSet = reactive({ hour: 0, minute: 0 })
let pointerElement: HTMLElement | undefined

const toClockMinutes = (value: number | null): number => {
  if (value === null || !Number.isFinite(value)) {
    return 0
  }

  return Math.min(1439, Math.max(0, Math.trunc(value)))
}

const setTimeSet = (value: number | null): void => {
  const minutes = toClockMinutes(value)
  timeSet.hour = Math.floor(minutes / 60)
  timeSet.minute = minutes % 60
}

watch(model, value => {
  inputText.value = value === null ? '' : formatTimeOfDay(value)
  timeIntermediate.value = value
  setTimeSet(value)
}, { immediate: true })

const hourString = computed(() => timeIntermediate.value === null ? '--' : String(timeSet.hour).padStart(2, '0'))
const minuteString = computed(() => timeIntermediate.value === null ? '--' : String(timeSet.minute).padStart(2, '0'))

const hoursButtonClasses = computed(() => [
  'mud-timepicker-button',
  {
    'mud-timepicker-toolbar-text': currentView.value === 'minutes',
  },
])

const minuteButtonClasses = computed(() => [
  'mud-timepicker-button',
  {
    'mud-timepicker-toolbar-text': currentView.value === 'hours',
  },
])

const hourDialClasses = computed(() => [
  'mud-time-picker-hour',
  'mud-time-picker-dial',
  {
    'mud-time-picker-dial-out': currentView.value !== 'hours',
    'mud-time-picker-dial-hidden': currentView.value !== 'hours',
  },
])

const minuteDialClasses = computed(() => [
  'mud-time-picker-minute',
  'mud-time-picker-dial',
  {
    'mud-time-picker-dial-out': currentView.value !== 'minutes',
    'mud-time-picker-dial-hidden': currentView.value !== 'minutes',
  },
])

const clockPointerClasses = computed(() => [
  'mud-picker-time-clock-pointer',
  {
    'mud-picker-time-clock-pointer-animation': !pointerMoving.value,
  },
  'mud-primary',
])

const clockPointerThumbClasses = computed(() => {
  const degrees = currentView.value === 'hours' ? timeSet.hour * 30 % 360 : timeSet.minute * 6 % 360

  return degrees % 30 === 0
    ? ['mud-picker-time-clock-pointer-thumb', 'mud-onclock-text', 'mud-onclock-primary', 'mud-primary']
    : ['mud-picker-time-clock-pointer-thumb', 'mud-onclock-minute', 'mud-primary-text']
})

const pointerHeight = computed(() => {
  if (currentView.value === 'hours' && timeSet.hour > 0 && timeSet.hour < 13) {
    return '26%'
  }

  return '40%'
})

const pointerRotation = computed(() => {
  const degrees = currentView.value === 'hours' ? timeSet.hour * 30 % 360 : timeSet.minute * 6 % 360
  return `rotateZ(${degrees}deg)`
})

const clockNumberClasses = (value: number) => [
  'mud-clock-number',
  {
    'mud-theme-primary': currentView.value === 'hours'
      ? timeSet.hour === value
      : timeSet.minute === value,
  },
]

const clockTransform = (angle: number, radius: number, offsetY: number): string => {
  const radians = angle / 180 * Math.PI
  const x = (Math.sin(radians) * radius).toFixed(3)
  const y = ((Math.cos(radians) + 1) * radius + offsetY).toFixed(3)
  return `translate(${x}px, ${y}px)`
}

const outerHour = (value: number): number => (value + 12) % 24

const updateIntermediate = (): void => {
  timeIntermediate.value = timeSet.hour * 60 + timeSet.minute
}

const handlePickerOpened = (): void => {
  currentView.value = 'hours'
  pointerMoving.value = false
  timeIntermediate.value = model.value
  setTimeSet(model.value)
}

const handlePickerClosed = (): void => {
  pointerMoving.value = false
  model.value = timeIntermediate.value
}

const selectHours = (): void => {
  currentView.value = 'hours'
}

const selectMinutes = (): void => {
  currentView.value = 'minutes'
}

const selectTimeFromStick = (value: number, moving: boolean): void => {
  if (value === -1) {
    return
  }

  pointerMoving.value = moving

  if (currentView.value === 'minutes') {
    timeSet.minute = Math.min(59, Math.max(0, value))
  } else {
    timeSet.hour = Math.min(23, Math.max(0, value))
  }

  updateIntermediate()
}

const handleStickClick = (value: number): void => {
  if (value === -1) {
    return
  }

  pointerMoving.value = false

  if (currentView.value === 'hours') {
    currentView.value = 'minutes'
    return
  }

  model.value = timeIntermediate.value
  pickerReference.value?.close()
}

const pointerTarget: MudTimePickerPointerTarget = {
  invokeMethodAsync: (method, value, moving) => {
    if (method === 'SelectTimeFromStick' && moving !== undefined) {
      selectTimeFromStick(value, moving)
    } else if (method === 'OnStickClick') {
      handleStickClick(value)
    }

    return Promise.resolve()
  },
}

const getInterop = (): MudTimePickerInterop | undefined =>
  (window as unknown as { mudTimePicker?: MudTimePickerInterop }).mudTimePicker

watch(clockElement, element => {
  if (pointerElement !== undefined && pointerElement !== element) {
    getInterop()?.destroyPointerEvents(pointerElement)
    pointerElement = undefined
  }

  if (element !== null) {
    getInterop()?.initPointerEvents(element, pointerTarget)
    pointerElement = element
  }
}, { flush: 'post' })

onBeforeUnmount(() => {
  if (pointerElement !== undefined) {
    getInterop()?.destroyPointerEvents(pointerElement)
  }
})

defineExpose({
  open: () => pickerReference.value?.open(),
  close: () => pickerReference.value?.close(),
})
</script>

<template>
  <MudPicker
    ref="pickerReference"
    picker-variant="inline"
    :class="{ 'mud-disabled': disabled }"
    @picker-opened="handlePickerOpened"
    @picker-closed="handlePickerClosed"
  >
    <template #input="{ toggle }">
      <MudTextField
        v-model="inputText"
        :label="label"
        :helper-text="helperText"
        :error="error"
        :error-text="errorText"
        :disabled="disabled"
        readonly
        variant="text"
        adornment="end"
        :adornment-icon="mdAccessTime"
        adornment-color="default"
        adornment-aria-label="Открыть"
        :on-adornment-click="toggle"
        @click="toggle"
      />
    </template>
    <template #picker-content>
      <MudPickerToolbar
        class="mud-picker-timepicker-toolbar"
        color="primary"
      >
        <div class="mud-timepicker-hourminute mud-ltr">
          <MudButton
            variant="text"
            color="inherit"
            :class="hoursButtonClasses"
            @click="selectHours"
          >
            {{ hourString }}
          </MudButton>
          <MudText
            typo="h2"
            class="mud-timepicker-separator"
          >
            :
          </MudText>
          <MudButton
            variant="text"
            color="inherit"
            :class="minuteButtonClasses"
            @click="selectMinutes"
          >
            {{ minuteString }}
          </MudButton>
        </div>
      </MudPickerToolbar>
      <MudPickerContent>
        <div class="mud-picker-time-container">
          <div class="mud-picker-time-clock">
            <div
              ref="clockElement"
              tabindex="-1"
              class="mud-picker-time-clock-mask"
              @contextmenu.prevent
            >
              <div class="mud-picker-time-clock-pin mud-primary" />
              <div
                :class="clockPointerClasses"
                :style="{ height: pointerHeight, transform: pointerRotation }"
              >
                <div :class="clockPointerThumbClasses" />
              </div>
              <div :class="hourDialClasses">
                <template
                  v-for="hour in 12"
                  :key="`outer-hour-${hour}`"
                >
                  <MudText
                    :class="clockNumberClasses(outerHour(hour))"
                    :style="{ transform: clockTransform((6 - hour) * 30, 109, 5) }"
                  >
                    {{ String(outerHour(hour)).padStart(2, '0') }}
                  </MudText>
                </template>
                <template
                  v-for="hour in 12"
                  :key="`inner-hour-${hour}`"
                >
                  <MudText
                    typo="body2"
                    :class="clockNumberClasses(hour)"
                    :style="{ transform: clockTransform((6 - hour) * 30, 74, 40) }"
                  >
                    {{ String(hour).padStart(2, '0') }}
                  </MudText>
                </template>
                <div
                  v-for="hour in 12"
                  :key="`hour-stick-${hour}`"
                  class="mud-picker-stick"
                  :style="{ transform: `rotateZ(${hour * 30}deg)` }"
                >
                  <div
                    class="mud-picker-stick-inner mud-hour"
                    :data-stick-value="hour"
                  />
                  <div
                    class="mud-picker-stick-outer mud-hour"
                    :data-stick-value="outerHour(hour)"
                  />
                </div>
              </div>
              <div :class="minuteDialClasses">
                <MudText
                  v-for="index in 12"
                  :key="`minute-number-${index}`"
                  :class="clockNumberClasses((index - 1) * 5)"
                  :style="{ transform: clockTransform((7 - index) * 30, 109, 5) }"
                >
                  {{ String((index - 1) * 5).padStart(2, '0') }}
                </MudText>
                <div
                  v-for="minute in 60"
                  :key="`minute-stick-${minute - 1}`"
                  class="mud-picker-stick mud-minute"
                  :style="{ transform: `rotateZ(${(minute - 1) * 6}deg)` }"
                  :data-stick-value="minute - 1"
                />
              </div>
            </div>
          </div>
        </div>
      </MudPickerContent>
    </template>
  </MudPicker>
</template>
