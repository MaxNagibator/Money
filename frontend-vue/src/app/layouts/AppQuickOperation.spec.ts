import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import type { Component } from 'vue'
import { defineComponent, h, nextTick } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { OperationFormValue } from '@/features/operations'
import { provideOperationSave, useOperationFiltersStore } from '@/features/operations'
import AppQuickOperation from './AppQuickOperation.vue'

vi.mock('@/features/categories', async () => {
  const { computed } = await import('vue')

  return {
    useCategories: () => ({
      categories: computed(() => []),
      isLoading: computed(() => false),
      loadError: computed(() => null),
    }),
  }
})

const SAVED_NOTICE = 'Операция сохранена'
const OUTSIDE_FILTER_NOTICE = 'Операция сохранена. Она не подходит под условия показа, поэтому на экране её нет.'

const NOTICES: readonly [string, string, Partial<OperationFormValue>][] = [
  ['подходит под фильтр целиком', SAVED_NOTICE, {}],
  ['записана вне периода', OUTSIDE_FILTER_NOTICE, { date: date('2026-03-10') }],
  ['записана в другую категорию', OUTSIDE_FILTER_NOTICE, { categoryId: 2 }],
  ['записана в другом месте', OUTSIDE_FILTER_NOTICE, { place: 'Ашан' }],
  ['записана с другим комментарием', OUTSIDE_FILTER_NOTICE, { comment: 'бензин' }],
]

const DialogStub = defineComponent({
  emits: { submit: (value: OperationFormValue) => value !== null },
  setup(_props, { expose }) {
    expose({ open: () => {} })

    return () => h('div')
  },
})

beforeEach(() => {
  setActivePinia(createPinia())
})

describe('уведомление о быстрой операции', () => {
  it.each(NOTICES)('запись, которая %s, объясняется пользователю так: %s', async (_reason, message, changed) => {
    const wrapper = mount(host(), { global: { stubs: { OperationDialog: DialogStub } } })

    onTestFinished(() => {
      wrapper.unmount()
    })

    await nextTick()
    wrapper.findComponent(DialogStub).vm.$emit('submit', { ...form(), ...changed })
    await nextTick()

    expect(wrapper.get('[data-testid="bottom-nav-notice"] .mud-snackbar-content-message').text()).toBe(message)
  })
})

function host(): Component {
  const store = useOperationFiltersStore()

  store.applied.from = date('2026-01-01')
  store.applied.to = date('2026-01-31')
  store.applied.categoryIds = [1]
  store.applied.comment = 'кофе'
  store.applied.place = 'Пятёрочка'

  return {
    setup() {
      provideOperationSave(() => Promise.resolve())

      return () => h(AppQuickOperation)
    },
  }
}

function form(): OperationFormValue {
  return {
    id: null,
    operationTypeId: 1,
    categoryId: 1,
    sum: Money.fromMinorUnits(2_500),
    date: date('2026-01-20'),
    comment: 'Утренний КОФЕ',
    place: 'ПЯТЁРОЧКА на углу',
  }
}

function date(value: string): LocalDate {
  return value as LocalDate
}
