<script setup lang="ts">
import { computed, inject, provide, type Ref } from 'vue'
import { mdArrowDropDown } from '@/shared/icons'
import MudCollapse from './MudCollapse.vue'
import MudIcon from './MudIcon.vue'
import MudNavMenu from './MudNavMenu.vue'

interface NavigationContext {
  disabled: boolean
  expanded: boolean
  menuId: string
}

let nextMenuId = 0

const props = defineProps<{
  icon?: string
  title?: string
}>()

const expanded = defineModel<boolean>('expanded', { default: false })

const parentNavigationContext = inject<Ref<NavigationContext> | undefined>('mud-nav-context', undefined)
nextMenuId += 1
const menuId = `mud-nav-menu-${nextMenuId}`

const navigationContext = computed<NavigationContext>(() => ({
  disabled: parentNavigationContext?.value.disabled === true,
  expanded: expanded.value && (parentNavigationContext === undefined || parentNavigationContext.value.expanded),
  menuId,
}))

provide('mud-nav-context', navigationContext)

const classes = computed(() => [
  'mud-nav-group',
])

const buttonClasses = computed(() => [
  'mud-nav-link',
  'mud-ripple',
  {
    'mud-expanded': expanded.value,
  },
])

const iconClasses = [
  'mud-nav-link-icon',
  'mud-nav-link-icon-default',
]

const expandIconClasses = computed(() => [
  'mud-nav-link-expand-icon',
  {
    'mud-transform': expanded.value,
  },
])

const buttonTabIndex = computed(() =>
  parentNavigationContext?.value.disabled === true || parentNavigationContext?.value.expanded === false ? -1 : 0,
)

const ariaLabel = computed(() => `Toggle ${props.title ?? menuId}`)

const toggle = () => {
  expanded.value = !expanded.value
}
</script>

<template>
  <nav
    :class="classes"
    :disabled="navigationContext.disabled"
    :aria-label="title"
  >
    <button
      :class="buttonClasses"
      :tabindex="buttonTabIndex"
      :aria-controls="menuId"
      :aria-expanded="navigationContext.expanded"
      :aria-label="ariaLabel"
      @click="toggle"
    >
      <MudIcon
        v-if="icon"
        :icon="icon"
        :class="iconClasses"
      />
      <div class="mud-nav-link-text">
        <slot name="titleContent">
          {{ title }}
        </slot>
      </div>
      <MudIcon
        :icon="mdArrowDropDown"
        :class="expandIconClasses"
      />
    </button>
    <MudCollapse
      :expanded="navigationContext.expanded"
      :aria-hidden="!navigationContext.expanded"
      class="mud-navgroup-collapse"
    >
      <MudNavMenu>
        <slot />
      </MudNavMenu>
    </MudCollapse>
  </nav>
</template>
