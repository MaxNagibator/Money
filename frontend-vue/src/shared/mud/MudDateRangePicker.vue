<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { addDays, addMonths, compareLocalDate, daysInMonth, fromDate, fromParts, toParts, weekdayIndex } from '@/domain/local-date'
import { formatLocalDate } from '@/platform/format'
import { mdArrowForward, mdChevronLeft, mdChevronRight, mdClose, mdEvent } from '@/shared/icons'
import MudButton from './MudButton.vue'
import MudIcon from './MudIcon.vue'
import MudIconButton from './MudIconButton.vue'
import MudInputAdornment from './MudInputAdornment.vue'
import MudInputControl from './MudInputControl.vue'
import MudPicker from './MudPicker.vue'
import MudPickerContent from './MudPickerContent.vue'
import MudPickerToolbar from './MudPickerToolbar.vue'
import MudText from './MudText.vue'
import type { MudAdornment, MudColor } from './types'

interface MudDateRange {
  start: LocalDate | null
  end: LocalDate | null
}

const props = withDefaults(defineProps<{
  adornment?: MudAdornment
  adornmentColor?: MudColor
  clearable?: boolean
  color?: MudColor
  dateFormat?: string
  editable?: boolean
  label?: string | undefined
  mask?: string
  pickerVariant?: 'inline'
  rounded?: boolean
  showWeekNumbers?: boolean
}>(), {
  adornment: 'end',
  adornmentColor: 'default',
  clearable: false,
  color: 'primary',
  dateFormat: 'dd.MM.yyyy',
  editable: false,
  label: '',
  pickerVariant: 'inline',
  rounded: false,
  showWeekNumbers: false,
})

const dateRange = defineModel<MudDateRange | null>('dateRange', { default: null })

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

const today = fromDate(new Date())
const pickerReference = ref<{ close: () => void }>()
const inputId = `date-range-${Math.random().toString(36).slice(2)}`
const startText = ref('')
const endText = ref('')
const firstDate = ref<LocalDate | null>(null)
const secondDate = ref<LocalDate | null>(null)
const pickerMonth = ref<LocalDate>(startOfMonth(dateRange.value?.start ?? today))
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

const asDate = (value: LocalDate): Date => {
  const parts = toParts(value)
  return new Date(Date.UTC(parts.year, parts.month - 1, parts.day))
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
  const firstVisibleDay = addDays(month, -weekdayIndex(month) + week * 7)

  return Array.from({ length: 7 }, (_, index) => ({
    value: addDays(firstVisibleDay, index),
    id: 1 + week * 7 + index,
  }))
}

