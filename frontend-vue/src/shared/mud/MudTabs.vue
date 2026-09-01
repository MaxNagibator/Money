<script lang="ts">
import type { MudTabHeaderPosition } from './types'
import type { InjectionKey, Ref } from 'vue'


export interface MudTabsPanelRegistration {
  uid: string
  text?: string | undefined
  toolTip?: string | undefined
  tabTestId?: string | undefined
  visible: boolean
  disabled: boolean
}

export interface MudTabsContext {
  panels: Readonly<Ref<MudTabsPanelRegistration[]>>
  registerPanel: (panel: MudTabsPanelRegistration) => () => void
  isActive: (panel: MudTabsPanelRegistration) => boolean
  activatePanel: (panel: MudTabsPanelRegistration) => void
  getTabId: (panel: MudTabsPanelRegistration) => string
  getTabPanelId: (panel: MudTabsPanelRegistration) => string
}

export const mudTabsContextKey: InjectionKey<MudTabsContext> = Symbol('mud-tabs')

let nextTabsId = 0

const createTabsId = () => {
  nextTabsId += 1
  return `tablist-${nextTabsId}`
}
</script>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, provide, ref, useTemplateRef, watch, type ComponentPublicInstance } from 'vue'
import { mdChevronLeft, mdChevronRight } from '@/shared/icons'
import MudIconButton from './MudIconButton.vue'
import MudTooltip from './MudTooltip.vue'

const props = withDefaults(defineProps<{
  elevation?: number
  applyEffectsToContainer?: boolean
  rounded?: boolean
  headerPosition?: MudTabHeaderPosition
}>(), {
  elevation: 0,
  applyEffectsToContainer: false,
  rounded: false,
  headerPosition: 'after',
})

const activePanelIndex = defineModel<number>('activePanelIndex', { default: 0 })

const panels = ref<MudTabsPanelRegistration[]>([])
const tabsInner = useTemplateRef<HTMLElement>('tabsInner')
const tabsContent = useTemplateRef<HTMLElement>('tabsContent')
const tabElements = new Map<string, HTMLElement>()
const sliderPosition = ref(0)
const sliderSize = ref(0)
const sliderReady = ref(false)
const totalTabsWidth = ref(0)
const tabBarContentWidth = ref(0)
const scrollPosition = ref(0)
const showScrollButtons = ref(false)
const tabsId = createTabsId()
let resizeObserver: ResizeObserver | undefined
let normalizeScheduled = false

const tabsClasses = computed(() => [
  'mud-tabs',
  {
    'mud-tabs-rounded': props.applyEffectsToContainer && props.rounded,
    [`mud-elevation-${props.elevation}`]: props.applyEffectsToContainer && props.elevation !== 0,
  },
])

const tabBarClasses = computed(() => [
  'mud-tabs-tabbar',
  {
    'mud-tabs-rounded': !props.applyEffectsToContainer && props.rounded,
    [`mud-elevation-${props.elevation}`]: !props.applyEffectsToContainer && props.elevation !== 0,
  },
])

const wrapperClasses = ['mud-tabs-tabbar-wrapper']
const panelsClasses = ['mud-tabs-panels']
const sliderClasses = ['mud-tab-slider', 'mud-tab-slider-horizontal']

const sliderStyle = computed(() => ({
  width: `${sliderSize.value}%`,
  left: `${sliderPosition.value}%`,
  transition: 'left .3s cubic-bezier(.64,.09,.08,1)',
}))

const wrapperStyle = computed(() => ({
  transform: `translateX(${-scrollPosition.value}px)`,
}))

const previousButtonDisabled = computed(() => Math.abs(scrollPosition.value) < 0.01)
const nextButtonDisabled = computed(() =>
  Math.abs(scrollPosition.value) >= Math.max(0, totalTabsWidth.value - tabBarContentWidth.value) - 0.01,
)

const setTabRef = (panel: MudTabsPanelRegistration, element: Element | ComponentPublicInstance | null) => {
  if (element instanceof HTMLElement) {
    tabElements.set(panel.uid, element)
  } else {
    tabElements.delete(panel.uid)
  }
}

const isSelectable = (panel: MudTabsPanelRegistration | undefined) => panel !== undefined && panel.visible && !panel.disabled

const findNearestSelectableIndex = (startIndex: number) => {
  if (panels.value.length === 0) {
    return -1
  }

  const clampedIndex = Math.min(Math.max(startIndex, 0), panels.value.length - 1)
  if (isSelectable(panels.value[clampedIndex])) {
    return clampedIndex
  }

  for (let index = clampedIndex; index >= 0; index -= 1) {
    if (isSelectable(panels.value[index])) {
      return index
    }
  }

  for (let index = clampedIndex + 1; index < panels.value.length; index += 1) {
    if (isSelectable(panels.value[index])) {
      return index
    }
  }

  return -1
}

