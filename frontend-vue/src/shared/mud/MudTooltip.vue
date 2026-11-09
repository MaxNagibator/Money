<script setup lang="ts">
import { computed } from 'vue'
import MudPopover from './MudPopover.vue'
import type { MudColor, MudOrigin, MudPlacement } from './types'

const props = withDefaults(defineProps<{
  text?: string | undefined
  placement?: MudPlacement
  color?: MudColor
  arrow?: boolean
  inline?: boolean
  disabled?: boolean
  duration?: number
  delay?: number
  rootClass?: string
  rootStyle?: string
}>(), {
  text: '',
  placement: 'bottom',
  color: 'default',
  arrow: false,
  inline: true,
  disabled: false,
  duration: 251,
  delay: 0,
})

const visible = defineModel<boolean>('visible', { default: false })

const slots = defineSlots<{
  default?: () => unknown
  tooltip?: () => unknown
}>()

const origins = computed<{ anchor: MudOrigin, transform: MudOrigin, placement: MudOrigin }>(() => {
  switch (props.placement) {
    case 'top':
      return { anchor: 'top-center', transform: 'bottom-center', placement: 'top-center' }
    case 'left':
    case 'start':
      return { anchor: 'center-left', transform: 'center-right', placement: 'center-left' }
    case 'right':
    case 'end':
      return { anchor: 'center-right', transform: 'center-left', placement: 'center-right' }
    default:
      return { anchor: 'bottom-center', transform: 'top-center', placement: 'bottom-center' }
  }
})

const hasTooltip = computed(() => !props.disabled && (props.text.length > 0 || Boolean(slots.tooltip)))

const containerClasses = computed(() => [
  'mud-tooltip-root',
  {
    'mud-tooltip-inline': props.inline,
  },
  props.rootClass,
])

const tooltipClasses = computed(() => [
  'mud-tooltip',
  'd-flex',
  `mud-tooltip-${origins.value.placement}`,
  {
    'mud-tooltip-default': props.color === 'default',
    'mud-tooltip-arrow': props.arrow,
    [`mud-border-${props.color}`]: props.arrow && props.color !== 'default',
    [`mud-theme-${props.color}`]: props.color !== 'default',
  },
])

const setVisible = (value: boolean): void => {
  visible.value = value
}
</script>

<template>
  <div
    :class="containerClasses"
    :style="rootStyle"
    @pointerenter="setVisible(true)"
    @pointerleave="setVisible(false)"
    @focusin="setVisible(true)"
    @focusout="setVisible(false)"
  >
    <slot />
    <MudPopover
      v-if="hasTooltip"
      :open="visible"
      :duration="duration"
      :delay="delay"
      :anchor-origin="origins.anchor"
      :transform-origin="origins.transform"
      :class="tooltipClasses"
      :paper="false"
    >
      <div
        v-if="$slots.tooltip"
        class="d-block"
      >
        <slot name="tooltip" />
      </div>
      <template v-else>
        {{ text }}
      </template>
    </MudPopover>
  </div>
</template>
