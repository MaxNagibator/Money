import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import type { Category, CategoryNode } from '@/domain/category'
import CategoryTree from './CategoryTree.vue'

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

function tree(children: CategoryNode[]): CategoryNode[] {
  return [node(1, 'Продукты', null, children)]
}

describe('дерево категорий', () => {
  it('раскрытая ветка остаётся раскрытой после пересборки списка узлов', async () => {
    const wrapper = mount(CategoryTree, { props: { nodes: tree([node(2, 'Хлеб', 1)]) } })

    const toggle = wrapper.get('.mud-treeview-item-arrow-expand')
    await toggle.trigger('click')

    expect(toggle.attributes('aria-label')).toBe('Collapse')

    await wrapper.setProps({ nodes: tree([node(2, 'Хлеб', 1), node(3, 'Молоко', 1)]) })

    expect(wrapper.get('.mud-treeview-item-arrow-expand').attributes('aria-label')).toBe('Collapse')
    expect(wrapper.text()).toContain('Молоко')
  })

  it('свёрнутая ветка после пересборки остаётся свёрнутой', async () => {
    const wrapper = mount(CategoryTree, { props: { nodes: tree([node(2, 'Хлеб', 1)]) } })

    await wrapper.setProps({ nodes: tree([node(2, 'Хлеб', 1), node(3, 'Молоко', 1)]) })

    expect(wrapper.get('.mud-treeview-item-arrow-expand').attributes('aria-label')).toBe('Expand')
  })
})
