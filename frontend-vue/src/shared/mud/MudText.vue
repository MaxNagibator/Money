<script setup lang="ts">
import { computed } from 'vue'
import type { MudAlign, MudColor, MudTypo } from './types'

const props = withDefaults(defineProps<{
  typo?: MudTypo
  color?: MudColor
  align?: MudAlign
  gutterBottom?: boolean
  inline?: boolean
  htmlTag?: string
}>(), {
  typo: 'body1',
  color: 'default',
  align: 'inherit',
})

const TAGS: Partial<Record<MudTypo, string>> = {
  h1: 'h1',
  h2: 'h2',
  h3: 'h3',
  h4: 'h4',
  h5: 'h5',
  h6: 'h6',
  subtitle1: 'p',
  subtitle2: 'p',
  body1: 'p',
  body2: 'p',
}

const tag = computed(() => props.htmlTag ?? TAGS[props.typo] ?? 'span')

const align = computed(() => (props.align === 'start' ? 'left' : props.align === 'end' ? 'right' : props.align))

const classes = computed(() => [
  'mud-typography',
  `mud-typography-${props.typo}`,
  {
    [`mud-${props.color}-text`]: props.color !== 'default' && props.color !== 'inherit',
    'mud-typography-gutterbottom': props.gutterBottom,
    [`mud-typography-align-${align.value}`]: props.align !== 'inherit',
    'd-inline': props.inline,
  },
])
</script>

<template>
  <component
    :is="tag"
    :class="classes"
  >
    <slot />
  </component>
</template>
