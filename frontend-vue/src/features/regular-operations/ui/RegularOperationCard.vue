<script setup lang="ts">
import { computed } from 'vue'
import { formatMoneyPlain } from '@/platform/format'
import {
  mdCalendarToday,
  mdDelete,
  mdEdit,
  mdRestore,
  mdWarningAmber,
} from '@/shared/icons'
import {
  MudCard,
  MudCardActions,
  MudCardContent,
  MudCardHeader,
  MudChip,
  MudDivider,
  MudIcon,
  MudIconButton,
  MudSpacer,
  MudText,
  MudTooltip,
} from '@/shared/mud'
import RecordRow from '@/shared/ui/RecordRow.vue'
import { operationRoleVariables } from '@/shared/ui/record-card'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { RegularOperationKind, RegularOperationView } from '../model/regular-operation-list'
import { categoryNameOf, formatRunTime, regularOperationKind, scheduleText } from '../model/regular-operation-list'

const TONE_CLASS: Record<RegularOperationKind, string | null> = {
  expense: null,
  income: null,
  deleted: 'record-card--muted',
}

const props = defineProps<{ operation: RegularOperationView }>()

const emit = defineEmits<{
  edit: [view: RegularOperationView]
  remove: [view: RegularOperationView]
  restore: [view: RegularOperationView]
}>()

const kind = computed(() => regularOperationKind(props.operation))
const categoryName = computed(() => categoryNameOf(props.operation))
const runTime = computed(() => formatRunTime(props.operation.runTime))
const role = computed(() => operationRoleVariables(props.operation.category?.operationTypeId ?? null))
const schedule = computed(() => scheduleText(props.operation))
const isNarrow = useNarrowViewport()
</script>

<template>
  <RecordRow
    v-if="isNarrow"
    :data-testid="`regular-operations-card-${operation.id}`"
    :deleted="operation.isDeleted"
    :edit-label="`Изменить регулярную операцию ${operation.name}, ${categoryName}, ${schedule}`"
    :edit-test-id="`regular-operations-edit-${operation.id}`"
    :operation-type-id="operation.category?.operationTypeId ?? null"
    :restore-label="`Восстановить регулярную операцию ${operation.name}, ${categoryName}`"
    :restore-test-id="`regular-operations-restore-${operation.id}`"
    :sum="operation.sum"
    :sum-test-id="`regular-operations-sum-${operation.id}`"
    @edit="emit('edit', operation)"
    @restore="emit('restore', operation)"
  >
    <template #title>
      <MudIcon
        v-if="operation.category === null"
        :icon="mdWarningAmber"
        color="warning"
        size="small"
        :title="categoryName"
      />
      <span :data-testid="`regular-operations-name-${operation.id}`">{{ operation.name }}</span>
    </template>

    <template #meta>
      <span :data-testid="`regular-operations-run-time-${operation.id}`">{{ schedule }}</span>
    </template>
  </RecordRow>

  <MudCard
    v-else
    :class="[`regular-operation-card--${kind}`, TONE_CLASS[kind]]"
    :data-testid="`regular-operations-card-${operation.id}`"
    :style="role"
    class="regular-operation-card record-card d-flex flex-column"
  >
    <MudCardHeader
      :data-testid="`regular-operations-name-${operation.id}`"
      style="padding: 4px 6px;"
    >
      <MudTooltip
        root-class="mud-card-header-text record-card-title"
        :text="operation.name"
      >
        <MudText
          class="mud-card-header-text record-card-title"
          :title="operation.name"
          typo="h6"
        >
          {{ operation.name }}
        </MudText>
      </MudTooltip>
    </MudCardHeader>

    <MudCardContent style="padding: 4px 6px">
      <MudTooltip
        arrow
        placement="right"
        text="Категория"
      >
        <MudText
          :class="{ 'record-card-muted-text': operation.category === null }"
          :data-testid="`regular-operations-category-${operation.id}`"
          class="d-flex align-center gap-1"
          html-tag="div"
          title="Категория"
          typo="subtitle2"
        >
          <MudIcon
            v-if="operation.category === null"
            :icon="mdWarningAmber"
            color="warning"
            size="small"
          />
          <span class="record-card-truncate">{{ categoryName }}</span>
        </MudText>
      </MudTooltip>

      <MudDivider class="mb-1" />

      <MudTooltip
        arrow
        placement="right"
        text="Сумма"
      >
        <MudText
          :data-testid="`regular-operations-sum-${operation.id}`"
          html-tag="div"
          title="Сумма"
          typo="body2"
        >
          {{ formatMoneyPlain(operation.sum) }}
        </MudText>
      </MudTooltip>

      <template v-if="operation.place !== null">
        <MudDivider class="mb-1" />

        <MudTooltip
          arrow
          placement="right"
          text="Место"
        >
          <MudText
            :data-testid="`regular-operations-place-${operation.id}`"
            html-tag="div"
            title="Место"
            typo="body2"
          >
            {{ operation.place }}
          </MudText>
        </MudTooltip>
      </template>

      <template v-if="operation.comment !== null">
        <MudDivider class="mb-1" />

        <MudTooltip
          arrow
          placement="right"
          text="Комментарий"
        >
          <MudText
            :data-testid="`regular-operations-comment-${operation.id}`"
            html-tag="div"
            title="Комментарий"
            typo="body2"
          >
            {{ operation.comment }}
          </MudText>
        </MudTooltip>
      </template>
    </MudCardContent>

    <MudCardActions
      class="mt-auto"
      style="padding: 4px 2px"
    >
      <MudTooltip text="Следующий запуск">
        <MudChip
          :data-testid="`regular-operations-run-time-${operation.id}`"
          :icon="mdCalendarToday"
          :text="runTime"
          size="small"
          title="Следующий запуск"
        />
      </MudTooltip>

      <MudSpacer />

      <MudTooltip
        v-if="operation.isDeleted"
        text="Восстановить"
      >
        <MudIconButton
          :data-testid="`regular-operations-restore-${operation.id}`"
          :icon="mdRestore"
          aria-label="Восстановить"
          color="success"
          size="small"
          title="Восстановить"
          @click="emit('restore', operation)"
        />
      </MudTooltip>

      <template v-else>
        <MudTooltip text="Редактировать">
          <MudIconButton
            :data-testid="`regular-operations-edit-${operation.id}`"
            :icon="mdEdit"
            aria-label="Редактировать"
            color="warning"
            size="small"
            title="Редактировать"
            @click="emit('edit', operation)"
          />
        </MudTooltip>

        <MudTooltip text="Удалить">
          <MudIconButton
            :data-testid="`regular-operations-delete-${operation.id}`"
            :icon="mdDelete"
            aria-label="Удалить"
            size="small"
            title="Удалить"
            @click="emit('remove', operation)"
          />
        </MudTooltip>
      </template>
    </MudCardActions>
  </MudCard>
</template>

<style scoped src="@/shared/ui/record-card.css"></style>

<style scoped>
.record-card-title,
.record-card-truncate {
  overflow: hidden;
  max-width: 100%;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.record-card-muted-text {
  color: var(--mud-palette-text-secondary);
}
</style>
