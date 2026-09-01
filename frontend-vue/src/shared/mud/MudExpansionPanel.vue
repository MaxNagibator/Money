<script lang="ts">
let nextPanelId = 0

const createPanelId = () => {
  nextPanelId += 1
  return `expansion-panel-${nextPanelId}`
}
</script>

<script setup lang="ts">
import { computed, inject, onBeforeUnmount, onMounted, watch, type InjectionKey, type Ref } from 'vue'
import { mdExpandMore } from '@/shared/icons'
import MudCollapse from './MudCollapse.vue'
import MudIcon from './MudIcon.vue'
import { mudExpansionPanelsContextKey } from './MudExpansionPanels.vue'

interface MudExpansionPanelRegistration {
  uid: string
  expanded: Ref<boolean>
}

interface MudExpansionPanelsContext {
  registerPanel: (panel: MudExpansionPanelRegistration) => () => void
  notifyPanelChanged: (panel: MudExpansionPanelRegistration) => void
  togglePanel: (panel: MudExpansionPanelRegistration) => void
  isNextPanelExpanded: (panel: MudExpansionPanelRegistration) => boolean
}

defineProps<{
  text?: string
}>()

const expanded = defineModel<boolean>('expanded', { default: false })
const context = inject(mudExpansionPanelsContextKey as InjectionKey<MudExpansionPanelsContext>)
const panel: MudExpansionPanelRegistration = {
  uid: createPanelId(),
  expanded,
}
let unregister: (() => void) | undefined

const stopExpandedWatch = context
  ? watch(expanded, () => context.notifyPanelChanged(panel))
  : undefined

onMounted(() => {
  if (context) {
    unregister = context.registerPanel(panel)
  }
})

onBeforeUnmount(() => {
  stopExpandedWatch?.()
  unregister?.()
})

const classes = computed(() => [
  'mud-expand-panel',
  'mud-elevation-1',
  'mud-expand-panel-border',
  {
    'mud-panel-expanded': expanded.value,
    'mud-panel-next-expanded': context?.isNextPanelExpanded(panel) ?? false,
  },
])

const iconClasses = computed(() => [
  'mud-expand-panel-icon',
  {
    'mud-transform': expanded.value,
  },
])

const toggle = () => {
  if (context) {
    context.togglePanel(panel)
  } else {
    expanded.value = !expanded.value
  }
}
</script>

<template>
  <div :class="classes">
    <div
      class="mud-expand-panel-header mud-expand-panel-header-gutters"
      role="button"
      tabindex="0"
      @click="toggle"
      @keydown.enter="toggle"
      @keydown.space.prevent="toggle"
    >
      <div class="mud-expand-panel-text">
        {{ text }}
      </div>
      <MudIcon
        :icon="mdExpandMore"
        :class="iconClasses"
      />
    </div>
    <MudCollapse :expanded="expanded">
      <div class="mud-expand-panel-content mud-expand-panel-gutters">
        <slot />
      </div>
    </MudCollapse>
  </div>
</template>
