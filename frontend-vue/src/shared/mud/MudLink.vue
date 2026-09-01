<script setup lang="ts">
import { computed, useAttrs } from 'vue'
import type { RouteLocationRaw } from 'vue-router'
import { RouterLink } from 'vue-router'

const props = defineProps<{
  href?: string
  target?: string
  to?: RouteLocationRaw
  disabled?: boolean
}>()

const attrs = useAttrs()

const hasClick = computed(() => attrs.onClick !== undefined)

const classes = computed(() => [
  'mud-typography mud-link',
  'mud-primary-text',
  'mud-link-underline-hover',
  {
    'cursor-pointer': props.href === undefined && props.to === undefined && hasClick.value && !props.disabled,
    'mud-link-disabled': props.disabled,
  },
])

const tag = computed(() => (props.to !== undefined && !props.disabled ? RouterLink : 'a'))

const linkAttributes = computed(() => {
  const role = hasClick.value ? { role: 'button' } : {}

  if (props.disabled) {
    return { 'aria-disabled': 'true', ...role }
  }

  return props.to !== undefined
    ? { to: props.to, ...role }
    : { href: props.href, target: props.target, rel: props.target === '_blank' ? 'noopener noreferrer' : undefined, ...role }
})

const onClick = (event: MouseEvent) => {
  if (props.disabled) {
    event.stopImmediatePropagation()
  }
}
</script>

<template>
  <component
    :is="tag"
    v-bind="linkAttributes"
    :class="classes"
    @click="onClick"
  >
    <slot />
  </component>
</template>
