<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { addDays, addMonths, compareLocalDate, daysInMonth, fromDate, fromParts, toParts, weekdayIndex } from '@/domain/local-date'
import { formatLocalDate } from '@/platform/format'
import { mdChevronLeft, mdChevronRight } from '@/shared/icons'
import MudButton from './MudButton.vue'
import MudIconButton from './MudIconButton.vue'
import MudPicker from './MudPicker.vue'
import MudPickerContent from './MudPickerContent.vue'
import MudPickerToolbar from './MudPickerToolbar.vue'
import MudText from './MudText.vue'
import type { MudAdornment, MudColor, MudOrigin, MudVariant } from './types'

const props = withDefaults(defineProps<{
  adornment?: MudAdornment
  adornmentColor?: MudColor
  anchorOrigin?: MudOrigin
  clearable?: boolean
  color?: MudColor
  dateFormat?: string
  editable?: boolean
  helperText?: string | undefined
  error?: boolean
  errorText?: string | undefined
  label?: string | undefined
  mask?: string
  maxDate?: LocalDate | null
  minDate?: LocalDate | null
  variant?: MudVariant
}>(), {
  adornment: 'end',
  adornmentColor: 'default',
  anchorOrigin: 'bottom-left',
  clearable: false,
  color: 'primary',
  dateFormat: 'dd.MM.yyyy',
  editable: false,
  helperText: '',
  error: false,
  errorText: '',
  label: '',
  variant: 'text',
})

const model = defineModel<LocalDate | null>({ default: null })

const emit = defineEmits<{
  'picker-closed': []
}>()

interface CalendarDay {
  value: LocalDate
  id: number
}

interface CalendarMonth {
  value: LocalDate
  index: number
  weeks: CalendarDay[][]
}

const pad = (value: number): string => String(value).padStart(2, '0')

const startOfMonth = (value: LocalDate): LocalDate => {
  const parts = toParts(value)
  const result = fromParts(parts.year, parts.month, 1)

  if (result === null) {
    throw new RangeError(`Некорректный месяц: ${value}`)
  }

  return result
}

const pickerReference = ref<{ open: () => void, close: () => void }>()
const today = fromDate(new Date())
const inputText = ref('')
const selectedDate = ref<LocalDate | null>(null)
const pickerMonth = ref<LocalDate>(startOfMonth(model.value ?? today))
const hoveredDay = ref(0)

const datePattern = computed(() => props.mask ?? props.dateFormat)

const formatDate = (value: LocalDate): string => {
  if (datePattern.value === 'dd.MM.yyyy') {
    return formatLocalDate(value)
  }

  const parts = toParts(value)
  return datePattern.value
    .replace(/yyyy/g, String(parts.year).padStart(4, '0'))
    .replace(/MM/g, pad(parts.month))
    .replace(/dd/g, pad(parts.day))
}

const escapePattern = (value: string): string => value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')

const parseDate = (value: string): LocalDate | null => {
  const raw = value.trim()

  if (raw.length === 0) {
    return null
  }

  const tokens: string[] = []
  const expression = escapePattern(datePattern.value).replace(/yyyy|MM|dd/g, token => {
    tokens.push(token)
    return token === 'yyyy' ? '(\\d{4})' : '(\\d{2})'
  })
  const match = new RegExp(`^${expression}$`).exec(raw)

  if (match === null) {
    return null
  }

  const valueOf = (token: string): number => Number(match[tokens.indexOf(token) + 1])
  return fromParts(valueOf('yyyy'), valueOf('MM'), valueOf('dd'))
}

const monthNames = [
  'январь',
  'февраль',
  'март',
  'апрель',
  'май',
  'июнь',
  'июль',
  'август',
  'сентябрь',
  'октябрь',
  'ноябрь',
  'декабрь',
]

const shortMonthNames = ['янв.', 'февр.', 'март', 'апр.', 'май', 'июнь', 'июль', 'авг.', 'сент.', 'окт.', 'нояб.', 'дек.']
const weekdayNames = ['пн', 'вт', 'ср', 'чт', 'пт', 'сб', 'вс']

