<script setup lang="ts">
import { computed, nextTick, ref, shallowRef, watch } from 'vue'
import type { Category } from '@/domain/category'
import type { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import {
  mdComment,
  mdSearch,
} from '@/shared/icons'
import {
  MudAutocomplete,
  MudButton,
  MudDialog,
  MudGrid,
  MudItem,
  MudNumericField,
  MudProgressCircular,
  MudText,
  MudTextField,
} from '@/shared/mud'
import SmartPlace from '@/shared/ui/SmartPlace.vue'
import SmartSum from '@/shared/ui/SmartSum.vue'
import type { FastOperationFormValue } from '../model/fast-operation-form'
import { categoryOptions, searchCategories, validateFastOperationForm } from '../model/fast-operation-form'

const props = defineProps<{
  value: FastOperationFormValue | null
  categories: readonly Category[]
  title: string
  isSaving: boolean
}>()

const emit = defineEmits<{
  submit: [value: FastOperationFormValue]
  cancel: []
}>()

const operationTypeId = ref<OperationTypeId | null>(null)
const categoryId = ref<number | null>(null)
const name = ref('')
const sum = shallowRef<Money | null>(null)
const order = ref<number | null>(null)
const comment = ref('')
const place = ref('')

const nameError = ref('')
const categoryError = ref('')

const sumField = ref<{ focusInput: () => void, getSum: () => Money | null }>()

const options = computed(() => categoryOptions(props.categories, operationTypeId.value))
const selectedCategory = computed<Category | null>({
  get: () => options.value.find(category => category.id === categoryId.value) ?? null,
  set: category => {
    categoryId.value = category?.id ?? null
  },
})
const dialogVisible = computed({
  get: () => props.value !== null,
  set: (visible: boolean) => {
    if (!visible) {
      emit('cancel')
    }
  },
})

watch(() => props.value, value => {
  if (value === null) {
    return
  }

  operationTypeId.value = value.operationTypeId
  categoryId.value = value.categoryId
  name.value = value.name
  sum.value = value.sum
  order.value = value.order
  comment.value = value.comment
  place.value = value.place

  nameError.value = ''
  categoryError.value = ''

  void nextTick(() => {
    sumField.value?.focusInput()
  })
}, { immediate: true })

function searchCategoryOptions(query: string): Category[] {
  return searchCategories(options.value, query)
}

function categoryText(value: unknown): string {
  return (value as Category | null)?.name ?? ''
}

function readOrder(): number | null {
  return order.value
}

function submit(): void {
  const current = props.value

  if (current === null) {
    return
  }

  const candidate: FastOperationFormValue = {
    ...current,
    operationTypeId: operationTypeId.value,
    categoryId: categoryId.value,
    name: name.value,
    sum: sumField.value?.getSum() ?? null,
    order: readOrder(),
    comment: comment.value,
    place: place.value,
  }

  const errors = validateFastOperationForm(candidate)

  nameError.value = errors.name ?? ''
  categoryError.value = errors.categoryId ?? ''

  if (nameError.value.length > 0 || categoryError.value.length > 0) {
    return
  }

  emit('submit', candidate)
}
</script>

<template>
  <MudDialog
    v-model:visible="dialogVisible"
    :aria-label="title"
    class="fast-operation-dialog"
    @keydown.esc.stop.prevent
  >
    <template #title>
      <MudText typo="h6">
        {{ title }}
      </MudText>
    </template>

    <template #content>
      <form
        id="fast-operations-dialog-form"
        data-testid="fast-operations-dialog"
        @keydown.esc.stop.prevent
        @submit.prevent="submit"
      >
        <MudGrid>
          <MudItem
            :md="12"
            :xs="12"
          >
            <MudTextField
              v-model="name"
              :error="nameError.length > 0"
              :error-text="nameError"
              :adornment-icon="mdComment"
              adornment-color="tertiary"
              data-testid="fast-operations-dialog-name"
              immediate
              label="Наименование"
              sizing="auto"
              @update:model-value="nameError = ''"
            />
          </MudItem>
          <MudItem
            :md="6"
            :xs="12"
          >
            <div data-testid="fast-operations-dialog-sum">
              <SmartSum
                ref="sumField"
                v-model="sum"
              />
            </div>
          </MudItem>
          <MudItem
            :md="6"
            :xs="12"
          >
            <MudNumericField
              v-model="order"
              adornment-color="tertiary"
              clearable
              data-testid="fast-operations-dialog-order"
              immediate
              label="Приоритет сортировки"
            />
          </MudItem>
          <MudItem
            :md="6"
            :xs="12"
          >
            <MudAutocomplete
              v-model="selectedCategory"
              :counter="0"
              :search-func="searchCategoryOptions"
              :to-string-func="categoryText"
              adornment-color="tertiary"
              :adornment-icon="mdSearch"
              clearable
              data-testid="fast-operations-dialog-category"
              :error="categoryError.length > 0"
              :error-text="categoryError"
              label="Категория"
              reset-value-on-empty-text
              select-value-on-tab
              @update:model-value="categoryError = ''"
            />
          </MudItem>
          <MudItem
            :md="6"
            :xs="12"
          >
            <SmartPlace
              v-model="place"
              data-testid="fast-operations-dialog-place"
            />
          </MudItem>
          <MudItem
            :md="12"
            :xs="12"
          >
            <MudTextField
              v-model="comment"
              :adornment-icon="mdComment"
              adornment-color="tertiary"
              :counter="0"
              data-testid="fast-operations-dialog-comment"
              immediate
              label="Комментарий"
              sizing="auto"
            />
          </MudItem>
        </MudGrid>
      </form>
    </template>

    <template #actions>
      <MudButton
        data-testid="fast-operations-dialog-cancel"
        @click="emit('cancel')"
      >
        Отмена
      </MudButton>
      <MudButton
        :disabled="isSaving"
        color="success"
        data-testid="fast-operations-dialog-save"
        form="fast-operations-dialog-form"
        button-type="submit"
      >
        <MudProgressCircular
          v-if="isSaving"
          class="ms-n1"
          indeterminate
          size="small"
        />
        <MudText
          v-else
          typo="button"
        >
          Сохранить
        </MudText>
      </MudButton>
    </template>
  </MudDialog>
</template>

