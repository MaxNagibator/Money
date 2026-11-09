<script setup lang="ts">
import { computed } from 'vue'
import type { DebtOwner } from '@/domain/debt'
import { mdInput, mdOutput, mdSwapHoriz } from '@/shared/icons'
import {
  MudAlert,
  MudButton,
  MudIconButton,
  MudPaper,
  MudProgressCircular,
  MudProgressLinear,
  MudSelect,
  MudSelectItem,
  MudSpacer,
  MudStack,
  MudText,
} from '@/shared/mud'

const props = defineProps<{
  owners: readonly DebtOwner[]
  isLoading: boolean
  isMerging: boolean
  loadError: string | null
}>()

const emit = defineEmits<{
  'swap': []
  'submit': []
  'close': []
}>()

const fromId = defineModel<number | null>('fromId', { default: null })
const toId = defineModel<number | null>('toId', { default: null })

const canMerge = computed(() => fromId.value !== null && toId.value !== null && fromId.value !== toId.value)
const fromName = computed(() => props.owners.find(owner => owner.id === fromId.value)?.name ?? '')
const toName = computed(() => props.owners.find(owner => owner.id === toId.value)?.name ?? '')
const title = computed(() => `Объединить долги ${fromName.value} с долгами ${toName.value}`)
</script>

<template>
  <MudPaper
    class="debt-merge"
    data-testid="debts-merge"
    :elevation="1"
  >
    <MudStack
      class="pa-3"
      :spacing="2"
    >
      <MudText>
        {{ title }}
      </MudText>

      <MudAlert
        v-if="loadError !== null"
        data-testid="debts-merge-error"
        dense
        role="alert"
        severity="error"
      >
        {{ loadError }}
      </MudAlert>

      <MudProgressLinear
        v-if="isLoading"
        color="tertiary"
        indeterminate
      />

      <MudSelect
        v-model="fromId"
        :adornment-icon="mdOutput"
        adornment="start"
        adornment-color="tertiary"
        clearable
        data-testid="debts-merge-from"
        helper-text="Кто удалится"
        label="Сливаемый должник"
      >
        <MudSelectItem
          v-for="owner in owners"
          :key="owner.id"
          :disabled="owner.id === toId"
          :value="owner.id"
        >
          {{ owner.name }}
        </MudSelectItem>
      </MudSelect>

      <MudSelect
        v-model="toId"
        :adornment-icon="mdInput"
        adornment="start"
        adornment-color="tertiary"
        clearable
        data-testid="debts-merge-to"
        helper-text="Кто останется"
        label="Поглощающий должник"
      >
        <MudSelectItem
          v-for="owner in owners"
          :key="owner.id"
          :disabled="owner.id === fromId"
          :value="owner.id"
        >
          {{ owner.name }}
        </MudSelectItem>
      </MudSelect>

      <MudStack
        align-items="center"
        class="mt-3"
        row
      >
        <MudButton
          color="error"
          data-testid="debts-merge-close"
          @click="emit('close')"
        >
          Закрыть
        </MudButton>

        <MudIconButton
          :icon="mdSwapHoriz"
          aria-label="Поменять держателей местами"
          color="tertiary"
          data-testid="debts-merge-swap"
          size="small"
          title="Поменять местами"
          @click="emit('swap')"
        />

        <MudSpacer />

        <MudButton
          :disabled="!canMerge || isMerging"
          color="tertiary"
          data-testid="debts-merge-submit"
          variant="filled"
          @click="emit('submit')"
        >
          <MudProgressCircular
            v-if="isMerging"
            indeterminate
            size="small"
          />
          <template v-else>
            Объединить
          </template>
        </MudButton>
      </MudStack>
    </MudStack>
  </MudPaper>
</template>

<style scoped>
.debt-merge {
  width: min(380px, 92vw);
}
</style>
