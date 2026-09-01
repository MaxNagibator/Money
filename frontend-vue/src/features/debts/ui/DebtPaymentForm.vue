<script setup lang="ts">
import { ref, shallowRef, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'
import { mdComment } from '@/shared/icons'
import {
  MudButton,
  MudPaper,
  MudProgressCircular,
  MudStack,
  MudTextField,
} from '@/shared/mud'
import SmartDatePicker from '@/shared/ui/SmartDatePicker.vue'
import SmartSum from '@/shared/ui/SmartSum.vue'
import { dateFieldError } from '../model/debt-form'
import type { PaymentFormValue } from '../model/payment-form'
import { validatePaymentForm } from '../model/payment-form'

const props = defineProps<{
  value: PaymentFormValue | null
  isSaving: boolean
}>()

const emit = defineEmits<{
  submit: [value: PaymentFormValue]
  cancel: []
}>()

const sum = shallowRef<Money | null>(null)
const date = ref<LocalDate | null>(null)
const comment = ref('')

const sumError = ref('')
const dateError = ref('')

const sumField = ref<{ getSum: () => Money | null }>()
const dateField = ref<{ getDate: () => LocalDate | null }>()

watch(() => props.value, value => {
  sumError.value = ''
  dateError.value = ''

  if (value === null) {
    return
  }

  sum.value = value.sum
  date.value = value.date
  comment.value = value.comment
}, { immediate: true })

function submit(): void {
  const current = props.value

  if (current === null || props.isSaving) {
    return
  }

  const parsedDate = dateField.value?.getDate() ?? null

  const candidate: PaymentFormValue = {
    ...current,
    sum: sumField.value?.getSum() ?? null,
    date: parsedDate,
    comment: comment.value,
  }

  const errors = validatePaymentForm(candidate)

  sumError.value = errors.sum ?? ''
  dateError.value = dateFieldError(date.value, parsedDate)

  if (sumError.value.length > 0 || dateError.value.length > 0) {
    return
  }

  emit('submit', candidate)
}
</script>

<template>
  <MudPaper
    class="debt-payment pa-3"
    data-testid="debts-payment"
  >
    <form @submit.prevent="submit">
      <MudStack :spacing="2">
        <div data-testid="debts-payment-sum">
          <SmartSum
            ref="sumField"
            v-model="sum"
            :error="sumError.length > 0"
            :error-text="sumError"
            @update:model-value="sumError = ''"
          />
        </div>

        <div data-testid="debts-payment-date">
          <SmartDatePicker
            ref="dateField"
            v-model="date"
            :error="dateError.length > 0"
            :error-text="dateError"
            label="Дата оплаты"
            @update:model-value="dateError = ''"
          />
        </div>

        <MudTextField
          v-model="comment"
          :adornment-icon="mdComment"
          adornment="start"
          adornment-color="tertiary"
          data-testid="debts-payment-comment"
          label="Комментарий"
        />

        <MudStack
          align-items="center"
          class="mt-3"
          justify="space-between"
          row
        >
          <MudButton
            color="error"
            data-testid="debts-payment-cancel"
            @click="emit('cancel')"
          >
            Закрыть
          </MudButton>

          <MudButton
            :button-type="'submit'"
            :disabled="isSaving"
            color="tertiary"
            data-testid="debts-payment-save"
            variant="filled"
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
        </MudStack>
      </MudStack>
    </form>
  </MudPaper>
</template>

<style scoped>
.debt-payment {
  min-width: 280px;
}
</style>
