<script setup lang="ts">
import { computed, useAttrs } from 'vue'
import MudIcon from './MudIcon.vue'
import type { MudColor, MudSize, MudVariant } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  color?: MudColor | undefined
  size?: MudSize
  variant?: MudVariant
  disabled?: boolean
  icon?: string
  text?: string
  value?: string
}>(), {
  color: 'default',
  size: 'medium',
  variant: 'filled',
})

const attrs = useAttrs()
const chipContainerId = `chip-container-${Math.random().toString(36).slice(2)}`

const elementAttrs = computed(() => ({
  tabindex: -1,
  ...attrs,
}))

const classes = computed(() => [
  'mud-chip',
  `mud-chip-${props.variant}`,
  `mud-chip-size-${props.size}`,
  `mud-chip-color-${props.color}`,
  {
    'mud-disabled': props.disabled,
  },
])
</script>

<template>
  <div
    :id="chipContainerId"
    class="mud-chip-container"
  >
    <div
      v-bind="elementAttrs"
      :class="classes"
    >
      <MudIcon
        v-if="props.icon"
        :icon="props.icon"
        class="mud-chip-icon"
        color="inherit"
        size="small"
        :disabled="props.disabled"
      />
      <span class="mud-chip-content">
        <template v-if="$slots.default">
          <slot />
        </template>
        <template v-else-if="props.text !== undefined">
          {{ props.text }}
        </template>
        <template v-else>
          {{ props.value }}
        </template>
      </span>
    </div>
  </div>
</template>
