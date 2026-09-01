<script setup lang="ts">
import {
  RegularOperationCard,
  RegularOperationDialog,
  RegularOperationsToolbar,
  useRegularOperationsScreen,
} from '@/features/regular-operations'
import { mdRepeat } from '@/shared/icons'
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

const screen = useRegularOperationsScreen()
</script>

<template>
  <MudText
    gutter-bottom
    typo="h3"
  >
    Регулярные операции
  </MudText>

  <RegularOperationsToolbar @create="screen.startCreate($event)" />

  <MudDivider class="my-4" />

  <MudProgressLinear
    v-if="screen.isLoading.value"
    class="my-4"
    color="tertiary"
    data-testid="regular-operations-loading"
    indeterminate
  />

  <MudAlert
    v-else-if="screen.loadError.value !== null"
    data-testid="regular-operations-error"
    severity="error"
  >
    <span>{{ screen.loadError.value }}</span>
    <MudButton
      class="ms-2"
      data-testid="regular-operations-retry"
      @click="screen.reload()"
    >
      Повторить
    </MudButton>
  </MudAlert>

  <MudPaper
    v-else-if="screen.isEmpty.value"
    class="pa-6 d-flex flex-column align-center"
    data-testid="regular-operations-empty"
    :elevation="0"
  >
    <MudIcon
      :icon="mdRepeat"
      color="default"
      size="large"
    />
    <MudText
      class="mt-2"
      color="secondary"
      typo="body1"
    >
      Регулярных операций пока нет. Добавьте первую.
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
      <RegularOperationCard
        :operation="operation"
        @edit="screen.startEdit($event)"
        @remove="screen.remove($event)"
        @restore="screen.restore($event)"
      />
    </MudItem>
  </MudGrid>

  <RegularOperationDialog
    :categories="screen.dialogCategories.value"
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
    data-testid="regular-operations-notice"
    @update:model-value="screen.notice.value = null"
  />
</template>
