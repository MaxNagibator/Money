<script setup lang="ts">
import { computed } from 'vue'
import { mdChevronRight } from '@/shared/icons'
import MudIconButton from './MudIconButton.vue'

withDefaults(defineProps<{
  visible?: boolean | undefined
  disabled?: boolean | undefined
}>(), {
  visible: false,
})

const expanded = defineModel<boolean | undefined>('expanded', { default: false })

const classes = computed(() => [
  'mud-treeview-item-expand-button',
  'mud-treeview-item-arrow-expand',
  {
    'mud-transform': expanded.value,
  },
])

const toggle = (): void => {
  expanded.value = !expanded.value
}
</script>

<template>
  <div class="mud-treeview-item-arrow">
    <MudIconButton
      v-if="visible"
      :class="classes"
      :disabled="disabled"
      :icon="mdChevronRight"
      :aria-label="expanded ? 'Collapse' : 'Expand'"
      @click.stop="toggle"
    />
  </div>
</template>
