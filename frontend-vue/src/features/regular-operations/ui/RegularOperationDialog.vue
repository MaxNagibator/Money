<script setup lang="ts">
import { computed, nextTick, ref, shallowRef, watch } from 'vue'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'
import {
  mdComment,
  mdDelete,
  mdSearch,
} from '@/shared/icons'
import {
  MudAutocomplete,
  MudButton,
  MudDialog,
  MudGrid,
  MudIconButton,
  MudItem,
  MudNumericField,
  MudProgressCircular,
  MudSelect,
  MudSelectItem,
  MudText,
  MudTextField,
} from '@/shared/mud'
import SmartDatePicker from '@/shared/ui/SmartDatePicker.vue'
import SmartPlace from '@/shared/ui/SmartPlace.vue'
import SmartSum from '@/shared/ui/SmartSum.vue'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { RegularOperationFormErrors, RegularOperationFormValue } from '../model/regular-operation-form'
import {
  DATE_UNPARSED,
  dateFieldError,
  searchCategories,
  validateRegularOperationForm,
} from '../model/regular-operation-form'
import { REGULAR_TIME_TYPES, timeValueHint, timeValueRules } from '../model/time-type'

const UNSIGNED_INTEGER_PATTERN = '[0-9]*'

const props = defineProps<{
  value: RegularOperationFormValue | null
  categories: readonly Category[]
  title: string
  isSaving: boolean
  removable?: boolean
}>()

const emit = defineEmits<{
  submit: [value: RegularOperationFormValue]
  cancel: []
  remove: []
}>()

const name = ref('')
const categoryId = ref<number | null>(null)
const sum = shallowRef<Money | null>(null)
const comment = ref('')
const place = ref('')
const timeTypeId = ref<number | null>(null)
const timeValue = ref<number | null>(null)
const dateFrom = ref<LocalDate | null>(null)
const dateTo = ref<LocalDate | null>(null)

const errors = ref<RegularOperationFormErrors>({})

const sumField = ref<{ focusInput: () => void, getSum: () => Money | null }>()
const dateFromField = ref<{ getDate: () => LocalDate | null }>()
const dateToField = ref<{ commitText: () => boolean, getDate: () => LocalDate | null }>()

const selectedCategory = computed<Category | null>({
  get: () => props.categories.find(category => category.id === categoryId.value) ?? null,
  set: category => {
    categoryId.value = category?.id ?? null
  },
})
const valueRules = computed(() => timeValueRules(timeTypeId.value))
const valueHint = computed(() => timeValueHint(timeTypeId.value, timeValue.value) ?? '')
const isNarrow = useNarrowViewport()
const canRemove = computed(() => props.removable === true && isNarrow.value && (props.value?.id ?? null) !== null)
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

  name.value = value.name
  categoryId.value = value.categoryId
  sum.value = value.sum
  comment.value = value.comment
  place.value = value.place
  timeTypeId.value = value.timeTypeId
  timeValue.value = value.timeValue
  dateFrom.value = value.dateFrom
  dateTo.value = value.dateTo
  errors.value = {}

  void nextTick(() => {
    sumField.value?.focusInput()
  })
}, { immediate: true })

function searchCategoryOptions(query: string): Category[] {
  return searchCategories(props.categories, query)
}

function categoryText(value: unknown): string {
  return (value as Category | null)?.name ?? ''
}

function messagesOf(field: keyof RegularOperationFormErrors): string[] {
  const message = errors.value[field]

  return message === undefined ? [] : [message]
}

function clearError(field: keyof RegularOperationFormErrors): void {
  const next = { ...errors.value }

  delete next[field]
  errors.value = next
}

function remove(): void {
  if (props.isSaving || !canRemove.value) {
    return
  }

  emit('remove')
}

function submit(): void {
  const current = props.value

  if (current === null || props.isSaving) {
    return
  }

  const parsedFrom = dateFromField.value?.getDate() ?? null
  const isDateToParsed = dateToField.value?.commitText() ?? true
  const parsedTo = dateToField.value?.getDate() ?? null

  const candidate: RegularOperationFormValue = {
    ...current,
    name: name.value,
    categoryId: categoryId.value,
    sum: sumField.value?.getSum() ?? null,
    comment: comment.value,
    place: place.value,
    timeTypeId: timeTypeId.value,
    timeValue: timeValue.value,
    dateFrom: parsedFrom,
    dateTo: parsedTo,
  }

  const nextErrors = validateRegularOperationForm(candidate)
  const dateFromMessage = dateFieldError(dateFrom.value, parsedFrom)

  if (dateFromMessage.length > 0) {
    nextErrors.dateFrom = dateFromMessage
  }

  if (!isDateToParsed) {
    nextErrors.dateTo = DATE_UNPARSED
  }

  errors.value = nextErrors

  if (Object.keys(errors.value).length > 0) {
    return
  }

  emit('submit', candidate)
}
</script>

