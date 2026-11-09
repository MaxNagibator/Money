<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'

type MudCollapseState = 'entering' | 'entered' | 'exiting' | 'exited'

const expanded = defineModel<boolean>('expanded', { default: false })

const emit = defineEmits<{
  'animation-end': []
}>()

const state = ref<MudCollapseState>('exited')

const classes = computed(() => [
  'mud-collapse-container',
  {
    'mud-collapse-entering': state.value === 'entering',
    'mud-collapse-entered': state.value === 'entered',
    'mud-collapse-exiting': state.value === 'exiting',
    invisible: state.value === 'exited',
  },
])

const onTransitionEnd = () => {
  if (state.value === 'entering') {
    state.value = 'entered'
    emit('animation-end')
  } else if (state.value === 'exiting') {
    state.value = 'exited'
    emit('animation-end')
  }
}

watch(expanded, value => {
  state.value = value ? 'entering' : 'exiting'
})

onMounted(() => {
  if (expanded.value) {
    state.value = 'entered'
  }
})
</script>

<template>
  <div
    :class="classes"
    @transitionend="onTransitionEnd"
  >
    <div class="mud-collapse-wrapper">
      <div class="mud-collapse-wrapper-inner">
        <slot />
      </div>
    </div>
  </div>
</template>
