<script setup lang="ts">
import { computed, ref, useTemplateRef, watch } from 'vue'
import { mdApps, mdClose, mdImportExport, mdPalette, mdTune } from '@/shared/icons'
import MudIconButton from './MudIconButton.vue'
import MudNumericField from './MudNumericField.vue'
import MudPicker from './MudPicker.vue'
import MudPickerContent from './MudPickerContent.vue'
import MudPickerToolbar from './MudPickerToolbar.vue'
import MudSlider from './MudSlider.vue'
import MudSpacer from './MudSpacer.vue'
import MudTextField from './MudTextField.vue'
import MudTooltip from './MudTooltip.vue'
import type { MudColorPickerMode, MudPickerVariant } from './types'

type MudColorPickerView = 'spectrum' | 'palette' | 'grid'

interface ColorValue {
  red: number
  green: number
  blue: number
  alpha: number
}

const MAX_X = 312

const MAX_Y = 250

const PALETTE = [
  '#424242', '#2196f3', '#00c853', '#ff9800', '#f44336',
  '#f6f9fb', '#9df1fa', '#bdffcf', '#fff0a3', '#ffd254',
  '#e6e9eb', '#27dbf5', '#7ef7a0', '#ffe273', '#ffb31f',
  '#c9cccf', '#13b8e8', '#14dc71', '#fdd22f', '#ff9102',
  '#858791', '#0989c2', '#1bbd66', '#ebb323', '#fe6800',
  '#585b62', '#17698e', '#17a258', '#d9980d', '#dc3f11',
  '#353940', '#113b53', '#127942', '#bf7d11', '#aa0000',
]

const GRID = [
  '#FFFFFF', '#ebebeb', '#d6d6d6', '#c2c2c2', '#adadad', '#999999', '#858586', '#707070', '#5c5c5c', '#474747', '#333333', '#000000',
  '#133648', '#071d53', '#0f0638', '#2a093b', '#370c1b', '#541107', '#532009', '#53350d', '#523e0f', '#65611b', '#505518', '#2b3d16',
  '#1e4c63', '#0f2e76', '#180b4e', '#3f1256', '#4e1629', '#781e0e', '#722f10', '#734c16', '#73591a', '#8c8629', '#707625', '#3f5623',
  '#2e6c8c', '#1841a3', '#280c72', '#591e77', '#6f223d', '#a62c17', '#a0451a', '#a06b23', '#9f7d28', '#c3bc3c', '#9da436', '#587934',
  '#3c8ab0', '#2155ce', '#331c8e', '#702898', '#8d2e4f', '#d03a20', '#ca5a24', '#c8862e', '#c99f35', '#f3ec4e', '#c6d047', '#729b44',
  '#479fd3', '#2660f5', '#4725ab', '#8c33b5', '#aa395d', '#eb512e', '#ed732e', '#f3ae3d', '#f5c944', '#fefb67', '#ddeb5c', '#86b953',
  '#59c4f7', '#4e85f6', '#5733e2', '#af43eb', '#d44a7a', '#ed6c59', '#ef8c56', '#f3b757', '#f6cd5b', '#fef881', '#e6ee7a', '#a3d16e',
  '#78d3f8', '#7fa6f8', '#7e52f5', '#c45ff6', '#de789d', '#f09286', '#f2a984', '#f6c983', '#f9da85', '#fef9a1', '#ebf29b', '#badc94',
  '#a5e1fa', '#adc5fa', '#ab8df7', '#d696f8', '#e8a7bf', '#f4b8b1', '#f6c7af', '#f9daae', '#fae5af', '#fefbc0', '#f3f7be', '#d2e7ba',
  '#d2effd', '#d6e1fc', '#d6c9fa', '#e9cbfb', '#f3d4df', '#f9dcd9', '#fae3d8', '#fcecd7', '#fdf2d8', '#fefce0', '#f7fade', '#e3edd6',
]

const props = withDefaults(defineProps<{
  colorPickerMode?: MudColorPickerMode
  colorPickerView?: MudColorPickerView
  pickerVariant?: MudPickerVariant
  rounded?: boolean
  required?: boolean
  showAlpha?: boolean
  showColorField?: boolean
  showInputs?: boolean
  showModeSwitch?: boolean
  showPreview?: boolean
  showSliders?: boolean
  showToolbar?: boolean
}>(), {
  colorPickerMode: 'rgb',
  colorPickerView: 'spectrum',
  pickerVariant: 'inline',
  rounded: false,
  required: false,
  showAlpha: true,
  showColorField: true,
  showInputs: true,
  showModeSwitch: true,
  showPreview: true,
  showSliders: true,
  showToolbar: false,
})

