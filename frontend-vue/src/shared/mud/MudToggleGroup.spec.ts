import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import { defineComponent, h, nextTick, ref } from 'vue'
import type { MudSelectionMode } from './types'
import MudToggleGroup from './MudToggleGroup.vue'
import MudToggleItem from './MudToggleItem.vue'

const values = ['light', 'dark', 'system']

const mountGroup = (selectionMode: MudSelectionMode, initial: unknown) => {
  const model = ref<unknown>(initial)
  const Host = defineComponent(() => () => h(
    MudToggleGroup,
    {
      'selectionMode': selectionMode,
      'modelValue': model.value,
      'onUpdate:modelValue': (value: unknown) => {
        model.value = value
      },
    },
    () => values.map(value => h(MudToggleItem, { key: value, value })),
  ))
  const wrapper = mount(Host, { attachTo: document.body })

  return { wrapper, model }
}

const buttons = (wrapper: ReturnType<typeof mount>) => wrapper.findAll('button').map(button => button.element)

const press = async (element: Element, key: string) => {
  element.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true }))
  await nextTick()
}

describe('MudToggleGroup', () => {
  it.each<[MudSelectionMode, string, string]>([
    ['single-selection', 'radiogroup', 'radio'],
    ['toggle-selection', 'radiogroup', 'radio'],
    ['multi-selection', 'group', 'checkbox'],
  ])('%s: группа %s, элементы %s', async (mode, groupRole, itemRole) => {
    const { wrapper } = mountGroup(mode, 'dark')
    await nextTick()

    expect(wrapper.get('.mud-toggle-group').attributes('role')).toBe(groupRole)
    expect(buttons(wrapper).map(button => button.getAttribute('role'))).toEqual([itemRole, itemRole, itemRole])
    expect(buttons(wrapper).map(button => button.getAttribute('aria-checked'))).toEqual(['false', 'true', 'false'])
    wrapper.unmount()
  })

  it.each<[unknown, (string | null)[]]>([
    ['dark', ['-1', '0', '-1']],
    [null, ['0', '-1', '-1']],
  ])('в порядке Tab только один элемент при выборе %s', async (initial, expected) => {
    const { wrapper } = mountGroup('toggle-selection', initial)
    await nextTick()

    expect(buttons(wrapper).map(button => button.getAttribute('tabindex'))).toEqual(expected)
    wrapper.unmount()
  })

  it('у флажков порядок Tab не переписывается', async () => {
    const { wrapper } = mountGroup('multi-selection', 'dark')
    await nextTick()

    expect(buttons(wrapper).map(button => button.getAttribute('tabindex'))).toEqual([null, null, null])
    wrapper.unmount()
  })

  it.each<[string, number, string]>([
    ['ArrowRight', 1, 'system'],
    ['ArrowDown', 1, 'system'],
    ['ArrowLeft', 1, 'light'],
    ['ArrowUp', 1, 'light'],
    ['ArrowRight', 2, 'light'],
    ['ArrowLeft', 0, 'system'],
  ])('%s с элемента %i выбирает %s и переносит фокус', async (key, from, expected) => {
    const { wrapper, model } = mountGroup('single-selection', values[from])
    await nextTick()
    const items = buttons(wrapper)
    items[from]?.focus()

    await press(items[from] as Element, key)

    expect(model.value).toBe(expected)
    expect(document.activeElement).toBe(items[values.indexOf(expected)])
    expect(items[values.indexOf(expected)]?.getAttribute('tabindex')).toBe('0')
    wrapper.unmount()
  })

  it('стрелка без выбора выбирает следующий элемент, а не снимает выбор', async () => {
    const { wrapper, model } = mountGroup('toggle-selection', null)
    await nextTick()

    await press(buttons(wrapper)[0] as Element, 'ArrowRight')

    expect(model.value).toBe('dark')
    wrapper.unmount()
  })

  it('стрелка пропускает выключенный элемент', async () => {
    const { wrapper, model } = mountGroup('single-selection', 'light')
    await nextTick()
    const items = buttons(wrapper)
    ;(items[1] as HTMLButtonElement).disabled = true

    await press(items[0] as Element, 'ArrowRight')

    expect(model.value).toBe('system')
    wrapper.unmount()
  })

  it('стрелки не трогают группу флажков', async () => {
    const { wrapper, model } = mountGroup('multi-selection', 'dark')
    await nextTick()

    await press(buttons(wrapper)[1] as Element, 'ArrowRight')

    expect(model.value).toBe('dark')
    wrapper.unmount()
  })
})
