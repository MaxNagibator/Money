<script setup lang="ts">
import { computed } from 'vue'
import type { FastOperation } from '@/api/adapters/fast-operations'
import { mdAdd } from '@/shared/icons'
import type { MudColor } from '@/shared/mud'
import { MudDivider, MudMenu, MudMenuItem } from '@/shared/mud'

const props = defineProps<{
  operations: readonly FastOperation[]
  actionLabel: string
  color: MudColor
  testId: string
  label?: string
  disabled?: boolean
}>()

const emit = defineEmits<{
  select: [operation: FastOperation]
}>()

const available = computed(() => props.operations.filter(operation => !operation.isDeleted))

const hasQuick = computed(() => available.value.length > 0)
</script>

<template>
  <MudMenu
    v-if="hasQuick || $slots.items"
    :aria-label="actionLabel"
    :color="color"
    :data-testid="testId"
    :disabled="disabled"
    :end-icon="label === undefined ? undefined : mdAdd"
    :icon="label === undefined ? mdAdd : undefined"
    :label="label"
    :max-height="500"
    :title="actionLabel"
    :variant="label === undefined ? 'text' : 'filled'"
    anchor-origin="bottom-center"
    dense
    size="small"
    transform-origin="top-center"
  >
    <slot name="items" />

    <MudDivider v-if="$slots.items && hasQuick" />

    <div
      v-if="hasQuick"
      :data-testid="`${testId}-list`"
    >
      <MudMenuItem
        v-for="operation in available"
        :key="operation.id"
        :data-testid="`${testId}-item-${operation.id}`"
        @click="emit('select', operation)"
      >
        {{ operation.name }}
      </MudMenuItem>
    </div>
  </MudMenu>
</template>
