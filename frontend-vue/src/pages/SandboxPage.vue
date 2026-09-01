<script setup lang="ts">
import { shallowRef } from 'vue'
import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'
import { formatLocalDate, formatMoney } from '@/platform/format'
import { MudCard, MudCardContent, MudCardHeader, MudText } from '@/shared/mud'
import SmartDatePicker from '@/shared/ui/SmartDatePicker.vue'
import SmartSum from '@/shared/ui/SmartSum.vue'

const sum = shallowRef<Money | null>(null)
const date = shallowRef<LocalDate | null>(null)
</script>

<template>
  <div>
    <MudCard
      class="mx-auto"
      style="max-width: 480px;"
    >
      <MudCardHeader>
        <MudText typo="h6">
          Песочница контролов
        </MudText>
      </MudCardHeader>
      <MudCardContent>
        <SmartSum v-model="sum" />
        <MudText
          html-tag="p"
          data-testid="sum-preview"
        >
          {{ sum === null ? 'сумма не задана' : formatMoney(sum) }}
        </MudText>

        <SmartDatePicker v-model="date" />
        <MudText
          html-tag="p"
          data-testid="date-preview"
        >
          {{ date === null ? 'дата не задана' : formatLocalDate(date) }}
        </MudText>
      </MudCardContent>
    </MudCard>
  </div>
</template>
