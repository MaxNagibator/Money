<script setup lang="ts">
import { computed } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import { OPERATION_TYPES } from '@/domain/operation-type'
import { formatMoney } from '@/platform/format'
import type { MudColor } from '@/shared/mud'
import { MudIcon, MudPaper, MudSpacer, MudStack, MudText } from '@/shared/mud'
import { formatPeriod } from '../model/period'

const props = defineProps<{
  from: LocalDate | null
  to: LocalDate | null
  totals: Readonly<Record<OperationTypeId, Money>>
}>()

const periodText = computed(() => formatPeriod(props.from, props.to))

const amounts = computed(() => OPERATION_TYPES.map(type => ({
  type,
  color: type.color as MudColor,
  amount: formatMoney(props.totals[type.id]),
})))
</script>

<template>
  <MudPaper
    class="my-1 pa-2"
    data-testid="operations-period"
  >
    <MudStack
      row
      wrap="wrap"
    >
      <MudText data-testid="operations-period-range">
        {{ periodText }}
      </MudText>

      <MudSpacer />

      <MudStack
        v-for="entry in amounts"
        :key="entry.type.id"
        :data-testid="`operations-total-${entry.type.id}`"
        align-items="center"
        justify="end"
        row
        stretch-items="end"
      >
        <MudIcon
          :color="entry.color"
          :icon="entry.type.icon"
          :title="entry.type.name"
          size="small"
        />
        <MudText>
          {{ entry.amount }}
        </MudText>
      </MudStack>
    </MudStack>
  </MudPaper>
</template>
