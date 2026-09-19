import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { defineComponent, h, nextTick } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { OperationFormValue } from '@/features/operations'
import { useOperationFiltersStore } from '@/features/operations'
import OperationsPage from './OperationsPage.vue'

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

vi.mock('@/features/fast-operations', async () => {
  const { computed, ref } = await import('vue')

  return {
    useFastOperations: () => ({
      operations: computed(() => []),
      isLoading: ref(false),
      loadError: computed(() => null),
      save: () => Promise.resolve(),
      remove: () => Promise.resolve(),
      restore: () => Promise.resolve(),
      reload: () => Promise.resolve(),
    }),
  }
})

vi.mock('@/shared/ui/useNarrowViewport', async () => {
  const { ref } = await import('vue')

  return { NARROW_VIEWPORT: '(max-width: 599.98px)', useNarrowViewport: () => ref(false) }
})

vi.mock('@/api/adapters/operations', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/operations')>(),
  fetchOperations: () => Promise.resolve([]),
}))

const DialogStub = defineComponent({
  emits: { submit: (value: OperationFormValue) => value !== null },
  setup: () => () => h('div'),
})

beforeEach(() => {
  setActivePinia(createPinia())
})

describe('страница операций', () => {
  it('о сохранённой записи вне периода говорит по-прежнему, без оговорки оболочки', async () => {
    const store = useOperationFiltersStore()

    store.filters.from = date('2026-01-01')
    store.filters.to = date('2026-01-31')

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    const wrapper = mount(OperationsPage, {
      global: {
        plugins: [[VueQueryPlugin, { queryClient }]],
        stubs: { OperationDialog: DialogStub, teleport: true },
      },
    })

    onTestFinished(() => {
      wrapper.unmount()
    })

    await nextTick()
    wrapper.findComponent(DialogStub).vm.$emit('submit', form('2026-03-10'))
    await nextTick()

    expect(wrapper.get('[data-testid="operations-notice"]').text()).toContain('Операция сохранена')
    expect(wrapper.get('[data-testid="operations-notice"]').text()).not.toContain('период')
  })
})

function form(day: string): OperationFormValue {
  return {
    id: null,
    operationTypeId: 1,
    categoryId: 1,
    sum: Money.fromMinorUnits(2_500),
    date: date(day),
    comment: '',
    place: '',
  }
}

function date(value: string): LocalDate {
  return value as LocalDate
}