const monthName = (value: LocalDate): string => {
  const parts = toParts(value)
  return `${monthNames[parts.month - 1]} ${parts.year} г.`
}

const titleDate = (value: LocalDate | null): string => {
  if (value === null) {
    return ''
  }

  const parts = toParts(value)
  return `${weekdayNames[weekdayIndex(value)]}, ${pad(parts.day)} ${shortMonthNames[parts.month - 1]}`
}

const getWeek = (month: LocalDate, week: number): CalendarDay[] => {
  const firstDayOffset = weekdayIndex(month)
  const firstVisibleDay = addDays(month, -firstDayOffset + week * 7)

  return Array.from({ length: 7 }, (_, index) => ({
    value: addDays(firstVisibleDay, index),
    id: 1 + week * 7 + index,
  }))
}

const calendarMonths = computed<CalendarMonth[]>(() => Array.from({ length: 1 }, (_, index) => {
  const month = addMonths(pickerMonth.value, index)
  return {
    value: month,
    index,
    weeks: Array.from({ length: 6 }, (_, week) => getWeek(month, week)),
  }
}))

const calendarStyle = computed<Record<string, string | number>>(() => ({
  '--selected-day': hoveredDay.value,
}))

const selectedValue = computed(() => selectedDate.value ?? model.value)

const monthEnd = (month: LocalDate): LocalDate => {
  const parts = toParts(month)
  const result = fromParts(parts.year, parts.month, daysInMonth(parts.year, parts.month))

  if (result === null) {
    throw new RangeError(`Некорректный месяц: ${month}`)
  }

  return result
}

const isOutsideMonth = (month: LocalDate, day: LocalDate): boolean =>
  compareLocalDate(day, month) < 0 || compareLocalDate(day, monthEnd(month)) > 0

const isDateDisabled = (day: LocalDate): boolean =>
  (props.minDate !== null && props.minDate !== undefined && compareLocalDate(day, props.minDate) < 0)
  || (props.maxDate !== null && props.maxDate !== undefined && compareLocalDate(day, props.maxDate) > 0)

const dayClasses = (month: LocalDate, day: LocalDate) => {
  const outsideMonth = isOutsideMonth(month, day)
  const selected = selectedValue.value === day
  const current = today === day && !selected

  return [
    'mud-day',
    {
      'mud-hidden': outsideMonth,
      'mud-selected': selected,
      [`mud-theme-${props.color}`]: selected,
      'mud-current': current,
      'mud-button-outlined': current,
      [`mud-button-outlined-${props.color}`]: current,
      [`mud-${props.color}-text`]: current,
      'mud-disabled': isDateDisabled(day),
    },
  ]
}

const dayStyle = (id: number): Record<string, string | number> => ({ '--day-id': id })

const monthHeaderClasses = (index: number) => [
  'mud-picker-calendar-header',
  `mud-picker-calendar-header-${index + 1}`,
  {
    'mud-picker-calendar-header-last': index === calendarMonths.value.length - 1,
  },
]

const calendarYear = computed(() => toParts(pickerMonth.value).year)

const moveMonth = (offset: number): void => {
  pickerMonth.value = addMonths(pickerMonth.value, offset)
}

const handleTextChanged = (value: string): void => {
  inputText.value = value

  if (!props.editable) {
    return
  }

  const parsed = parseDate(value)
  if (parsed === null) {
    if (value.trim().length === 0) {
      model.value = null
    }
    return
  }

  model.value = parsed
}

const handlePickerOpened = (): void => {
  selectedDate.value = null
  pickerMonth.value = startOfMonth(model.value ?? today)
  hoveredDay.value = 0
}

const handlePickerClosed = (): void => {
  selectedDate.value = null
  emit('picker-closed')
}

const selectDay = (day: LocalDate): void => {
  if (isOutsideMonth(pickerMonth.value, day) || isDateDisabled(day)) {
    return
  }

  selectedDate.value = day
  model.value = day
  inputText.value = formatDate(day)
  pickerReference.value?.close()
}

const clearDate = (): void => {
  selectedDate.value = null
  inputText.value = ''
  model.value = null
}

