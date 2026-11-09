<script setup lang="ts">
import { computed, provide, ref, type Ref } from 'vue'

interface DrawerState {
  open: boolean
  breakpoint: 'md'
  variant: 'responsive'
  position: 'left'
  clipMode: 'never'
}

interface LayoutContext {
  drawer: Ref<DrawerState | undefined>
}

const drawer = ref<DrawerState>()

provide<LayoutContext>('mud-layout-context', { drawer })

const classes = computed(() => {
  const state = drawer.value

  return [
    'mud-layout',
    state ? `mud-drawer-${state.open ? 'open' : 'close'}-${state.variant}-${state.breakpoint}-${state.position}` : '',
    state ? `mud-drawer-${state.position}-clipped-${state.clipMode}` : '',
  ]
})
</script>

<template>
  <div :class="classes">
    <slot />
  </div>
</template>
