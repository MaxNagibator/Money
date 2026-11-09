import { describe, expect, it } from 'vitest'
import { ref } from 'vue'
import { useSmartTimePicker } from '@/shared/ui/useSmartTimePicker'

function setup(initial = '18:00') {
  const model = ref(initial)
  return { model, state: useSmartTimePicker(model) }
}

describe('useSmartTimePicker', () => {
  it('показывает значение модели в 24-часовом виде', () => {
    const { state } = setup('06:00')

    expect(state.timeText.value).toBe('06:00')
  })

  it.each([
    ['1930', '19:30'],
    ['7', '07:00'],
    ['9:5', '09:05'],
  ])('нормализует ввод %j при фиксации', (raw, expected) => {
    const { model, state } = setup()

    state.timeText.value = raw

    expect(state.commitText()).toBe(true)
    expect(state.timeText.value).toBe(expected)
    expect(model.value).toBe(expected)
    expect(state.hasError.value).toBe(false)
  })

  it('помечает ошибкой неразобранный ввод и не выдаёт минуты', () => {
    const { model, state } = setup()

    state.timeText.value = '6:00 PM'

    expect(state.commitText()).toBe(false)
    expect(state.hasError.value).toBe(true)
    expect(model.value).toBe('6:00 PM')
    expect(state.getMinutes()).toBeNull()
  })

  it('пустой ввод очищает значение без ошибки', () => {
    const { model, state } = setup()

    state.timeText.value = '   '

    expect(state.commitText()).toBe(true)
    expect(state.hasError.value).toBe(false)
    expect(model.value).toBe('')
  })

  it('не открывает часы, пока ввод не разобран', () => {
    const { state } = setup()

    state.timeText.value = 'вечер'

    expect(state.openClock()).toBe(false)
    expect(state.isClockVisible.value).toBe(false)
  })

  it('возврат к тексту снимает ошибку и берёт значение модели', () => {
    const { model, state } = setup()

    state.timeText.value = '25:00'
    state.commitText()
    model.value = '21:15'
    state.switchToTextInput()

    expect(state.hasError.value).toBe(false)
    expect(state.timeText.value).toBe('21:15')
  })

  it('отдаёт минуты от полуночи для разобранного времени', () => {
    const { state } = setup()

    state.timeText.value = '18:45'

    expect(state.getMinutes()).toBe(1125)
  })
})
