<script setup lang="ts">
import type { CarEvent } from '@/domain/car'
import { mdAdd } from '@/shared/icons'
import { MudGrid, MudIconButton, MudItem, MudProgressCircular, MudText } from '@/shared/mud'
import { pinLeavingItem, unpinItem } from '@/shared/ui/list-flow'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import CarEventCard from './CarEventCard.vue'

defineProps<{
  events: readonly CarEvent[]
  isLoading: boolean
}>()

const emit = defineEmits<{
  create: []
  edit: [event: CarEvent]
  remove: [event: CarEvent]
  restore: [event: CarEvent]
}>()

const isNarrow = useNarrowViewport()
</script>

<template>
  <section>
    <div class="car-events-header d-flex align-center gap-2 mb-2">
      <MudText>
        События
      </MudText>

      <MudIconButton
        :icon="mdAdd"
        aria-label="Добавить событие"
        data-testid="cars-event-add"
        title="Добавить событие"
        variant="outlined"
        @click="emit('create')"
      />
    </div>

    <div
      v-if="isLoading"
      class="car-events-loading d-flex align-center gap-3"
      data-testid="cars-events-loading"
    >
      <MudProgressCircular
        indeterminate
        size="small"
      />
      <MudText typo="body1">
        Загрузка..
      </MudText>
    </div>

    <MudText
      v-else-if="events.length === 0"
      class="car-events-empty"
      color="secondary"
      data-testid="cars-events-empty"
      typo="body2"
    >
      Событий пока нет. Нажмите «+», чтобы добавить первое.
    </MudText>

    <div
      v-else-if="isNarrow"
      class="list-flow-host"
      data-testid="cars-event-rows"
    >
      <TransitionGroup
        name="list-flow"
        @before-enter="unpinItem"
        @before-leave="pinLeavingItem"
      >
        <CarEventCard
          v-for="event in events"
          :key="event.id"
          :event="event"
          @edit="emit('edit', $event)"
          @remove="emit('remove', $event)"
          @restore="emit('restore', $event)"
        />
      </TransitionGroup>
    </div>

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
          v-for="event in events"
          :key="event.id"
          class="mb-1"
          :lg="3"
          :md="4"
          :xs="12"
        >
          <CarEventCard
            :event="event"
            @edit="emit('edit', $event)"
            @remove="emit('remove', $event)"
            @restore="emit('restore', $event)"
          />
        </MudItem>
      </TransitionGroup>
    </MudGrid>
  </section>
</template>

<style scoped>
@media (max-width: 599.98px) {
  .car-events-header,
  .car-events-loading,
  .car-events-empty {
    padding: 0 12px;
  }
}
</style>
