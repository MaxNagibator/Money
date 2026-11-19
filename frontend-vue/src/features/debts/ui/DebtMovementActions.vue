<script setup lang="ts">
import type { Debt } from '@/domain/debt'
import { mdMoreVert } from '@/shared/icons'
import { MudMenu, MudMenuItem } from '@/shared/mud'
import type { DebtMovement } from '../model/debt-accounts'

defineProps<{ movement: DebtMovement }>()
const emit = defineEmits<{ edit: [debt: Debt], remove: [debt: Debt], restore: [debt: Debt] }>()
</script>

<template>
  <MudMenu
    v-if="movement.kind === 'entry'"
    :icon="mdMoreVert"
    :aria-label="'Действия записи ' + movement.source.id"
    :data-testid="'debt-movement-menu-' + movement.source.id"
    anchor-origin="bottom-right"
    transform-origin="top-right"
  >
    <MudMenuItem
      v-if="movement.source.isDeleted"
      :data-testid="'debt-movement-restore-' + movement.source.id"
      @click="emit('restore', movement.source)"
    >
      Восстановить запись с её оплатами
    </MudMenuItem>
    <template v-else>
      <MudMenuItem
        v-if="movement.source.paySum.compare(movement.source.sum) < 0"
        :data-testid="'debt-movement-edit-' + movement.source.id"
        @click="emit('edit', movement.source)"
      >
        Редактировать
      </MudMenuItem>
      <MudMenuItem
        :data-testid="'debt-movement-delete-' + movement.source.id"
        @click="emit('remove', movement.source)"
      >
        Удалить запись с её оплатами
      </MudMenuItem>
    </template>
  </MudMenu>
</template>
