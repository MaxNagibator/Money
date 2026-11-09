<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, useId, watch } from 'vue'
import type { Category } from '@/domain/category'
import { mdSearch } from '@/shared/icons'
import type { MudAdornment } from '@/shared/mud'
import { MudTextField } from '@/shared/mud'
import SuggestionList from './SuggestionList.vue'

defineOptions({ inheritAttrs: false })

const BLUR_DELAY_MS = 200
const CATEGORY_MISSING = 'Категория удалена, выберите другую'

const props = withDefaults(defineProps<{
  options: readonly Category[]
  adornment?: MudAdornment
  error?: boolean
  errorText?: string
}>(), {
  adornment: 'start',
  error: false,
  errorText: '',
})

const model = defineModel<number | null>({ default: null })

const id = `sc-${useId()}`

const text = ref('')
const suggestions = ref<readonly Category[]>([])
const showSuggestions = ref(false)
const selectedIndex = ref(-1)
const isFocused = ref(false)

let blurTimer: ReturnType<typeof setTimeout> | null = null

const selected = computed(() => props.options.find(category => category.id === model.value) ?? null)

const isMissing = computed(() => model.value !== null && selected.value === null)

// TODO: имя удалённой категории показать нечем – операция несёт один categoryId, а справочник отдаёт только живые; показывать имя, когда API начнёт отдавать категорию операции
const label = computed(() => selected.value?.name ?? '')

const names = computed(() => suggestions.value.map(category => category.name))

const stopBlur = (): void => {
  if (blurTimer !== null) {
    clearTimeout(blurTimer)
    blurTimer = null
  }
}

const updateSuggestions = (filter: string): void => {
  const normalized = filter.toLocaleLowerCase()

  suggestions.value = filter.trim().length === 0
    ? [...props.options]
    : props.options.filter(category => category.name.toLocaleLowerCase().includes(normalized))

  showSuggestions.value = isFocused.value && suggestions.value.length > 0
}

const scrollToSelected = (): void => {
  const index = selectedIndex.value

  if (index < 0) {
    return
  }

  void nextTick(() => document.getElementById(`${id}-item-${index}`)?.scrollIntoView({ block: 'nearest' }))
}

const select = (index: number): void => {
  const suggestion = suggestions.value[index]

  if (suggestion === undefined) {
    return
  }

  stopBlur()
  text.value = suggestion.name
  showSuggestions.value = false
  selectedIndex.value = -1
  model.value = suggestion.id
}

const handleFocus = (): void => {
  stopBlur()
  isFocused.value = true
  selectedIndex.value = -1
  updateSuggestions(text.value)
}

const handleBlur = (): void => {
  isFocused.value = false
  stopBlur()

  blurTimer = setTimeout(() => {
    blurTimer = null
    showSuggestions.value = false
    selectedIndex.value = -1

    if (text.value !== label.value) {
      text.value = label.value
    }
  }, BLUR_DELAY_MS)
}

const handleTextChange = (value: string): void => {
  text.value = value
  selectedIndex.value = -1

  if (value.length === 0 && model.value !== null) {
    model.value = null
  }

  updateSuggestions(value)
}

const handleKeydown = (event: KeyboardEvent): void => {
  if (event.key === 'ArrowDown') {
    if (suggestions.value.length === 0) {
      return
    }

    showSuggestions.value = true
    selectedIndex.value = Math.min(selectedIndex.value + 1, suggestions.value.length - 1)
    scrollToSelected()
  } else if (event.key === 'ArrowUp') {
    if (suggestions.value.length === 0) {
      return
    }

    showSuggestions.value = true
    selectedIndex.value = Math.max(selectedIndex.value - 1, -1)
    scrollToSelected()
  } else if (event.key === 'Enter') {
    if (showSuggestions.value && selectedIndex.value >= 0) {
      event.preventDefault()
      select(selectedIndex.value)
    }
  } else if (event.key === 'Escape') {
    showSuggestions.value = false
    selectedIndex.value = -1
  } else if (event.key === 'Tab') {
    if (showSuggestions.value && selectedIndex.value >= 0) {
      select(selectedIndex.value)
      return
    }

    showSuggestions.value = false
    selectedIndex.value = -1
  }
}

watch(label, value => {
  text.value = value
}, { immediate: true })

onBeforeUnmount(stopBlur)
</script>

<template>
  <div style="position: relative;">
    <MudTextField
      v-bind="$attrs"
      :adornment="adornment"
      :adornment-icon="mdSearch"
      adornment-color="tertiary"
      clearable
      :error="error || isMissing"
      :error-text="isMissing ? CATEGORY_MISSING : errorText"
      immediate
      label="Категория"
      :model-value="text"
      @blur="handleBlur"
      @focus="handleFocus"
      @keydown="handleKeydown"
      @update:model-value="handleTextChange"
    />

    <SuggestionList
      v-if="showSuggestions && suggestions.length > 0"
      :id-prefix="id"
      :items="names"
      :selected-index="selectedIndex"
      test-id="category-suggestions"
      @select="select"
    />
  </div>
</template>
