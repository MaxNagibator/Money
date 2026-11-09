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
  hasError: Ref<boolean>
  enterTextMode: () => void
  exitTextMode: () => void
  onNumericFocus: () => void
  onNumericKey: (key: string) => void
  commitNumeric: () => boolean
  clear: () => void
  getSum: () => Money | null
}

export function useSmartSum(model: Ref<Money | null>): SmartSumState {
  const isNumericVisible = ref(true)
  const numericText = ref('')
  const calculationText = ref('')
  const hasError = ref(false)

  const hasExpression = computed(() => {
    const raw = calculationText.value.trim()
    return raw.length > 0 && isMoneyExpression(raw)
  })

  watch(
    model,
    value => {
      numericText.value = value === null ? '' : formatMoneyInput(value)
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
    numericText.value = formatMoneyInput(value)
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
    if (isNumericVisible.value) {
      return commitNumeric() ? model.value : null
    }

    return tryUpdateSum() ? model.value : null
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
