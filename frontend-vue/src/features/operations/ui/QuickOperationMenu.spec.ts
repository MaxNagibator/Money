import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import type { FastOperation } from '@/api/adapters/fast-operations'
import { Money } from '@/domain/money'
import QuickOperationMenu from './QuickOperationMenu.vue'

const TEST_ID = 'operations-day-quick-2026-09-16'

const coffee: FastOperation = {
  id: 20,
  name: 'Кофе',
  categoryId: 1,
  sum: Money.fromWire(150),
  comment: null,
  place: null,
  order: null,
  isDeleted: false,
}

function render(operations: readonly FastOperation[], withItems: boolean) {
  return mount(QuickOperationMenu, {
    props: {
      operations,
      actionLabel: 'Добавить за 16.09.2026',
      color: 'default',
      testId: TEST_ID,
    },
    slots: withItems ? { items: '<div data-testid="operations-day-add-1">Расход</div>' } : {},
  })
}

describe('QuickOperationMenu', () => {
  it('рисует кнопку без быстрых операций, когда пункты пришли слотом', () => {
    const wrapper = render([], true)

    expect(wrapper.find(`[data-testid="${TEST_ID}"]`).exists()).toBe(true)
  })

  it('рисует кнопку, когда есть только быстрые операции', () => {
    const wrapper = render([coffee], false)

    expect(wrapper.find(`[data-testid="${TEST_ID}"]`).exists()).toBe(true)
  })

  it('не рисует кнопку без быстрых операций и без пунктов слота', () => {
    const wrapper = render([], false)

    expect(wrapper.find(`[data-testid="${TEST_ID}"]`).exists()).toBe(false)
  })

  it('не рисует кнопку, когда все быстрые операции удалены', () => {
    const wrapper = render([{ ...coffee, isDeleted: true }], false)

    expect(wrapper.find(`[data-testid="${TEST_ID}"]`).exists()).toBe(false)
  })
})