const model = defineModel<string | null>({ default: null })
const pickerReference = ref<{ close: () => void }>()
const overlay = useTemplateRef<HTMLElement>('overlay')
const colorView = ref<MudColorPickerView>(props.colorPickerView)
const pickerMode = ref<MudColorPickerMode>(props.colorPickerMode)
const collectionOpen = ref(false)
const isDragging = ref(false)

const parseColor = (value: string | null): ColorValue | null => {
  if (value === null) {
    return null
  }

  const match = /^#([\da-f]{3,4}|[\da-f]{6}|[\da-f]{8})$/i.exec(value.trim())
  if (match === null) {
    return null
  }

  const digits = match[1] ?? ''
  const expanded = digits.length <= 4
    ? digits.split('').map(part => `${part}${part}`).join('')
    : digits

  return {
    red: Number.parseInt(expanded.slice(0, 2), 16),
    green: Number.parseInt(expanded.slice(2, 4), 16),
    blue: Number.parseInt(expanded.slice(4, 6), 16),
    alpha: expanded.length === 8 ? Number.parseInt(expanded.slice(6, 8), 16) : 255,
  }
}

const DEFAULT_COLOR: ColorValue = { red: 89, green: 74, blue: 226, alpha: 255 }

const value = ref<ColorValue | null>(null)
const lastColor = ref<ColorValue | null>(null)
const current = computed(() => value.value ?? lastColor.value ?? DEFAULT_COLOR)

const clamp = (part: number, limit: number): number => Math.min(Math.max(0, part), limit)

const channel = (part: number): number => clamp(Math.round(part), 255)

const formatHex = (color: ColorValue, includeAlpha: boolean): string => {
  const hex = (part: number): string => channel(part).toString(16).padStart(2, '0')
  const base = `#${hex(color.red)}${hex(color.green)}${hex(color.blue)}`
  return includeAlpha ? `${base}${hex(color.alpha)}` : base
}

const hueOf = (color: ColorValue): number => {
  const red = color.red / 255
  const green = color.green / 255
  const blue = color.blue / 255
  const max = Math.max(red, green, blue)
  const min = Math.min(red, green, blue)
  const span = max - min

  if (span === 0) {
    return 0
  }

  let hue: number

  if (max === red) {
    hue = ((green - blue) / span) % 6
  } else if (max === green) {
    hue = (blue - red) / span + 2
  } else {
    hue = (red - green) / span + 4
  }

  hue *= 60

  return hue < 0 ? hue + 360 : hue
}

const lightnessOf = (color: ColorValue): number => {
  const max = Math.max(color.red, color.green, color.blue) / 255
  const min = Math.min(color.red, color.green, color.blue) / 255

  return (max + min) / 2
}

const saturationOf = (color: ColorValue): number => {
  const max = Math.max(color.red, color.green, color.blue) / 255
  const min = Math.min(color.red, color.green, color.blue) / 255
  const span = max - min

  if (span === 0) {
    return 0
  }

  const lightness = (max + min) / 2

  return span / (1 - Math.abs(2 * lightness - 1))
}

const fromHsl = (hue: number, saturation: number, lightness: number, alpha: number): ColorValue => {
  const chroma = (1 - Math.abs(2 * lightness - 1)) * saturation
  const sector = ((hue % 360) + 360) % 360 / 60
  const second = chroma * (1 - Math.abs(sector % 2 - 1))
  const offset = lightness - chroma / 2

  const parts: readonly [number, number, number] = sector < 1
    ? [chroma, second, 0]
    : sector < 2
      ? [second, chroma, 0]
      : sector < 3
        ? [0, chroma, second]
        : sector < 4
          ? [0, second, chroma]
          : sector < 5
            ? [second, 0, chroma]
            : [chroma, 0, second]

  return {
    red: channel((parts[0] + offset) * 255),
    green: channel((parts[1] + offset) * 255),
    blue: channel((parts[2] + offset) * 255),
    alpha,
  }
}

