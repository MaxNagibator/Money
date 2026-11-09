<script setup lang="ts">
import { computed, onBeforeUnmount, useId, watch } from 'vue'

const props = withDefaults(defineProps<{
  darkBackground?: boolean
  lightBackground?: boolean
  autoClose?: boolean
  absolute?: boolean
  lockScroll?: boolean
}>(), {
  darkBackground: false,
  lightBackground: false,
  autoClose: false,
  absolute: false,
  lockScroll: false,
})

const visible = defineModel<boolean>('visible', { default: false })

const emit = defineEmits<{
  click: [event: MouseEvent | KeyboardEvent]
  closed: []
}>()

const id = `overlay-${useId()}`

const classes = computed(() => [
  'mud-overlay',
  {
    'mud-overlay-absolute': props.absolute,
  },
])

const scrimClasses = computed(() => [
  'mud-overlay-scrim',
  {
    'mud-overlay-dark': props.darkBackground,
    'mud-overlay-light': props.lightBackground,
  },
])

const lockScrollClass = 'scroll-locked'
let appliedClass: string | null = null

const applyScrollLock = (): void => {
  const shouldLock = visible.value && props.lockScroll && !props.absolute

  if (shouldLock === (appliedClass !== null)) {
    return
  }

  if (appliedClass !== null) {
    document.body.classList.remove(appliedClass)
    appliedClass = null
    return
  }

  appliedClass = window.innerWidth > document.body.clientWidth
    ? lockScrollClass
    : `${lockScrollClass}-no-padding`

  document.body.classList.add(appliedClass)
}

watch([visible, () => props.lockScroll, () => props.absolute], applyScrollLock, { immediate: true })

onBeforeUnmount(() => {
  if (appliedClass !== null) {
    document.body.classList.remove(appliedClass)
    appliedClass = null
  }
})

const handleClick = (event: MouseEvent | KeyboardEvent): void => {
  if (props.autoClose) {
    visible.value = false
    emit('closed')
  }

  emit('click', event)
}
</script>

<template>
  <div
    v-if="visible"
    :id="id"
    :class="classes"
    role="button"
    tabindex="0"
    @click.stop="handleClick"
    @keydown.enter.stop="handleClick"
    @keydown.space.stop.prevent="handleClick"
  >
    <div
      v-if="darkBackground || lightBackground"
      :class="scrimClasses"
    />
    <div
      v-if="$slots.default"
      class="mud-overlay-content"
    >
      <slot />
    </div>
  </div>
</template>
