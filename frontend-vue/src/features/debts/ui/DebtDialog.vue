<script setup lang="ts">
import { computed, nextTick, ref, shallowRef, useId, watch } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { formatRubles } from '@/platform/format'
import { mdComment, mdDelete } from '@/shared/icons'
import {
  MudAlert,
  MudButton,
  MudDialog,
  MudIconButton,
  MudProgressCircular,
  MudSelect,
  MudSelectItem,
  MudText,
  MudTextField,
} from '@/shared/mud'
import SmartDatePicker from '@/shared/ui/SmartDatePicker.vue'
import SmartDebtOwner from '@/shared/ui/SmartDebtOwner.vue'
import SmartSum from '@/shared/ui/SmartSum.vue'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import { useOwnerSuggestions } from '@/shared/ui/useOwnerSuggestions'
import { DEBT_TYPES } from '../model/debt'
import type { DebtFormValue } from '../model/debt-form'
import type { DebtAccount } from '../model/debt-accounts'
import type { DebtViewMode } from '../model/debt-presentation'
import {
  dateFieldError,
  shouldResetOwners,
  SUM_UNPARSED,
  validateDebtForm,
} from '../model/debt-form'

const props = defineProps<{
  value: DebtFormValue | null
  isSaving: boolean
  removable?: boolean
  movement?: boolean
  mode?: DebtViewMode
  accounts?: readonly DebtAccount[]
}>()

const emit = defineEmits<{
  submit: [value: DebtFormValue]
  cancel: []
  remove: []
}>()

const typeId = ref<number | null>(null)
const directionId = useId()
const ownerName = ref('')
const sum = shallowRef<Money | null>(null)
const previewSum = shallowRef<Money | null>(null)
const date = ref<LocalDate | null>(null)
const comment = ref('')

const typeError = ref('')
const ownerError = ref('')
const dateError = ref('')
const notice = ref('')

const sumField = ref<{ focusInput: () => void, getSum: () => Money | null }>()
const dateField = ref<{ getDate: () => LocalDate | null }>()

const owners = useOwnerSuggestions()

const isNarrow = useNarrowViewport()

const accounting = computed(() => props.mode === 'accounting')
const title = computed(() => props.value?.id === null ? props.movement ? 'Новая запись' : 'Новый долг' : 'Редактировать запись')
const selectedAccount = computed(() => props.accounts?.find(account => account.name.trim().toLowerCase() === ownerName.value.trim().toLowerCase()))
const editedDebt = computed(() => props.accounts?.flatMap(account => account.debts).find(debt => debt.id === props.value?.id))
const outgoingLabel = computed(() => accounting.value ? 'Уход (Выдано)' : selectedAccount.value === undefined || selectedAccount.value.balance.isZero ? 'Я дал / вернул' : selectedAccount.value.balance.isNegative ? 'Я вернул' : 'Дал ещё')
const incomingLabel = computed(() => accounting.value ? 'Приход (Получено)' : selectedAccount.value === undefined || selectedAccount.value.balance.isZero ? 'Мне дали / вернули' : selectedAccount.value.balance.isNegative ? 'Занял ещё' : 'Вернул мне')
const balancePreview = computed(() => {
  if (!props.movement || ownerName.value.trim().length === 0 || previewSum.value === null || typeId.value === null) { return null }
  const current = selectedAccount.value?.balance ?? Money.zero
  const original = editedDebt.value
  let baseline = current
  if (original && !original.isDeleted && selectedAccount.value?.debts.some(debt => debt.id === original.id)) {
    const contribution = original.sum.subtract(original.paySum)
    baseline = baseline.subtract(original.typeId === 1 ? contribution : contribution.negate())
  }
  const contribution = previewSum.value.subtract(original?.paySum ?? Money.zero)
  return { current, next: baseline.add(typeId.value === 1 ? contribution : contribution.negate()) }
})
function balanceDescription(value: Money): string {
  if (value.isZero) { return accounting.value ? 'расчёты закрыты' : 'долгов нет' }
  const label = accounting.value ? value.isNegative ? 'нужно отдать' : 'нужно забрать' : value.isNegative ? 'я должен' : 'мне должны'
  return label + ' ' + formatRubles(value.isNegative ? value.negate() : value, !accounting.value)
}
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

  typeId.value = value.typeId
  ownerName.value = value.ownerName
  sum.value = value.sum
  previewSum.value = value.sum
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
    @focus="sumField?.focusInput()"
  >
    <template #title>
      <MudText typo="h6">
        {{ title }}
      </MudText>
    </template>

    <template #content>
      <form
        class="debt-form-fields"
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

        <SmartDebtOwner
          v-model="ownerName"
          data-testid="debts-dialog-owner"
          :error="ownerError.length > 0"
          :error-text="ownerError"
          :label="movement ? accounting ? 'Контрагент' : 'Человек' : 'Держатель'"
          @update:model-value="ownerError = ''"
        />
        <fieldset
          v-if="movement"
          class="debt-directions"
          data-testid="debts-dialog-directions"
        >
          <legend>Направление</legend>
          <div class="debt-direction-options">
            <label
              :for="`${directionId}-out`"
              :class="{ active: typeId === 1 }"
            >
              <input
                :id="`${directionId}-out`"
                v-model="typeId"
                :name="directionId"
                type="radio"
                :value="1"
                data-testid="debts-dialog-outgoing"
                @change="typeError = ''"
              >
              <span aria-hidden="true">−</span> {{ outgoingLabel }}
            </label>
            <label
              :for="`${directionId}-in`"
              :class="{ active: typeId === 2 }"
            >
              <input
                :id="`${directionId}-in`"
                v-model="typeId"
                :name="directionId"
                type="radio"
                :value="2"
                data-testid="debts-dialog-incoming"
                @change="typeError = ''"
              >
              <span aria-hidden="true">+</span> {{ incomingLabel }}
            </label>
          </div>
          <span
            v-if="typeError"
            role="alert"
          >{{ typeError }}</span>
        </fieldset>
        <MudSelect
          v-else
          v-model="typeId"
          adornment-color="primary"
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
        <div>
          <div
            class="debt-sum-field"
            data-testid="debts-dialog-sum"
          >
            <SmartSum
              ref="sumField"
              v-model="sum"
              :amount-format="movement ? accounting ? 'fixed' : 'compact' : undefined"
              helper-text="Например: 500 или 350+150"
              placeholder="500 или 350+150"
              @preview="previewSum = $event"
            />
            <span
              class="debt-currency"
              aria-hidden="true"
            >₽</span>
          </div>
          <div
            v-if="balancePreview !== null"
            class="debt-balance-preview"
            data-testid="debts-dialog-preview"
            role="status"
          >
            <span>Баланс: {{ ownerName.trim() }}</span>
            {{ balanceDescription(balancePreview.current) }} → {{ balanceDescription(balancePreview.next) }}
          </div>
        </div>
        <div data-testid="debts-dialog-date">
          <SmartDatePicker
            ref="dateField"
            v-model="date"
            :clearable="false"
            color="primary"
            :error="dateError.length > 0"
            :error-text="dateError"
            helper-text="Можно ввести «в» (вчера) или «пв» (позавчера)"
            @update:model-value="dateError = ''"
          />
        </div>
        <MudTextField
          v-model="comment"
          :adornment-icon="mdComment"
          adornment-color="primary"
          :counter="comment.length >= 3600 ? 4000 : undefined"
          data-testid="debts-dialog-comment"
          label="Комментарий"
          sizing="auto"
        />
      </form>
    </template>

    <template #actions>
      <MudButton
        data-testid="debts-dialog-cancel"
        variant="outlined"
        @click="emit('cancel')"
      >
        Отмена
      </MudButton>

      <MudIconButton
        v-if="canRemove"
        :icon="mdDelete"
        aria-label="Удалить долг"
        color="error"
        data-testid="debts-dialog-remove"
        size="small"
        title="Удалить"
        @click="remove"
      />

      <MudButton
        :disabled="isSaving"
        color="primary"
        data-testid="debts-dialog-save"
        variant="filled"
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

