import { mount } from '@vue/test-utils'
import type { VueWrapper } from '@vue/test-utils'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import type { Category, CategoryNode } from '@/domain/category'
import CategoryTree from './CategoryTree.vue'

const viewport = vi.hoisted(() => ({ isNarrow: false }))

vi.mock('@/shared/ui/useNarrowViewport', () => ({
  NARROW_VIEWPORT: '(max-width: 599.98px)',
  useNarrowViewport: () => ref(viewport.isNarrow),
}))

function category(id: number, name: string, parentId: number | null): Category {
  return {
    id,
    parentId,
    name,
    color: null,
    order: null,
    operationTypeId: 1,
    isDeleted: false,
  }
}

function node(id: number, name: string, parentId: number | null, children: CategoryNode[] = []): CategoryNode {
  return { category: category(id, name, parentId), children }
}

let mounted: VueWrapper | null = null

function mountTree(nodes: CategoryNode[], isNarrow: boolean): VueWrapper {
  viewport.isNarrow = isNarrow
  mounted = mount(CategoryTree, { props: { nodes }, attachTo: document.body })

  return mounted
}

function menuItem(testId: string): HTMLElement | null {
  return document.querySelector<HTMLElement>(`.mud-popover-provider [data-testid="${testId}"]`)
}

afterEach(() => {
  mounted?.unmount()
  mounted = null
})

function tree(children: CategoryNode[]): CategoryNode[] {
  return [node(1, 'Продукты', null, children)]
}

describe('дерево категорий', () => {
  it('раскрытая ветка остаётся раскрытой после пересборки списка узлов', async () => {
    const wrapper = mountTree(tree([node(2, 'Хлеб', 1)]), false)

    const toggle = wrapper.get('.mud-treeview-item-arrow-expand')
    await toggle.trigger('click')

    expect(toggle.attributes('aria-label')).toBe('Свернуть')

    await wrapper.setProps({ nodes: tree([node(2, 'Хлеб', 1), node(3, 'Молоко', 1)]) })

    expect(wrapper.get('.mud-treeview-item-arrow-expand').attributes('aria-label')).toBe('Свернуть')
    expect(wrapper.text()).toContain('Молоко')
  })

  it('свёрнутая ветка после пересборки остаётся свёрнутой', async () => {
    const wrapper = mountTree(tree([node(2, 'Хлеб', 1)]), false)

    await wrapper.setProps({ nodes: tree([node(2, 'Хлеб', 1), node(3, 'Молоко', 1)]) })

    expect(wrapper.get('.mud-treeview-item-arrow-expand').attributes('aria-label')).toBe('Развернуть')
  })
})

describe('строка категории на узком экране', () => {
  function coloured(id: number, name: string, color: string | null, isDeleted = false): CategoryNode {
    const item = node(id, name, null)

    return { category: { ...item.category, color, isDeleted }, children: [] }
  }

  it('цвет показан точкой перед именем, а не таблеткой', () => {
    const wrapper = mountTree([coloured(1, 'Продукты', '#ff0000')], true)

    const row = wrapper.get('.category-tree-item')

    expect(row.find('.category-color').exists()).toBe(false)

    const children = [...row.element.children]
    const dot = row.get('.category-dot')

    expect(children.indexOf(dot.element)).toBeLessThan(children.findIndex(child => child.classList.contains('category-name')))
  })

  it.each([
    ['category-add-child', 'create'],
    ['category-edit', 'edit'],
    ['category-remove', 'remove'],
  ] as const)('пункт %s доступен только после открытия меню и шлёт событие', async (testId, event) => {
    const wrapper = mountTree([coloured(1, 'Продукты', '#ff0000')], true)

    expect(menuItem(testId)).toBeNull()

    await wrapper.get('[data-testid="category-actions-menu"] button').trigger('click')

    menuItem(testId)?.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted(event)).toHaveLength(1)
  })

  it('открытие меню не раскрывает ветку', async () => {
    const wrapper = mountTree(tree([node(2, 'Хлеб', 1)]), true)

    await wrapper.get('[data-testid="category-actions-menu"] button').trigger('click')

    expect(wrapper.get('.mud-treeview-item-arrow-expand').attributes('aria-label')).toBe('Развернуть')
  })

  it('удалённая категория восстанавливается иконкой без текста', async () => {
    const wrapper = mountTree([coloured(1, 'Продукты', '#ff0000', true)], true)

    const restore = wrapper.get('[data-testid="category-restore"]')

    expect(restore.text()).toBe('')

    await restore.trigger('click')

    expect(wrapper.emitted('restore')).toHaveLength(1)
  })

  it('на широком экране остаются таблетка и три кнопки', () => {
    const wrapper = mountTree([coloured(1, 'Продукты', '#ff0000')], false)

    expect(wrapper.find('.category-dot').exists()).toBe(false)
    expect(wrapper.find('.category-color').exists()).toBe(true)
    expect(wrapper.find('[data-testid="category-actions-menu"]').exists()).toBe(false)

    for (const testId of ['category-add-child', 'category-edit', 'category-remove']) {
      expect(wrapper.find(`[data-testid="${testId}"]`).exists()).toBe(true)
    }
  })
})
