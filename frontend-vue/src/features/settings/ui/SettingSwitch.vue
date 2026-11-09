<script setup lang="ts">
import { MudSwitch } from '@/shared/mud'

defineOptions({ inheritAttrs: false })

const { label, disabled = false } = defineProps<{
  label: string
  disabled?: boolean
}>()

const model = defineModel<boolean>({ required: true })
</script>

<template>
  <div class="setting-switch">
    <MudSwitch
      v-bind="$attrs"
      :model-value="model"
      :disabled="disabled"
      :label="label"
      color="primary"
      label-placement="start"
      @update:model-value="model = $event === true"
    />
  </div>
</template>

<style scoped>
.setting-switch :deep(.mud-input-control) {
  width: 100%;
}

.setting-switch :deep(label.mud-switch) {
  display: flex;
  width: 100%;
  min-height: 48px;
  justify-content: space-between;
  gap: 16px;
}

.setting-switch :deep(label.mud-switch > .mud-typography) {
  display: block;
  flex: 1 1 auto;
}
</style>
