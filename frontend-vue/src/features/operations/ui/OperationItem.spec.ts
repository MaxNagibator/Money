import { mount } from '@vue/test-utils'
import type { VueWrapper } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { defineComponent, h, ref } from 'vue'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { Operation } from '@/domain/operation'
import type { OperationTypeId } from '@/domain/operation-type'
import { formatMoney } from '@/platform/format'
import { provideOperationSave } from '../model/operation-save'
import OperationItem from './OperationItem.vue'

const viewport = vi.hoisted(() => ({ isNarrow: true }))

vi.mock('@/shared/ui/useNarrowViewport', () => ({
  NARROW_VIEWPORT: '(max-width: 599.98px)',
  useNarrowViewport: () => ref(viewport.isNarrow),
}))

vi.mock('../model/category-inference', () => ({
  inferCategoryCached: () => Promise.resolve(null),
  resetCategoryInference: () => {},
}))

vi.mock('@/shared/ui/usePlaceSuggestions', () => ({
  usePlaceSuggestions: () => ({ find: () => Promise.resolve([]), reset: () => {} }),
  findPlaces: () => Promise.resolve([]),
  resetPlaces: () => {},
}))

const SUM = Money.fromMinorUnits(123400)

function localDate(value: string): LocalDate {
  const parsed = parseLocalDate(value)

  if (parsed === null) {
    throw new Error(`Не разобрана дата ${value}`)
  }

  return parsed
}

const expense: Category = {
  id: 1,
  parentId: null,
  name: 'Еда',
  color: null,
  order: null,
  operationTypeId: 1,
  isDeleted: false,
}

const income: Category = { ...expense, id: 2, name: 'Зарплата', operationTypeId: 2 }

const categories: readonly Category[] = [expense, income]

function operation(overrides: Partial<Operation> = {}): Operation {
  return {
    id: 10,
    categoryId: expense.id,
    category: expense,
    sum: SUM,
    comment: 'Обед с коллегами',
    place: 'Кафе у дома',
    date: localDate('2026-09-17'),
    createdTaskId: null,
    isDeleted: false,
    ...overrides,
  }
}

function byType(operationTypeId: OperationTypeId): Operation {
  const category = operationTypeId === 2 ? income : expense

  return operation({ categoryId: category.id, category })
}

function mountItem(value: Operation, isNarrow: boolean): VueWrapper {
  viewport.isNarrow = isNarrow

  const host = defineComponent({
    setup() {
      provideOperationSave(() => Promise.resolve())

      return () => h(OperationItem, {
        operation: value,
        categories,
        isSaving: false,
      })
    },
  })

  return mount(host, { global: { stubs: { teleport: true } } })
}

describe('строка операции', () => {
  it('на узком экране правку открывает вся строка, корзины в ней нет', () => {
    const wrapper = mountItem(operation(), true)

    const edit = wrapper.get('[data-testid="operation-item-edit"]')

    expect(edit.element.tagName).toBe('BUTTON')
    expect(edit.classes()).toContain('operation-narrow')
    expect(edit.find('[data-testid="operation-item-category"]').exists()).toBe(true)
    expect(edit.find('[data-testid="operation-item-place"]').exists()).toBe(true)
    expect(edit.find('[data-testid="operation-item-comment"]').exists()).toBe(true)
    expect(edit.find('[data-testid="operation-item-sum"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="operation-item-remove"]').exists()).toBe(false)

    wrapper.unmount()
  })

  it('на узком экране у удалённой операции правка выключена, есть восстановление', () => {
    const wrapper = mountItem(operation({ isDeleted: true }), true)

    expect(wrapper.get('[data-testid="operation-item-edit"]').attributes('disabled')).toBeDefined()
    expect(wrapper.find('[data-testid="operation-item-restore"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="operation-item-remove"]').exists()).toBe(false)
    expect(wrapper.get('[data-testid="operation-item-sum"]').classes()).toContain('large-text')

    wrapper.unmount()
  })

  it('на широком экране в строке остаются и правка, и удаление', () => {
    const wrapper = mountItem(operation(), false)

    const edit = wrapper.get('[data-testid="operation-item-edit"]')

    expect(edit.classes()).not.toContain('operation-narrow')
    expect(wrapper.find('[data-testid="operation-item-remove"]').exists()).toBe(true)
    expect(wrapper.find('.operation-narrow-row').exists()).toBe(false)

    wrapper.unmount()
  })

  it.each<[string, OperationTypeId, string]>([
    ['доход', 2, `+${formatMoney(SUM)}`],
    ['расход', 1, formatMoney(SUM)],
  ])('на узком экране %s печатается суммой %s', (_name, operationTypeId, expected) => {
    const wrapper = mountItem(byType(operationTypeId), true)

    expect(wrapper.get('[data-testid="operation-item-sum"]').text()).toBe(expected)

    wrapper.unmount()
  })

  it('отрицательный доход не получает второго знака', () => {
    const negative = Money.fromMinorUnits(-50000)
    const wrapper = mountItem(operation({ categoryId: income.id, category: income, sum: negative }), true)

    expect(wrapper.get('[data-testid="operation-item-sum"]').text()).toBe(formatMoney(negative))

    wrapper.unmount()
  })
})
