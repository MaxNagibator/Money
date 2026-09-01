<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { formatMileage } from '@/platform/format'
import { mdComment } from '@/shared/icons'
import { DATE_UNPARSED } from '@/shared/ui/field-messages'
import SmartDatePicker from '@/shared/ui/SmartDatePicker.vue'
import {
  MudButton,
  MudDialog,
  MudGrid,
  MudImage,
  MudItem,
  MudNumericField,
  MudPaper,
  MudProgressCircular,
  MudStack,
  MudText,
  MudTextField,
} from '@/shared/mud'
import type { CarEventFormValue } from '../model/car-event-form'
import { validateCarEventForm } from '../model/car-event-form'
import { CAR_EVENT_TYPES } from '../model/car-event-type'

const props = defineProps<{
  value: CarEventFormValue | null
  title: string
  isSaving: boolean
  topMileage: number
}>()

const emit = defineEmits<{
  submit: [value: CarEventFormValue]
  cancel: []
}>()

const SPACES = /[\s\u00A0\u202F]/g

const typeId = ref<number | null>(null)
const eventTitle = ref('')
const comment = ref('')
const mileage = ref<number | null>(null)
const date = ref<LocalDate | null>(null)

const mileageText = computed({
  get: () => (mileage.value === null ? '' : formatMileage(mileage.value)),
  set: text => {
    const parsed = Number.parseInt(text.replace(SPACES, ''), 10)

    mileage.value = Number.isNaN(parsed) ? null : parsed
  },
})

const typeError = ref('')
const dateError = ref('')

const dateField = ref<{ getDate: () => LocalDate | null }>()

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

  typeId.value = value.typeId
  eventTitle.value = value.title
  comment.value = value.comment
  mileage.value = value.mileage
  date.value = value.date

  typeError.value = ''
  dateError.value = ''
}, { immediate: true })

function selectType(id: number): void {
  typeId.value = id
  typeError.value = ''
}

function submit(): void {
  const current = props.value

  if (current === null) {
    return
  }

  const parsedDate = dateField.value?.getDate() ?? null

  const candidate: CarEventFormValue = {
    ...current,
    typeId: typeId.value,
    title: eventTitle.value,
    comment: comment.value,
    mileage: mileage.value,
    date: date.value,
  }

  const errors = validateCarEventForm(candidate)

  typeError.value = errors.typeId ?? ''
  dateError.value = errors.date ?? ''

  if (typeError.value.length > 0 || dateError.value.length > 0) {
    return
  }

  if (parsedDate === null) {
    dateError.value = DATE_UNPARSED
    return
  }

  emit('submit', candidate)
}
</script>

<template>
  <MudDialog
    v-model:visible="dialogVisible"
    :aria-label="title"
    class="cars-dialog"
  >
    <template #title>
      <MudText typo="h6">
        <span data-testid="cars-dialog-title">{{ title }}</span>
      </MudText>
    </template>

    <template #content>
      <form
        data-testid="cars-dialog"
        @keydown.esc.stop.prevent
        @submit.prevent="submit"
      >
        <MudGrid>
          <MudItem
            :md="12"
            :xs="12"
          >
            <MudTextField
              v-model="eventTitle"
              :adornment-icon="mdComment"
              adornment-color="tertiary"
              data-testid="cars-dialog-title-input"
              helper-text="Что произошло"
              label="Описание"
              sizing="auto"
            />
          </MudItem>

          <MudItem
            :md="12"
            :xs="12"
          >
            <MudText
              class="mb-2"
              typo="subtitle1"
            >
              Тип события
            </MudText>

            <MudText
              v-if="typeError.length > 0"
              class="mb-2"
              data-testid="cars-dialog-type-error"
              html-tag="div"
              role="alert"
              typo="body2"
              color="error"
            >
              {{ typeError }}
            </MudText>

            <MudStack
              aria-label="Тип события"
              justify="space-between"
              role="radiogroup"
              row
              :spacing="1"
              wrap="wrap"
            >
              <MudPaper
                v-for="type in CAR_EVENT_TYPES"
                :key="type.id"
                :aria-checked="type.id === typeId"
                :aria-label="type.name"
                :class="{ 'car-event-type--selected': type.id === typeId }"
                :data-testid="`cars-dialog-type-${type.id}`"
                class="car-event-type d-flex flex-column align-center pa-1"
                role="radio"
                tabindex="0"
                @click="selectType(type.id)"
                @keydown.enter.prevent="selectType(type.id)"
                @keydown.space.prevent="selectType(type.id)"
              >
                <MudImage
                  :alt="type.name"
                  class="d-flex flex-grow-1"
                  fluid
                  :src="type.icon"
                />
                <MudText
                  class="d-flex"
                  align="center"
                  typo="body2"
                >
                  {{ type.name }}
                </MudText>
              </MudPaper>
            </MudStack>
          </MudItem>

          <MudItem
            :md="6"
            :xs="12"
          >
            <div data-testid="cars-dialog-date">
              <SmartDatePicker
                ref="dateField"
                v-model="date"
                adornment="start"
                :error="dateError.length > 0"
                :error-text="dateError"
                helper-text="Когда"
                @update:model-value="dateError = ''"
              />
            </div>
          </MudItem>

          <MudItem
            :md="6"
            :xs="12"
          >
            <MudNumericField
              v-model:text="mileageText"
              :helper-text="`Последнее значение: ${formatMileage(topMileage)}`"
              clearable
              data-testid="cars-dialog-mileage"
              label="Пробег (км)"
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
              data-testid="cars-dialog-comment"
              helper-text="Подробности"
              label="Комментарий"
              sizing="auto"
            />
          </MudItem>
        </MudGrid>
      </form>
    </template>

    <template #actions>
      <MudButton
        data-testid="cars-dialog-cancel"
        @click="emit('cancel')"
      >
        Отмена
      </MudButton>

      <MudButton
        :disabled="isSaving"
        button-type="submit"
        color="success"
        data-testid="cars-dialog-save"
        @click="submit"
      >
        <MudProgressCircular
          v-if="isSaving"
          class="ms-n1"
          color="inherit"
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

<style scoped>
.car-event-type {
  width: 108px;
  height: 128px;
  cursor: pointer;
}

.car-event-type--selected {
  border: 4px solid var(--mud-palette-tertiary);
  border-radius: 4px;
}
</style>
