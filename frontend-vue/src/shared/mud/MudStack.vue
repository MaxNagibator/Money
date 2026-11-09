<script setup lang="ts">
import { computed } from 'vue'
import type { MudAlignItems, MudJustify, MudStretchItems, MudWrap } from './types'

const props = withDefaults(defineProps<{
  row?: boolean
  reverse?: boolean
  spacing?: number
  justify?: MudJustify
  alignItems?: MudAlignItems
  wrap?: MudWrap
  stretchItems?: MudStretchItems
  htmlTag?: string
}>(), {
  spacing: 3,
  htmlTag: 'div',
})

const direction = computed(() => `flex-${props.row ? 'row' : 'column'}${props.reverse ? '-reverse' : ''}`)

const classes = computed(() => [
  'd-flex',
  direction.value,
  {
    [`justify-${props.justify}`]: props.justify !== undefined,
    [`align-${props.alignItems}`]: props.alignItems !== undefined,
    [`flex-${props.wrap}`]: props.wrap !== undefined,
    [`gap-${props.spacing}`]: props.spacing >= 0,
    [`flex-grow-${props.stretchItems}`]: props.stretchItems !== undefined && props.stretchItems !== 'none',
  },
])
</script>

<template>
  <component
    :is="htmlTag"
    :class="classes"
  >
    <slot />
  </component>
</template>
