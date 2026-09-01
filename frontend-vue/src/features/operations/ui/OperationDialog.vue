<script setup lang="ts">
import { computed, nextTick, ref, shallowRef, watch } from 'vue'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import {
  mdAutoAwesome,
  mdClose,
  mdComment,
} from '@/shared/icons'
import {
  MudButton,
  MudChip,
  MudGrid,
  MudIconButton,
  MudItem,
  MudPopover,
  MudStack,
  MudTextField,
  MudTooltip,
} from '@/shared/mud'
import SmartCategory from '@/shared/ui/SmartCategory.vue'
import SmartDatePicker from '@/shared/ui/SmartDatePicker.vue'
import SmartPlace from '@/shared/ui/SmartPlace.vue'
import SmartSum from '@/shared/ui/SmartSum.vue'
import { usePlaceSuggestions } from '@/shared/ui/usePlaceSuggestions'
import { inferCategoryCached, resetCategoryInference } from '../model/category-inference'
import type { OperationFormValue } from '../model/operation-form'
import {
  categoriesForType,
  DATE_UNPARSED,
  shouldResetPlaces,
  SUM_UNPARSED,
  validateOperationForm,
} from '../model/operation-form'
import { useOperationSave } from '../model/operation-save'

const props = defineProps<{
  categories: readonly Category[]
  isSaving: boolean
}>()

const emit = defineEmits<{
  submit: [value: OperationFormValue]
  notice: [text: string]
}>()

const places = usePlaceSuggestions()
const save = useOperationSave()

const value = shallowRef<OperationFormValue | null>(null)
const isSubmitting = ref(false)

const operationTypeId = ref<OperationTypeId>(1)
const categoryId = ref<number | null>(null)
const sum = shallowRef<Money | null>(null)
const date = ref<LocalDate | null>(null)
const comment = ref('')
const place = ref('')

const categoryError = ref('')
const dateError = ref('')

const suggestedCategoryId = ref<number | null>(null)
const isSuggestionBlocked = ref(false)

const sumField = ref<{ focusInput: () => void, getSum: () => Money | null }>()
const dateField = ref<{ getDate: () => LocalDate | null }>()

let session = 0

const isOpen = computed(() => value.value !== null)

const options = computed(() => categoriesForType(props.categories, operationTypeId.value))

const suggestedCategory = computed(() => {
  if (suggestedCategoryId.value === null || categoryId.value !== suggestedCategoryId.value) {
    return null
  }

  return options.value.find(category => category.id === suggestedCategoryId.value) ?? null
})

watch(value, next => {
  if (next === null) {
    return
  }

  operationTypeId.value = next.operationTypeId
  categoryId.value = next.categoryId
  sum.value = next.sum
  date.value = next.date
  comment.value = next.comment
  place.value = next.place

  categoryError.value = ''
  dateError.value = ''
  suggestedCategoryId.value = null
  isSuggestionBlocked.value = false

  focusSum()
  void suggestCategory()
})

function open(next: OperationFormValue): void {
  session += 1
  value.value = next
}

function close(): void {
  value.value = null
}

function focusSum(): void {
  void nextTick(() => {
    sumField.value?.focusInput()
  })
}

function clearSuggestion(): void {
  categoryId.value = null
  suggestedCategoryId.value = null
  isSuggestionBlocked.value = true
}

function onPlaceCommitted(): void {
  void suggestCategory()
}

async function suggestCategory(): Promise<void> {
  const name = place.value.trim()

  if (isSuggestionBlocked.value || categoryId.value !== null || name.length === 0) {
    return
  }

  const opened = session
  const requestedType = operationTypeId.value
  const suggestion = await inferCategoryCached(name, requestedType)

  if (opened !== session) {
    return
  }

  if (suggestion === null
    || name !== place.value.trim()
    || requestedType !== operationTypeId.value
    || isSuggestionBlocked.value
    || categoryId.value !== null) {
    return
  }

  suggestedCategoryId.value = suggestion
  categoryId.value = suggestion
  categoryError.value = ''
}

