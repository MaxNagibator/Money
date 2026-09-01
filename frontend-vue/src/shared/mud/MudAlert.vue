<script setup lang="ts">
import { computed } from 'vue'
import { mdCheckCircleOutline, mdErrorOutline, mdEvent, mdInfo, mdReportProblem } from '@/shared/icons'
import MudIcon from './MudIcon.vue'
import type { MudHorizontalAlignment, MudSeverity, MudVariant } from './types'

const props = withDefaults(defineProps<{
  severity?: MudSeverity
  variant?: MudVariant
  contentAlignment?: MudHorizontalAlignment
  dense?: boolean
  icon?: string
}>(), {
  severity: 'normal',
  variant: 'text',
  contentAlignment: 'left',
})

const defaultIcons: Record<MudSeverity, string> = {
  normal: mdEvent,
  info: mdInfo,
  success: mdCheckCircleOutline,
  warning: mdReportProblem,
  error: mdErrorOutline,
}

const icon = computed(() =>
  props.icon !== undefined && props.icon.length > 0 ? props.icon : defaultIcons[props.severity],
)

const alignment = computed(() =>
  props.contentAlignment === 'left'
    ? 'start'
    : props.contentAlignment === 'right'
      ? 'end'
      : props.contentAlignment,
)

const classes = computed(() => [
  'mud-alert',
  `mud-alert-${props.variant}-${props.severity}`,
  'mud-elevation-0',
  {
    'mud-dense': props.dense,
  },
])

const positionClasses = computed(() => [
  'mud-alert-position',
  `justify-${alignment.value}`,
])
</script>

<template>
  <div :class="classes">
    <div :class="positionClasses">
      <div class="mud-alert-icon mud-alert-icon-left">
        <MudIcon :icon="icon" />
      </div>
      <div class="mud-alert-message">
        <slot />
      </div>
    </div>
  </div>
</template>
