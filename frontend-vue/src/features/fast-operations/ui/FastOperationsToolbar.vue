<script setup lang="ts">
import type { OperationTypeId } from '@/domain/operation-type'
import { OPERATION_TYPES } from '@/domain/operation-type'
import { mdAdd } from '@/shared/icons'
import { MudButton, MudGrid, MudItem, type MudColor } from '@/shared/mud'

const emit = defineEmits<{ create: [operationTypeId: OperationTypeId | null] }>()

function mudColorOf(color: string): MudColor {
  return color as MudColor
}
</script>

<template>
  <MudGrid
    class="mb-2"
    :spacing="2"
  >
    <MudItem
      v-for="type in OPERATION_TYPES"
      :key="type.id"
      :lg="3"
      :md="4"
      :xs="12"
    >
      <MudButton
        :color="mudColorOf(type.color)"
        :data-testid="`fast-operations-add-${type.id}`"
        :end-icon="mdAdd"
        full-width
        variant="filled"
        @click="emit('create', type.id)"
      >
        {{ type.addLabel }}
      </MudButton>
    </MudItem>
    <MudItem
      :lg="3"
      :md="4"
      :xs="12"
    >
      <MudButton
        :end-icon="mdAdd"
        data-testid="fast-operations-add"
        full-width
        variant="filled"
        @click="emit('create', null)"
      >
        Добавить
      </MudButton>
    </MudItem>
  </MudGrid>
</template>
