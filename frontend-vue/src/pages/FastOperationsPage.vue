<script setup lang="ts">
import {
  FastOperationCard,
  FastOperationDialog,
  FastOperationsToolbar,
  useFastOperationsScreen,
} from '@/features/fast-operations'
import { mdFlashOn } from '@/shared/icons'
import {
  MudAlert,
  MudButton,
  MudDivider,
  MudGrid,
  MudIcon,
  MudItem,
  MudPaper,
  MudProgressLinear,
  MudSnackbar,
  MudText,
} from '@/shared/mud'

const screen = useFastOperationsScreen()
</script>

<template>
  <MudText
    gutter-bottom
    typo="h3"
  >
    Быстрые операции
  </MudText>

  <FastOperationsToolbar @create="screen.startCreate($event)" />

  <MudDivider class="my-4" />

  <MudProgressLinear
    v-if="screen.isLoading.value"
    class="my-4"
    color="tertiary"
    data-testid="fast-operations-loading"
    indeterminate
  />

  <MudAlert
    v-else-if="screen.loadError.value !== null"
    data-testid="fast-operations-error"
    severity="error"
  >
    <span>{{ screen.loadError.value }}</span>
    <MudButton
      class="ms-2"
      data-testid="fast-operations-retry"
      @click="screen.reload()"
    >
      Повторить
    </MudButton>
  </MudAlert>

  <MudPaper
    v-else-if="screen.isEmpty.value"
    class="pa-6 d-flex flex-column align-center"
    data-testid="fast-operations-empty"
    :elevation="0"
  >
    <MudIcon
      :icon="mdFlashOn"
      color="default"
      size="large"
    />
    <MudText
      class="mt-2"
      color="secondary"
      typo="body1"
    >
      Шаблонов пока нет. Добавьте первый.
    </MudText>
  </MudPaper>

  <MudGrid
    v-else
    :spacing="2"
  >
    <MudItem
      v-for="operation in screen.operations.value"
      :key="operation.id"
      class="mb-1"
      :lg="3"
      :md="4"
      :xs="12"
    >
      <FastOperationCard
        :operation="operation"
        @edit="screen.startEdit($event)"
        @remove="screen.remove($event)"
        @restore="screen.restore($event)"
      />
    </MudItem>
  </MudGrid>

  <FastOperationDialog
    :categories="screen.categories.value"
    :is-saving="screen.isSaving.value"
    :title="screen.dialogTitle.value"
    :value="screen.form.value"
    @cancel="screen.cancel()"
    @submit="screen.submit($event)"
  />

  <MudSnackbar
    :message="screen.notice.value?.text ?? ''"
    :model-value="screen.notice.value !== null"
    :severity="screen.notice.value?.type ?? 'normal'"
    data-testid="fast-operations-notice"
    @update:model-value="screen.notice.value = null"
  />
</template>
