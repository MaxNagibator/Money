<script setup lang="ts">
import type { CarEvent } from '@/domain/car'
import { mdAdd } from '@/shared/icons'
import { MudGrid, MudIconButton, MudItem, MudProgressCircular, MudText } from '@/shared/mud'
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
</script>

<template>
  <section>
    <div class="d-flex align-center gap-2 mb-2">
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
      class="d-flex align-center gap-3"
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
      color="secondary"
      data-testid="cars-events-empty"
      typo="body2"
    >
      Событий пока нет. Нажмите «+», чтобы добавить первое.
    </MudText>

    <MudGrid
      v-else
      :spacing="2"
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
    </MudGrid>
  </section>
</template>
