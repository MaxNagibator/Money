import type { ComputedRef, Ref } from 'vue'
import { computed, ref, watch } from 'vue'
import { Money, normalizeNumericInput } from '@/domain/money'
import { evaluateMoneyExpression, isMoneyExpression } from '@/domain/money-expression'
import { formatMoneyInput } from '@/platform/format'

const EXPRESSION_TRIGGERS = new Set(['(', ')', '+', '-', '*', '/', '.', ','])

export interface SmartSumState {
  isNumericVisible: Ref<boolean>
  numericText: Ref<string>
  calculationText: Ref<string>
  hasExpression: ComputedRef<boolean>
  parsedSum: ComputedRef<Money | null>
  hasError: Ref<boolean>
  enterTextMode: () => void
  exitTextMode: () => void
  onNumericFocus: () => void
  onNumericKey: (key: string) => void
  commitNumeric: () => boolean
  clear: () => void
  getSum: () => Money | null
}

export function useSmartSum(model: Ref<Money | null>, format: (value: Money) => string = formatMoneyInput): SmartSumState {
  const isNumericVisible = ref(true)
  const numericText = ref('')
  const calculationText = ref('')
  const hasError = ref(false)

  const hasExpression = computed(() => {
    const raw = calculationText.value.trim()
    return raw.length > 0 && isMoneyExpression(raw)
  })
  const parsedSum = computed(() => {
    const raw = (isNumericVisible.value ? numericText.value : calculationText.value).trim()
    return !isNumericVisible.value && isMoneyExpression(raw) ? evaluateMoneyExpression(raw) : Money.parseInput(raw)
  })

  watch(
    model,
    value => {
      numericText.value = value === null ? '' : format(value)
    },
    { immediate: true },
  )

  function enterTextMode(): void {
    if (!isNumericVisible.value) {
      return
    }

    if (!hasExpression.value) {
      calculationText.value = numericText.value.trim()
    }

    isNumericVisible.value = false
  }

  function exitTextMode(): void {
    if (isNumericVisible.value) {
      return
    }

    if (!tryUpdateSum()) {
      return
    }

    isNumericVisible.value = true
  }

  function tryUpdateSum(): boolean {
    const raw = calculationText.value.trim()

    if (raw.length === 0) {
      hasError.value = false
      return true
    }

    const value = isMoneyExpression(raw) ? evaluateMoneyExpression(raw) : Money.parseInput(raw)

    if (value === null) {
      hasError.value = true
      return false
    }

    hasError.value = false
    model.value = value
    numericText.value = format(value)
    return true
  }

  function onNumericFocus(): void {
    if (!isNumericVisible.value || !hasExpression.value) {
      return
    }

    enterTextMode()
  }

  function onNumericKey(key: string): void {
    if (!isNumericVisible.value || hasExpression.value) {
      return
    }

    if (key.length !== 1 || !EXPRESSION_TRIGGERS.has(key)) {
      return
    }

    enterTextMode()
  }

  function commitNumeric(): boolean {
    const raw = numericText.value.trim()

    if (raw.length === 0) {
      hasError.value = false
      model.value = null
      return true
    }

    const value = Money.parseInput(normalizeNumericInput(raw))

    if (value === null) {
      hasError.value = true
      return false
    }

    hasError.value = false
    model.value = value
    return true
  }

  function getSum(): Money | null {
    const value = !isNumericVisible.value && calculationText.value.trim().length === 0 ? model.value : parsedSum.value
    if (isNumericVisible.value) {
      return commitNumeric() ? value : null
    }

    return tryUpdateSum() ? value : null
  }

  function clear(): void {
    hasError.value = false
    calculationText.value = ''
  }

  return {
    isNumericVisible,
    numericText,
    calculationText,
    hasExpression,
    parsedSum,
    hasError,
    enterTextMode,
    exitTextMode,
    onNumericFocus,
    onNumericKey,
    commitNumeric,
    clear,
    getSum,
  }
}
