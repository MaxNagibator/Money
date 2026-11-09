<script setup lang="ts">
import { useWindowSize } from '@vueuse/core'
import { computed } from 'vue'
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
  MudSkeleton,
  MudSnackbar,
  MudText,
} from '@/shared/mud'
import { pinLeavingItem, unpinItem } from '@/shared/ui/list-flow'
import { GRID_CARD_ESTIMATE, GRID_SKELETON_HEIGHT, resolveGridSkeletonCount } from '@/shared/ui/loading-skeletons'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'

const screen = useFastOperationsScreen()

const isNarrow = useNarrowViewport()

const { height: viewportHeight, width: viewportWidth } = useWindowSize()

const skeletonCount = computed(() => resolveGridSkeletonCount(viewportWidth.value, viewportHeight.value, GRID_CARD_ESTIMATE))
</script>

<template>
  <div class="page-header">
    <MudText
      gutter-bottom
      typo="h3"
    >
      Быстрые операции
    </MudText>

    <FastOperationsToolbar @create="screen.startCreate($event)" />
  </div>

  <MudDivider class="my-4" />

  <MudGrid
    v-if="screen.isLoading.value"
    aria-busy="true"
    aria-label="Загрузка шаблонов операций"
    data-testid="fast-operations-loading"
    role="status"
    :spacing="2"
  >
    <MudItem
      v-for="index in skeletonCount"
      :key="index"
      class="mb-1"
      :lg="3"
      :md="4"
      :xs="12"
    >
      <MudSkeleton
        :height="`${GRID_SKELETON_HEIGHT}px`"
        skeleton-type="rectangle"
      />
    </MudItem>
  </MudGrid>

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

  <MudPaper
    v-else-if="isNarrow"
    class="list-flow-host overflow-hidden"
    data-testid="fast-operations-rows"
    :elevation="1"
  >
    <TransitionGroup
      name="list-flow"
      @before-enter="unpinItem"
      @before-leave="pinLeavingItem"
    >
      <FastOperationCard
        v-for="operation in screen.operations.value"
        :key="operation.id"
        :operation="operation"
        @edit="screen.startEdit($event)"
        @remove="screen.remove($event)"
        @restore="screen.restore($event)"
      />
    </TransitionGroup>
  </MudPaper>

  <MudGrid
    v-else
    class="list-flow-host"
    :spacing="2"
  >
    <TransitionGroup
      name="list-flow"
      @before-enter="unpinItem"
      @before-leave="pinLeavingItem"
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
    </TransitionGroup>
  </MudGrid>

  <FastOperationDialog
    :categories="screen.categories.value"
    :is-saving="screen.isSaving.value"
    :title="screen.dialogTitle.value"
    :value="screen.form.value"
    removable
    @cancel="screen.cancel()"
    @remove="screen.removeEdited()"
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

<style scoped>
@media (max-width: 599.98px) {
  .page-header {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 12px;
  }

  .page-header > :first-child {
    flex: 1 1 auto;
    min-width: 0;
  }

  .page-header > * {
    margin-bottom: 0 !important;
  }
}
</style>
