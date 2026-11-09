<script setup lang="ts">
import { computed, inject, type Ref } from 'vue'
import { RouterLink } from 'vue-router'
import MudIcon from './MudIcon.vue'
import type { MudColor, MudNavLinkMatch } from './types'


interface NavigationContext {
  disabled: boolean
  expanded: boolean
  menuId: string
}

const props = withDefaults(defineProps<{
  href?: string
  icon?: string
  iconColor?: MudColor
  match?: MudNavLinkMatch
}>(), {
  iconColor: 'default',
  match: 'prefix',
})

const navigationContext = inject<Ref<NavigationContext> | undefined>('mud-nav-context', undefined)

const tabIndex = computed(() =>
  navigationContext?.value.disabled === true || navigationContext?.value.expanded === false ? -1 : 0,
)

const iconClasses = computed(() => [
  'mud-nav-link-icon',
  {
    'mud-nav-link-icon-default': props.iconColor === 'default',
  },
])

const matchesActive = (isActive: boolean, isExactActive: boolean) =>
  props.match === 'all' ? isExactActive : isActive

const linkClasses = (isActive: boolean, isExactActive: boolean) => [
  'mud-nav-link',
  'mud-ripple',
  {
    active: matchesActive(isActive, isExactActive),
  },
]
</script>

<template>
  <div class="mud-nav-item">
    <RouterLink
      v-if="href !== undefined"
      v-slot="{ href: linkHref, navigate, isActive, isExactActive }"
      :to="href"
      custom
    >
      <a
        :href="linkHref"
        :class="linkClasses(isActive, isExactActive)"
        :tabindex="tabIndex"
        :aria-current="isExactActive ? 'page' : undefined"
        @click="navigate"
      >
        <MudIcon
          v-if="icon"
          :icon="icon"
          :color="iconColor"
          :class="iconClasses"
        />
        <div class="mud-nav-link-text">
          <slot />
        </div>
      </a>
    </RouterLink>
    <div
      v-else
      class="mud-nav-link mud-ripple"
      :tabindex="tabIndex"
    >
      <MudIcon
        v-if="icon"
        :icon="icon"
        :color="iconColor"
        :class="iconClasses"
      />
      <div class="mud-nav-link-text">
        <slot />
      </div>
    </div>
  </div>
</template>