async function submit(): Promise<void> {
  const current = value.value

  if (current === null || isSubmitting.value) {
    return
  }

  const parsedSum = sumField.value?.getSum() ?? null
  const parsedDate = dateField.value?.getDate() ?? null

  const candidate: OperationFormValue = {
    ...current,
    operationTypeId: operationTypeId.value,
    categoryId: categoryId.value,
    sum: parsedSum,
    date: date.value,
    comment: comment.value,
    place: place.value,
  }

  const errors = validateOperationForm(candidate)

  categoryError.value = errors.categoryId ?? ''
  dateError.value = errors.date ?? ''

  if (categoryError.value.length > 0 || dateError.value.length > 0) {
    return
  }

  if (parsedSum === null) {
    emit('notice', SUM_UNPARSED)
    return
  }

  if (parsedDate === null) {
    emit('notice', DATE_UNPARSED)
    return
  }

  const submitted = session

  isSubmitting.value = true

  try {
    await save(candidate)

    if (shouldResetPlaces(current.place, candidate.place)) {
      places.reset()
    }

    resetCategoryInference()

    emit('submit', candidate)

    if (submitted === session) {
      close()
    }
  } catch (error) {
    emit('notice', error instanceof Error ? error.message : 'Не удалось сохранить операцию')
  } finally {
    isSubmitting.value = false
  }
}

defineExpose({ open, close })
</script>

<template>
  <slot
    :is-open="isOpen"
    :open="open"
  />

  <MudPopover
    anchor-origin="bottom-left"
    class="mt-4 mx-4 pa-4 operation-popover"
    :open="isOpen"
    overflow-behavior="flip-never"
    transform-origin="top-left"
  >
    <form
      data-testid="operation-dialog"
      @submit.prevent="submit"
    >
      <MudGrid>
        <MudItem
          :md="6"
          :xs="12"
        >
          <div data-testid="operation-dialog-sum">
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
          <div data-testid="operation-dialog-date">
            <SmartDatePicker
              ref="dateField"
              v-model="date"
              blank-keeps-value
              :error="dateError.length > 0"
              :error-text="dateError"
              @update:model-value="dateError = ''"
            />
          </div>
        </MudItem>
        <MudItem
          :md="6"
          :xs="12"
        >
          <SmartCategory
            v-model="categoryId"
            data-testid="operation-dialog-category"
            :error="categoryError.length > 0"
            :error-text="categoryError"
            :options="options"
            @update:model-value="categoryError = ''"
          />

          <MudChip
            v-if="suggestedCategory !== null"
            :icon="mdAutoAwesome"
            class="mt-1 operation-popover__suggestion"
            color="tertiary"
            data-testid="operation-dialog-suggestion"
            size="small"
            variant="outlined"
          >
            <MudTooltip
              :text="`Подставлено: ${suggestedCategory.name}`"
              placement="top"
            >
              <span class="operation-popover__suggestion-label">
                {{ suggestedCategory.name }}
              </span>
            </MudTooltip>
            <MudTooltip
              text="Убрать подстановку"
              placement="top"
            >
              <MudIconButton
                :icon="mdClose"
                aria-label="Убрать подстановку"
                class="ml-1"
                data-testid="operation-dialog-suggestion-clear"
                size="small"
                @click="clearSuggestion"
              />
            </MudTooltip>
          </MudChip>
        </MudItem>
        <MudItem
          :md="6"
          :xs="12"
        >
          <SmartPlace
            v-model="place"
            data-testid="operation-dialog-place"
            @committed="onPlaceCommitted"
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
            data-testid="operation-dialog-comment"
            label="Комментарий"
            sizing="auto"
          />
        </MudItem>
      </MudGrid>

      <MudStack
        align-items="center"
        class="mt-3"
        justify="space-between"
        row
      >
        <MudButton
          color="error"
          data-testid="operation-dialog-cancel"
          tabindex="-1"
          @click="close"
        >
          Закрыть
        </MudButton>
        <MudButton
          :disabled="isSaving"
          button-type="submit"
          color="tertiary"
          data-testid="operation-dialog-save"
          variant="filled"
          @click="submit"
        >
          Сохранить
        </MudButton>
      </MudStack>
    </form>
  </MudPopover>
</template>

<style>
.operation-popover {
  max-width: 600px !important;
}

.operation-popover__suggestion {
  max-width: 100%;
}

.operation-popover__suggestion-label {
  display: inline-block;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  vertical-align: middle;
}
</style>
