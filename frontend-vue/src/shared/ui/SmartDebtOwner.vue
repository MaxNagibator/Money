<script setup lang="ts">
import { mdPerson } from '@/shared/icons'
import type { MudAdornment } from '@/shared/mud'
import SuggestionField from './SuggestionField.vue'
import { useOwnerSuggestions } from './useOwnerSuggestions'

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

const owners = useOwnerSuggestions()
</script>

<template>
  <SuggestionField
    v-bind="$attrs"
    v-model="model"
    :adornment="adornment"
    :debounce-interval="debounceInterval"
    :error="error"
    :error-text="errorText"
    :find="owners.find"
    :icon="mdPerson"
    id-prefix="sdo"
    label="Держатель"
    test-id="owner-suggestions"
    @committed="emit('committed')"
  />
</template>
