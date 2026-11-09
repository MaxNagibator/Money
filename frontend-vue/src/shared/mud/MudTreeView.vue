<script setup lang="ts">
import { computed, provide } from 'vue'
import MudTreeViewItem from './MudTreeViewItem.vue'
import type { MudColor, MudTreeItemData } from './types'

type MudSelectionMode = 'single-selection' | 'multi-selection' | 'toggle-selection'

interface MudTreeViewContext {
  selectionMode: () => MudSelectionMode
  isSelected: (value: unknown) => boolean
  getCheckboxState: (value: unknown, descendants: readonly unknown[]) => boolean | null
  toggleSelection: (value: unknown, descendants: readonly unknown[]) => void
  readOnly: () => boolean
  disabled: () => boolean
  expandOnClick: () => boolean
  ripple: () => boolean
  triState: () => boolean
  color: () => MudColor
  checkBoxColor: () => MudColor
}

const props = withDefaults(defineProps<{
  color?: MudColor
  checkBoxColor?: MudColor
  selectionMode?: MudSelectionMode
  triState?: boolean
  autoSelectParent?: boolean
  expandOnClick?: boolean
  hover?: boolean
  dense?: boolean
  maxHeight?: string
  readOnly?: boolean
  ripple?: boolean
  items?: readonly MudTreeItemData[]
}>(), {
  color: 'primary',
  checkBoxColor: 'default',
  selectionMode: 'single-selection',
  triState: true,
  autoSelectParent: true,
  ripple: true,
})

const selectedValue = defineModel<unknown>('selectedValue')
const selectedValues = defineModel<readonly unknown[] | null>('selectedValues')

const parentByValue = new Map<unknown, unknown>()
const childrenByValue = new Map<unknown, unknown[]>()

const items = computed(() => props.items ?? [])

const isValue = (value: unknown): boolean => value !== null && value !== undefined

const sameValue = (left: unknown, right: unknown): boolean => Object.is(left, right)

const valueSelected = (value: unknown, selection: ReadonlySet<unknown>): boolean => {
  for (const selected of selection) {
    if (sameValue(selected, value)) {
      return true
    }
  }

  return false
}

const directValuesFor = (nodes: readonly MudTreeItemData[]): unknown[] => {
  const values: unknown[] = []

  for (const node of nodes) {
    if (isValue(node.value)) {
      values.push(node.value)
    }
  }

  return values
}

const rebuildRelationships = (nodes: readonly MudTreeItemData[], parent?: unknown): void => {
  for (const node of nodes) {
    if (isValue(node.value)) {
      if (isValue(parent)) {
        parentByValue.set(node.value, parent)
      }

      childrenByValue.set(node.value, node.children ? directValuesFor(node.children) : [])
    }

    if (node.children) {
      rebuildRelationships(node.children, node.value)
    }
  }
}

const refreshRelationships = (): void => {
  parentByValue.clear()
  childrenByValue.clear()
  rebuildRelationships(items.value)
}

const currentSelection = (): Set<unknown> => new Set(selectedValues.value ?? [])

const isSelected = (value: unknown): boolean => {
  if (!isValue(value)) {
    return false
  }

  if (props.selectionMode === 'multi-selection') {
    return valueSelected(value, currentSelection())
  }

  return sameValue(selectedValue.value, value)
}

const getCheckboxState = (value: unknown, descendants: readonly unknown[]): boolean | null => {
  const selected = isSelected(value)
  const allChildrenSelected = descendants.every(child => isSelected(child))
  const noChildrenSelected = descendants.every(child => !isSelected(child))

  if (allChildrenSelected && selected) {
    return true
  }

  if (noChildrenSelected && !selected) {
    return false
  }

  return props.triState ? null : selected
}

const toggleSelection = (value: unknown, descendants: readonly unknown[]): void => {
  if (!isValue(value) || props.readOnly) {
    return
  }

  if (props.selectionMode === 'multi-selection') {
    refreshRelationships()
    const selection = currentSelection()
    const values = [value, ...descendants].filter(isValue)
    const allSelected = values.every(item => valueSelected(item, selection))

    for (const item of values) {
      if (allSelected) {
        selection.delete(item)
      }
      else {
        selection.add(item)
      }
    }

    if (props.autoSelectParent) {
      let parent = parentByValue.get(value)

      while (isValue(parent)) {
        const childValues = childrenByValue.get(parent) ?? []
        const parentSelected = childValues.every(item => valueSelected(item, selection))

        if (parentSelected) {
          selection.add(parent)
        }
        else {
          selection.delete(parent)
        }

        parent = parentByValue.get(parent)
      }
    }

    selectedValues.value = [...selection]
    return
  }

  if (props.selectionMode === 'toggle-selection') {
    selectedValue.value = isSelected(value) ? null : value
    return
  }

  if (!isSelected(value)) {
    selectedValue.value = value
  }
}

refreshRelationships()

const context: MudTreeViewContext = {
  selectionMode: () => props.selectionMode,
  isSelected,
  getCheckboxState,
  toggleSelection,
  readOnly: () => props.readOnly === true,
  disabled: () => false,
  expandOnClick: () => props.expandOnClick === true,
  ripple: () => props.ripple,
  triState: () => props.triState,
  color: () => props.color,
  checkBoxColor: () => props.checkBoxColor,
}

provide('mud-tree-view-context', context)

const classes = computed(() => [
  'mud-treeview',
  {
    'mud-treeview-dense': props.dense,
    'mud-treeview-hover': props.hover && (!props.readOnly || props.expandOnClick),
  },
  `mud-treeview-selected-${props.color}`,
  `mud-treeview-checked-${props.checkBoxColor}`,
])

const styles = computed(() => ({
  'max-height': props.maxHeight,
}))

const itemKey = (item: MudTreeItemData, index: number): string | number =>
  typeof item.value === 'string' || typeof item.value === 'number' ? item.value : index

const setExpanded = (nodes: readonly MudTreeItemData[], value: boolean): void => {
  for (const node of nodes) {
    if (node.children && node.children.length > 0) {
      node.expanded = value
      setExpanded(node.children, value)
    }
  }
}

const expandAll = (): void => {
  refreshRelationships()
  setExpanded(items.value, true)
}

const collapseAll = (): void => {
  refreshRelationships()
  setExpanded(items.value, false)
}

defineExpose({
  collapseAll,
  collapseAllAsync: collapseAll,
  expandAll,
  expandAllAsync: expandAll,
})
</script>

<template>
  <ul
    :class="classes"
    :style="styles"
  >
    <MudTreeViewItem
      v-for="(item, index) in items"
      :key="itemKey(item, index)"
      :expanded="item.expanded ?? false"
      :disabled="item.disabled ?? false"
      :end-icon="item.endIcon"
      :end-icon-color="item.endIconColor ?? 'default'"
      :icon="item.icon"
      :items="item.children"
      :text="item.text"
      :value="item.value"
      :visible="item.visible ?? true"
      @update:expanded="value => item.expanded = value"
    >
      <template
        v-if="$slots.itemTemplate"
        #content="{ item: currentItem }"
      >
        <slot
          name="itemTemplate"
          :item="currentItem"
        />
      </template>
    </MudTreeViewItem>
  </ul>
</template>
