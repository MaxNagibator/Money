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
  MudIconButton,
  MudStack,
  MudText,
  MudTooltip,
} from '@/shared/mud'
import { mdCheck, mdClose } from '@/shared/icons'
import type { ScheduleFormValue } from '../model/schedule-form'
import { scheduleFormFrom, validateScheduleForm } from '../model/schedule-form'
import { useThemeSettings } from '../model/useThemeSettings'

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
  <MudCard class="mb-6">
    <MudCardHeader>
      <template #card-header-content>
        <MudText typo="h6">
          Расписание темной темы
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <MudStack
        row
        align-items="center"
        class="mt-4"
        :spacing="2"
      >
        <MudTooltip :text="theme.schedule.isLocationChangedEnabled ? 'Включено' : 'Выключено'">
          <MudIconButton
            aria-label="Переключить отслеживание перехода по страницам"
            :color="theme.schedule.isLocationChangedEnabled ? 'primary' : 'default'"
            :disabled="isScheduleDisabled"
            :icon="theme.schedule.isLocationChangedEnabled ? mdCheck : mdClose"
            data-testid="settings-location-changed"
            @click="theme.toggleLocationChanged()"
          />
        </MudTooltip>
        <MudText typo="body1">
          Активировать отслеживание перехода по страницам приложения (не вкладкам)
        </MudText>
      </MudStack>

      <MudDivider class="mb-6" />

      <MudText typo="body2">
        Включите расписание, чтобы автоматически переключать темную тему в заданные часы.
      </MudText>

      <MudStack
        row
        align-items="center"
        :spacing="2"
      >
        <MudTooltip :text="theme.schedule.isEnabled ? 'Включено' : 'Выключено'">
          <MudIconButton
            aria-label="Переключить расписание темной темы"
            :color="theme.schedule.isEnabled ? 'primary' : 'default'"
            :disabled="isScheduleDisabled"
            :icon="theme.schedule.isEnabled ? mdCheck : mdClose"
            data-testid="settings-schedule-enabled"
            @click="theme.updateSchedule({ isEnabled: !theme.schedule.isEnabled })"
          />
        </MudTooltip>
        <MudText typo="body1">
          Активировать расписание
        </MudText>
      </MudStack>

      <MudGrid
        class="mt-4"
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

      <MudDivider class="mb-6" />

      <MudStack
        row
        align-items="center"
        class="mt-4"
        :spacing="2"
      >
        <MudTooltip :text="theme.schedule.isRecheckEnabled ? 'Включено' : 'Выключено'">
          <MudIconButton
            aria-label="Переключить таймер"
            :color="theme.schedule.isRecheckEnabled ? 'primary' : 'default'"
            :disabled="isScheduleDisabled"
            :icon="theme.schedule.isRecheckEnabled ? mdCheck : mdClose"
            data-testid="settings-schedule-recheck"
            @click="theme.updateSchedule({ isRecheckEnabled: !theme.schedule.isRecheckEnabled })"
          />
        </MudTooltip>
        <MudText typo="body1">
          Активировать таймер
        </MudText>
      </MudStack>

      <MudGrid
        class="mt-4"
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
        class="mt-4"
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
