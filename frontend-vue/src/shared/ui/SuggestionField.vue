<script setup lang="ts">
import { nextTick, onBeforeUnmount, ref, useId, watch } from 'vue'
import type { MudAdornment } from '@/shared/mud'
import { MudProgressLinear, MudTextField } from '@/shared/mud'
import SuggestionList from './SuggestionList.vue'

defineOptions({ inheritAttrs: false })

const BLUR_DELAY_MS = 200

const props = withDefaults(defineProps<{
  find: (value: string) => Promise<readonly string[]>
  icon: string
  idPrefix: string
  label: string
  testId: string
  adornment?: MudAdornment
  debounceInterval?: number
  error?: boolean
  errorText?: string
}>(), {
  adornment: 'end',
  debounceInterval: 300,
  error: false,
  errorText: '',
})

const model = defineModel<string>({ default: '' })

const emit = defineEmits<{
  committed: []
}>()

const id = `${props.idPrefix}-${useId()}`

const suggestions = ref<readonly string[]>([])
const showSuggestions = ref(false)
const isLoading = ref(false)
const selectedIndex = ref(-1)
const isFocused = ref(false)

let debounceTimer: ReturnType<typeof setTimeout> | null = null
let blurTimer: ReturnType<typeof setTimeout> | null = null
let searchVersion = 0
let committedValue: string | null = null

const stopDebounce = (): void => {
  if (debounceTimer !== null) {
    clearTimeout(debounceTimer)
    debounceTimer = null
  }
}

const stopBlur = (): void => {
  if (blurTimer !== null) {
    clearTimeout(blurTimer)
    blurTimer = null
  }
}

const loadSuggestions = async (): Promise<void> => {
  const version = ++searchVersion

  isLoading.value = true
  selectedIndex.value = -1

  try {
    const found = await props.find(model.value)

    if (version !== searchVersion) {
      return
    }

    suggestions.value = found
    showSuggestions.value = isFocused.value && found.length > 0
  } catch {
    if (version !== searchVersion) {
      return
    }

    suggestions.value = []
    showSuggestions.value = false
  } finally {
    if (version === searchVersion) {
      isLoading.value = false
    }
  }
}

const startSearchDebounced = (): void => {
  if (!isFocused.value) {
    return
  }

  stopDebounce()

  debounceTimer = setTimeout(() => {
    debounceTimer = null
    void loadSuggestions()
  }, props.debounceInterval)
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
  stopDebounce()
  searchVersion += 1
  isLoading.value = false
  committedValue = suggestion
  model.value = suggestion
  showSuggestions.value = false
  selectedIndex.value = -1
  emit('committed')
}

const accept = (): void => {
  showSuggestions.value = false
  selectedIndex.value = -1
  emit('committed')
}

const handleFocus = (): void => {
  stopBlur()
  isFocused.value = true
  selectedIndex.value = -1
  stopDebounce()
  void loadSuggestions()
}

const handleBlur = (): void => {
  isFocused.value = false
  stopDebounce()
  stopBlur()

  blurTimer = setTimeout(() => {
    blurTimer = null
    showSuggestions.value = false
    selectedIndex.value = -1
    emit('committed')
  }, BLUR_DELAY_MS)
}

const handleKeydown = (event: KeyboardEvent): void => {
  if (event.key === 'ArrowDown') {
    if (!showSuggestions.value || suggestions.value.length === 0) {
      return
    }

    selectedIndex.value = Math.min(selectedIndex.value + 1, suggestions.value.length - 1)
    scrollToSelected()
  } else if (event.key === 'ArrowUp') {
    if (!showSuggestions.value || suggestions.value.length === 0) {
      return
    }

    selectedIndex.value = Math.max(selectedIndex.value - 1, -1)
    scrollToSelected()
  } else if (event.key === 'Enter') {
    if (showSuggestions.value && selectedIndex.value >= 0) {
      event.preventDefault()
      select(selectedIndex.value)
      return
    }

    accept()
  } else if (event.key === 'Escape') {
    showSuggestions.value = false
    selectedIndex.value = -1
  } else if (event.key === 'Tab') {
    if (!showSuggestions.value || selectedIndex.value < 0) {
      accept()
      return
    }

    select(selectedIndex.value)
  }
}

watch(model, value => {
  const wasCommitted = value === committedValue

  committedValue = null
  selectedIndex.value = -1

  if (wasCommitted) {
    return
  }

  startSearchDebounced()
})

onBeforeUnmount(() => {
  stopDebounce()
  stopBlur()
})
</script>

<template>
  <div style="position: relative;">
    <MudTextField
      v-bind="$attrs"
      v-model="model"
      :adornment="adornment"
      :adornment-icon="icon"
      adornment-color="tertiary"
      clearable
      :error="error"
      :error-text="errorText"
      immediate
      :label="label"
      @blur="handleBlur"
      @focus="handleFocus"
      @keydown="handleKeydown"
    />

    <SuggestionList
      v-if="showSuggestions && suggestions.length > 0"
      :id-prefix="id"
      :items="suggestions"
      :selected-index="selectedIndex"
      :test-id="testId"
      @select="select"
    />

    <MudProgressLinear
      v-if="isLoading"
      color="tertiary"
      indeterminate
      style="position: absolute; bottom: 0; left: 0; right: 0;"
    />
  </div>
</template>
