<script setup lang="ts">
import { computed, useAttrs } from 'vue'
import { mdCheckBox, mdCheckBoxOutlineBlank } from '@/shared/icons'
import MudIcon from './MudIcon.vue'
import type { MudColor, MudSize } from './types'

defineOptions({ inheritAttrs: false })

const props = withDefaults(defineProps<{
  color?: MudColor
  size?: MudSize
}>(), {
  color: 'default',
  size: 'medium',
})

const model = defineModel<boolean>({ default: false })
const elementId = `checkbox-${Math.random().toString(36).slice(2)}`
const attrs = useAttrs()

const isRootAttribute = (name: string): boolean => name === 'class' || name === 'style' || name.startsWith('data-')

const rootAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => isRootAttribute(name)),
))

const inputAttrs = computed(() => Object.fromEntries(
  Object.entries(attrs).filter(([name]) => !isRootAttribute(name)),
))

const iconClasses = computed(() => [
  'mud-button-root mud-icon-button',
  `mud-${props.color}-text hover:mud-${props.color}-hover`,
  'mud-ripple mud-ripple-checkbox',
  `mud-checkbox-${model.value ? 'true' : 'false'}`,
])

const icon = computed(() => model.value ? mdCheckBox : mdCheckBoxOutlineBlank)

const handleChange = (event: Event) => {
  if (event.target instanceof HTMLInputElement) {
    model.value = event.target.checked
  }
}
</script>

<template>
  <div
    v-bind="rootAttrs"
    class="mud-input-control mud-input-control-boolean-input"
  >
    <div class="mud-input-control-input-container">
      <label
        :id="elementId"
        :for="`${elementId}-input`"
        class="mud-checkbox mud-input-content-placement-end"
        @click.stop
      >
        <span :class="iconClasses">
          <input
            :id="`${elementId}-input`"
            v-bind="inputAttrs"
            :tabindex="0"
            type="checkbox"
            class="mud-checkbox-input"
            :checked="model"
            @change="handleChange"
          >
          <MudIcon
            :icon="icon"
            color="inherit"
            :size="size"
          />
        </span>
      </label>
    </div>
  </div>
</template>
