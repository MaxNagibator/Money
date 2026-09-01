<script setup lang="ts">
import { computed } from 'vue'
import { UNNAMED_DEBT_OWNER } from '@/api/adapters/debts'
import type { Debt } from '@/domain/debt'
import { formatLocalDate, formatMoneyWhole } from '@/platform/format'
import {
  mdArrowCircleUp,
  mdAttachMoney,
  mdCalendarToday,
  mdListAlt,
} from '@/shared/icons'
import {
  MudChip,
  MudGrid,
  MudItem,
  MudPaper,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'
import type { DebtOwnerGroup } from '../model/debt'
import { lastPaymentDate, NO_PAYMENT_MARK, ownerPercent, ownerRemainder, ownerTotal } from '../model/debt'
import type { PaymentFormValue } from '../model/payment-form'
import DebtCard from './DebtCard.vue'

const props = defineProps<{
  owner: DebtOwnerGroup
  isSelectable: (debt: Debt) => boolean
  selectedIds: ReadonlySet<number>
  paymentValue: PaymentFormValue | null
  isPaying: boolean
}>()

const emit = defineEmits<{
  'toggle-selection': [debt: Debt]
  'edit': [debt: Debt]
  'remove': [debt: Debt]
  'restore': [debt: Debt]
  'open-payment': [debt: Debt]
  'cancel-payment': [debt: Debt]
  'submit-payment': [value: PaymentFormValue]
}>()

const title = computed(() => props.owner.name.trim().length === 0 ? UNNAMED_DEBT_OWNER : props.owner.name)
const total = computed(() => ownerTotal(props.owner))
const remainder = computed(() => ownerRemainder(props.owner))
const percent = computed(() => ownerPercent(props.owner))
const lastPayment = computed(() => lastPaymentDate(props.owner))
</script>

<template>
  <div :data-testid="`debts-owner-${owner.name}`">
    <MudPaper
      class="pa-2"
      :elevation="1"
    >
      <MudStack
        align-items="center"
        row
        :spacing="1"
        wrap="wrap"
      >
        <MudText
          class="debt-owner__title"
          html-tag="h3"
          typo="h6"
        >
          {{ title }}
        </MudText>

        <MudSpacer />

        <MudChip
          :icon="mdListAlt"
          :text="String(owner.debts.length)"
          data-testid="debts-owner-count"
          size="small"
          title="Количество долгов"
          variant="filled"
        />

        <MudChip
          :icon="mdCalendarToday"
          :text="lastPayment === null ? NO_PAYMENT_MARK : formatLocalDate(lastPayment)"
          data-testid="debts-owner-last-payment"
          size="small"
          title="Последний платеж"
          variant="filled"
        />

        <MudChip
          :icon="mdAttachMoney"
          :text="`${formatMoneyWhole(total)}/${formatMoneyWhole(remainder)}`"
          data-testid="debts-owner-money"
          size="small"
          title="Всего/Остаток"
          variant="filled"
        />

        <MudChip
          :icon="mdArrowCircleUp"
          :text="`${percent} %`"
          data-testid="debts-owner-percent"
          size="small"
          title="Процент погашения"
          variant="filled"
        />
      </MudStack>
    </MudPaper>

    <MudGrid
      class="my-1"
      :spacing="1"
    >
      <MudItem
        v-for="debt in owner.debts"
        :key="debt.id"
        class="mb-3"
        :lg="3"
        :md="4"
        :xs="12"
      >
        <DebtCard
          :debt="debt"
          :is-paying="isPaying"
          :is-payment-open="paymentValue !== null && paymentValue.debtId === debt.id"
          :is-selectable="isSelectable(debt)"
          :is-selected="selectedIds.has(debt.id)"
          :payment-value="paymentValue"
          @cancel-payment="emit('cancel-payment', $event)"
          @edit="emit('edit', $event)"
          @open-payment="emit('open-payment', $event)"
          @remove="emit('remove', $event)"
          @restore="emit('restore', $event)"
          @submit-payment="emit('submit-payment', $event)"
          @toggle-selection="emit('toggle-selection', $event)"
        />
      </MudItem>
    </MudGrid>
  </div>
</template>

<style scoped>
.debt-owner__title {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
