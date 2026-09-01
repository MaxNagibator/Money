<script lang="ts">
let nextPanelId = 0

const createPanelId = () => {
  nextPanelId += 1
  return `panel-${nextPanelId}`
}
</script>

<script setup lang="ts">
import { computed, inject, onBeforeUnmount, onMounted, reactive, watch, type InjectionKey } from 'vue'
import { mudTabsContextKey } from './MudTabs.vue'

interface MudTabsPanelRegistration {
  uid: string
  text?: string | undefined
  toolTip?: string | undefined
  tabTestId?: string | undefined
  visible: boolean
  disabled: boolean
}

interface MudTabsContext {
  registerPanel: (panel: MudTabsPanelRegistration) => () => void
  isActive: (panel: MudTabsPanelRegistration) => boolean
  getTabId: (panel: MudTabsPanelRegistration) => string
  getTabPanelId: (panel: MudTabsPanelRegistration) => string
}

const props = withDefaults(defineProps<{
  id?: string
  panelClass?: string
  showCloseIcon?: boolean
  text?: string
  toolTip?: string
  tabTestId?: string
}>(), {
  showCloseIcon: true,
})

const context = inject(mudTabsContextKey as InjectionKey<MudTabsContext>)
const panel = reactive<MudTabsPanelRegistration>({
  uid: createPanelId(),
  text: props.text,
  toolTip: props.toolTip,
  tabTestId: props.tabTestId,
  visible: true,
  disabled: false,
})
let unregister: (() => void) | undefined

watch(() => props.text, value => {
  panel.text = value
})

watch(() => props.toolTip, value => {
  panel.toolTip = value
})

watch(() => props.tabTestId, value => {
  panel.tabTestId = value
})

onMounted(() => {
  if (context) {
    unregister = context.registerPanel(panel)
  }
})

onBeforeUnmount(() => unregister?.())

const active = computed(() => context?.isActive(panel) ?? false)
const panelClasses = computed(() => [
  'mud-tab-panel',
  {
    'mud-tab-panel-hidden': !panel.visible,
  },
  props.panelClass,
])
const panelId = computed(() => context?.getTabPanelId(panel))
const tabId = computed(() => context?.getTabId(panel))
</script>

<template>
  <div
    v-if="active"
    :id="panelId"
    :class="panelClasses"
    role="tabpanel"
    :aria-labelledby="tabId"
  >
    <slot />
  </div>
</template>
