<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, useTemplateRef } from 'vue'
import MudIcon from './MudIcon.vue'
import type { MudColor, MudOrigin } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  origin?: MudOrigin
  color?: MudColor
  bordered?: boolean
  overlap?: boolean
  icon?: string
}>(), {
  origin: 'top-right',
  color: 'default',
  bordered: false,
  overlap: false,
})

const emit = defineEmits<{
  click: [event: MouseEvent]
}>()

const badgeElement = useTemplateRef<HTMLElement>('badgeElement')

const originClass = computed(() => `mud-badge-${props.origin.replace('-', ' ')}`)

const wrapperClasses = computed(() => [
  'mud-badge-wrapper',
  originClass.value,
])

const badgeClasses = computed(() => [
  'mud-badge',
  {
    'mud-badge-bordered': props.bordered,
    'mud-badge-icon': !!props.icon,
    [`mud-theme-${props.color}`]: props.color !== 'default',
    'mud-badge-default': props.color === 'default',
    'mud-badge-overlap': props.overlap,
  },
  originClass.value,
  'mud-elevation-0',
])

const handleBadgeClick = (event: MouseEvent) => emit('click', event)

onMounted(() => badgeElement.value?.addEventListener('click', handleBadgeClick))
onBeforeUnmount(() => badgeElement.value?.removeEventListener('click', handleBadgeClick))
</script>

<template>
  <span
    v-bind="$attrs"
    class="mud-badge-root"
  >
    <slot />
    <span :class="wrapperClasses">
      <span
        ref="badgeElement"
        role="status"
        aria-live="polite"
        :class="badgeClasses"
      >
        <MudIcon
          v-if="icon"
          :icon="icon"
          class="mud-icon-badge"
        />
      </span>
    </span>
  </span>
</template>
