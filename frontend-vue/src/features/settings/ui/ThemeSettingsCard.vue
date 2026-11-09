<script setup lang="ts">
import { computed } from 'vue'
import {
  mdComputer,
  mdDarkMode,
  mdLightMode,
  mdSchedule,
} from '@/shared/icons'
import {
  MudCard,
  MudCardContent,
  MudCardHeader,
  MudIcon,
  MudText,
  MudToggleGroup,
  MudToggleItem,
} from '@/shared/mud'
import type { ThemeMode } from '@/shared/ui/useAppTheme'
import { themeStatusLabel } from '../model/theme-mode'
import { useThemeSettings } from '../model/useThemeSettings'
import './settings-card.css'

const theme = useThemeSettings()

const modes = computed<readonly { value: ThemeMode, label: string, icon: string, testId: string }[]>(() => [
  { value: 'light', label: 'Светлая', icon: mdLightMode, testId: 'settings-mode-light' },
  { value: 'dark', label: 'Тёмная', icon: mdDarkMode, testId: 'settings-mode-dark' },
  { value: 'system', label: 'Авто', icon: theme.schedule.isEnabled ? mdSchedule : mdComputer, testId: 'settings-mode-system' },
])

const statusLabel = computed(() => themeStatusLabel(theme.mode.value, theme.effective.value, theme.schedule.isEnabled))

function selectMode(value: unknown): void {
  const mode = modes.value.find(item => item.value === value)

  if (mode !== undefined) {
    theme.setMode(mode.value)
  }
}
</script>

<template>
  <MudCard class="mb-6 settings-card">
    <MudCardHeader>
      <template #card-header-content>
        <MudText typo="h6">
          Настройки темы
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent data-testid="settings-mode">
      <MudToggleGroup
        :model-value="theme.mode.value"
        aria-label="Режим темы"
        class="theme-mode-group"
        color="primary"
        @update:model-value="selectMode"
      >
        <MudToggleItem
          v-for="mode in modes"
          :key="mode.value"
          :data-testid="mode.testId"
          :value="mode.value"
        >
          <span class="theme-mode-segment">
            <MudIcon :icon="mode.icon" />
            <span>{{ mode.label }}</span>
          </span>
        </MudToggleItem>
      </MudToggleGroup>

      <MudText
        class="mt-3"
        data-testid="settings-mode-status"
        typo="body2"
      >
        {{ statusLabel }}
      </MudText>
    </MudCardContent>
  </MudCard>
</template>

<style scoped>
.theme-mode-group :deep(.mud-toggle-item) {
  min-height: 56px;
}

.theme-mode-segment {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  text-transform: none;
}
</style>