const hueChannels = (index: number, part: number): ColorValue => {
  switch (index) {
    case 0:
      return { red: 255, green: part, blue: 0, alpha: 255 }
    case 1:
      return { red: 255 - part, green: 255, blue: 0, alpha: 255 }
    case 2:
      return { red: 0, green: 255, blue: part, alpha: 255 }
    case 3:
      return { red: 0, green: 255 - part, blue: 255, alpha: 255 }
    case 4:
      return { red: part, green: 0, blue: 255, alpha: 255 }
    default:
      return { red: 255, green: 0, blue: 255 - part, alpha: 255 }
  }
}

const dominantValues = (index: number, color: ColorValue): readonly [number, number] => {
  switch (index) {
    case 0:
      return [color.red, color.blue]
    case 1:
      return [color.green, color.blue]
    case 2:
      return [color.green, color.red]
    case 3:
      return [color.blue, color.red]
    case 4:
      return [color.blue, color.green]
    default:
      return [color.red, color.green]
  }
}

const baseColorOf = (color: ColorValue): ColorValue => {
  const hue = Math.trunc(hueOf(color))
  const index = Math.min(Math.trunc(hue / 60), 5)
  const part = Math.trunc((hue - index * 60) / 60 * 255)

  return hueChannels(index, part)
}

const selectorOf = (color: ColorValue): readonly [number, number] => {
  const index = Math.min(Math.trunc(hueOf(color) / 360 * 6 * 255 / 255), 5)
  const [primary, secondary] = dominantValues(index, color)
  const primaryDelta = primary / 255
  const secondaryX = primaryDelta === 0 ? 0 : secondary / primaryDelta

  return [
    clamp((255 - secondaryX) / 255 * MAX_X, MAX_X),
    clamp((255 - primary) / 255 * MAX_Y, MAX_Y),
  ]
}

const initialSelector = selectorOf(current.value)

const baseColor = ref<ColorValue>(baseColorOf(current.value))
const selectorX = ref(initialSelector[0])
const selectorY = ref(initialSelector[1])

const colorText = (color: ColorValue | null): string => {
  if (color === null) {
    return ''
  }

  return formatHex(color, props.showAlpha && colorView.value !== 'palette')
}

const setColor = (next: ColorValue | null, skipFeedback = false): void => {
  value.value = next

  if (next !== null) {
    lastColor.value = next

    if (!skipFeedback) {
      baseColor.value = baseColorOf(next)
      const [x, y] = selectorOf(next)
      selectorX.value = x
      selectorY.value = y
    }
  }

  model.value = next === null ? null : colorText(next)
}

watch(() => props.colorPickerMode, next => {
  pickerMode.value = next
})

watch(() => props.colorPickerView, next => {
  colorView.value = next
})

const colorRgb = (color: ColorValue): string => `rgb(${channel(color.red)},${channel(color.green)},${channel(color.blue)})`

const colorRgba = (value: string): string => {
  const parsed = parseColor(value)
  if (parsed === null) {
    return value
  }

  return `rgba(${parsed.red}, ${parsed.green}, ${parsed.blue}, ${parsed.alpha / 255})`
}

const colorLabel = (value: string): string => {
  const parsed = parseColor(value)
  return parsed === null ? value : formatHex(parsed, true)
}

const isSelected = (item: string): boolean => {
  const parsed = parseColor(item)
  const selected = value.value

  return parsed !== null
    && selected !== null
    && parsed.red === selected.red
    && parsed.green === selected.green
    && parsed.blue === selected.blue
    && parsed.alpha === selected.alpha
}

const dotClasses = (item: string) => [
  'mud-picker-color-dot',
  {
    selected: isSelected(item),
  },
]

const selectColor = (item: string): void => {
  const parsed = parseColor(item)

  if (parsed !== null) {
    setColor(parsed)
  }

  collectionOpen.value = false
}

const buttonColor = (view: MudColorPickerView) => colorView.value === view ? 'primary' : 'inherit'

const changeView = (view: MudColorPickerView): void => {
  colorView.value = view
}

const changeMode = (): void => {
  pickerMode.value = pickerMode.value === 'rgb' ? 'hsl' : pickerMode.value === 'hsl' ? 'hex' : 'rgb'
}

const toggleCollection = (): void => {
  collectionOpen.value = !collectionOpen.value
}

const closePicker = (): void => {
  pickerReference.value?.close()
}

const overlayStyle = computed(() => ({ backgroundColor: colorRgb(baseColor.value) }))

const selectorStyle = computed(() => ({
  transform: `translate(${Math.round(selectorX.value * 100) / 100}px, ${Math.round(selectorY.value * 100) / 100}px)`,
}))