const normalizeActivePanelIndex = () => {
  const nextIndex = findNearestSelectableIndex(activePanelIndex.value)
  if (activePanelIndex.value !== nextIndex) {
    activePanelIndex.value = nextIndex
  }
}

const scheduleNormalize = () => {
  if (normalizeScheduled) {
    return
  }

  normalizeScheduled = true
  void nextTick(() => {
    normalizeScheduled = false
    normalizeActivePanelIndex()
  })
}

const registerPanel = (panel: MudTabsPanelRegistration) => {
  panels.value = [...panels.value, panel]
  scheduleNormalize()

  return () => {
    const activePanel = panels.value[activePanelIndex.value]
    panels.value = panels.value.filter(item => item.uid !== panel.uid)
    tabElements.delete(panel.uid)

    if (activePanel && activePanel.uid !== panel.uid) {
      const nextIndex = panels.value.findIndex(item => item.uid === activePanel.uid)
      if (nextIndex >= 0 && activePanelIndex.value !== nextIndex) {
        activePanelIndex.value = nextIndex
      }
    } else {
      scheduleNormalize()
    }
  }
}

const isActive = (panel: MudTabsPanelRegistration) => panels.value[activePanelIndex.value]?.uid === panel.uid

const activatePanel = (panel: MudTabsPanelRegistration) => {
  if (!isSelectable(panel)) {
    return
  }

  const index = panels.value.findIndex(item => item.uid === panel.uid)
  if (index >= 0) {
    activePanelIndex.value = index
  }
}

const getTabId = (panel: MudTabsPanelRegistration) => `${tabsId}-tab-${panel.uid}`
const getTabPanelId = (panel: MudTabsPanelRegistration) => `${tabsId}-tabpanel-${panel.uid}`

const context: MudTabsContext = {
  panels,
  registerPanel,
  isActive,
  activatePanel,
  getTabId,
  getTabPanelId,
}

provide(mudTabsContextKey, context)

const measureTabs = async () => {
  await nextTick()

  const measurements = panels.value
    .filter(panel => panel.visible)
    .map(panel => ({ panel, element: tabElements.get(panel.uid) }))
    .filter((item): item is { panel: MudTabsPanelRegistration; element: HTMLElement } => item.element !== undefined)
    .map(item => ({ ...item, width: item.element.getBoundingClientRect().width }))
  const totalWidth = measurements.reduce((total, item) => total + item.width, 0)
  let contentWidth = tabsContent.value?.getBoundingClientRect().width ?? 0
  const shouldShowScrollButtons = totalWidth > contentWidth
  if (showScrollButtons.value !== shouldShowScrollButtons) {
    showScrollButtons.value = shouldShowScrollButtons
    await nextTick()
    contentWidth = tabsContent.value?.getBoundingClientRect().width ?? contentWidth
  }
  totalTabsWidth.value = totalWidth
  tabBarContentWidth.value = contentWidth
  const activePanel = panels.value[activePanelIndex.value]
  const activeMeasurement = measurements.find(item => item.panel.uid === activePanel?.uid)

  if (!activeMeasurement || totalWidth <= 0) {
    sliderReady.value = false
    return
  }

  if (!showScrollButtons.value) {
    scrollPosition.value = 0
  } else {
    const maxScroll = Math.max(0, totalWidth - contentWidth)
    let activePrecedingWidth = 0
    for (const item of measurements) {
      if (item.panel.uid === activePanel?.uid) {
        break
      }
      activePrecedingWidth += item.width
    }
    const targetPosition = activePrecedingWidth - contentWidth / 2 + activeMeasurement.width / 2
    scrollPosition.value = Math.min(Math.max(targetPosition, 0), maxScroll)
  }

  let precedingWidth = 0
  for (const item of measurements) {
    if (item.panel.uid === activePanel?.uid) {
      break
    }
    precedingWidth += item.width
  }

  sliderPosition.value = precedingWidth / totalWidth * 100
  sliderSize.value = activeMeasurement.width / totalWidth * 100
  sliderReady.value = true
  for (const panel of panels.value) {
    tabElements.get(panel.uid)?.setAttribute('tabindex', isActive(panel) ? '0' : '-1')
  }
}

const observeTabs = () => {
  resizeObserver?.disconnect()
  if (typeof ResizeObserver === 'undefined') {
    return
  }

  resizeObserver = new ResizeObserver(() => {
    void measureTabs()
  })
  if (tabsInner.value) {
    resizeObserver.observe(tabsInner.value)
  }
  if (tabsContent.value) {
    resizeObserver.observe(tabsContent.value)
  }
  for (const element of tabElements.values()) {
    resizeObserver.observe(element)
  }
}

