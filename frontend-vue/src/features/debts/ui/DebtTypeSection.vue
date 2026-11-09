<script setup lang="ts">
import { computed } from 'vue'
import type { Debt } from '@/domain/debt'
import { formatMoney } from '@/platform/format'
import { mdExpandLess, mdExpandMore, mdLabel } from '@/shared/icons'
import {
  MudCollapse,
  MudIcon,
  MudIconButton,
  MudSpacer,
  MudStack,
  MudText,
  MudTooltip,
} from '@/shared/mud'
import { pinLeavingItem, unpinItem } from '@/shared/ui/list-flow'
import type { DebtTypeGroup } from '../model/debt'
import { typeTotal } from '../model/debt'
import type { PaymentFormValue } from '../model/payment-form'
import DebtOwnerPanel from './DebtOwnerPanel.vue'

const props = defineProps<{
  group: DebtTypeGroup
  isExpanded: boolean
  isSelectable: (debt: Debt) => boolean
  selectedIds: ReadonlySet<number>
  paymentValue: PaymentFormValue | null
  isPaying: boolean
}>()

const emit = defineEmits<{
  'toggle': [typeId: number]
  'toggle-selection': [debt: Debt]
  'edit': [debt: Debt]
  'remove': [debt: Debt]
  'restore': [debt: Debt]
  'open-payment': [debt: Debt]
  'cancel-payment': [debt: Debt]
  'submit-payment': [value: PaymentFormValue]
}>()

const total = computed(() => typeTotal(props.group))
</script>

<template>
  <section :data-testid="`debts-type-${group.typeId}`">
    <MudStack
      align-items="center"
      row
    >
      <MudIcon :icon="mdLabel" />

      <MudText
        class="mt-3 mb-2"
        html-tag="h2"
        typo="h5"
      >
        {{ group.name }}
      </MudText>

      <MudSpacer />

      <MudTooltip text="Общая сумма">
        <MudText
          :data-testid="`debts-type-total-${group.typeId}`"
          html-tag="span"
          typo="subtitle2"
        >
          {{ formatMoney(total) }}
        </MudText>
      </MudTooltip>

      <MudIconButton
        :aria-expanded="isExpanded"
        :aria-label="isExpanded ? 'Свернуть раздел' : 'Развернуть раздел'"
        :data-testid="`debts-type-toggle-${group.typeId}`"
        :icon="isExpanded ? mdExpandLess : mdExpandMore"
        @click="emit('toggle', group.typeId)"
      />
    </MudStack>

    <MudCollapse :expanded="isExpanded">
      <MudStack
        v-if="isExpanded"
        class="list-flow-host"
        :spacing="2"
      >
        <TransitionGroup
          name="list-flow"
          @before-enter="unpinItem"
          @before-leave="pinLeavingItem"
        >
          <DebtOwnerPanel
            v-for="owner in group.owners"
            :key="owner.name"
            :is-paying="isPaying"
            :is-selectable="isSelectable"
            :owner="owner"
            :payment-value="paymentValue"
            :selected-ids="selectedIds"
            @cancel-payment="emit('cancel-payment', $event)"
            @edit="emit('edit', $event)"
            @open-payment="emit('open-payment', $event)"
            @remove="emit('remove', $event)"
            @restore="emit('restore', $event)"
            @submit-payment="emit('submit-payment', $event)"
            @toggle-selection="emit('toggle-selection', $event)"
          />
        </TransitionGroup>
      </MudStack>
    </MudCollapse>
  </section>
</template>
