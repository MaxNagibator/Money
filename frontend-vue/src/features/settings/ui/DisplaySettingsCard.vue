<script setup lang="ts">
import { mdBorderLeft, mdCheck, mdClose, mdFormatColorFill } from '@/shared/icons'
import {
  MudCard,
  MudCardContent,
  MudCardHeader,
  MudChip,
  MudDivider,
  MudIcon,
  MudSlider,
  MudStack,
  MudIconButton,
  MudText,
  MudTooltip,
  MudToggleGroup,
  MudToggleItem,
} from '@/shared/mud'
import type { AlternateRowStyle } from '../model/display-settings'
import {
  MONEY_COLUMN_WIDTH_MAX,
  MONEY_COLUMN_WIDTH_MIN,
  useDisplaySettings,
} from '../model/display-settings'

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
  <MudCard class="mb-6">
    <MudCardHeader>
      <template #card-header-content>
        <MudText typo="h6">
          Настройки отображения
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <MudText>
        Выберите, показывать ли разделители между записями операций. Это может помочь улучшить читаемость.
      </MudText>
      <MudStack
        row
        align-items="center"
        :spacing="2"
      >
        <MudTooltip :text="settings.showDividers ? 'Включены' : 'Выключены'">
          <MudIconButton
            aria-label="Показывать разделители"
            :color="settings.showDividers ? 'primary' : 'default'"
            data-testid="settings-dividers"
            :icon="settings.showDividers ? mdCheck : mdClose"
            @click="update({ showDividers: !settings.showDividers })"
          />
        </MudTooltip>
        <MudText typo="body1">
          Показывать разделители в операциях
        </MudText>
      </MudStack>

      <MudDivider class="my-4" />

      <MudText>
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

      <MudText>
        Выравнивание копеек: суммы в списке операций прижимаются к правому краю колонки, дробные части располагаются
        строго одна под другой.
      </MudText>
      <MudStack
        row
        align-items="center"
        :spacing="2"
      >
        <MudTooltip :text="settings.alignMoneyDecimals ? 'Включено' : 'Выключено'">
          <MudIconButton
            aria-label="Выравнивать копейки"
            :color="settings.alignMoneyDecimals ? 'primary' : 'default'"
            data-testid="settings-align-decimals"
            :icon="settings.alignMoneyDecimals ? mdCheck : mdClose"
            @click="update({ alignMoneyDecimals: !settings.alignMoneyDecimals })"
          />
        </MudTooltip>
        <MudText typo="body1">
          Выравнивать копейки в списке операций
        </MudText>
      </MudStack>

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

      <MudText>
        Использовать цвета темы для графиков (адаптируются под темную/светлую тему). Если выключено, используются
        стандартные цвета Chart.js.
      </MudText>
      <MudStack
        row
        align-items="center"
        :spacing="2"
      >
        <MudTooltip :text="settings.useChartThemeColors ? 'Включены' : 'Выключены'">
          <MudIconButton
            aria-label="Использовать цвета темы на графиках"
            :color="settings.useChartThemeColors ? 'primary' : 'default'"
            data-testid="settings-chart-colors"
            :icon="settings.useChartThemeColors ? mdCheck : mdClose"
            @click="update({ useChartThemeColors: !settings.useChartThemeColors })"
          />
        </MudTooltip>
        <MudText typo="body1">
          Использовать цвета темы на графиках
        </MudText>
      </MudStack>
    </MudCardContent>
  </MudCard>
</template>
