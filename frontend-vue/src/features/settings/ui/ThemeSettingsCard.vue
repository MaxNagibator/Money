<script setup lang="ts">
import { computed } from 'vue'
import {
  mdBrightness4,
  mdComputer,
  mdCheck,
  mdSchedule,
  mdSwapHoriz,
} from '@/shared/icons'
import {
  MudCard,
  MudCardContent,
  MudCardHeader,
  MudDivider,
  MudIconButton,
  MudText,
} from '@/shared/mud'
import { useThemeSettings } from '../model/useThemeSettings'

const theme = useThemeSettings()

const modeLabel = computed(() => theme.isManual.value ? 'Ручной' : 'Авто')
const autoLabel = computed(() => theme.schedule.isEnabled ? 'По расписанию' : 'Согласно настройкам системы')
const appliedLabel = computed(() => theme.isDark.value ? 'Темный' : 'Светлый')
const brightnessLabel = computed(() => theme.isDark.value ? 'Переключить на светлую тему' : 'Переключить на темную тему')
const manualLabel = computed(() => theme.isManual.value ? 'Переключить на автоматический режим' : 'Переключить на ручной режим')
const scheduleLabel = computed(() => theme.schedule.isEnabled ? 'Переключить на режим системы' : 'Переключить на расписание')

function toggleManualMode(): void {
  theme.setMode(theme.isManual.value ? 'system' : theme.effective.value)
}

function toggleScheduleMode(): void {
  theme.updateSchedule({ isEnabled: !theme.schedule.isEnabled })
}
</script>

<template>
  <MudCard class="mb-6">
    <MudCardHeader>
      <template #card-header-content>
        <MudText typo="h6">
          Настройки темы
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent data-testid="settings-mode">
      <MudIconButton
        :aria-label="manualLabel"
        :color="theme.isManual.value ? 'primary' : 'secondary'"
        :icon="theme.isManual.value ? mdCheck : mdSwapHoriz"
        :title="modeLabel"
        data-testid="settings-mode-manual"
        @click="toggleManualMode()"
      />
      {{ ' ' }}
      <MudIconButton
        :aria-label="scheduleLabel"
        :color="theme.schedule.isEnabled ? 'primary' : 'secondary'"
        :disabled="theme.isManual.value"
        :icon="theme.schedule.isEnabled ? mdSchedule : mdComputer"
        :title="autoLabel"
        data-testid="settings-mode-schedule"
        @click="toggleScheduleMode()"
      />
      {{ ' ' }}
      <MudIconButton
        :aria-label="brightnessLabel"
        :icon="mdBrightness4"
        :title="appliedLabel"
        color="inherit"
        data-testid="settings-brightness"
        @click="theme.toggleBrightness()"
      />

      <MudDivider class="my-4" />

      <MudText
        :color="theme.isManual.value ? 'warning' : 'default'"
        data-testid="settings-mode-status"
      >
        Текущий режим темной темы: {{ modeLabel }}
      </MudText>
      <MudText
        :style="theme.isManual.value ? 'text-decoration: line-through;' : ''"
        data-testid="settings-auto-status"
      >
        Текущий автоматический режим: {{ autoLabel }}
      </MudText>
      <MudText data-testid="settings-applied-status">
        Текущий режим темной темы: {{ appliedLabel }}
      </MudText>
    </MudCardContent>
  </MudCard>
</template>
