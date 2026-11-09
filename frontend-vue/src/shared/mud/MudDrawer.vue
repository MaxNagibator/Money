<script setup lang="ts">
import type { MudBreakpoint } from './types'
import { computed, inject, onMounted, onUnmounted, reactive, ref, watch, type Ref } from 'vue'
import MudOverlay from './MudOverlay.vue'


interface DrawerState {
  open: boolean
  breakpoint: MudBreakpoint
  variant: 'responsive'
  position: 'left'
  clipMode: 'never'
}

interface LayoutContext {
  drawer: Ref<DrawerState | undefined>
}

const props = withDefaults(defineProps<{
  elevation?: number
  breakpoint?: MudBreakpoint
}>(), {
  elevation: 1,
  breakpoint: 'md',
})

const open = defineModel<boolean>('open', { default: false })
const initial = ref(true)
const mounted = ref(false)
const preserveInitial = ref(false)
const belowBreakpoint = ref(false)
const mediaQuery = ref<MediaQueryList>()
const layoutContext = inject<LayoutContext | undefined>('mud-layout-context')
const drawerState = reactive<DrawerState>({
  open: open.value,
  breakpoint: props.breakpoint,
  variant: 'responsive',
  position: 'left',
  clipMode: 'never',
})

const syncLayout = () => {
  drawerState.open = open.value
  drawerState.breakpoint = props.breakpoint
}

const classes = computed(() => [
  'mud-drawer',
  {
    'mud-drawer-fixed': layoutContext !== undefined,
  },
  'mud-drawer-pos-left',
  {
    'mud-drawer--open': open.value,
    'mud-drawer--closed': !open.value,
    'mud-drawer--initial': initial.value,
  },
  `mud-drawer-${props.breakpoint}`,
  'mud-drawer-clipped-never',
  `mud-elevation-${props.elevation}`,
  'mud-drawer-responsive',
])

const overlayVisible = computed(() => open.value && belowBreakpoint.value)

const closeDrawer = () => {
  open.value = false
}

const updateViewport = () => {
  const currentMediaQuery = mediaQuery.value
  if (!currentMediaQuery) {
    return
  }

  const isBelow = !currentMediaQuery.matches
  const wasBelow = belowBreakpoint.value

  if (isBelow && !wasBelow) {
    open.value = false
  } else if (!isBelow && wasBelow) {
    open.value = true
  }

  belowBreakpoint.value = isBelow
}

const updateViewportInitially = () => {
  const currentMediaQuery = mediaQuery.value
  if (!currentMediaQuery) {
    return
  }

  belowBreakpoint.value = !currentMediaQuery.matches
  if (belowBreakpoint.value && open.value) {
    preserveInitial.value = true
    open.value = false
  }
}

const classesForOverlay = computed(() => [
  'mud-drawer-overlay',
  'mud-overlay-drawer',
  'mud-drawer-pos-left',
  'mud-drawer-overlay--open',
  'mud-drawer-overlay-responsive',
  `mud-drawer-overlay-${props.breakpoint}`,
  {
    'mud-drawer-overlay--initial': initial.value,
  },
  'mud-skip-overlay-positioning',
  'mud-skip-overlay-section',
])

watch([open, () => props.breakpoint], () => {
  if (mounted.value && !preserveInitial.value) {
    initial.value = false
  }
  preserveInitial.value = false
  syncLayout()
}, { immediate: true })

if (layoutContext) {
  layoutContext.drawer.value = drawerState
}

onMounted(() => {
  if (typeof window.matchMedia !== 'function') {
    mounted.value = true
    return
  }

  mediaQuery.value = window.matchMedia('(min-width: 960px)')
  updateViewportInitially()
  mediaQuery.value.addEventListener('change', updateViewport)
  mounted.value = true
})

onUnmounted(() => {
  mediaQuery.value?.removeEventListener('change', updateViewport)
  if (layoutContext?.drawer.value === drawerState) {
    layoutContext.drawer.value = undefined
  }
})
</script>

<template>
  <aside
    v-bind="$attrs"
    :class="classes"
  >
    <div class="mud-drawer-content">
      <slot />
    </div>
  </aside>
  <MudOverlay
    :visible="overlayVisible"
    :class="classesForOverlay"
    auto-close
    dark-background
    @closed="closeDrawer"
  />
</template>