<style>
.mud-dialog.debt-dialog {
  --mud-palette-tertiary: var(--mud-palette-primary);
  width: min(600px, calc(100vw - 32px));
  overflow: hidden;
}

.debt-dialog .mud-dialog-title { flex: 0 0 auto; padding: 20px 24px; }
.debt-dialog .mud-dialog-content { flex: 1 1 auto; min-height: 0; padding: 0 24px 20px; overflow-y: auto; }
.debt-dialog .mud-dialog-actions { flex: 0 0 auto; padding: 12px 24px 20px; gap: 8px; border-top: 1px solid var(--mud-palette-divider); }
.debt-dialog .mud-input .mud-icon-button { flex: 0 0 36px; width: 36px; height: 36px; margin: 0; padding: 6px; }
.debt-form-fields { display: flex; flex-direction: column; gap: 20px; min-width: 0; }
.debt-dialog .mud-button-label { text-transform: none; }

.debt-directions {
  margin: 0;
  border: 0;
  padding: 0;
  min-width: 0;
}
.debt-direction-options {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 4px;
  border-radius: 10px;
  background: var(--mud-palette-background-gray);
  padding: 4px;
}

.debt-directions legend { margin-bottom: 8px; padding: 0; color: var(--mud-palette-text-secondary); font-size: var(--mud-typography-caption-size); }
.debt-directions label {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 10px 8px;
  min-height: 44px;
  border-radius: 7px;
  text-align: center;
  cursor: pointer;
  font-size: 0.875rem;
}
.debt-directions input { position: absolute; width: 1px; height: 1px; opacity: 0; }
.debt-directions label:focus-within { outline: 2px solid var(--mud-palette-primary); outline-offset: 2px; }
.debt-directions .active { background: var(--mud-palette-primary); color: var(--mud-palette-primary-text); }
.debt-directions [role="alert"] { grid-column: 1 / -1; color: var(--mud-palette-error); }
.debt-sum-field { position: relative; }
.debt-sum-field input { padding-right: 30px; }
.debt-currency { position: absolute; right: 12px; top: 26px; color: var(--mud-palette-text-secondary); pointer-events: none; }
.debt-balance-preview { margin-top: 12px; padding: 10px 12px; border-radius: 8px; background: color-mix(in srgb, var(--mud-palette-primary) 8%, transparent); font-size: 0.875rem; font-variant-numeric: tabular-nums; line-height: 1.5; }
.debt-balance-preview > span { display: block; margin-bottom: 3px; color: var(--mud-palette-text-secondary); font-size: 0.75rem; }

@media (max-width: 599.98px) {
  .mud-dialog.debt-dialog {
    width: 100%;
    max-width: none;
    max-height: calc(var(--viewport-height, 100dvh) * 0.85);
    margin: auto 0 var(--keyboard-inset, 0px);
    border-radius: 12px 12px 0 0;
  }
  .debt-dialog .mud-dialog-title { padding: 16px; }
  .debt-dialog .mud-dialog-content { padding: 0 16px 16px; }
  .debt-dialog .mud-dialog-actions { padding: 12px 16px max(12px, env(safe-area-inset-bottom)); }
  .debt-form-fields { gap: 16px; }
}
</style>

