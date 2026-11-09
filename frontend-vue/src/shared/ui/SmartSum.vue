<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import type { Money } from '@/domain/money'
import { mdCalculate, mdNumbers } from '@/shared/icons'
import { MudNumericField, MudTextField } from '@/shared/mud'
import { useSmartSum } from '@/shared/ui/useSmartSum'

const props = withDefaults(defineProps<{
  label?: string
  error?: boolean
  errorText?: string
}>(), {
  label: 'Сумма',
  error: false,
  errorText: '',
})

const model = defineModel<Money | null>({ default: null })
const state = useSmartSum(model)
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
