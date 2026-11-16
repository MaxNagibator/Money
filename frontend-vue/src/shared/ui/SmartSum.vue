<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import type { Money } from '@/domain/money'
import { formatMoneyEditor, formatMoneyInput } from '@/platform/format'
import { mdCalculate, mdNumbers } from '@/shared/icons'
import { MudNumericField, MudTextField } from '@/shared/mud'
import { useSmartSum } from '@/shared/ui/useSmartSum'

const props = withDefaults(defineProps<{
  label?: string
  error?: boolean
  errorText?: string
  amountFormat?: 'compact' | 'fixed' | undefined
  placeholder?: string
  helperText?: string
}>(), {
  label: 'Сумма',
  error: false,
  errorText: '',
  amountFormat: undefined,
  placeholder: '',
  helperText: '',
})

const model = defineModel<Money | null>({ default: null })
const emit = defineEmits<{ preview: [value: Money | null] }>()
const state = useSmartSum(model, value => {
  if (props.amountFormat === undefined) { return formatMoneyInput(value) }
  return formatMoneyEditor(value, props.amountFormat === 'fixed')
})
watch(state.parsedSum, value => emit('preview', value), { immediate: true })
const numericField = ref<{ focus: () => void }>()
const textField = ref<{ focus: () => void }>()

watch(state.isNumericVisible, async isNumeric => {
  if (isNumeric) {
    return
  }

  await nextTick()
  textField.value?.focus()
})

const hasError = computed(() => state.hasError.value || props.error)

const numericErrorText = computed(() => state.hasError.value ? 'Не удалось разобрать сумму' : props.errorText)
const textErrorText = computed(() => state.hasError.value ? 'Не удалось вычислить сумму' : props.errorText)

defineExpose({
  focusInput: () => numericField.value?.focus(),
  getSum: state.getSum,
})
</script>

<template>
  <MudNumericField
    v-if="state.isNumericVisible.value"
    ref="numericField"
    v-model:text="state.numericText.value"
    data-testid="sum-numeric"
    :label="label"
    :placeholder="placeholder"
    :helper-text="helperText"
    hide-spin-buttons
    :readonly="state.hasExpression.value"
    :error="hasError"
    :error-text="numericErrorText"
    :adornment-icon="mdCalculate"
    adornment="start"
    adornment-color="tertiary"
    input-mode="decimal"
    immediate
    :on-adornment-click="() => state.enterTextMode()"
    @focus="state.onNumericFocus()"
    @keydown="state.onNumericKey($event.key)"
    @blur="state.commitNumeric()"
  />
  <MudTextField
    v-else
    ref="textField"
    v-model="state.calculationText.value"
    data-testid="sum-expression"
    :label="label"
    :placeholder="placeholder"
    :helper-text="helperText"
    :error="hasError"
    :error-text="textErrorText"
    :adornment-icon="mdNumbers"
    adornment="start"
    adornment-color="tertiary"
    clearable
    immediate
    :on-adornment-click="() => state.exitTextMode()"
    :on-clear-button-click="() => state.clear()"
    @blur="state.exitTextMode()"
    @keydown.enter="state.exitTextMode()"
  />
</template>
