<script setup lang="ts">
import { computed } from 'vue'
import type { MudColor, MudSize } from './types'

const props = withDefaults(defineProps<{
  icon: string
  color?: MudColor | undefined
  size?: MudSize
  disabled?: boolean
  title?: string
  viewBox?: string
}>(), {
  color: 'inherit',
  size: 'medium',
  viewBox: '0 0 24 24',
})

const classes = computed(() => [
  'mud-icon-root',
  'mud-svg-icon',
  `mud-icon-size-${props.size}`,
  {
    'mud-icon-default': props.color === 'default' && !props.disabled,
    [`mud-${props.color}-text`]: props.color !== 'default' && props.color !== 'inherit' && !props.disabled,
  },
])

const escape = (value: string) => value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')

const content = computed(() => (props.title === undefined ? props.icon : `<title>${escape(props.title)}</title>${props.icon}`))
</script>

<template>
  <svg
    :class="classes"
    :viewBox="viewBox"
    focusable="false"
    aria-hidden="true"
    role="img"
    v-html="content"
  />
</template>
