import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import CategoriesPage from './CategoriesPage.vue'

const viewport = vi.hoisted(() => ({ narrow: false }))

vi.mock('@/shared/ui/useNarrowViewport', async () => {
  const { computed } = await import('vue')

  return {
    NARROW_VIEWPORT: '(max-width: 599.98px)',
    useNarrowViewport: () => computed(() => viewport.narrow),
  }
})

vi.mock('@/features/categories', async () => {
  const { computed, defineComponent, h } = await import('vue')

  const Stub = defineComponent({ setup: () => () => h('div') })

  return {
    CategoryDialog: Stub,
    CategoryTree: Stub,
    editCategoryForm: () => null,
    newCategoryForm: () => null,
    toDraft: () => ({}),
    useCategories: () => ({
      categories: computed(() => []),
      isLoading: computed(() => false),
      loadError: computed(() => null),
      save: () => Promise.resolve(),
      remove: () => Promise.resolve(),
      restore: () => Promise.resolve(),
    }),
  }
})

async function mountPage(narrow: boolean) {
  viewport.narrow = narrow

  const wrapper = mount(CategoriesPage, { attachTo: document.body })

  await nextTick()

  return wrapper
}

describe('страница категорий', () => {
  it('на узком экране кнопка добавления стоит в строке заголовка, а не в полосе вкладок', async () => {
    const wrapper = await mountPage(true)

    expect(wrapper.find('.categories-heading [data-testid="category-add-root"]').exists()).toBe(true)
    expect(wrapper.find('.mud-tabs-panel-header').exists()).toBe(false)

    wrapper.unmount()
  })

  it('на широком экране кнопка добавления остаётся в заголовке активной вкладки', async () => {
    const wrapper = await mountPage(false)

    expect(wrapper.find('.categories-heading [data-testid="category-add-root"]').exists()).toBe(false)
    expect(wrapper.find('.mud-tabs-panel-header [data-testid="category-add-root"]').exists()).toBe(true)

    wrapper.unmount()
  })
})
