<script setup lang="ts">
import { computed, reactive, watch } from 'vue'
import { formatMoney } from '@/platform/format'
import { mdCurrencyRuble } from '@/shared/icons'
import {
  MudChip,
  MudIcon,
  MudSpacer,
  MudStack,
  MudText,
  MudTreeView,
  MudTreeViewItemToggleButton,
} from '@/shared/mud'
import type { MudTreeItemData } from '@/shared/mud'
import type { SumTreeNode } from '../model/category-sums'

interface TreeItem extends MudTreeItemData {
  value: string
  text: string
  children?: TreeItem[]
}

const props = defineProps<{
  nodes: readonly SumTreeNode[]
  testId: string
}>()

const opened = defineModel<string[]>('opened', { required: true })

const amountsByKey = computed(() => {
  const amounts = new Map<string, string>()

  const collect = (nodes: readonly SumTreeNode[]): void => {
    for (const node of nodes) {
      amounts.set(node.key, formatMoney(node.total))
      collect(node.children)
    }
  }

  collect(props.nodes)
  return amounts
})

const items = computed(() => reactive(props.nodes.map(toItem)))

function toItem(node: SumTreeNode): TreeItem {
  const item: TreeItem = {
    expanded: opened.value.includes(node.key),
    text: node.name,
    value: node.key,
  }

  if (node.children.length > 0) {
    item.children = node.children.map(toItem)
  }

  return item
}

function amountOf(item: MudTreeItemData): string {
  return typeof item.value === 'string' ? amountsByKey.value.get(item.value) ?? '' : ''
}

function collectOpened(nodes: readonly TreeItem[], result: string[] = []): string[] {
  for (const node of nodes) {
    if (node.expanded && node.children !== undefined && node.children.length > 0) {
      result.push(node.value)
    }

    if (node.children !== undefined) {
      collectOpened(node.children, result)
    }
  }

  return result
}

const openedKeys = computed(() => collectOpened(items.value))

watch(openedKeys, next => {
  const current = new Set(opened.value)

  if (next.length === current.size && next.every(key => current.has(key))) {
    return
  }

  opened.value = next
})
</script>

<template>
  <MudTreeView
    :items="items"
    :data-testid="testId"
    dense
    expand-on-click
    hover
    read-only
    ripple
  >
    <template #itemTemplate="{ item }">
      <MudTreeViewItemToggleButton
        v-model:expanded="item.expanded"
        :visible="(item.children?.length ?? 0) > 0"
      />
      <MudText>
        {{ item.text }}
      </MudText>
      <MudSpacer />
      <MudChip>
        <MudStack
          align-items="center"
          :spacing="0"
          row
        >
          {{ amountOf(item) }}
          <MudIcon
            :icon="mdCurrencyRuble"
            size="small"
            style="width: 14px;"
          />
        </MudStack>
      </MudChip>
    </template>
  </MudTreeView>
</template>
