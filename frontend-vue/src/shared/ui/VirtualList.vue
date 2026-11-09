<script setup lang="ts" generic="T">
import { computed, onBeforeUnmount, onMounted, onUpdated, ref, useTemplateRef, watch } from 'vue'
import { buildOffsets, resolveVirtualWindow } from './virtual-window'

const props = withDefaults(defineProps<{
  items: readonly T[]
  itemKey: (item: T) => string | number
  estimate?: number
  overscan?: number
}>(), {
  estimate: 220,
  overscan: 4,
})

const rootRef = useTemplateRef<HTMLElement>('rootRef')
const sizes = new Map<string, number>()
const sizeVersion = ref(0)
const viewportTop = ref(0)
const viewportBottom = ref(0)

let observer: ResizeObserver | null = null
let frameHandle = 0

const keys = computed(() => props.items.map(item => String(props.itemKey(item))))

const offsets = computed(() => {
  void sizeVersion.value

  return buildOffsets(keys.value.map(key => sizes.get(key) ?? props.estimate))
})

const frame = computed(() => resolveVirtualWindow(
  offsets.value,
  viewportTop.value,
  viewportBottom.value,
  props.overscan,
))

const visible = computed(() => props.items
  .slice(frame.value.start, frame.value.end)
  .map((item, index) => ({ item, key: keys.value[frame.value.start + index] ?? '', index: frame.value.start + index })))

function schedule(): void {
  if (frameHandle !== 0) {
    return
  }

  frameHandle = requestAnimationFrame(() => {
    frameHandle = 0
    measure()
  })
}

function measure(): void {
  const root = rootRef.value

  if (root === null) {
    return
  }

  const rootTop = root.getBoundingClientRect().top

  viewportTop.value = -rootTop
  viewportBottom.value = -rootTop + window.innerHeight

  measureSizes(root)
}

function measureSizes(root: HTMLElement): void {
  const nodes = [...root.querySelectorAll<HTMLElement>('[data-virtual-key]')]
  const rects = nodes.map(node => node.getBoundingClientRect())

  let changed = false

  for (const [index, node] of nodes.entries()) {
    const key = node.dataset.virtualKey
    const rect = rects[index]

    if (key === undefined || rect === undefined) {
      continue
    }

    const next = rects[index + 1]
    const size = next === undefined ? rect.height : next.top - rect.top

    if (size <= 0 || sizes.get(key) === size) {
      continue
    }

    sizes.set(key, size)
    changed = true
  }

  if (changed) {
    sizeVersion.value += 1
  }
}

function observeItems(): void {
  const root = rootRef.value

  if (root === null || observer === null) {
    return
  }

  observer.disconnect()

  for (const node of root.querySelectorAll('[data-virtual-key]')) {
    observer.observe(node)
  }
}

onMounted(() => {
  observer = new ResizeObserver(schedule)

  window.addEventListener('scroll', schedule, { passive: true })
  window.addEventListener('resize', schedule, { passive: true })

  observeItems()
  measure()
})

onUpdated(() => {
  observeItems()
  schedule()
})

onBeforeUnmount(() => {
  window.removeEventListener('scroll', schedule)
  window.removeEventListener('resize', schedule)

  observer?.disconnect()
  observer = null

  if (frameHandle !== 0) {
    cancelAnimationFrame(frameHandle)
    frameHandle = 0
  }
})

watch(keys, next => {
  const live = new Set(next)

  for (const key of [...sizes.keys()]) {
    if (!live.has(key)) {
      sizes.delete(key)
    }
  }

  schedule()
})
</script>

<template>
  <div ref="rootRef">
    <div :style="{ paddingTop: `${frame.padStart}px`, paddingBottom: `${frame.padEnd}px` }">
      <div
        v-for="entry in visible"
        :key="entry.key"
        :data-virtual-key="entry.key"
      >
        <slot
          :index="entry.index"
          :item="entry.item"
        />
      </div>
    </div>
  </div>
</template>
