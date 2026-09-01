import { describe, expect, it } from 'vitest'
import { nextTick, shallowRef } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import type { Clock } from '@/platform/clock'
import { useSmartDatePicker } from '@/shared/ui/useSmartDatePicker'

const TODAY = '2026-08-22' as LocalDate

const clock: Clock = {
  now: () => new Date('2026-08-22T12:00:00Z'),
  today: () => TODAY,
}

function setup(initial: LocalDate | null = null, blankKeepsValue = false) {
  const model = shallowRef<LocalDate | null>(initial)
  return { model, state: useSmartDatePicker(model, clock, { blankKeepsValue }) }
}

describe('useSmartDatePicker, текстовый режим', () => {
  it('начинает в текстовом режиме', () => {
    const { state } = setup()

    expect(state.isCalendarVisible.value).toBe(false)
    expect(state.dateText.value).toBe('')
  })

  it('показывает начальное значение в формате дд.мм.гггг', () => {
    const { state } = setup('2026-01-05' as LocalDate)

    expect(state.dateText.value).toBe('05.01.2026')
  })

  it.each([
    ['з', '2026-08-23', '23.08.2026'],
    ['22.08.2026', '2026-08-22', '22.08.2026'],
    ['1.1.2026', '2026-01-01', '01.01.2026'],
  ])('коммитит %j как %s и переформатирует текст', (input, expected, text) => {
    const { model, state } = setup()

    state.dateText.value = input

    expect(state.commitText()).toBe(true)
    expect(model.value).toBe(expected)
    expect(state.dateText.value).toBe(text)
    expect(state.hasError.value).toBe(false)
  })

  it('на невалидном вводе ставит ошибку, оставляет текст и прежнее значение', () => {
    const { model, state } = setup('2026-01-05' as LocalDate)

    state.dateText.value = '31.02.2026'

    expect(state.commitText()).toBe(false)
    expect(state.hasError.value).toBe(true)
    expect(state.dateText.value).toBe('31.02.2026')
    expect(model.value).toBe('2026-01-05')
    expect(state.getDate()).toBeNull()
  })

  it.each([[''], ['   ']])('пустой ввод %j очищает значение', input => {
    const { model, state } = setup('2026-01-05' as LocalDate)

    state.dateText.value = input

    expect(state.commitText()).toBe(true)
    expect(state.hasError.value).toBe(false)
    expect(state.dateText.value).toBe('')
    expect(model.value).toBeNull()
  })

  it.each([[''], ['   ']])('пустой ввод %j при blankKeepsValue не трёт значение и не отдаётся наружу', input => {
    const { model, state } = setup('2026-01-05' as LocalDate, true)

    state.dateText.value = input

    expect(state.commitText()).toBe(false)
    expect(state.hasError.value).toBe(false)
    expect(state.getDate()).toBeNull()
    expect(model.value).toBe('2026-01-05')
  })

  it('getDate отдаёт то же значение, что показано в поле', () => {
    const { state } = setup('2026-01-05' as LocalDate)

    state.dateText.value = 'asdf'

    expect(state.getDate()).toBeNull()

    state.dateText.value = 'в'

    expect(state.getDate()).toBe('2026-08-21')
    expect(state.dateText.value).toBe('21.08.2026')
  })

  it('внешняя запись значения обновляет поле поверх ошибки', async () => {
    const { model, state } = setup('2026-01-05' as LocalDate)

    state.dateText.value = 'asdf'
    state.commitText()
    model.value = '2026-03-09' as LocalDate
    await nextTick()

    expect(state.dateText.value).toBe('09.03.2026')
    expect(state.getDate()).toBe('2026-03-09')
    expect(state.hasError.value).toBe(false)
  })

  it('clear сбрасывает текст, ошибку и значение', () => {
    const { model, state } = setup('2026-01-05' as LocalDate)

    state.dateText.value = 'asdf'
    state.commitText()
    state.clear()

    expect(state.hasError.value).toBe(false)
    expect(state.dateText.value).toBe('')
    expect(model.value).toBeNull()
  })

  it('clear при blankKeepsValue оставляет значение', () => {
    const { model, state } = setup('2026-01-05' as LocalDate, true)

    state.clear()

    expect(state.dateText.value).toBe('')
    expect(model.value).toBe('2026-01-05')
  })
})

describe('useSmartDatePicker, календарь', () => {
  it('открывает календарь после разбора текущего текста', () => {
    const { model, state } = setup()

    state.dateText.value = 'завтра'

    expect(state.openCalendar()).toBe(true)
    expect(state.isCalendarVisible.value).toBe(true)
    expect(model.value).toBe('2026-08-23')
  })

  it('не открывает календарь при невалидном тексте', () => {
    const { state } = setup()

    state.dateText.value = 'зз'

    expect(state.openCalendar()).toBe(false)
    expect(state.isCalendarVisible.value).toBe(false)
    expect(state.hasError.value).toBe(true)
  })

  it('правка текста при открытом календаре не уходит в значение', () => {
    const { model, state } = setup('2026-01-05' as LocalDate)

    state.openCalendar()
    state.dateText.value = '09.03.2026'

    expect(state.commitText()).toBe(true)
    expect(state.hasError.value).toBe(false)
    expect(model.value).toBe('2026-01-05')

    state.closeCalendar()

    expect(state.dateText.value).toBe('05.01.2026')
  })

  it('закрытие календаря возвращает текст из значения', () => {
    const { model, state } = setup('2026-01-05' as LocalDate)

    state.openCalendar()
    model.value = '2026-03-09' as LocalDate
    state.closeCalendar()

    expect(state.isCalendarVisible.value).toBe(false)
    expect(state.dateText.value).toBe('09.03.2026')
  })

  it('карандаш возвращает в текстовый режим без валидации', () => {
    const { model, state } = setup('2026-01-05' as LocalDate)

    state.openCalendar()
    state.switchToTextInput()

    expect(state.isCalendarVisible.value).toBe(false)
    expect(state.dateText.value).toBe('05.01.2026')
    expect(model.value).toBe('2026-01-05')
  })
})