const calendarMonths = computed<CalendarMonth[]>(() => Array.from({ length: 2 }, (_, index) => {
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

const shrink = computed(() => startText.value.length > 0
  || endText.value.length > 0
  || props.adornment === 'start')

const rangeInputClasses = computed(() => [
  'mud-input',
  'mud-input-text',
  'mud-input-underline',
  {
    'mud-input-text-with-label': props.label.length > 0,
    [`mud-input-adorned-${props.adornment}`]: props.adornment !== 'none',
    'mud-shrink': shrink.value,
  },
  'mud-typography-subtitle1',
])

const inputClasses = computed(() => [
  'mud-input-root-text',
  {
    [`mud-input-root-adorned-${props.adornment}`]: props.adornment !== 'none',
  },
])

const adornmentClasses = computed(() => ({
  [`mud-input-adornment-${props.adornment}`]: props.adornment !== 'none',
}))

const clearButtonClasses = computed(() => [
  'mud-input-clear-button',
  props.adornment === 'start' ? 'me-0' : 'me-n4',
])

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

const isDateDisabled = (day: LocalDate): boolean => {
  void day
  return false
}

const same = (left: LocalDate | null, right: LocalDate | null): boolean => left !== null && left === right

const rangeClasses = (month: LocalDate, day: LocalDate) => {
  const base = ['mud-day']

  if (isOutsideMonth(month, day)) {
    return [...base, 'mud-hidden']
  }

  const committedStart = dateRange.value?.start ?? null
  const committedEnd = dateRange.value?.end ?? null
  const betweenCurrent = firstDate.value !== null
    && secondDate.value !== null
    && firstDate.value < day
    && secondDate.value > day
  const betweenCommitted = firstDate.value === null
    && committedStart !== null
    && committedEnd !== null
    && committedStart < day
    && committedEnd > day

  if (betweenCurrent || betweenCommitted) {
    return [
      ...base,
      'mud-range',
      'mud-range-between',
      ...(day === today ? [
        'mud-current',
        `mud-${props.color}-text`,
        'mud-button-outlined',
        `mud-button-outlined-${props.color}`,
      ] : []),
    ]
  }

  if (same(firstDate.value, day) && same(secondDate.value, day)) {
    return [...base, 'mud-selected', `mud-theme-${props.color}`]
  }

  if (same(firstDate.value, day) || (firstDate.value === null && same(committedStart, day) && committedEnd !== day)) {
    return [
      ...base,
      'mud-selected',
      'mud-range',
      'mud-range-start-selected',
      ...(firstDate.value !== null ? ['mud-range-selection'] : []),
      `mud-theme-${props.color}`,
    ]
  }

  if ((firstDate.value !== null && same(secondDate.value, day))
    || (firstDate.value === null && committedStart !== day && same(committedEnd, day))) {
    return [...base, 'mud-selected', 'mud-range', 'mud-range-end-selected', `mud-theme-${props.color}`]
  }

  if (firstDate.value === null && same(committedStart, day) && same(committedEnd, day)) {
    return [...base, 'mud-selected', `mud-theme-${props.color}`]
  }

  if (firstDate.value !== null && firstDate.value < day) {
    return [
      ...base,
      ...(secondDate.value === null && day !== today ? ['mud-range'] : []),
      'mud-range-selection',
      'mud-range-selection-' + props.color,
      ...(day === today ? [
        'mud-current',
        `mud-${props.color}-text`,
        'mud-button-outlined',
        `mud-button-outlined-${props.color}`,
      ] : []),
    ]
  }

  if (day === today) {
    return [...base, 'mud-current', 'mud-button-outlined', `mud-button-outlined-${props.color}`, `mud-${props.color}-text`]
  }

  return base
}

const dayStyle = (id: number): Record<string, string | number> => ({ '--day-id': id })

const weekNumber = (month: LocalDate, index: number): string => {
  const weekStart = addDays(month, -weekdayIndex(month) + index * 7)
  const weekEnd = addDays(weekStart, 6)

  if (toParts(weekStart).month !== toParts(month).month && toParts(weekEnd).month !== toParts(month).month) {
    return ''
  }

  const weekYear = toParts(addDays(weekStart, 3)).year
  const januaryFourth = fromParts(weekYear, 1, 4)

  if (januaryFourth === null) {
    return ''
  }

  const firstWeekStart = addDays(januaryFourth, -weekdayIndex(januaryFourth))
  const difference = Math.round((asDate(weekStart).getTime() - asDate(firstWeekStart).getTime()) / 86_400_000)
  return String(Math.floor(difference / 7) + 1)
}

const monthHeaderClasses = (index: number) => [
  'mud-picker-calendar-header',
  `mud-picker-calendar-header-${index + 1}`,
  {
    'mud-picker-calendar-header-last': index === calendarMonths.value.length - 1,
  },
]

const calendarYear = computed(() => toParts(pickerMonth.value).year)
const titleText = computed(() => {
  if (firstDate.value !== null) {
    return `${titleDate(firstDate.value)} – ${titleDate(secondDate.value)}`
  }

  if (dateRange.value?.start !== null && dateRange.value?.start !== undefined) {
    return `${titleDate(dateRange.value.start)} – ${titleDate(dateRange.value.end ?? null)}`
  }

  return ''
})

const moveMonth = (offset: number): void => {
  pickerMonth.value = addMonths(pickerMonth.value, offset)
}

const commitRangeText = (): void => {
  const start = parseDate(startText.value)
  const end = parseDate(endText.value)

  if (startText.value.trim().length === 0 && endText.value.trim().length === 0) {
    dateRange.value = null
    return
  }

  dateRange.value = { start, end }
}

const handlePickerOpened = (): void => {
  firstDate.value = null
  secondDate.value = null
  pickerMonth.value = startOfMonth(dateRange.value?.start ?? today)
  hoveredDay.value = 0
}

const handlePickerClosed = (): void => {
  firstDate.value = null
  secondDate.value = null
  emit('picker-closed')
}

const selectDay = (month: LocalDate, day: LocalDate): void => {
  if (isOutsideMonth(month, day) || isDateDisabled(day)) {
    return
  }

  if (firstDate.value === null || secondDate.value !== null) {
    firstDate.value = day
    secondDate.value = null
    return
  }

  if (firstDate.value > day) {
    secondDate.value = firstDate.value
    firstDate.value = day
  } else {
    secondDate.value = day
  }

  const start = firstDate.value
  const end = secondDate.value

  if (start === null || end === null) {
    return
  }

  dateRange.value = { start, end }
  startText.value = formatDate(start)
  endText.value = formatDate(end)
  pickerReference.value?.close()
}

const clearRange = (): void => {
  firstDate.value = null
  secondDate.value = null
  startText.value = ''
  endText.value = ''
  dateRange.value = null
  pickerReference.value?.close()
}

watch(dateRange, value => {
  startText.value = value?.start === null || value?.start === undefined ? '' : formatDate(value.start)
  endText.value = value?.end === null || value?.end === undefined ? '' : formatDate(value.end)
  if (value?.start !== null && value?.start !== undefined) {
    pickerMonth.value = startOfMonth(value.start)
  }
}, { immediate: true })
</script>

<template>
  <MudPicker
    ref="pickerReference"
    :editable="editable"
    :picker-variant="pickerVariant"
    anchor-origin="bottom-left"
    @picker-opened="handlePickerOpened"
    @picker-closed="handlePickerClosed"
  >
    <template #input="{ toggle }">
      <MudInputControl
        :label="label"
        :for-id="`${inputId}-start`"
        @click="editable ? undefined : toggle"
      >
        <template #input-content>
          <div
            class="mud-input-input-control"
            :class="rangeInputClasses"
          >
            <MudInputAdornment
              v-if="adornment === 'start'"
              :class="adornmentClasses"
              :icon="mdEvent"
              :color="adornmentColor"
              placement="start"
              :on-adornment-click="toggle"
            />
            <input
              :id="`${inputId}-start`"
              v-model="startText"
              class="mud-input-slot mud-input-root"
              :class="inputClasses"
              type="text"
              :readonly="!editable"
              inputmode="text"
              @change="commitRangeText"
            >
            <MudIcon
              :icon="mdArrowForward"
              class="mud-range-input-separator mud-flip-x-rtl"
              color="default"
            />
            <input
              :id="`${inputId}-end`"
              v-model="endText"
              :aria-label="label ? `${label}, конец` : undefined"
              class="mud-input-slot mud-input-root"
              :class="inputClasses"
              type="text"
              :readonly="!editable"
              inputmode="text"
              @change="commitRangeText"
            >
            <MudIconButton
              v-if="clearable && (startText.length > 0 || endText.length > 0)"
              :class="clearButtonClasses"
              :icon="mdClose"
              color="default"
              size="small"
              aria-label="Очистить"
              tabindex="-1"
              @click.stop="clearRange"
              @mousedown.stop
            />
            <MudInputAdornment
              v-if="adornment === 'end'"
              :class="adornmentClasses"
              :icon="mdEvent"
              :color="adornmentColor"
              placement="end"
              :on-adornment-click="toggle"
            />
          </div>
        </template>
      </MudInputControl>
    </template>
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
          {{ titleText }}
        </MudButton>
      </MudPickerToolbar>
      <MudPickerContent>
        <div
          class="mud-picker-calendar-content mud-picker-calendar-content-2"
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
                <div
                  v-if="showWeekNumbers"
                  class="mud-picker-calendar-week"
                >
                  <MudText
                    typo="caption"
                    class="mud-picker-calendar-week-text"
                  />
                </div>
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
                  <div
                    v-if="showWeekNumbers"
                    class="mud-picker-calendar-week"
                  >
                    <MudText
                      typo="caption"
                      class="mud-picker-calendar-week-text"
                    >
                      {{ weekNumber(calendarMonth.value, weekIndex) }}
                    </MudText>
                  </div>
                  <button
                    v-for="day in week"
                    :key="`${day.value}-${day.id}`"
                    type="button"
                    :style="dayStyle(calendarMonth.index * 42 + day.id)"
                    :class="[
                      'mud-button-root',
                      'mud-icon-button',
                      'mud-ripple',
                      'mud-ripple-icon',
                      'mud-picker-calendar-day',
                      rangeClasses(calendarMonth.value, day.value),
                    ]"
                    :disabled="isDateDisabled(day.value)"
                    @click.stop="selectDay(calendarMonth.value, day.value)"
                    @pointerover="hoveredDay = calendarMonth.index * 42 + day.id"
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