const scrollBy = (direction: -1 | 1) => {
  const maxScroll = Math.max(0, totalTabsWidth.value - tabBarContentWidth.value)
  const activePanel = panels.value[activePanelIndex.value]
  const activeElement = activePanel ? tabElements.get(activePanel.uid) : undefined
  const step = Math.max(tabBarContentWidth.value, activeElement?.getBoundingClientRect().width ?? 0)
  scrollPosition.value = Math.min(Math.max(scrollPosition.value + direction * step, 0), maxScroll)
}

const refreshLayout = async () => {
  await measureTabs()
  observeTabs()
}

const moveFocus = (panel: MudTabsPanelRegistration, direction: -1 | 1) => {
  const enabledPanels = panels.value.filter(isSelectable)
  if (enabledPanels.length <= 1) {
    return
  }

  const currentIndex = enabledPanels.findIndex(item => item.uid === panel.uid)
  const nextIndex = currentIndex < 0
    ? 0
    : (currentIndex + direction + enabledPanels.length) % enabledPanels.length
  const nextPanel = enabledPanels[nextIndex]
  if (nextPanel) {
    tabElements.get(nextPanel.uid)?.focus()
  }
}

const handleTabKeydown = (event: KeyboardEvent, panel: MudTabsPanelRegistration) => {
  if (event.key === 'Enter' || event.key === ' ') {
    event.preventDefault()
    activatePanel(panel)
  } else if (event.key === 'ArrowLeft') {
    moveFocus(panel, -1)
  } else if (event.key === 'ArrowRight') {
    moveFocus(panel, 1)
  }
}

watch([activePanelIndex, () => panels.value.length], () => {
  scheduleNormalize()
  void refreshLayout()
}, { flush: 'post' })

onMounted(() => {
  void refreshLayout()
})

onBeforeUnmount(() => resizeObserver?.disconnect())
</script>

<template>
  <div :class="tabsClasses">
    <div :class="tabBarClasses">
      <div
        ref="tabsInner"
        class="mud-tabs-tabbar-inner"
      >
        <div
          v-if="headerPosition === 'before' && $slots.header"
          class="mud-tabs-header mud-tabs-header-before"
        >
          <slot name="header" />
        </div>
        <div
          v-if="showScrollButtons"
          class="mud-tabs-scroll-button"
        >
          <MudIconButton
            :icon="mdChevronLeft"
            color="inherit"
            :disabled="previousButtonDisabled"
            aria-label="Предыдущие вкладки"
            @click="scrollBy(-1)"
          />
        </div>
        <div
          ref="tabsContent"
          class="mud-tabs-tabbar-content"
        >
          <div
            :class="wrapperClasses"
            :style="wrapperStyle"
            role="tablist"
          >
            <template
              v-for="panel in panels"
              :key="panel.uid"
            >
              <MudTooltip
                v-if="panel.visible"
                :text="panel.toolTip"
              >
                <div
                  :id="getTabId(panel)"
                  :ref="element => setTabRef(panel, element)"
                  :class="[
                    'mud-tab',
                    {
                      'mud-tab-active': isActive(panel),
                      'mud-disabled': panel.disabled,
                      'mud-ripple': true,
                    },
                  ]"
                  :style="{ 'min-width': '160px' }"
                  :aria-controls="getTabPanelId(panel)"
                  :data-testid="panel.tabTestId"
                  role="tab"
                  :aria-selected="isActive(panel) ? 'true' : 'false'"
                  tabindex="0"
                  @click="activatePanel(panel)"
                  @keydown="handleTabKeydown($event, panel)"
                >
                  <template v-if="panel.text !== undefined">
                    {{ panel.text }}
                  </template>
                  <div
                    v-if="$slots.tabPanelHeader"
                    class="mud-tabs-panel-header mud-tabs-panel-header-after"
                  >
                    <slot
                      name="tabPanelHeader"
                      :panel="panel"
                    />
                  </div>
                </div>
              </MudTooltip>
            </template>
            <div
              v-if="sliderReady"
              :class="sliderClasses"
              :style="sliderStyle"
            />
          </div>
        </div>
        <div
          v-if="showScrollButtons"
          class="mud-tabs-scroll-button"
        >
          <MudIconButton
            :icon="mdChevronRight"
            color="inherit"
            :disabled="nextButtonDisabled"
            aria-label="Следующие вкладки"
            @click="scrollBy(1)"
          />
        </div>
        <div
          v-if="headerPosition === 'after' && $slots.header"
          class="mud-tabs-header mud-tabs-header-after"
        >
          <slot name="header" />
        </div>
      </div>
    </div>
    <div :class="panelsClasses">
      <slot />
    </div>
  </div>
</template>
