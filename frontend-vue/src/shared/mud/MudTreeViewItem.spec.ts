import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { h } from 'vue'
import MudTreeViewItem from './MudTreeViewItem.vue'

const keys: [string, string][] = [
  ['Enter', 'Enter'],
  ['Space', ' '],
]

const treeContext = () => ({
  selectionMode: () => 'single-selection',
  isSelected: () => false,
  getCheckboxState: () => false,
  toggleSelection: vi.fn(),
  readOnly: () => false,
  disabled: () => false,
  expandOnClick: () => true,
  ripple: () => false,
  triState: () => false,
  color: () => 'primary',
  checkBoxColor: () => 'default',
})

const mountItem = (onActionKeydown = vi.fn()) => {
  const wrapper = mount(MudTreeViewItem, {
    props: {
      value: 'родитель',
      text: 'Родитель',
      items: [{ value: 'ребёнок', text: 'Ребёнок' }],
    },
    global: { provide: { 'mud-tree-view-context': treeContext() } },
    slots: {
      content: () => h('span', [
        'Родитель',
        h('button', { 'data-testid': 'row-action', 'onKeydown': onActionKeydown }, 'Ещё'),
      ]),
    },
  })

  return { wrapper, onActionKeydown }
}

const press = (element: Element, key: string) => {
  const event = new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true })
  const prevented = vi.spyOn(event, 'preventDefault')
  element.dispatchEvent(event)

  return prevented
}

describe('строка дерева и клавиатура', () => {
  it.each(keys)('%s на вложенной кнопке не трогает узел', (_name, key) => {
    const { wrapper, onActionKeydown } = mountItem()
    const prevented = press(wrapper.get('[data-testid="row-action"]').element, key)

    expect(wrapper.emitted('update:expanded')).toBeUndefined()
    expect(onActionKeydown).toHaveBeenCalledTimes(1)
    expect(prevented).not.toHaveBeenCalled()
  })

  it.each(keys)('%s на самой строке раскрывает узел', (_name, key) => {
    const { wrapper } = mountItem()
    const prevented = press(wrapper.get('div[role="button"]').element, key)

    expect(wrapper.emitted('update:expanded')).toEqual([[true]])
    expect(prevented).toHaveBeenCalled()
  })
})
