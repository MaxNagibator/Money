<script setup lang="ts">
import { computed, nextTick, ref, shallowRef, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'
import { mdComment } from '@/shared/icons'
import {
  MudAlert,
  MudButton,
  MudDialog,
  MudGrid,
  MudItem,
  MudProgressCircular,
  MudSelect,
  MudSelectItem,
  MudText,
  MudTextField,
} from '@/shared/mud'
import SmartDatePicker from '@/shared/ui/SmartDatePicker.vue'
import SmartDebtOwner from '@/shared/ui/SmartDebtOwner.vue'
import SmartSum from '@/shared/ui/SmartSum.vue'
import { useOwnerSuggestions } from '@/shared/ui/useOwnerSuggestions'
import { DEBT_TYPES } from '../model/debt'
import type { DebtFormValue } from '../model/debt-form'
import {
  dateFieldError,
  shouldResetOwners,
  SUM_UNPARSED,
  validateDebtForm,
} from '../model/debt-form'

const props = defineProps<{
  value: DebtFormValue | null
  isSaving: boolean
}>()

const emit = defineEmits<{
  submit: [value: DebtFormValue]
  cancel: []
}>()

const typeId = ref<number | null>(null)
const ownerName = ref('')
const sum = shallowRef<Money | null>(null)
const date = ref<LocalDate | null>(null)
const comment = ref('')

const typeError = ref('')
const ownerError = ref('')
const dateError = ref('')
const notice = ref('')

const sumField = ref<{ focusInput: () => void, getSum: () => Money | null }>()
const dateField = ref<{ getDate: () => LocalDate | null }>()

const owners = useOwnerSuggestions()

const title = computed(() => props.value?.id === null ? 'Создать' : 'Обновить')
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
  ownerName.value = value.ownerName
  sum.value = value.sum
  date.value = value.date
  comment.value = value.comment

  typeError.value = ''
  ownerError.value = ''
  dateError.value = ''
  notice.value = ''

  void nextTick(() => {
    sumField.value?.focusInput()
  })
}, { immediate: true })

function submit(): void {
  const current = props.value

  if (current === null || props.isSaving) {
    return
  }

  const parsedSum = sumField.value?.getSum() ?? null
  const parsedDate = dateField.value?.getDate() ?? null

  const candidate: DebtFormValue = {
    ...current,
    typeId: typeId.value,
    ownerName: ownerName.value,
    sum: parsedSum,
    date: parsedDate,
    comment: comment.value,
  }

  const errors = validateDebtForm(candidate)

  typeError.value = errors.typeId ?? ''
  ownerError.value = errors.ownerName ?? ''
  dateError.value = dateFieldError(date.value, parsedDate)

  if (typeError.value.length > 0 || ownerError.value.length > 0 || dateError.value.length > 0) {
    notice.value = ''
    return
  }

  if (parsedSum === null) {
    notice.value = SUM_UNPARSED
    return
  }

  notice.value = ''

  if (shouldResetOwners(current.ownerName, candidate.ownerName)) {
    owners.reset()
  }

  emit('submit', candidate)
}
</script>

<template>
  <MudDialog
    v-model:visible="dialogVisible"
    :aria-label="title"
    class="debt-dialog"
  >
    <template #title>
      <MudText typo="h6">
        {{ title }}
      </MudText>
    </template>

    <template #content>
      <form
        data-testid="debts-dialog"
        @submit.prevent="submit"
      >
        <MudAlert
          v-if="notice.length > 0"
          class="mb-4"
          data-testid="debts-dialog-notice"
          dense
          severity="warning"
        >
          {{ notice }}
        </MudAlert>

        <MudGrid>
          <MudItem :xs="12">
            <SmartDebtOwner
              v-model="ownerName"
              data-testid="debts-dialog-owner"
              :error="ownerError.length > 0"
              :error-text="ownerError"
              @update:model-value="ownerError = ''"
            />
          </MudItem>

          <MudItem :xs="12">
            <div data-testid="debts-dialog-date">
              <SmartDatePicker
                ref="dateField"
                v-model="date"
                :clearable="false"
                :error="dateError.length > 0"
                :error-text="dateError"
                @update:model-value="dateError = ''"
              />
            </div>
          </MudItem>

          <MudItem :xs="12">
            <div data-testid="debts-dialog-sum">
              <SmartSum
                ref="sumField"
                v-model="sum"
              />
            </div>
          </MudItem>

          <MudItem :xs="12">
            <MudSelect
              v-model="typeId"
              adornment-color="tertiary"
              :error="typeError.length > 0"
              :error-text="typeError"
              data-testid="debts-dialog-type"
              label="Тип"
              @update:model-value="typeError = ''"
            >
              <MudSelectItem
                v-for="type in DEBT_TYPES"
                :key="type.id"
                :value="type.id"
              >
                {{ type.name }}
              </MudSelectItem>
            </MudSelect>
          </MudItem>

          <MudItem :xs="12">
            <MudTextField
              v-model="comment"
              :adornment-icon="mdComment"
              adornment-color="tertiary"
              :counter="0"
              data-testid="debts-dialog-comment"
              label="Комментарий"
              sizing="auto"
            />
          </MudItem>
        </MudGrid>
      </form>
    </template>

    <template #actions>
      <MudButton
        data-testid="debts-dialog-cancel"
        @click="emit('cancel')"
      >
        Отмена
      </MudButton>

      <MudButton
        :disabled="isSaving"
        color="success"
        data-testid="debts-dialog-save"
        variant="text"
        @click="submit"
      >
        <MudProgressCircular
          v-if="isSaving"
          indeterminate
          size="small"
        />
        <template v-else>
          Сохранить
        </template>
      </MudButton>
    </template>
  </MudDialog>
</template>

