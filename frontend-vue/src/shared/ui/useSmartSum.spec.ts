import { describe, expect, it } from 'vitest'
import { nextTick, shallowRef } from 'vue'
import type { Money } from '@/domain/money'
import type { SmartSumState } from '@/shared/ui/useSmartSum'
import { useSmartSum } from '@/shared/ui/useSmartSum'

const brokenInputs: [string, (state: SmartSumState) => void][] = [
  ['числовом поле', state => {
    state.numericText.value = 'абв'
  }],
  ['поле выражения', state => {
    state.enterTextMode()
    state.calculationText.value = '(1+2'
  }],
]

function setup(initial: Money | null = null) {
  const model = shallowRef<Money | null>(initial)
  return { model, state: useSmartSum(model) }
}

describe('useSmartSum, режимы ввода', () => {
  it('начинает в числовом режиме', () => {
    const { state } = setup()

    expect(state.isNumericVisible.value).toBe(true)
    expect(state.hasExpression.value).toBe(false)
  })

  it.each([['+'], ['-'], ['*'], ['/'], ['('], [')'], ['.'], [',']])(
    'переходит в режим выражения по вводу %j',
    key => {
      const { state } = setup()

      state.onNumericKey(key)

      expect(state.isNumericVisible.value).toBe(false)
    },
  )

  it.each([['1'], ['a'], ['Backspace'], ['Enter']])('не переходит по вводу %j', key => {
    const { state } = setup()

    state.onNumericKey(key)

    expect(state.isNumericVisible.value).toBe(true)
  })

  it('переносит текущее число в поле выражения при переходе', () => {
    const { state } = setup()

    state.numericText.value = '1234,56'
    state.commitNumeric()
    state.enterTextMode()

    expect(state.calculationText.value).toBe('1234,56')
  })

  it('переносит набранное, но не зафиксированное число в поле выражения', () => {
    const { state } = setup()

    state.numericText.value = '1'
    state.onNumericKey('+')

    expect(state.isNumericVisible.value).toBe(false)
    expect(state.calculationText.value).toBe('1')
  })

  it.each([
    ['числовом поле', (state: SmartSumState) => {
      state.numericText.value = '1e3'
      state.commitNumeric()
    }],
    ['поле выражения', (state: SmartSumState) => {
      state.enterTextMode()
      state.calculationText.value = '1e3'
      state.exitTextMode()
    }],
  ])('принимает экспоненциальную запись в %s', (_, enterInput) => {
    const { model, state } = setup()

    enterInput(state)

    expect(state.hasError.value).toBe(false)
    expect(model.value?.toMinorUnits()).toBe(100_000)
  })

  it('вычисляет выражение при выходе и возвращает числовой режим', () => {
    const { model, state } = setup()

    state.enterTextMode()
    state.calculationText.value = '1+5 555,00'
    state.exitTextMode()

    expect(state.isNumericVisible.value).toBe(true)
    expect(model.value?.toMinorUnits()).toBe(555_600)
    expect(state.numericText.value).toBe('5556')
  })

  it('держит поле выражения открытым, пока выражение неверно', () => {
    const { model, state } = setup()

    state.numericText.value = '2,5'
    state.commitNumeric()

    state.enterTextMode()
    state.calculationText.value = '(1+2'
    state.exitTextMode()

    expect(state.isNumericVisible.value).toBe(false)
    expect(state.hasError.value).toBe(true)
    expect(model.value?.toMinorUnits()).toBe(250)
  })

  it('возвращает в режим выражения по фокусу, пока выражение живо', () => {
    const { state } = setup()

    state.enterTextMode()
    state.calculationText.value = '1+2'
    state.exitTextMode()

    expect(state.isNumericVisible.value).toBe(true)
    expect(state.hasExpression.value).toBe(true)

    state.onNumericFocus()

    expect(state.isNumericVisible.value).toBe(false)
    expect(state.calculationText.value).toBe('1+2')
  })

  it('не трогает режим по фокусу, когда выражения нет', () => {
    const { state } = setup()

    state.numericText.value = '10'
    state.commitNumeric()
    state.onNumericFocus()

    expect(state.isNumericVisible.value).toBe(true)
  })

  it('позволяет дополнить сохранённое выражение', () => {
    const { model, state } = setup()

    state.enterTextMode()
    state.calculationText.value = '1+2'
    state.exitTextMode()

    state.onNumericFocus()
    state.calculationText.value = '1+2+5'
    state.exitTextMode()

    expect(model.value?.toMinorUnits()).toBe(800)
  })

  it('очистка сбрасывает выражение, не трогая сумму', () => {
    const { model, state } = setup()

    state.enterTextMode()
    state.calculationText.value = '1+2'
    state.exitTextMode()
    state.clear()

    expect(model.value?.toMinorUnits()).toBe(300)
    expect(state.hasExpression.value).toBe(false)
    expect(state.calculationText.value).toBe('')
  })

  it('пустое выражение оставляет прежнюю сумму', () => {
    const { model, state } = setup()

    state.numericText.value = '10'
    state.commitNumeric()
    state.enterTextMode()
    state.calculationText.value = '   '
    state.exitTextMode()

    expect(model.value?.toMinorUnits()).toBe(1000)
    expect(state.isNumericVisible.value).toBe(true)
  })
})

describe('useSmartSum, снятие суммы на сохранении', () => {
  it('фиксирует незафиксированный числовой ввод', () => {
    const { model, state } = setup()

    state.numericText.value = '1234,56'

    expect(state.getSum()?.toMinorUnits()).toBe(123_456)
    expect(model.value?.toMinorUnits()).toBe(123_456)
  })

  it('масштаб набранной суммы переживает потерю фокуса', async () => {
    const { model, state } = setup()

    state.numericText.value = '250,50'
    state.commitNumeric()

    await nextTick()

    expect(state.numericText.value).toBe('250,50')
    expect(state.getSum()?.wireScale).toBe(2)
    expect(model.value?.wireScale).toBe(2)
  })

  it('вычисленная из выражения сумма масштаба не получает', () => {
    const { state } = setup()

    state.enterTextMode()
    state.calculationText.value = '250,50*1'

    expect(state.getSum()?.wireScale).toBeNull()
  })

  it('вычисляет выражение, не переключая режим', () => {
    const { state } = setup()

    state.enterTextMode()
    state.calculationText.value = '1+2'

    expect(state.getSum()?.toMinorUnits()).toBe(300)
    expect(state.isNumericVisible.value).toBe(false)
  })

  it.each(brokenInputs)('сообщает о нераспознанном значении в %s, не отдавая прежнюю сумму', (_, breakInput) => {
    const { model, state } = setup()

    state.numericText.value = '2,5'
    state.commitNumeric()
    breakInput(state)

    expect(state.getSum()).toBeNull()
    expect(state.hasError.value).toBe(true)
    expect(model.value?.toMinorUnits()).toBe(250)
  })

  it('отдаёт null на пустом поле', () => {
    const { model, state } = setup()

    state.numericText.value = '10'
    state.commitNumeric()
    state.numericText.value = '   '

    expect(state.getSum()).toBeNull()
    expect(model.value).toBeNull()
  })
})
