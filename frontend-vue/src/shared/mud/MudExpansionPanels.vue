<script lang="ts">
import type { InjectionKey, Ref } from 'vue'

export interface MudExpansionPanelRegistration {
  uid: string
  expanded: Ref<boolean>
}

export interface MudExpansionPanelsContext {
  panels: Readonly<Ref<MudExpansionPanelRegistration[]>>
  multiExpansion: Readonly<Ref<boolean>>
  elevation: number
  dense: boolean
  gutters: boolean
  outlined: boolean
  registerPanel: (panel: MudExpansionPanelRegistration) => () => void
  notifyPanelChanged: (panel: MudExpansionPanelRegistration) => void
  togglePanel: (panel: MudExpansionPanelRegistration) => void
  isNextPanelExpanded: (panel: MudExpansionPanelRegistration) => boolean
}

export const mudExpansionPanelsContextKey: InjectionKey<MudExpansionPanelsContext> = Symbol('mud-expansion-panels')
</script>

<script setup lang="ts">
import { computed, provide, shallowRef } from 'vue'

const props = withDefaults(defineProps<{
  multiExpansion?: boolean
}>(), {
  multiExpansion: false,
})

const panels = shallowRef<MudExpansionPanelRegistration[]>([])
const multiExpansion = computed(() => props.multiExpansion)

const notifyPanelChanged = (panel: MudExpansionPanelRegistration) => {
  if (!multiExpansion.value && panel.expanded.value) {
    for (const otherPanel of panels.value) {
      if (otherPanel !== panel) {
        otherPanel.expanded.value = false
      }
    }
  }
}

const registerPanel = (panel: MudExpansionPanelRegistration) => {
  if (!multiExpansion.value && panels.value.some(item => item.expanded.value)) {
    panel.expanded.value = false
  }
  panels.value = [...panels.value, panel]

  return () => {
    panels.value = panels.value.filter(item => item !== panel)
  }
}

const togglePanel = (panel: MudExpansionPanelRegistration) => {
  panel.expanded.value = !panel.expanded.value
  notifyPanelChanged(panel)
}

const isNextPanelExpanded = (panel: MudExpansionPanelRegistration) => {
  const index = panels.value.indexOf(panel)
  return index >= 0 ? panels.value[index + 1]?.expanded.value === true : false
}

const context: MudExpansionPanelsContext = {
  panels,
  multiExpansion,
  elevation: 1,
  dense: false,
  gutters: true,
  outlined: true,
  registerPanel,
  notifyPanelChanged,
  togglePanel,
  isNextPanelExpanded,
}

provide(mudExpansionPanelsContextKey, context)
</script>

<template>
  <div class="mud-expansion-panels">
    <slot />
  </div>
</template>
