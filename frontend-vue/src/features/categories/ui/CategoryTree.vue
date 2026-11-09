<script setup lang="ts">
import { computed, reactive } from 'vue'
import type { Category, CategoryNode } from '@/domain/category'
import { MudSpacer, MudText, MudTreeView, MudTreeViewItemToggleButton } from '@/shared/mud'
import type { MudTreeItemData } from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import CategoryNodeActions from './CategoryNodeActions.vue'

interface TreeItem extends MudTreeItemData {
  value: number
  text: string
  category: Category
  children?: TreeItem[]
}

const props = defineProps<{ nodes: CategoryNode[] }>()

const emit = defineEmits<{
  create: [parentId: number]
  edit: [category: Category]
  remove: [category: Category]
  restore: [category: Category]
}>()

const categoriesById = computed(() => {
  const result = new Map<number, Category>()

  const collect = (nodes: readonly CategoryNode[]): void => {
    for (const node of nodes) {
      result.set(node.category.id, node.category)
      collect(node.children)
    }
  }

  collect(props.nodes)
  return result
})

const isNarrow = useNarrowViewport()

const expandedIds = reactive(new Set<number>())

const items = computed<TreeItem[]>(() => props.nodes.map(toItem))

function toItem(node: CategoryNode): TreeItem {
  const id = node.category.id

  const item: TreeItem = {
    category: node.category,
    disabled: node.category.isDeleted,
    text: node.category.name,
    value: id,

    get expanded(): boolean {
      return expandedIds.has(id)
    },

    set expanded(value: boolean) {
      if (value) {
        expandedIds.add(id)
      } else {
        expandedIds.delete(id)
      }
    },
  }

  if (node.children.length > 0) {
    item.children = node.children.map(toItem)
  }

  return item
}

function categoryOf(item: MudTreeItemData): Category {
  if (typeof item.value !== 'number') {
    throw new Error('Узел категории не содержит идентификатор')
  }

  const category = categoriesById.value.get(item.value)

  if (category === undefined) {
    throw new Error(`Категория не найдена: ${item.value}`)
  }

  return category
}

function dotColor(item: MudTreeItemData): string | null {
  const category = categoryOf(item)

  return category.isDeleted ? null : category.color
}
</script>

<template>
  <MudTreeView
    :items="items"
    data-testid="category-tree"
    expand-on-click
    hover
    read-only
    ripple
  >
    <template #itemTemplate="{ item }">
      <div class="category-tree-item d-flex align-center flex-grow-1">
        <MudTreeViewItemToggleButton
          v-model:expanded="item.expanded"
          :disabled="item.disabled"
          :visible="(item.children?.length ?? 0) > 0"
        />
        <span
          v-if="isNarrow && dotColor(item) !== null"
          :style="{ backgroundColor: dotColor(item) ?? undefined }"
          class="category-dot"
        />
        <MudText
          class="category-name"
          html-tag="span"
        >
          {{ categoryOf(item).name }}
        </MudText>
        <MudSpacer />
        <CategoryNodeActions
          :category="categoryOf(item)"
          :narrow="isNarrow"
          @create="emit('create', $event)"
          @edit="emit('edit', $event)"
          @remove="emit('remove', $event)"
          @restore="emit('restore', $event)"
        />
      </div>
    </template>
  </MudTreeView>
</template>

<style scoped>
@media (max-width: 599.98px) {
  .category-dot {
    flex: none;
    width: 10px;
    height: 10px;
    margin-inline-end: 0.5rem;
    border: 1px solid rgb(0 0 0 / 20%);
    border-radius: 50%;
  }

  .category-name {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
}
</style>
