<script setup lang="ts">
import { computed } from 'vue'
import MudIcon from './MudIcon.vue'
import MudIconButton from './MudIconButton.vue'
import MudText from './MudText.vue'
import type { MudAdornment, MudColor, MudEdge, MudSize } from './types'

const props = withDefaults(defineProps<{
  text?: string | undefined
  icon?: string | undefined
  placement?: MudAdornment
  size?: MudSize
  color?: MudColor
  ariaLabel?: string | undefined
  testId?: string | undefined
  onAdornmentClick?: ((event: MouseEvent) => void) | undefined
}>(), {
  text: '',
  icon: '',
  placement: 'none',
  size: 'medium',
  color: 'default',
})

const hasText = computed(() => props.text.trim().length > 0)

const hasIcon = computed(() => props.icon.trim().length > 0)

const edge = computed<MudEdge>(() => (props.placement === 'none' ? 'false' : props.placement))

const handleClick = (event: MouseEvent): void => props.onAdornmentClick?.(event)
</script>

<template>
  <div class="mud-input-adornment">
    <MudText
      v-if="hasText"
      class="mud-input-adornment-text"
      :color="color"
      tabindex="-1"
    >
      {{ text }}
    </MudText>
    <template v-else-if="hasIcon">
      <MudIconButton
        v-if="onAdornmentClick"
        class="mud-input-adornment-icon-button mud-no-activator"
        :icon="icon"
        :edge="edge"
        :size="size"
        :color="color"
        :aria-label="ariaLabel"
        :data-testid="testId"
        tabindex="-1"
        @click="handleClick"
        @mousedown.stop
      />
      <MudIcon
        v-else
        class="mud-input-adornment-icon"
        :icon="icon"
        :size="size"
        :color="color"
        :aria-label="ariaLabel"
        tabindex="-1"
      />
    </template>
  </div>
</template>
