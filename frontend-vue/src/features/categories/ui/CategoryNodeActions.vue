<script setup lang="ts">
import type { Category } from '@/domain/category'
import {
  mdAdd,
  mdDelete,
  mdEdit,
  mdCancel,
} from '@/shared/icons'
import { MudButton, MudIconButton } from '@/shared/mud'

defineProps<{ category: Category }>()

const emit = defineEmits<{
  create: [parentId: number]
  edit: [category: Category]
  remove: [category: Category]
  restore: [category: Category]
}>()
</script>

<template>
  <div class="category-actions">
    <span
      v-if="category.color !== null && !category.isDeleted"
      :style="{ backgroundColor: category.color }"
      class="category-color"
    />

    <MudButton
      v-if="category.isDeleted"
      :end-icon="mdCancel"
      color="info"
      data-testid="category-restore"
      size="large"
      @click.stop="emit('restore', category)"
    >
      Восстановить
    </MudButton>
    <template v-else>
      <MudIconButton
        :icon="mdAdd"
        :title="`Добавить внутрь «${category.name}»`"
        color="success"
        data-testid="category-add-child"
        @click.stop="emit('create', category.id)"
      />
      <MudIconButton
        :icon="mdEdit"
        :title="`Изменить «${category.name}»`"
        color="warning"
        data-testid="category-edit"
        @click.stop="emit('edit', category)"
      />
      <MudIconButton
        :icon="mdDelete"
        :title="`Удалить «${category.name}»`"
        data-testid="category-remove"
        @click.stop="emit('remove', category)"
      />
    </template>
  </div>
</template>

<style scoped>
.category-actions {
  display: flex;
  align-items: center;
  pointer-events: auto;
}

.category-color {
  display: inline-block;
  margin: 0 0.5rem;
  padding: 0.8rem 1.5em;
  border: 1px solid #000;
  border-radius: 0.75rem;
  color: #fff;
  font-size: 1rem;
  font-weight: 500;
  line-height: 1.5;
}
</style>
