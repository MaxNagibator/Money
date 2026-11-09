<script setup lang="ts">
import { mdBorderLeft, mdClose, mdFormatColorFill } from '@/shared/icons'
import {
  MudCard,
  MudCardContent,
  MudCardHeader,
  MudChip,
  MudDivider,
  MudIcon,
  MudSlider,
  MudStack,
  MudText,
  MudToggleGroup,
  MudToggleItem,
} from '@/shared/mud'
import type { AlternateRowStyle } from '../model/display-settings'
import {
  MONEY_COLUMN_WIDTH_MAX,
  MONEY_COLUMN_WIDTH_MIN,
  useDisplaySettings,
} from '../model/display-settings'
import SettingSwitch from './SettingSwitch.vue'
import './settings-card.css'

const { settings, update } = useDisplaySettings()

const rowStyles: readonly { value: AlternateRowStyle, label: string, icon: string, testId: string }[] = [
  { value: 'none', label: 'Выкл', icon: mdClose, testId: 'settings-alternate-none' },
  { value: 'background', label: 'Фон', icon: mdFormatColorFill, testId: 'settings-alternate-background' },
  { value: 'border', label: 'Полоса', icon: mdBorderLeft, testId: 'settings-alternate-border' },
]

function selectRowStyle(value: unknown): void {
  const style = rowStyles.find(item => item.value === value)

  if (style !== undefined) {
    update({ alternateRowStyle: style.value })
  }
}
</script>

<template>
  <MudCard class="mb-6 settings-card">
    <MudCardHeader>
      <template #card-header-content>
        <MudText typo="h6">
          Настройки отображения
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <MudText typo="body2">
        Выберите, показывать ли разделители между записями операций. Это может помочь улучшить читаемость.
      </MudText>
      <SettingSwitch
        :model-value="settings.showDividers"
        data-testid="settings-dividers"
        label="Показывать разделители в операциях"
        @update:model-value="update({ showDividers: $event })"
      />

      <MudDivider class="my-4" />

      <MudText typo="body2">
        Чередование строк операций: подсветка фона или цветная полоса слева у каждой второй строки для более
        удобного чтения списка.
      </MudText>
      <MudStack
        row
        align-items="center"
        :spacing="2"
      >
        <MudToggleGroup
          :model-value="settings.alternateRowStyle"
          aria-label="Чередование строк"
          color="primary"
          data-testid="settings-alternate"
          @update:model-value="selectRowStyle"
        >
          <MudToggleItem
            v-for="style in rowStyles"
            :key="style.value"
            :data-testid="style.testId"
            :value="style.value"
          >
            <MudStack
              row
              align-items="center"
              :spacing="1"
            >
              <MudIcon
                :icon="style.icon"
                size="small"
              />
              <MudText>{{ style.label }}</MudText>
            </MudStack>
          </MudToggleItem>
        </MudToggleGroup>
      </MudStack>

      <MudDivider class="my-4" />

      <MudText typo="body2">
        Выравнивание копеек: суммы в списке операций прижимаются к правому краю колонки, дробные части располагаются
        строго одна под другой.
      </MudText>
      <SettingSwitch
        :model-value="settings.alignMoneyDecimals"
        data-testid="settings-align-decimals"
        label="Выравнивать копейки в списке операций"
        @update:model-value="update({ alignMoneyDecimals: $event })"
      />

      <MudStack
        class="mt-2"
        :spacing="1"
      >
        <MudStack
          row
          align-items="baseline"
          :spacing="2"
        >
          <MudText
            :color="settings.alignMoneyDecimals ? 'default' : 'dark'"
            typo="body2"
          >
            Ширина колонки суммы
          </MudText>
          <MudChip
            :color="settings.alignMoneyDecimals ? 'primary' : 'default'"
            :disabled="!settings.alignMoneyDecimals"
            data-testid="settings-money-width-value"
            size="small"
            variant="text"
          >
            {{ settings.moneyColumnWidth }} симв.
          </MudChip>
        </MudStack>
        <MudSlider
          :disabled="!settings.alignMoneyDecimals"
          :max="MONEY_COLUMN_WIDTH_MAX"
          :min="MONEY_COLUMN_WIDTH_MIN"
          :model-value="settings.moneyColumnWidth"
          :step="1"
          aria-label="Ширина колонки суммы"
          color="primary"
          data-testid="settings-money-width"
          size="medium"
          tick-marks
          value-label
          @update:model-value="update({ moneyColumnWidth: $event })"
        />
      </MudStack>

      <MudDivider class="my-4" />

      <MudText typo="body2">
        Использовать цвета темы для графиков (адаптируются под темную/светлую тему). Если выключено, используются
        стандартные цвета Chart.js.
      </MudText>
      <SettingSwitch
        :model-value="settings.useChartThemeColors"
        data-testid="settings-chart-colors"
        label="Использовать цвета темы на графиках"
        @update:model-value="update({ useChartThemeColors: $event })"
      />
    </MudCardContent>
  </MudCard>
</template>
