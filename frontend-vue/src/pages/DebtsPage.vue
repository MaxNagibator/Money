<script setup lang="ts">
import { useCategories } from '@/features/categories'
import {
  DEBT_TYPES,
  DebtDialog,
  DebtForgivePanel,
  DebtMergePanel,
  DebtTypeSection,
  useDebtsScreen,
} from '@/features/debts'
import {
  mdAdd,
  mdHandshake,
  mdMerge,
  mdSearch,
  mdVisibility,
  mdVisibilityOff,
} from '@/shared/icons'
import {
  MudAlert,
  MudButton,
  MudDivider,
  MudGrid,
  MudItem,
  MudPopover,
  MudProgressCircular,
  MudSnackbar,
  MudSpacer,
  MudStack,
  MudText,
  MudTextField,
  MudToggleIconButton,
  MudTooltip,
} from '@/shared/mud'

const categoriesState = useCategories()
const screen = useDebtsScreen(categoriesState.categories)
</script>

<template>
  <MudStack
    align-items="center"
    class="mb-3"
    row
    :spacing="1"
  >
    <MudText
      html-tag="h1"
      typo="h3"
    >
      Долги
    </MudText>

    <MudSpacer />

    <MudTooltip :text="screen.showPaid.value ? 'Скрыть погашенные долги' : 'Показать погашенные долги'">
      <MudToggleIconButton
        :toggled="screen.showPaid.value"
        :aria-label="screen.showPaid.value ? 'Скрыть погашенные долги' : 'Показать погашенные долги'"
        color="tertiary"
        data-testid="debts-toggle-paid"
        :icon="screen.showPaid.value ? mdVisibilityOff : mdVisibility"
        toggled-variant="outlined"
        variant="filled"
        @update:toggled="screen.showPaid.value = $event"
      />
    </MudTooltip>

    <MudTooltip text="Простить долги">
      <MudToggleIconButton
        :toggled="screen.isForgiveOpen.value"
        aria-label="Простить долги"
        color="tertiary"
        data-testid="debts-toggle-forgive"
        :icon="mdHandshake"
        toggled-variant="outlined"
        variant="filled"
        @update:toggled="screen.openForgive($event)"
      />
    </MudTooltip>

    <MudTooltip text="Слияние держателей">
      <MudToggleIconButton
        :toggled="screen.isMergeOpen.value"
        aria-label="Объединить держателей"
        color="tertiary"
        data-testid="debts-toggle-merge"
        :icon="mdMerge"
        toggled-variant="outlined"
        variant="filled"
        @update:toggled="screen.openMerge($event)"
      />
    </MudTooltip>

    <MudPopover
      :open="screen.isForgiveOpen.value"
      anchor-origin="bottom-right"
      transform-origin="top-right"
    >
      <DebtForgivePanel
        v-model:category-id="screen.forgiveCategoryId.value"
        v-model:comment="screen.forgiveComment.value"
        :categories="screen.expenseCategories.value"
        :is-forgiving="screen.isForgiving.value"
        :selected-count="screen.selectedCount.value"
        @clear="screen.clearSelection()"
        @close="screen.openForgive(false)"
        @select-all="screen.selectAllVisible()"
        @submit="screen.forgive()"
      />
    </MudPopover>

    <MudPopover
      :open="screen.isMergeOpen.value"
      anchor-origin="bottom-right"
      transform-origin="top-right"
    >
      <DebtMergePanel
        v-model:from-id="screen.ownerFromId.value"
        v-model:to-id="screen.ownerToId.value"
        :is-loading="screen.owners.isLoading.value"
        :is-merging="screen.isMerging.value"
        :load-error="screen.owners.loadError.value"
        :owners="screen.owners.owners.value"
        @close="screen.openMerge(false)"
        @submit="screen.merge()"
        @swap="screen.swapOwners()"
      />
    </MudPopover>
  </MudStack>

  <MudGrid
    class="mb-4"
    :spacing="2"
  >
    <MudItem
      v-for="type in DEBT_TYPES"
      :key="type.id"
      :lg="2"
      :md="3"
      :xs="12"
    >
      <MudButton
        :data-testid="`debts-add-${type.id}`"
        :end-icon="mdAdd"
        full-width
        variant="filled"
        @click="screen.startCreate(type.id)"
      >
        {{ type.addLabel }}
      </MudButton>
    </MudItem>

    <MudItem
      :lg="2"
      :md="2"
      :xs="12"
    >
      <MudButton
        :end-icon="mdAdd"
        data-testid="debts-add"
        full-width
        variant="filled"
        @click="screen.startCreate()"
      >
        Добавить
      </MudButton>
    </MudItem>

    <MudItem
      :lg="6"
      :md="4"
      :xs="12"
    >
      <MudTextField
        :model-value="screen.search.value"
        :adornment-icon="mdSearch"
        adornment="start"
        clearable
        data-testid="debts-search"
        full-width
        immediate
        placeholder="Поиск..."
        @update:model-value="screen.search.value = $event ?? ''"
      />
    </MudItem>
  </MudGrid>

  <MudDivider class="my-4" />

  <div
    v-if="screen.debts.isLoading.value"
    aria-busy="true"
    class="d-flex align-center gap-3"
    data-testid="debts-loading"
    role="status"
  >
    <MudProgressCircular
      color="primary"
      indeterminate
      size="small"
    />
    <MudText typo="body2">
      Загрузка долгов..
    </MudText>
  </div>

  <MudAlert
    v-else-if="screen.debts.loadError.value !== null"
    data-testid="debts-error"
    role="alert"
    severity="error"
  >
    {{ screen.debts.loadError.value }}
  </MudAlert>

  <MudText
    v-else-if="screen.groups.value.length === 0"
    align="center"
    color="secondary"
    data-testid="debts-empty"
    html-tag="div"
    role="status"
    typo="h6"
  >
    Нет данных для отображения
  </MudText>

  <template v-else>
    <DebtTypeSection
      v-for="group in screen.groups.value"
      :key="group.typeId"
      :group="group"
      :is-expanded="screen.isTypeExpanded(group.typeId)"
      :is-paying="screen.isPaying.value"
      :is-selectable="screen.isSelectable"
      :payment-value="screen.payment.value"
      :selected-ids="screen.selectedIds.value"
      @cancel-payment="screen.cancelPayment($event)"
      @edit="screen.startEdit($event)"
      @open-payment="screen.startPayment($event)"
      @remove="screen.remove($event)"
      @restore="screen.restore($event)"
      @submit-payment="screen.submitPayment($event)"
      @toggle="screen.toggleType($event)"
      @toggle-selection="screen.toggleSelection($event)"
    />
  </template>

  <DebtDialog
    :is-saving="screen.isSaving.value"
    :value="screen.form.value"
    @cancel="screen.cancelEdit()"
    @submit="screen.submit($event)"
  />

  <MudSnackbar
    :message="screen.notice.value?.text ?? ''"
    :model-value="screen.notice.value !== null"
    :severity="screen.notice.value?.type ?? 'normal'"
    :visible-state-duration="4000"
    data-testid="debts-notice"
    @update:model-value="screen.notice.value = null"
  />
</template>
