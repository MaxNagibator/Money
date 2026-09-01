<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Category } from '@/domain/category'
import { buildCategoryTree } from '@/domain/category'
import type { OperationTypeId } from '@/domain/operation-type'
import { OPERATION_TYPES, operationTypeById } from '@/domain/operation-type'
import type { CategoryFormValue } from '@/features/categories'
import {
  CategoryDialog,
  CategoryTree,
  editCategoryForm,
  newCategoryForm,
  toDraft,
  useCategories,
} from '@/features/categories'
import { mdAdd } from '@/shared/icons'
import {
  MudAlert,
  MudIconButton,
  MudProgressCircular,
  MudSnackbar,
  MudTabPanel,
  MudTabs,
  MudText,
  MudTooltip,
} from '@/shared/mud'
import type { MudColor } from '@/shared/mud'

const state = useCategories()

const activeType = ref<OperationTypeId>(OPERATION_TYPES[0]?.id ?? 1)
const form = ref<CategoryFormValue | null>(null)
const isSaving = ref(false)
const notice = ref('')

const trees = computed(() => OPERATION_TYPES.map(type => ({
  type,
  nodes: buildCategoryTree(state.categories.value, type.id),
})))

const activePanelIndex = computed({
  get: () => OPERATION_TYPES.findIndex(type => type.id === activeType.value),
  set: (index: number) => {
    const type = OPERATION_TYPES[index]

    if (type !== undefined) {
      activeType.value = type.id
    }
  },
})

const activeOperationType = computed(() => operationTypeById(activeType.value))
const activeOperationColor = computed<MudColor>(() => activeOperationType.value.color as MudColor)
const dialogTitle = computed(() => form.value?.id === null ? 'Создать' : 'Обновить')
const noticeVisible = computed({
  get: () => notice.value.length > 0,
  set: (visible: boolean) => {
    if (!visible) {
      notice.value = ''
    }
  },
})

function create(parentId: number | null): void {
  form.value = newCategoryForm(activeType.value, parentId)
}

function edit(category: Category): void {
  form.value = editCategoryForm(category)
}

async function submit(value: CategoryFormValue): Promise<void> {
  isSaving.value = true

  try {
    await state.save(value.id, toDraft(value))
    form.value = null
    notice.value = 'Категория сохранена'
  } catch (error) {
    notice.value = error instanceof Error ? error.message : 'Не удалось сохранить категорию'
  } finally {
    isSaving.value = false
  }
}

async function remove(category: Category): Promise<void> {
  try {
    await state.remove(category.id)
  } catch (error) {
    notice.value = error instanceof Error ? error.message : 'Не удалось удалить категорию'
  }
}

async function restore(category: Category): Promise<void> {
  try {
    await state.restore(category.id)
  } catch (error) {
    notice.value = error instanceof Error ? error.message : 'Не удалось восстановить категорию'
  }
}
</script>

<template>
  <MudText
    gutter-bottom
    typo="h3"
  >
    Категории
  </MudText>

  <div
    v-if="state.isLoading.value"
    class="d-flex align-center gap-3"
  >
    <MudProgressCircular indeterminate />
    <MudText>Загрузка..</MudText>
  </div>

  <MudAlert
    v-else-if="state.loadError.value !== null"
    data-testid="categories-error"
    severity="error"
    variant="text"
  >
    {{ state.loadError.value }}
  </MudAlert>

  <template v-else>
    <MudTabs
      v-model:active-panel-index="activePanelIndex"
      :elevation="4"
    >
      <template #tabPanelHeader="{ panel }">
        <MudTooltip
          v-if="panel.text === activeOperationType.name"
          :text="activeOperationType.addLabel"
        >
          <MudIconButton
            :color="activeOperationColor"
            :icon="mdAdd"
            data-testid="category-add-root"
            size="small"
            variant="filled"
            @click.stop="create(null)"
          />
        </MudTooltip>
      </template>

      <MudTabPanel
        v-for="{ type, nodes } in trees"
        :key="type.id"
        :text="type.name"
      >
        <CategoryTree
          :nodes="nodes"
          @create="create"
          @edit="edit"
          @remove="remove"
          @restore="restore"
        />
      </MudTabPanel>
    </MudTabs>
  </template>

  <CategoryDialog
    :is-busy="isSaving"
    :title="dialogTitle"
    :value="form"
    @cancel="form = null"
    @submit="submit"
  />

  <MudSnackbar
    v-model="noticeVisible"
    :message="notice"
    data-testid="categories-notice"
  />
</template>
