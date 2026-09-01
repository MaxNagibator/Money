<script setup lang="ts">
import { computed } from 'vue'
import type { CarEvent } from '@/domain/car'
import { formatLocalDate, formatMileage } from '@/platform/format'
import {
  mdCalendarToday,
  mdDelete,
  mdDirectionsCar,
  mdEdit,
  mdRestore,
} from '@/shared/icons'
import {
  MudCard,
  MudCardActions,
  MudCardContent,
  MudChip,
  MudDivider,
  MudIcon,
  MudIconButton,
  MudImage,
  MudSpacer,
  MudStack,
  MudText,
  MudTooltip,
} from '@/shared/mud'
import { carEventType } from '../model/car-event-type'

const props = defineProps<{ event: CarEvent }>()

const emit = defineEmits<{
  edit: [event: CarEvent]
  remove: [event: CarEvent]
  restore: [event: CarEvent]
}>()

const type = computed(() => carEventType(props.event.typeId))
const hasTitle = computed(() => (props.event.title ?? '') !== '')
const hasComment = computed(() => (props.event.comment ?? '') !== '')
</script>

<template>
  <MudCard
    :class="event.isDeleted ? 'car-event-card car-event-card--deleted' : 'car-event-card'"
    :data-testid="`cars-event-card-${event.id}`"
    class="d-flex flex-column"
    :elevation="2"
  >
    <MudCardContent style="padding: 4px 6px;">
      <MudStack
        row
        :spacing="2"
      >
        <MudTooltip :text="type.name">
          <MudImage
            :alt="type.name"
            class="flex-grow-0"
            :height="64"
            :src="type.icon"
            :width="64"
          />
        </MudTooltip>

        <MudStack
          :spacing="2"
          class="overflow-hidden"
        >
          <div
            v-if="hasTitle"
            class="d-flex"
            :data-testid="`cars-event-title-${event.id}`"
          >
            <MudTooltip
              placement="bottom"
              text="Название"
            >
              <MudText typo="subtitle2">
                {{ event.title }}
              </MudText>
            </MudTooltip>
          </div>

          <MudTooltip
            v-if="event.mileage !== null"
            placement="bottom"
            text="Пробег"
          >
            <MudStack
              row
              align-items="center"
              :data-testid="`cars-event-mileage-${event.id}`"
            >
              <MudIcon
                :icon="mdDirectionsCar"
                color="primary"
                size="small"
              />
              <MudText>
                {{ formatMileage(event.mileage) }} км
              </MudText>
            </MudStack>
          </MudTooltip>
        </MudStack>
      </MudStack>

      <MudDivider flex-item />

      <MudTooltip
        v-if="hasComment"
        placement="bottom"
        text="Комментарий"
      >
        <MudText
          :data-testid="`cars-event-comment-${event.id}`"
          align="center"
          class="mt-2"
          html-tag="div"
          title="Комментарий"
          typo="body2"
        >
          {{ event.comment }}
        </MudText>
      </MudTooltip>
    </MudCardContent>

    <MudCardActions style="padding: 4px 2px;">
      <MudTooltip text="Дата">
        <MudChip
          :data-testid="`cars-event-date-${event.id}`"
          :icon="mdCalendarToday"
          :text="formatLocalDate(event.date)"
          size="small"
          title="Дата"
        />
      </MudTooltip>

      <MudSpacer />

      <MudTooltip
        v-if="event.isDeleted"
        text="Восстановить"
      >
        <MudIconButton
          :data-testid="`cars-event-restore-${event.id}`"
          :icon="mdRestore"
          aria-label="Восстановить"
          color="success"
          size="small"
          title="Восстановить"
          @click="emit('restore', event)"
        />
      </MudTooltip>

      <template v-else>
        <MudTooltip text="Редактировать">
          <MudIconButton
            :data-testid="`cars-event-edit-${event.id}`"
            :icon="mdEdit"
            aria-label="Редактировать"
            color="warning"
            size="small"
            title="Редактировать"
            @click="emit('edit', event)"
          />
        </MudTooltip>

        <MudTooltip text="Удалить">
          <MudIconButton
            :data-testid="`cars-event-delete-${event.id}`"
            :icon="mdDelete"
            aria-label="Удалить"
            size="small"
            title="Удалить"
            @click="emit('remove', event)"
          />
        </MudTooltip>
      </template>
    </MudCardActions>
  </MudCard>
</template>

<style scoped>
.car-event-card {
  height: 100%;
  border: 1px solid var(--mud-palette-lines-default);
  border-radius: 8px;
  background-color: var(--mud-palette-surface);
  transition: background-color 0.2s ease-in-out, box-shadow 0.2s ease-in-out, opacity 0.2s ease-in-out;
}

.car-event-card:hover {
  background-color: var(--mud-palette-action-default-hover);
}

.car-event-card--deleted {
  border: 1px dashed var(--mud-palette-error);
  opacity: 0.7;
  background-color: var(--mud-palette-background-gray);
}

.car-event-card--deleted:hover {
  opacity: 0.9;
  background-color: var(--mud-palette-background-gray);
}
</style>
