<script setup lang="ts">
import { computed } from 'vue'
import type { RouteLocationRaw } from 'vue-router'
import { RouterLink } from 'vue-router'
import MudIcon from './MudIcon.vue'
import MudText from './MudText.vue'
import type { MudButtonType, MudColor, MudEdge, MudSize, MudVariant } from './types'

const props = withDefaults(defineProps<{
  icon?: string
  variant?: MudVariant
  color?: MudColor
  size?: MudSize
  edge?: MudEdge
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
  edge: 'false',
  ripple: true,
  dropShadow: true,
  buttonType: 'button',
})

const asButton = computed(() => props.variant !== 'text')

const classes = computed(() => [
  'mud-button-root',
  'mud-icon-button',
  {
    'mud-button': asButton.value,
    [`mud-${props.color}-text hover:mud-${props.color}-hover`]: !asButton.value && props.color !== 'default',
    [`mud-button-${props.variant}`]: asButton.value,
    [`mud-button-${props.variant}-${props.color}`]: asButton.value,
    [`mud-button-${props.variant}-size-${props.size}`]: asButton.value,
    'mud-ripple': props.ripple,
    'mud-ripple-icon': props.ripple && !asButton.value,
    [`mud-icon-button-size-${props.size}`]: props.size !== 'medium',
    [`mud-icon-button-edge-${props.edge}`]: props.edge !== 'false',
    'mud-button-disable-elevation': !props.dropShadow,
  },
])

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
    <span
      v-if="icon"
      class="mud-icon-button-label"
    >
      <MudIcon
        :icon="icon"
        :size="size"
        :disabled="disabled"
      />
    </span>
    <MudText
      v-else
      typo="body2"
      color="inherit"
    >
      <slot />
    </MudText>
  </component>
</template>
