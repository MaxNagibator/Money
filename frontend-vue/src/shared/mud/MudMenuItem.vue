<script setup lang="ts">
import { computed, inject, onBeforeUnmount, onMounted, useTemplateRef, watch, type Ref } from 'vue'
import MudText from './MudText.vue'

defineOptions({ inheritAttrs: false })

const emit = defineEmits<{
  click: [event: MouseEvent | KeyboardEvent]
}>()

interface MudMenuItemRegistration {
  element: HTMLElement
  disabled: Readonly<Ref<boolean>>
}

interface MudMenuContext {
  dense: Readonly<Ref<boolean>>
  disabled: Readonly<Ref<boolean>>
  registerItem: (item: MudMenuItemRegistration) => () => void
  close: () => void
  closeAll: () => void
}

const context = inject<MudMenuContext | undefined>('mud-menu-context')
const element = useTemplateRef<HTMLElement>('element')
const disabled = computed(() => context?.disabled.value ?? false)
const dense = computed(() => context?.dense.value ?? false)
let unregister: (() => void) | undefined

const classes = computed(() => [
  'mud-menu-item',
  {
    'mud-disabled': disabled.value,
    'mud-ripple': !disabled.value,
    'mud-list-item-clickable': !disabled.value,
    'mud-menu-item-dense': dense.value,
  },
])

const handleActivate = (event: MouseEvent | KeyboardEvent): void => {
  if (disabled.value) {
    return
  }

  context?.closeAll()
  emit('click', event)
}

const updateTabIndex = (): void => {
  if (element.value) {
    element.value.tabIndex = disabled.value ? -1 : 0
  }
}

watch(disabled, updateTabIndex)

onMounted(() => {
  updateTabIndex()
  if (context && element.value) {
    const item: MudMenuItemRegistration = {
      element: element.value,
      disabled,
    }
    unregister = context.registerItem(item)
  }
})

onBeforeUnmount(() => unregister?.())
</script>

<template>
  <div
    ref="element"
    :class="classes"
    role="menuitem"
    tabindex="0"
    :aria-disabled="disabled ? 'true' : 'false'"
    v-bind="$attrs"
    @click="handleActivate"
    @keydown.enter.prevent.stop="handleActivate"
    @keydown.space.prevent.stop="handleActivate"
  >
    <MudText
      class="mud-menu-item-text"
      :typo="dense ? 'body2' : 'body1'"
    >
      <slot />
    </MudText>
  </div>
</template>