watch(model, value => {
  inputText.value = value === null ? '' : formatDate(value)
  if (value !== null) {
    pickerMonth.value = startOfMonth(value)
  }
}, { immediate: true })

defineExpose({
  open: () => pickerReference.value?.open(),
  close: () => pickerReference.value?.close(),
})
</script>

<template>
  <MudPicker
    ref="pickerReference"
    v-model="inputText"
    :label="label"
    :helper-text="helperText"
    :error="error"
    :error-text="errorText"
    :editable="editable"
    :variant="variant"
    :adornment="adornment"
    :adornment-color="adornmentColor"
    :clearable="clearable"
    :anchor-origin="anchorOrigin"
    picker-variant="inline"
    @update:model-value="handleTextChanged"
    @clear="clearDate"
    @picker-opened="handlePickerOpened"
    @picker-closed="handlePickerClosed"
  >
    <template #picker-content>
      <MudPickerToolbar
        class="mud-picker-datepicker-toolbar"
        :color="color"
        @mousedown.prevent
      >
        <MudButton
          variant="text"
          color="inherit"
          class="mud-button-year"
        >
          {{ calendarYear }}
        </MudButton>
        <MudButton
          variant="text"
          color="inherit"
          class="mud-button-date"
        >
          {{ titleDate(selectedValue) }}
        </MudButton>
      </MudPickerToolbar>
      <MudPickerContent>
        <div
          class="mud-picker-calendar-content mud-picker-calendar-content-1"
          :style="calendarStyle"
          @mousedown.prevent
        >
          <div
            v-for="calendarMonth in calendarMonths"
            :key="calendarMonth.value"
            class="mud-picker-calendar-container"
          >
            <div :class="monthHeaderClasses(calendarMonth.index)">
              <div class="mud-picker-calendar-header-switch">
                <MudIconButton
                  class="mud-picker-nav-button-prev mud-flip-x-rtl"
                  :icon="mdChevronLeft"
                  :aria-label="`Предыдущий месяц (${monthName(addMonths(calendarMonth.value, -1))})`"
                  @click.stop="moveMonth(-1)"
                />
                <button
                  type="button"
                  class="mud-picker-slide-transition mud-picker-calendar-header-transition mud-button-month"
                  @click.stop
                >
                  <MudText
                    typo="body1"
                    align="center"
                  >
                    {{ monthName(calendarMonth.value) }}
                  </MudText>
                </button>
                <MudIconButton
                  class="mud-picker-nav-button-next mud-flip-x-rtl"
                  :icon="mdChevronRight"
                  :aria-label="`Следующий месяц (${monthName(addMonths(calendarMonth.value, 1))})`"
                  @click.stop="moveMonth(1)"
                />
              </div>
              <div class="mud-picker-calendar-header-day">
                <MudText
                  v-for="dayName in weekdayNames"
                  :key="dayName"
                  typo="caption"
                  class="mud-day-label"
                >
                  {{ dayName }}
                </MudText>
              </div>
            </div>
            <div class="mud-picker-calendar-transition mud-picker-slide-transition">
              <div class="mud-picker-calendar">
                <template
                  v-for="(week, weekIndex) in calendarMonth.weeks"
                  :key="weekIndex"
                >
                  <button
                    v-for="day in week"
                    :key="`${day.value}-${day.id}`"
                    type="button"
                    :style="dayStyle(day.id)"
                    :class="[
                      'mud-button-root',
                      'mud-icon-button',
                      'mud-ripple',
                      'mud-ripple-icon',
                      'mud-picker-calendar-day',
                      dayClasses(calendarMonth.value, day.value),
                    ]"
                    :disabled="isDateDisabled(day.value)"
                    @click.stop="selectDay(day.value)"
                    @pointerover="hoveredDay = day.id"
                  >
                    <p class="mud-typography mud-typography-body2 mud-inherit-text">
                      {{ toParts(day.value).day }}
                    </p>
                  </button>
                </template>
              </div>
            </div>
          </div>
        </div>
      </MudPickerContent>
    </template>
  </MudPicker>
</template>
