<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, watch } from 'vue'

const props = withDefaults(defineProps<{
  selector?: string | undefined
  topOffset?: number
}>(), {
  topOffset: 300,
})

const emit = defineEmits<{
  click: []
}>()

const visible = defineModel<boolean>('visible', { default: false })

let target: HTMLElement | Document | null = null

const classes = computed(() => [
  'mud-scroll-to-top',
  visible.value ? 'visible' : 'hidden',
])

const resolveTarget = (): HTMLElement | Document | null => {
  if (props.selector === undefined) {
    return document
  }

  return document.querySelector<HTMLElement>(props.selector)
}

const readScrollTop = (): number => {
  if (target === null || target === document) {
    return document.documentElement.scrollTop
  }

  return (target as HTMLElement).scrollTop
}

const updateVisibility = (): void => {
  visible.value = readScrollTop() >= props.topOffset
}

const scrollToTop = (): void => {
  const scroller = target === document ? window : target as HTMLElement | null

  scroller?.scrollTo({ top: 0, behavior: 'smooth' })
  emit('click')
}

const listen = (): void => {
  target?.removeEventListener('scroll', updateVisibility)
  target = resolveTarget()
  target?.addEventListener('scroll', updateVisibility)
  updateVisibility()
}

onMounted(listen)

watch(() => props.selector, listen)

onBeforeUnmount(() => {
  target?.removeEventListener('scroll', updateVisibility)
})
</script>

<template>
  <span
    :class="classes"
    role="button"
    tabindex="0"
    @click="scrollToTop"
    @keydown.enter="scrollToTop"
    @keydown.space.prevent="scrollToTop"
  >
    <slot />
  </span>
</template>
