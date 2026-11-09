<script setup lang="ts">
import { computed } from 'vue'
import type { Category } from '@/domain/category'
import { mdComment } from '@/shared/icons'
import {
  MudAlert,
  MudButton,
  MudPaper,
  MudProgressCircular,
  MudSpacer,
  MudStack,
  MudText,
  MudTextField,
} from '@/shared/mud'
import { categoryTreeItems } from '@/shared/ui/category-items'
import CategorySelector from '@/shared/ui/CategorySelector.vue'

const props = defineProps<{
  selectedCount: number
  categories: readonly Category[]
  isForgiving: boolean
}>()

const emit = defineEmits<{
  'select-all': []
  'clear': []
  'submit': []
  'close': []
}>()

const categoryId = defineModel<number | null>('categoryId', { default: null })
const comment = defineModel<string>('comment', { default: '' })

const items = computed(() => categoryTreeItems(props.categories))

const namesById = computed(() => new Map(props.categories.map(category => [category.id, category.name])))

const categoryHelperText = computed(() => (categoryId.value === null
  ? 'Выберите категорию'
  : namesById.value.get(categoryId.value) ?? 'Выберите категорию'))

function selectCategory(value: unknown): void {
  categoryId.value = typeof value === 'number' ? value : null
}
</script>

<template>
  <MudPaper
    class="debt-forgive"
    data-testid="debts-forgive"
    :elevation="1"
  >
    <MudStack
      class="pa-3"
      :spacing="2"
    >
      <MudText>
        Перенос долгов держателей в расходы
      </MudText>

      <MudAlert
        :severity="selectedCount > 0 ? 'info' : 'warning'"
        dense
      >
        <MudStack
          align-items="center"
          row
          :spacing="1"
        >
          <MudText
            data-testid="debts-forgive-count"
            typo="body2"
          >
            {{ selectedCount > 0 ? `Выбрано долгов: ${selectedCount}` : 'Выберите долги для прощения, отметив их галочками' }}
          </MudText>

          <MudSpacer />

          <MudButton
            color="primary"
            data-testid="debts-forgive-select-all"
            size="small"
            variant="text"
            @click="emit('select-all')"
          >
            Выбрать все
          </MudButton>

          <MudButton
            v-if="selectedCount > 0"
            color="secondary"
            data-testid="debts-forgive-clear"
            size="small"
            variant="text"
            @click="emit('clear')"
          >
            Очистить
          </MudButton>
        </MudStack>
      </MudAlert>

      <CategorySelector
        :helper-text="categoryHelperText"
        :items="items"
        :selected-value="categoryId"
        selection-mode="single-selection"
        test-id="debts-forgive-category"
        @update:selected-value="selectCategory"
      />

      <MudTextField
        v-model="comment"
        :adornment-icon="mdComment"
        adornment="start"
        adornment-color="tertiary"
        clearable
        data-testid="debts-forgive-comment"
        immediate
        label="Приписка"
      />

      <MudStack
        align-items="center"
        class="mt-3"
        row
      >
        <MudButton
          color="error"
          data-testid="debts-forgive-close"
          @click="emit('close')"
        >
          Закрыть
        </MudButton>

        <MudSpacer />

        <MudButton
          :disabled="selectedCount === 0 || isForgiving"
          color="tertiary"
          data-testid="debts-forgive-submit"
          variant="filled"
          @click="emit('submit')"
        >
          <template v-if="isForgiving">
            <MudProgressCircular
              class="ms-n1"
              indeterminate
              size="small"
            />
            <MudText class="ms-2">
              Прощаю...
            </MudText>
          </template>
          <MudText v-else>
            Простить
          </MudText>
        </MudButton>
      </MudStack>
    </MudStack>
  </MudPaper>
</template>