const previewStyle = computed(() => ({ background: `rgba(${current.value.red}, ${current.value.green}, ${current.value.blue}, ${current.value.alpha / 255})` }))

const alphaSliderStyle = computed(() => ({
  backgroundImage: `linear-gradient(to right, transparent, ${colorRgb(current.value)})`,
}))

const updateColorFromSelection = (): void => {
  const base = baseColor.value
  const x = selectorX.value / MAX_X
  const y = 1 - selectorY.value / MAX_Y

  const redX = 255 - Math.trunc((255 - base.red) * x)
  const greenX = 255 - Math.trunc((255 - base.green) * x)
  const blueX = 255 - Math.trunc((255 - base.blue) * x)

  setColor({
    red: Math.trunc(redX * y),
    green: Math.trunc(greenX * y),
    blue: Math.trunc(blueX * y),
    alpha: current.value.alpha,
  }, true)
}

const moveSelector = (event: PointerEvent): void => {
  const element = overlay.value

  if (element === null) {
    return
  }

  const bounds = element.getBoundingClientRect()

  selectorX.value = clamp(event.clientX - bounds.left, MAX_X)
  selectorY.value = clamp(event.clientY - bounds.top, MAX_Y)
}

const onOverlayPointerDown = (event: PointerEvent): void => {
  isDragging.value = true
  overlay.value?.setPointerCapture(event.pointerId)
  moveSelector(event)
  updateColorFromSelection()
}

const onOverlayPointerMove = (event: PointerEvent): void => {
  if (!isDragging.value) {
    return
  }

  moveSelector(event)
  updateColorFromSelection()
}

const onOverlayPointerUp = (event: PointerEvent): void => {
  if (!isDragging.value) {
    return
  }

  isDragging.value = false
  overlay.value?.releasePointerCapture(event.pointerId)
  moveSelector(event)
  updateColorFromSelection()
}

const onOverlayPointerLeave = (): void => {
  isDragging.value = false
}

const hue = computed({
  get: () => Math.trunc(hueOf(current.value)),
  set: next => {
    if (next === Math.trunc(hueOf(current.value))) {
      return
    }

    setColor(fromHsl(next, saturationOf(current.value), lightnessOf(current.value), current.value.alpha))
  },
})

const alpha = computed({
  get: () => current.value.alpha,
  set: next => {
    setColor({ ...current.value, alpha: next })
  },
})

const red = computed<number | null>({
  get: () => current.value.red,
  set: next => setColor({ ...current.value, red: channel(next ?? 0) }),
})

const green = computed<number | null>({
  get: () => current.value.green,
  set: next => setColor({ ...current.value, green: channel(next ?? 0) }),
})

const blue = computed<number | null>({
  get: () => current.value.blue,
  set: next => setColor({ ...current.value, blue: channel(next ?? 0) }),
})

const hueInput = computed<number | null>({
  get: () => Math.round(hueOf(current.value) * 100) / 100,
  set: next => setColor(fromHsl(next ?? 0, saturationOf(current.value), lightnessOf(current.value), current.value.alpha)),
})

const saturationInput = computed<number | null>({
  get: () => Math.round(saturationOf(current.value) * 100) / 100,
  set: next => setColor(fromHsl(hueOf(current.value), next ?? 0, lightnessOf(current.value), current.value.alpha)),
})

const lightnessInput = computed<number | null>({
  get: () => Math.round(lightnessOf(current.value) * 100) / 100,
  set: next => setColor(fromHsl(hueOf(current.value), saturationOf(current.value), next ?? 0, current.value.alpha)),
})

const alphaInput = computed<number | null>({
  get: () => Math.round(current.value.alpha / 255 * 100) / 100,
  set: next => setColor({ ...current.value, alpha: channel((next ?? 0) * 255) }),
})

const hexInput = computed({
  get: () => colorText(value.value),
  set: next => setColor(parseColor(next)),
})

const hexMaxLength = computed(() => props.showAlpha ? 9 : 7)

const inputText = computed(() => model.value ?? '')

const isControlVisible = computed(() => props.showSliders || props.showInputs || props.showPreview)
</script>

