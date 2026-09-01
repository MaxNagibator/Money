<script setup lang="ts">
import { computed } from 'vue'
import type { RouteLocationRaw } from 'vue-router'
import { RouterLink } from 'vue-router'
import MudIcon from './MudIcon.vue'
import type { MudButtonType, MudColor, MudSize, MudVariant } from './types'

const props = withDefaults(defineProps<{
  variant?: MudVariant
  color?: MudColor
  size?: MudSize
  startIcon?: string | undefined
  endIcon?: string | undefined
  iconColor?: MudColor
  iconSize?: MudSize
  fullWidth?: boolean
  ripple?: boolean
  dropShadow?: boolean
  disabled?: boolean
  buttonType?: MudButtonType
  href?: string
  target?: string
  to?: RouteLocationRaw
}>(), {
  variant: 'text',
  color: 'default',
  size: 'medium',
  iconColor: 'inherit',
  ripple: true,
  dropShadow: true,
  buttonType: 'button',
})

const classes = computed(() => [
  'mud-button-root',
  'mud-button',
  `mud-button-${props.variant}`,
  `mud-button-${props.variant}-${props.color}`,
  `mud-button-${props.variant}-size-${props.size}`,
  {
    'mud-width-full': props.fullWidth,
    'mud-ripple': props.ripple,
    'mud-button-disable-elevation': !props.dropShadow,
  },
])

const iconSize = computed(() => props.iconSize ?? props.size)

const asLink = computed(() => !props.disabled && (props.to !== undefined || props.href !== undefined))

const tag = computed(() => (!asLink.value ? 'button' : props.to !== undefined ? RouterLink : 'a'))

const linkAttributes = computed(() =>
  !asLink.value
    ? { type: props.buttonType, disabled: props.disabled }
    : props.to !== undefined
      ? { to: props.to }
      : { href: props.href, target: props.target, rel: props.target === '_blank' ? 'noopener noreferrer' : undefined },
)
</script>

<template>
  <component
    :is="tag"
    v-bind="linkAttributes"
    :class="classes"
  >
    <span class="mud-button-label">
      <span
        v-if="startIcon"
        :class="['mud-button-icon-start', `mud-button-icon-size-${iconSize}`]"
      >
        <MudIcon
          :icon="startIcon"
          :size="iconSize"
          :color="iconColor"
          :disabled="disabled"
        />
      </span>
      <slot />
      <span
        v-if="endIcon"
        :class="['mud-button-icon-end', `mud-button-icon-size-${iconSize}`]"
      >
        <MudIcon
          :icon="endIcon"
          :size="iconSize"
          :color="iconColor"
          :disabled="disabled"
        />
      </span>
    </span>
  </component>
</template>
