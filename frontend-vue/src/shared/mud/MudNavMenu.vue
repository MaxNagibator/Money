<script setup lang="ts">
import { computed, inject, type Ref } from 'vue'

interface NavigationContext {
  disabled: boolean
  expanded: boolean
  menuId: string
}

const navigationContext = inject<Ref<NavigationContext> | undefined>('mud-nav-context', undefined)

const menuId = computed(() => navigationContext?.value.menuId)

const classes = [
  'mud-navmenu',
  'mud-navmenu-default',
  'mud-navmenu-margin-none',
]
</script>

<template>
  <nav
    :id="menuId"
    :class="classes"
  >
    <slot />
  </nav>
</template>
