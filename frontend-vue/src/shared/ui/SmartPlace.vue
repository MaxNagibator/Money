<script setup lang="ts">
import { mdPlace } from '@/shared/icons'
import type { MudAdornment } from '@/shared/mud'
import SuggestionField from './SuggestionField.vue'
import { usePlaceSuggestions } from './usePlaceSuggestions'

defineOptions({ inheritAttrs: false })

withDefaults(defineProps<{
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

const places = usePlaceSuggestions()
</script>

<template>
  <SuggestionField
    v-bind="$attrs"
    v-model="model"
    :adornment="adornment"
    :debounce-interval="debounceInterval"
    :error="error"
    :error-text="errorText"
    :find="places.find"
    :icon="mdPlace"
    id-prefix="sp"
    label="Место"
    test-id="place-suggestions"
    @committed="emit('committed')"
  />
</template>
