<script setup lang="ts">
import { computed, inject } from 'vue'
import { mdCheckBox, mdCheckBoxOutlineBlank, mdIndeterminateCheckBox } from '@/shared/icons'
import MudCollapse from './MudCollapse.vue'
import MudIcon from './MudIcon.vue'
import MudText from './MudText.vue'
import MudTreeViewItemToggleButton from './MudTreeViewItemToggleButton.vue'
import type { MudColor, MudTreeItemData } from './types'

interface MudTreeViewContext {
  selectionMode: () => string
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
  value?: unknown
  text?: string | undefined
  visible?: boolean
  disabled?: boolean
  items?: readonly MudTreeItemData[] | undefined
  icon?: string | undefined
  endIcon?: string | undefined
  endIconColor?: MudColor
}>(), {
  visible: true,
  endIconColor: 'default',
})

const expanded = defineModel<boolean>('expanded', { default: false })

defineSlots<{
  content?: (props: { item: MudTreeItemData }) => unknown
}>()
const treeContext = inject<MudTreeViewContext | undefined>('mud-tree-view-context')
const checkboxId = `mud-treeview-checkbox-${Math.random().toString(36).slice(2)}`

const slotItem = computed(() => ({
  value: props.value,
  text: props.text,
  icon: props.icon,
  endIcon: props.endIcon,
  endIconColor: props.endIconColor,
  get expanded() {
    return expanded.value
  },
  set expanded(value: boolean) {
    expanded.value = value
  },
  visible: props.visible,
  disabled: props.disabled,
  children: props.items,
}))

const childItems = computed(() => props.items ?? [])
const hasChildren = computed(() => childItems.value.length > 0)
const areChildrenVisible = computed(() => childItems.value.some(item => item.visible !== false))
const disabled = computed(() => props.disabled === true || treeContext?.disabled() === true)
const readOnly = computed(() => treeContext?.readOnly() === true)
const expandOnClick = computed(() => treeContext?.expandOnClick() === true)
const ripple = computed(() => treeContext?.ripple() === true)
const multiSelection = computed(() => treeContext?.selectionMode() === 'multi-selection')
const selected = computed(() => treeContext?.isSelected(props.value) === true)
const canInteract = computed(() => !disabled.value && (!readOnly.value || (expandOnClick.value && hasChildren.value)))

const currentIcon = computed(() => props.icon)
const hasIcon = computed(() => currentIcon.value !== undefined && currentIcon.value.length > 0)
const text = computed(() => {
  if (props.text) {
    return props.text
  }

  if (typeof props.value === 'string' || typeof props.value === 'number') {
    return String(props.value)
  }

  return ''
})

const contentClasses = computed(() => [
  'mud-treeview-item-content',
  {
    'cursor-pointer': canInteract.value,
    'mud-ripple': ripple.value && canInteract.value,
    'mud-treeview-item-selected': !disabled.value && !multiSelection.value && selected.value,
  },
])

const classes = computed(() => [
  'mud-treeview-item',
  {
    'mud-treeview-item-disabled': disabled.value,
  },
])

const descendantValues = computed(() => {
  const values: unknown[] = []

  const collect = (items: readonly MudTreeItemData[]): void => {
    for (const item of items) {
      if (item.value !== undefined && item.value !== null) {
        values.push(item.value)
      }

      if (item.children) {
        collect(item.children)
      }
    }
  }

  collect(childItems.value)
  return values
})

const checkboxState = computed(() => treeContext?.getCheckboxState(props.value, descendantValues.value) ?? false)
const checkboxIcon = computed(() => {
  if (checkboxState.value === null && treeContext?.triState() === true) {
    return mdIndeterminateCheckBox
  }

  return checkboxState.value ? mdCheckBox : mdCheckBoxOutlineBlank
})

const checkboxClasses = computed(() => [
  'mud-button-root mud-icon-button',
  'mud-ripple mud-ripple-checkbox',
  `mud-checkbox-${checkboxState.value === null ? 'null' : checkboxState.value ? 'true' : 'false'}`,
  {
    'mud-disabled': disabled.value,
    'mud-readonly': readOnly.value,
  },
])

const groupClasses = computed(() => [
  'mud-treeview',
  `mud-treeview-selected-${treeContext?.color() ?? 'primary'}`,
  `mud-treeview-checked-${treeContext?.checkBoxColor() ?? 'default'}`,
  'mud-treeview-group',
])

const itemKey = (item: MudTreeItemData, index: number): string | number =>
  typeof item.value === 'string' || typeof item.value === 'number' ? item.value : index

const handleClick = (event: MouseEvent | KeyboardEvent): void => {
  if (
    event.target instanceof Element
    && event.target !== event.currentTarget
    && event.target.closest('button, a, input, [role="button"]') !== event.currentTarget
  ) {
    return
  }

  if (event.type === 'keydown') {
    event.preventDefault()
  }

  if (hasChildren.value && expandOnClick.value) {
    expanded.value = !expanded.value
  }

  if (!disabled.value && !readOnly.value) {
    treeContext?.toggleSelection(props.value, descendantValues.value)
  }
}

const handleCheckboxChange = (): void => {
  if (!disabled.value && !readOnly.value) {
    treeContext?.toggleSelection(props.value, descendantValues.value)
  }
}
</script>

<template>
  <li
    v-if="visible"
    :class="classes"
  >
    <div
      :class="contentClasses"
      role="button"
      tabindex="0"
      @click.stop="handleClick"
      @keydown.enter.stop="handleClick"
      @keydown.space.stop="handleClick"
    >
      <template v-if="$slots.content">
        <slot
          name="content"
          :item="slotItem"
        />
      </template>
      <template v-else>
        <MudTreeViewItemToggleButton
          v-model:expanded="expanded"
          :disabled="disabled"
          :visible="hasChildren && areChildrenVisible"
        />

        <div
          v-if="multiSelection"
          class="mud-input-control mud-input-control-boolean-input mud-treeview-item-checkbox"
        >
          <div class="mud-input-control-input-container">
            <label
              :for="checkboxId"
              class="mud-checkbox mud-input-content-placement-end"
              @click.stop
            >
              <span :class="checkboxClasses">
                <input
                  :id="checkboxId"
                  type="checkbox"
                  class="mud-checkbox-input"
                  :checked="checkboxState === true"
                  :disabled="disabled || readOnly"
                  @change.stop="handleCheckboxChange"
                >
                <MudIcon
                  :icon="checkboxIcon"
                  color="inherit"
                />
              </span>
            </label>
          </div>
        </div>

        <div
          v-if="hasIcon"
          class="mud-treeview-item-icon"
        >
          <MudIcon
            :disabled="disabled"
            :icon="currentIcon ?? ''"
          />
        </div>

        <MudText
          class="mud-treeview-item-label"
        >
          {{ text }}
        </MudText>

        <div
          v-if="endIcon"
          class="mud-treeview-item-icon"
        >
          <MudIcon
            :color="endIconColor"
            :disabled="disabled"
            :icon="endIcon"
          />
        </div>
      </template>
    </div>

    <MudCollapse
      v-if="hasChildren"
      :expanded="expanded"
    >
      <ul :class="groupClasses">
        <MudTreeViewItem
          v-for="(item, index) in childItems"
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
            v-if="$slots.content"
            #content="{ item: childItem }"
          >
            <slot
              name="content"
              :item="childItem"
            />
          </template>
        </MudTreeViewItem>
      </ul>
    </MudCollapse>
  </li>
</template>
