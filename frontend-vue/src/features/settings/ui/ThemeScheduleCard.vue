<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import SmartTimePicker from '@/shared/ui/SmartTimePicker.vue'
import {
  MudAlert,
  MudButton,
  MudCard,
  MudCardContent,
  MudCardHeader,
  MudDivider,
  MudGrid,
  MudItem,
  MudText,
} from '@/shared/mud'
import type { ScheduleFormValue } from '../model/schedule-form'
import { scheduleFormFrom, validateScheduleForm } from '../model/schedule-form'
import { useThemeSettings } from '../model/useThemeSettings'
import SettingSwitch from './SettingSwitch.vue'
import './settings-card.css'

const theme = useThemeSettings()

const form = ref<ScheduleFormValue>(scheduleFormFrom(theme.schedule))
const error = ref('')
const notice = ref('')

const isScheduleDisabled = computed(() => !theme.schedule.isEnabled || theme.isManual.value)
const isIntervalDisabled = computed(() => isScheduleDisabled.value || !theme.schedule.isRecheckEnabled)

watch(() => theme.schedule.isEnabled, () => {
  form.value = scheduleFormFrom(theme.schedule)
})

function apply(): void {
  const result = validateScheduleForm(form.value)

  if (!result.ok) {
    error.value = result.error
    notice.value = ''
    return
  }

  theme.updateSchedule(result.patch)
  form.value = scheduleFormFrom(theme.schedule)
  error.value = ''
  notice.value = 'Расписание обновлено'
}
</script>

<template>
  <MudCard class="mb-6 settings-card">
    <MudCardHeader>
      <template #card-header-content>
        <MudText typo="h6">
          Расписание темной темы
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <MudText typo="body2">
        Включите расписание, чтобы в режиме «Авто» темная тема включалась в заданные часы.
      </MudText>

      <SettingSwitch
        :disabled="theme.isManual.value"
        :model-value="theme.schedule.isEnabled"
        data-testid="settings-schedule-enabled"
        label="Активировать расписание"
        @update:model-value="theme.updateSchedule({ isEnabled: $event })"
      />

      <SettingSwitch
        :disabled="isScheduleDisabled"
        :model-value="theme.schedule.isLocationChangedEnabled"
        data-testid="settings-location-changed"
        label="Активировать отслеживание перехода по страницам приложения (не вкладкам)"
        @update:model-value="theme.setLocationChanged($event)"
      />

      <MudGrid
        class="mt-4 settings-gap"
        :spacing="3"
      >
        <MudItem
          :xs="12"
          :sm="6"
        >
          <SmartTimePicker
            v-model="form.start"
            :disabled="isScheduleDisabled"
            hint="Выберите время начала темной темы"
            label="Время начала"
            test-id="settings-schedule-start"
          />
        </MudItem>
        <MudItem
          :xs="12"
          :sm="6"
        >
          <SmartTimePicker
            v-model="form.end"
            :disabled="isScheduleDisabled"
            hint="Выберите время окончания темной темы"
            label="Время окончания"
            test-id="settings-schedule-end"
          />
        </MudItem>
      </MudGrid>

      <MudDivider class="my-4" />

      <SettingSwitch
        :disabled="isScheduleDisabled"
        :model-value="theme.schedule.isRecheckEnabled"
        data-testid="settings-schedule-recheck"
        label="Активировать таймер"
        @update:model-value="theme.updateSchedule({ isRecheckEnabled: $event })"
      />

      <MudGrid
        class="mt-4 settings-gap"
        :spacing="3"
      >
        <MudItem :xs="12">
          <SmartTimePicker
            v-model="form.checkInterval"
            :disabled="isIntervalDisabled"
            hint="Выберите интервал проверки расписания"
            label="Интервал проверки"
            test-id="settings-schedule-interval"
          />
        </MudItem>
      </MudGrid>

      <MudDivider class="mb-6" />

      <MudButton
        :disabled="isScheduleDisabled"
        class="mt-4 settings-gap"
        color="primary"
        data-testid="settings-schedule-apply"
        variant="filled"
        @click="apply()"
      >
        Обновить расписание
      </MudButton>

      <MudAlert
        v-if="error.length > 0"
        class="mt-4"
        data-testid="settings-schedule-error"
        severity="error"
        variant="filled"
      >
        {{ error }}
      </MudAlert>

      <MudAlert
        v-else-if="notice.length > 0"
        class="mt-4"
        data-testid="settings-schedule-notice"
        severity="success"
        variant="filled"
      >
        {{ notice }}
      </MudAlert>
    </MudCardContent>
  </MudCard>
</template>