<template>
  <MudPicker
    ref="pickerReference"
    :picker-variant="pickerVariant"
    :class="{ 'mud-rounded': rounded }"
  >
    <template #input="{ toggle }">
      <MudTextField
        v-if="pickerVariant !== 'static'"
        :model-value="inputText"
        readonly
        variant="text"
        adornment="end"
        :adornment-icon="mdPalette"
        adornment-color="default"
        adornment-aria-label="Открыть"
        :on-adornment-click="toggle"
        @click="toggle"
        @update:model-value="model = $event"
      />
    </template>
    <template #picker-content>
      <MudPickerToolbar
        v-if="showToolbar"
        class="mud-picker-color-toolbar"
      >
        <MudIconButton
          v-if="pickerVariant !== 'static'"
          class="pa-1 mud-close-picker-button"
          size="small"
          color="inherit"
          :icon="mdClose"
          aria-label="Закрыть"
          @click="closePicker"
        />
        <MudSpacer />
        <MudTooltip text="Спектр">
          <MudIconButton
            class="pa-1"
            size="small"
            :color="buttonColor('spectrum')"
            :icon="mdTune"
            aria-label="Спектр"
            @click="changeView('spectrum')"
          />
        </MudTooltip>
        <MudTooltip text="Сетка цветов">
          <MudIconButton
            class="pa-1 mx-1"
            size="small"
            :color="buttonColor('grid')"
            :icon="mdApps"
            aria-label="Сетка цветов"
            @click="changeView('grid')"
          />
        </MudTooltip>
        <MudTooltip text="Палитра цветов">
          <MudIconButton
            class="pa-1"
            size="small"
            :color="buttonColor('palette')"
            :icon="mdPalette"
            aria-label="Палитра цветов"
            @click="changeView('palette')"
          />
        </MudTooltip>
      </MudPickerToolbar>
      <MudPickerContent class="mud-picker-color-content">
        <div
          v-if="showColorField"
          class="mud-picker-color-picker"
        >
          <div
            v-if="colorView === 'spectrum'"
            class="mud-picker-color-overlay"
            :style="overlayStyle"
          >
            <div class="mud-picker-color-overlay mud-picker-color-overlay-white">
              <div class="mud-picker-color-overlay mud-picker-color-overlay-black">
                <div
                  ref="overlay"
                  class="mud-picker-color-overlay"
                  @pointerdown="onOverlayPointerDown"
                  @pointermove="onOverlayPointerMove"
                  @pointerup="onOverlayPointerUp"
                  @pointerleave="onOverlayPointerLeave"
                >
                  <svg
                    class="mud-picker-color-selector"
                    height="26"
                    width="26"
                    :style="selectorStyle"
                  >
                    <defs>
                      <filter
                        id="mud-picker-color-selector-shadow"
                        x="-50%"
                        y="-50%"
                        width="200%"
                        height="200%"
                      >
                        <feGaussianBlur
                          in="SourceAlpha"
                          stdDeviation="1"
                        />
                        <feOffset
                          dx="0"
                          dy="5"
                          result="offsetblur"
                        />
                        <feOffset
                          dx="0"
                          dy="-5"
                          result="offsetblur"
                        />
                        <feMerge>
                          <feMergeNode />
                          <feMergeNode in="SourceGraphic" />
                        </feMerge>
                      </filter>
                    </defs>
                    <circle
                      r="10"
                      cx="13"
                      cy="13"
                      stroke="white"
                      stroke-width="1"
                      fill="transparent"
                      style="filter: url(#mud-picker-color-selector-shadow)"
                    />
                    <circle
                      r="11"
                      cx="13"
                      cy="13"
                      stroke="white"
                      stroke-width="1.5"
                      fill="transparent"
                    />
                  </svg>
                </div>
              </div>
            </div>
          </div>
          <div
            v-if="colorView === 'grid'"
            class="mud-picker-color-grid"
          >
            <div
              v-for="color in GRID"
              :key="color"
              :class="dotClasses(color)"
              :style="{ background: colorRgba(color) }"
              :aria-label="colorLabel(color)"
              role="button"
              tabindex="0"
              @click="selectColor(color)"
              @keydown.enter.prevent="selectColor(color)"
              @keydown.space.prevent="selectColor(color)"
            />
          </div>
        </div>
        <div
          v-if="isControlVisible"
          class="mud-picker-color-controls"
        >
          <div
            v-if="showSliders || showPreview"
            class="mud-picker-color-controls-row"
          >
            <div
              v-if="showPreview"
              class="mud-picker-color-dot mud-picker-color-dot-current mud-ripple"
              role="button"
              tabindex="0"
              aria-label="Показать образцы цветов"
              @click="toggleCollection"
              @keydown.enter.prevent="toggleCollection"
              @keydown.space.prevent="toggleCollection"
            >
              <div
                class="mud-picker-color-fill"
                :style="previewStyle"
              />
            </div>
            <div
              v-if="showSliders && !collectionOpen"
              class="mud-picker-color-sliders"
            >
              <MudSlider
                v-if="colorView !== 'grid'"
                v-model="hue"
                class="mud-picker-color-slider hue"
                dir="ltr"
                :step="1"
                :min="0"
                :max="360"
              />
              <MudSlider
                v-if="showAlpha"
                v-model="alpha"
                class="mud-picker-color-slider alpha"
                :step="1"
                :min="0"
                :max="255"
                :style="alphaSliderStyle"
              />
            </div>
            <div
              v-if="collectionOpen"
              class="mud-picker-color-collection"
            >
              <div
                v-for="color in PALETTE.slice(0, 5)"
                :key="`quick-${color}`"
                :class="dotClasses(color)"
                :style="{ background: colorRgba(color) }"
                :aria-label="colorLabel(color)"
                role="button"
                tabindex="0"
                @click="selectColor(color)"
                @keydown.enter.prevent="selectColor(color)"
                @keydown.space.prevent="selectColor(color)"
              />
            </div>
          </div>
          <div
            v-if="showInputs"
            class="mud-picker-color-controls-row"
          >
            <div class="mud-picker-color-inputs">
              <template v-if="pickerMode === 'rgb'">
                <MudNumericField
                  v-model="red"
                  class="mud-picker-color-inputfield"
                  helper-text="R"
                  :min="0"
                  :max="255"
                  variant="outlined"
                  hide-spin-buttons
                  :required="required"
                />
                <MudNumericField
                  v-model="green"
                  class="mud-picker-color-inputfield"
                  helper-text="G"
                  :min="0"
                  :max="255"
                  variant="outlined"
                  hide-spin-buttons
                  :required="required"
                />
                <MudNumericField
                  v-model="blue"
                  class="mud-picker-color-inputfield"
                  helper-text="B"
                  :min="0"
                  :max="255"
                  variant="outlined"
                  hide-spin-buttons
                  :required="required"
                />
              </template>
              <template v-else-if="pickerMode === 'hsl'">
                <MudNumericField
                  v-model="hueInput"
                  class="mud-picker-color-inputfield"
                  helper-text="H"
                  :step="1"
                  :min="0"
                  :max="360"
                  variant="outlined"
                  hide-spin-buttons
                  :required="required"
                />
                <MudNumericField
                  v-model="saturationInput"
                  class="mud-picker-color-inputfield"
                  helper-text="S"
                  :step="0.01"
                  :min="0"
                  :max="100"
                  variant="outlined"
                  hide-spin-buttons
                  :required="required"
                />
                <MudNumericField
                  v-model="lightnessInput"
                  class="mud-picker-color-inputfield"
                  helper-text="L"
                  :step="0.01"
                  :min="0"
                  :max="100"
                  variant="outlined"
                  hide-spin-buttons
                  :required="required"
                />
              </template>
              <MudTextField
                v-else
                v-model="hexInput"
                class="mud-picker-color-inputfield"
                variant="outlined"
                :maxlength="hexMaxLength"
                helper-text="HEX"
                :required="required"
              />
              <MudNumericField
                v-if="showAlpha && pickerMode !== 'hex'"
                v-model="alphaInput"
                class="mud-picker-color-inputfield input-field-alpha"
                helper-text="A"
                :min="0"
                :max="1"
                :step="0.01"
                variant="outlined"
                hide-spin-buttons
                :required="required"
              />
            </div>
            <div
              v-if="showModeSwitch"
              class="mud-picker-control-switch"
            >
              <MudTooltip text="Переключить режим">
                <MudIconButton
                  class="pa-1 me-n1"
                  :icon="mdImportExport"
                  aria-label="Переключить режим"
                  @click="changeMode"
                />
              </MudTooltip>
            </div>
          </div>
        </div>
        <div
          v-if="colorView === 'palette'"
          class="mud-picker-color-view"
        >
          <div class="mud-picker-color-view-collection">
            <div
              v-for="color in PALETTE"
              :key="color"
              :class="dotClasses(color)"
              :style="{ background: colorRgba(color) }"
              :aria-label="colorLabel(color)"
              role="button"
              tabindex="0"
              @click="selectColor(color)"
              @keydown.enter.prevent="selectColor(color)"
              @keydown.space.prevent="selectColor(color)"
            />
          </div>
        </div>
      </MudPickerContent>
    </template>
  </MudPicker>
</template>
