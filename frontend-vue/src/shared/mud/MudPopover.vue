<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { createMudPopoverId, mudPopoverContainerSelector, useMudPopover } from './popover'
import type { MudDropdownWidth, MudOrigin, MudOverflowBehavior } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  open?: boolean
  anchorOrigin?: MudOrigin
  transformOrigin?: MudOrigin
  overflowBehavior?: MudOverflowBehavior
  relativeWidth?: MudDropdownWidth
  maxHeight?: number | undefined
  paper?: boolean
  square?: boolean
  dropShadow?: boolean
  elevation?: number
  fixed?: boolean
  duration?: number
  delay?: number
}>(), {
  open: false,
  anchorOrigin: 'top-left',
  transformOrigin: 'top-left',
  overflowBehavior: 'flip-always',
  relativeWidth: 'ignore',
  paper: true,
  square: false,
  dropShadow: true,
  elevation: 8,
  fixed: false,
  duration: 251,
  delay: 0,
})

const id = createMudPopoverId()

const ticks = ref(props.open ? Date.now() : 0)
const hasContent = ref(props.open)

watch(() => props.open, value => {
  ticks.value = value ? Date.now() : 0

  if (value) {
    hasContent.value = true
  }
})

const classes = computed(() => [
  'mud-popover',
  `mud-popover-${props.transformOrigin}`,
  `mud-popover-anchor-${props.anchorOrigin}`,
  `mud-popover-overflow-${props.overflowBehavior}`,
  {
    'mud-popover-fixed': props.fixed,
    'mud-popover-open': props.open,
    [`mud-popover-${props.relativeWidth}-width`]: props.relativeWidth !== 'ignore',
    'mud-paper': props.paper,
    'mud-paper-square': props.paper && props.square,
    [`mud-elevation-${props.elevation}`]: props.paper && props.dropShadow,
    'overflow-y-auto': props.maxHeight !== undefined,
  },
])

useMudPopover(id, hasContent)

const styles = computed(() => ({
  'transition-duration': `${props.duration}ms`,
  'transition-delay': `${props.delay}ms`,
  ...(props.maxHeight === undefined ? {} : { 'max-height': `${props.maxHeight}px` }),
}))
</script>

<template>
  <div
    :id="`popover-${id}`"
    class="mud-popover-cascading-value"
  />
  <Teleport
    v-if="hasContent"
    :to="mudPopoverContainerSelector"
  >
    <div
      :id="`popovercontent-${id}`"
      :data-ticks="ticks"
      :class="classes"
      :style="styles"
      v-bind="$attrs"
    >
      <slot v-if="open" />
    </div>
  </Teleport>
</template>
