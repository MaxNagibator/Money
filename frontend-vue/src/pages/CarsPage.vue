<script setup lang="ts">
import { computed } from 'vue'
import { CarAddMenu, CarEventDialog, CarEventsPanel, useCarsScreen } from '@/features/cars'
import { mdElectricCar } from '@/shared/icons'
import {
  MudAlert,
  MudIcon,
  MudPaper,
  MudProgressCircular,
  MudSnackbar,
  MudTabPanel,
  MudTabs,
  MudText,
} from '@/shared/mud'

const screen = useCarsScreen()

const activePanelIndex = computed({
  get: () => screen.cars.cars.value.findIndex(car => car.id === screen.cars.activeCarId.value),
  set: (index: number) => {
    const car = screen.cars.cars.value[index]

    if (car !== undefined) {
      screen.cars.activeCarId.value = car.id
    }
  },
})

const noticeVisible = computed({
  get: () => screen.notice.value !== null,
  set: (visible: boolean) => {
    if (!visible) {
      screen.notice.value = null
    }
  },
})
</script>

<template>
  <MudText
    gutter-bottom
    typo="h3"
  >
    Авто
  </MudText>

  <div
    v-if="screen.isInitializing.value"
    class="d-flex align-center gap-3"
    data-testid="cars-loading"
  >
    <MudProgressCircular indeterminate />
    <MudText>Загрузка..</MudText>
  </div>

  <MudAlert
    v-else-if="screen.cars.loadError.value !== null"
    data-testid="cars-error"
    severity="error"
    variant="text"
  >
    {{ screen.cars.loadError.value }}
  </MudAlert>

  <template v-else>
    <MudTabs
      v-model:active-panel-index="activePanelIndex"
      apply-effects-to-container
      :elevation="4"
      header-position="before"
      rounded
    >
      <template #header>
        <CarAddMenu
          v-model:is-open="screen.cars.isAddOpen.value"
          v-model:name="screen.cars.newCarName.value"
          :can-create="screen.cars.canCreate.value"
          :is-creating="screen.cars.isCreating.value"
          @submit="screen.createCar()"
        />
      </template>

      <MudPaper
        v-if="screen.cars.isEmpty.value"
        class="pa-6 d-flex flex-column align-center"
        data-testid="cars-empty"
        :elevation="0"
      >
        <MudIcon
          :icon="mdElectricCar"
          color="default"
          size="large"
        />
        <MudText
          class="mt-2"
          align="center"
          color="secondary"
          typo="body1"
        >
          Машинок пока нет. Нажмите «+» сверху, чтобы добавить первую.
        </MudText>
      </MudPaper>

      <MudTabPanel
        v-for="car in screen.cars.cars.value"
        :key="car.id"
        panel-class="px-4 py-6"
        :show-close-icon="false"
        :text="car.name"
        :tab-test-id="`cars-tab-${car.id}`"
      >
        <CarEventsPanel
          :events="screen.events.events.value"
          :is-loading="screen.events.isLoading.value"
          @create="screen.startCreateEvent()"
          @edit="screen.startEditEvent($event)"
          @remove="screen.removeEvent($event)"
          @restore="screen.restoreEvent($event)"
        />
      </MudTabPanel>
    </MudTabs>
  </template>

  <CarEventDialog
    :is-saving="screen.isSaving.value"
    :title="screen.dialogTitle.value"
    :top-mileage="screen.events.topMileage.value"
    :value="screen.form.value"
    @cancel="screen.cancelEvent()"
    @submit="screen.submitEvent($event)"
  />

  <MudSnackbar
    v-model="noticeVisible"
    :message="screen.notice.value?.text ?? ''"
    :severity="screen.notice.value?.type ?? 'normal'"
    data-testid="cars-notice"
  />
</template>
