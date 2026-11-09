import { flushPromises, mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import SuggestionField from './SuggestionField.vue'

const PLACES = ['Пекарня', 'Пятёрочка']

const notAccepting: [string, readonly string[]][] = [
  ['подсказки открыты, но ничего не подсвечено', PLACES],
  ['подсказок нет', []],
]

function field(found: readonly string[]) {
  return mount(SuggestionField, {
    props: {
      find: () => Promise.resolve(found),
      icon: '',
      idPrefix: 'sp',
      label: 'Место',
      modelValue: '',
      testId: 'place-suggestions',
    },
  })
}

function pressKey(element: Element, key: string): KeyboardEvent {
  const event = new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true })

  element.dispatchEvent(event)

  return event
}

describe('поле с подсказками', () => {
  it.each(notAccepting)('Enter не гасится, когда %s, и оставляет неявную отправку формы', async (_, found) => {
    const wrapper = field(found)
    const input = wrapper.get('input')

    await input.trigger('focus')
    await flushPromises()

    const event = pressKey(input.element, 'Enter')

    expect(event.defaultPrevented).toBe(false)
    expect(wrapper.emitted('committed')).toHaveLength(1)

    wrapper.unmount()
  })

  it('Enter принимает подсвеченную подсказку и гасит отправку формы', async () => {
    const wrapper = field(PLACES)
    const input = wrapper.get('input')

    await input.trigger('focus')
    await flushPromises()
    await input.trigger('keydown', { key: 'ArrowDown' })

    const event = pressKey(input.element, 'Enter')

    expect(event.defaultPrevented).toBe(true)
    expect(wrapper.emitted('update:modelValue')).toEqual([['Пекарня']])

    wrapper.unmount()
  })
})
