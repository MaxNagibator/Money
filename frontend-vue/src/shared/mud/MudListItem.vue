<script setup lang="ts">
import { computed, inject, type Ref } from 'vue'
import MudText from './MudText.vue'

defineOptions({ inheritAttrs: false })

const emit = defineEmits<{
  click: [event: MouseEvent | KeyboardEvent]
}>()

const listContext = inject<{ dense: Ref<boolean> } | undefined>('mud-list-context')

const dense = computed(() => listContext?.dense.value ?? false)

const classes = computed(() => [
  'mud-list-item',
  {
    'mud-list-item-dense': dense.value,
    'mud-list-item-gutters': true,
    'mud-list-item-clickable': true,
    'mud-ripple': true,
  },
])

const handleActivate = (event: MouseEvent | KeyboardEvent) => emit('click', event)
</script>

<template>
  <div
    :class="classes"
    role="button"
    tabindex="0"
    v-bind="$attrs"
    @click="handleActivate"
    @keydown.enter="handleActivate"
    @keydown.space.prevent="handleActivate"
  >
    <div class="mud-list-item-text">
      <MudText :typo="dense ? 'body2' : 'body1'">
        <slot />
      </MudText>
    </div>
  </div>
</template>
