<script setup lang="ts">
import type { OperationTypeId } from '@/domain/operation-type'
import { OPERATION_TYPES } from '@/domain/operation-type'
import { mdAdd } from '@/shared/icons'
import { MudButton, MudGrid, MudIcon, MudItem, MudMenu, MudMenuItem, type MudColor } from '@/shared/mud'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'

const emit = defineEmits<{ create: [operationTypeId: OperationTypeId | null] }>()

const isNarrow = useNarrowViewport()

function mudColorOf(color: string): MudColor {
  return color as MudColor
}
</script>

<template>
  <MudMenu
    v-if="isNarrow"
    :icon="mdAdd"
    anchor-origin="bottom-right"
    aria-label="Добавить регулярную операцию"
    data-testid="regular-operations-add-menu"
    dense
    transform-origin="top-right"
    variant="filled"
  >
    <MudMenuItem
      v-for="type in OPERATION_TYPES"
      :key="type.id"
      :data-testid="`regular-operations-add-${type.id}`"
      @click="emit('create', type.id)"
    >
      <span class="align-center d-flex flex-row gap-2">
        <MudIcon
          :color="mudColorOf(type.color)"
          :icon="type.icon"
          size="small"
        />
        {{ type.shortAddLabel }}
      </span>
    </MudMenuItem>

    <MudMenuItem
      data-testid="regular-operations-add"
      @click="emit('create', null)"
    >
      <span class="align-center d-flex flex-row gap-2">
        <MudIcon
          :icon="mdAdd"
          size="small"
        />
        Без типа
      </span>
    </MudMenuItem>
  </MudMenu>

  <MudGrid
    v-else
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
        :data-testid="`regular-operations-add-${type.id}`"
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
        data-testid="regular-operations-add"
        full-width
        variant="filled"
        @click="emit('create', null)"
      >
        Добавить
      </MudButton>
    </MudItem>
  </MudGrid>
</template>