<template>
  <MudDialog
    v-model:visible="dialogVisible"
    :aria-label="title"
    class="regular-operation-dialog"
    @keydown.esc.stop.prevent
  >
    <template #title>
      <MudText typo="h6">
        {{ title }}
      </MudText>
    </template>

    <template #content>
      <form
        id="regular-operations-dialog-form"
        data-testid="regular-operations-dialog"
        novalidate
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
              :error="messagesOf('name').length > 0"
              :error-text="messagesOf('name')[0] ?? ''"
              :adornment-icon="mdComment"
              adornment-color="tertiary"
              data-testid="regular-operations-dialog-name"
              immediate
              label="Наименование"
              sizing="auto"
              @update:model-value="clearError('name')"
            />
          </MudItem>

          <MudItem
            :md="6"
            :xs="12"
          >
            <div data-testid="regular-operations-dialog-date-from">
              <SmartDatePicker
                ref="dateFromField"
                v-model="dateFrom"
                :clearable="false"
                :error="messagesOf('dateFrom').length > 0"
                :error-text="messagesOf('dateFrom')[0] ?? ''"
                label="Действует с"
                @update:model-value="clearError('dateFrom')"
              />
            </div>
          </MudItem>

          <MudItem
            :md="6"
            :xs="12"
          >
            <div data-testid="regular-operations-dialog-date-to">
              <SmartDatePicker
                ref="dateToField"
                v-model="dateTo"
                :clearable="false"
                :error="messagesOf('dateTo').length > 0"
                :error-text="messagesOf('dateTo')[0] ?? ''"
                label="Действует по"
                @update:model-value="clearError('dateTo')"
              />
            </div>
          </MudItem>

          <MudItem
            :md="6"
            :xs="12"
          >
            <MudSelect
              v-model="timeTypeId"
              adornment-color="tertiary"
              clearable
              data-testid="regular-operations-dialog-time-type"
              :error="messagesOf('timeTypeId').length > 0"
              :error-text="messagesOf('timeTypeId')[0] ?? ''"
              label="Временной шаг"
              @update:model-value="clearError('timeTypeId')"
            >
              <MudSelectItem
                v-for="timeType in REGULAR_TIME_TYPES"
                :key="timeType.id"
                :value="timeType.id"
              >
                {{ timeType.name }}
              </MudSelectItem>
            </MudSelect>
          </MudItem>

          <MudItem
            :md="6"
            :xs="12"
          >
            <MudNumericField
              v-if="valueRules !== null"
              v-model="timeValue"
              adornment-color="tertiary"
              clearable
              :error="messagesOf('timeValue').length > 0"
              :error-text="messagesOf('timeValue')[0] ?? ''"
              :helper-text="valueHint"
              :label="valueRules.label"
              :min="valueRules.min"
              :pattern="isNarrow ? UNSIGNED_INTEGER_PATTERN : undefined"
              data-testid="regular-operations-dialog-time-value"
              immediate
              @update:model-value="clearError('timeValue')"
            />
          </MudItem>

          <MudItem
            :md="6"
            :xs="12"
          >
            <div data-testid="regular-operations-dialog-sum">
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
            <MudAutocomplete
              v-model="selectedCategory"
              :counter="0"
              :search-func="searchCategoryOptions"
              :to-string-func="categoryText"
              adornment-color="tertiary"
              :adornment-icon="mdSearch"
              clearable
              data-testid="regular-operations-dialog-category"
              :error="messagesOf('categoryId').length > 0"
              :error-text="messagesOf('categoryId')[0] ?? ''"
              label="Категория"
              reset-value-on-empty-text
              select-value-on-tab
              @update:model-value="clearError('categoryId')"
            />
          </MudItem>

          <MudItem
            :md="12"
            :xs="12"
          >
            <SmartPlace
              v-model="place"
              data-testid="regular-operations-dialog-place"
              :error="messagesOf('place').length > 0"
              :error-text="messagesOf('place')[0] ?? ''"
              @update:model-value="clearError('place')"
            />
          </MudItem>

          <MudItem
            :md="12"
            :xs="12"
          >
            <MudTextField
              v-model="comment"
              :counter="0"
              :error="messagesOf('comment').length > 0"
              :error-text="messagesOf('comment')[0] ?? ''"
              :adornment-icon="mdComment"
              adornment-color="tertiary"
              data-testid="regular-operations-dialog-comment"
              immediate
              label="Комментарий"
              sizing="auto"
              @update:model-value="clearError('comment')"
            />
          </MudItem>
        </MudGrid>
      </form>
    </template>

    <template #actions>
      <MudButton
        data-testid="regular-operations-dialog-cancel"
        @click="emit('cancel')"
      >
        Отмена
      </MudButton>

      <MudIconButton
        v-if="canRemove"
        :disabled="isSaving"
        :icon="mdDelete"
        aria-label="Удалить регулярную операцию"
        color="error"
        data-testid="regular-operations-dialog-remove"
        size="small"
        title="Удалить"
        @click="remove"
      />

      <MudButton
        :disabled="isSaving"
        color="success"
        data-testid="regular-operations-dialog-save"
        form="regular-operations-dialog-form"
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

<style>
@media (max-width: 599.98px) {
  .mud-dialog.regular-operation-dialog {
    width: 100%;
    max-width: none;
    max-height: calc(var(--viewport-height, 100dvh) * 0.85);
    margin: auto 0 var(--keyboard-inset, 0px);
    border-radius: 12px 12px 0 0;
  }
}
</style>

