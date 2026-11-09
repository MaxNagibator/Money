<script setup lang="ts">
import { computed, onBeforeUnmount, ref, useTemplateRef, watch } from 'vue'
import { mdDelete, mdSearch } from '@/shared/icons'
import type { MudTreeItemData } from '@/shared/mud'
import { MudIconButton, MudPopover, MudStack, MudTextField, MudTreeView } from '@/shared/mud'

const POPOVER_MAX_HEIGHT = 400

const props = defineProps<{
  items: readonly MudTreeItemData[]
  helperText: string
  testId: string
  selectionMode: 'single-selection' | 'multi-selection'
  selectedValue?: unknown
  selectedValues?: readonly unknown[] | null
}>()

const emit = defineEmits<{
  'update:selectedValue': [value: unknown]
  'update:selectedValues': [values: readonly unknown[]]
}>()

const search = ref('')
const isOpen = ref(false)
const rootElement = useTemplateRef<HTMLElement>('rootElement')
const tree = ref<MudTreeItemData[]>([])

const selection = computed<readonly unknown[] | null>(() => props.selectedValues ?? null)

let source: readonly MudTreeItemData[] | null = null

function cloneItems(items: readonly MudTreeItemData[]): MudTreeItemData[] {
  return items.map(item => ({
    ...item,
    children: item.children === undefined ? undefined : cloneItems(item.children),
  }))
}

function applyFilter(items: readonly MudTreeItemData[], normalized: string): void {
  for (const item of items) {
    applyFilter(item.children ?? [], normalized)
    item.visible = normalized.length === 0 || isVisible(item, normalized)
  }
}

function isVisible(item: MudTreeItemData, normalized: string): boolean {
  if (matches(item, normalized)) {
    return true
  }

  return (item.children ?? []).some(child => matches(child, normalized))
}

function matches(item: MudTreeItemData, normalized: string): boolean {
  return (item.text ?? '').toLocaleLowerCase().includes(normalized)
}

function toggle(open?: boolean): void {
  isOpen.value = open ?? !isOpen.value
}

function reset(): void {
  if (props.selectionMode === 'single-selection') {
    emit('update:selectedValue', null)
  }
  else {
    emit('update:selectedValues', [])
  }

  isOpen.value = false
}

function handleOutsidePointerdown(event: PointerEvent): void {
  if (!isOpen.value || !(event.target instanceof Node) || rootElement.value?.contains(event.target) === true) {
    return
  }

  isOpen.value = false
}

watch([() => props.items, search], ([items, query]) => {
  if (items !== source) {
    source = items
    tree.value = cloneItems(items)
  }

  applyFilter(tree.value, query.trim().toLocaleLowerCase())
}, { immediate: true })

watch(isOpen, open => {
  if (open) {
    document.addEventListener('pointerdown', handleOutsidePointerdown)
    return
  }

  document.removeEventListener('pointerdown', handleOutsidePointerdown)
})

onBeforeUnmount(() => document.removeEventListener('pointerdown', handleOutsidePointerdown))
</script>

<template>
  <div
    ref="rootElement"
    class="d-flex flex-column"
  >
    <MudStack
      align-items="center"
      justify="start"
      row
    >
      <MudTextField
        v-model="search"
        :adornment-icon="mdSearch"
        :adornment-test-id="`${testId}-toggle`"
        :data-testid="`${testId}-search`"
        :helper-text="helperText"
        :on-adornment-click="() => toggle()"
        adornment="start"
        adornment-aria-label="Показать дерево категорий"
        adornment-color="tertiary"
        clearable
        immediate
        label="Категории"
        @click="toggle(true)"
      />

      <MudIconButton
        :data-testid="`${testId}-reset`"
        :icon="mdDelete"
        aria-label="Очистить выбранные категории"
        class="trash-btn"
        color="tertiary"
        size="small"
        @click="reset"
      />
    </MudStack>

    <MudPopover
      :data-testid="`${testId}-popover`"
      :max-height="POPOVER_MAX_HEIGHT"
      :open="isOpen"
      anchor-origin="bottom-left"
      relative-width="relative"
      transform-origin="top-left"
      @pointerdown.stop
    >
      <MudTreeView
        :data-testid="`${testId}-tree`"
        :items="tree"
        :selection-mode="selectionMode"
        :selected-value="selectedValue"
        :selected-values="selection"
        auto-select-parent
        expand-on-click
        hover
        max-height="400px"
        ripple
        tri-state
        @update:selected-value="emit('update:selectedValue', $event)"
        @update:selected-values="emit('update:selectedValues', $event ?? [])"
      />
    </MudPopover>
  </div>
</template>
