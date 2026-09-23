<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Category, CategoryNode } from '@/domain/category'
import { buildCategoryTree } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { compareLocalDate } from '@/domain/local-date'
import { OPERATION_TYPES } from '@/domain/operation-type'
import {
  mdChevronLeft,
  mdChevronRight,
  mdComment,
  mdClose,
  mdDeleteSweep,
  mdExpandLess,
  mdExpandMore,
  mdMoveToInbox,
  mdSearch,
  mdStar,
  mdStarOutline,
  mdSwapHoriz,
} from '@/shared/icons'
import {
  MudButton,
  MudDatePicker,
  MudDateRangePicker,
  MudGrid,
  MudIcon,
  MudIconButton,
  MudItem,
  MudPaper,
  MudPopover,
  MudSpacer,
  MudStack,
  MudText,
  MudTextField,
  MudToggleGroup,
  MudToggleIconButton,
  MudToggleItem,
  MudTooltip,
} from '@/shared/mud'
import { categoryTreeItems } from '@/shared/ui/category-items'
import CategorySelector from '@/shared/ui/CategorySelector.vue'
import SmartPlace from '@/shared/ui/SmartPlace.vue'
import { useNarrowViewport } from '@/shared/ui/useNarrowViewport'
import type { OperationFiltersState, OperationInterval } from '../model/filters'
import { OPERATION_INTERVALS } from '../model/filters'
import { formatPeriodRange } from '../model/period'

interface IntervalLabel {
  name: string
  shortName: string
  changeName: string
}

const INTERVAL_LABELS: Record<OperationInterval, IntervalLabel> = {
  day: { name: 'День', shortName: 'Д', changeName: 'ий день' },
  week: { name: 'Неделя', shortName: 'Н', changeName: 'ая неделя' },
  month: { name: 'Месяц', shortName: 'М', changeName: 'ий месяц' },
  year: { name: 'Год', shortName: 'Г', changeName: 'ий год' },
}

const intervalOptions = OPERATION_INTERVALS.map(value => ({ value, ...INTERVAL_LABELS[value] }))

const props = defineProps<{
  state: OperationFiltersState
  categories: readonly Category[]
}>()

const emit = defineEmits<{
  search: []
  transfer: [categoryId: number]
}>()

const isTransferOpen = ref(false)
const transferCategoryId = ref<number | null>(null)
const showDateRange = ref(true)
const isExpanded = ref(false)

const isNarrow = useNarrowViewport()

const controls = computed(() => props.state)
const filters = computed(() => controls.value.filters)
const isPanelOpen = computed(() => !isNarrow.value || isExpanded.value)
const periodRange = computed(() => formatPeriodRange(controls.value.applied.from, controls.value.applied.to))
const toggleLabel = computed(() => isExpanded.value ? 'Свернуть фильтр' : 'Развернуть фильтр')
const hasInterval = computed(() => filters.value.interval !== null)
const previousRangeTooltip = computed(() => rangeTooltip('Предыдущ'))
const nextRangeTooltip = computed(() => rangeTooltip('Следующ'))

const rangeError = computed(() => {
  const { from, to } = filters.value

  if (from === null || to === null || compareLocalDate(from, to) <= 0) {
    return ''
  }

  return 'Дата «по» раньше даты «с»'
})

const roots = computed(() => OPERATION_TYPES.flatMap(type => buildCategoryTree(props.categories, type.id)))
const items = computed(() => categoryTreeItems(props.categories))

const namesById = computed(() => new Map(props.categories.map(category => [category.id, category.name])))

const selectedNames = computed(() => filters.value.categoryIds
  .map(id => namesById.value.get(id))
  .filter((name): name is string => name !== undefined))

const categoriesHelperText = computed(() => selectedNames.value.length === 0
  ? 'Выберите категории'
  : selectedNames.value.join(', '))

const transferHelperText = computed(() => transferCategoryId.value === null
  ? 'Выберите категорию'
  : namesById.value.get(transferCategoryId.value) ?? 'Выберите категорию')

const dateRange = computed({
  get: () => ({ start: filters.value.from, end: filters.value.to }),
  set: (value: { start: LocalDate | null, end: LocalDate | null } | null) => {
    filters.value.from = value?.start ?? null
    filters.value.to = value?.end ?? null
  },
})

const descendants = computed(() => {
  const map = new Map<number, readonly number[]>()
  collectDescendants(roots.value, map)
  return map
})

const comment = computed({
  get: () => filters.value.comment,
  set: value => {
    filters.value.comment = value ?? ''
  },
})

const place = computed({
  get: () => filters.value.place,
  set: value => {
    filters.value.place = value ?? ''
  },
})

function rangeTooltip(prefix: string): string {
  const interval = filters.value.interval
  return interval === null ? 'Не выбран диапазон' : `${prefix}${INTERVAL_LABELS[interval].changeName}`
}

function collectDescendants(nodes: readonly CategoryNode[], map: Map<number, readonly number[]>): number[] {
  const collected: number[] = []

  for (const node of nodes) {
    const children = collectDescendants(node.children, map)
    map.set(node.category.id, children)
    collected.push(node.category.id, ...children)
  }

  return collected
}

function selectInterval(value: unknown): void {
  controls.value.applyInterval(OPERATION_INTERVALS.find(interval => interval === value) ?? null)
  requestSearch()
}

function shift(direction: -1 | 1): void {
  if (!hasInterval.value) {
    return
  }

  controls.value.shift(direction)
  requestSearch()
}

function requestSearch(): void {
  if (rangeError.value.length > 0) {
    return
  }

  emit('search')
}

function selectTransferCategory(value: unknown): void {
  transferCategoryId.value = typeof value === 'number' ? value : null
}

function confirmTransfer(): void {
  const categoryId = transferCategoryId.value

  if (categoryId === null) {
    return
  }

  emit('transfer', categoryId)
  transferCategoryId.value = null
  isTransferOpen.value = false
}

function selectCategories(value: readonly unknown[] | null | undefined): void {
  const next = [...(value ?? [])].filter((id): id is number => typeof id === 'number')
  const previous = filters.value.categoryIds

  filters.value.categoryIds = resolveSelection(previous, next)
}

function resolveSelection(previous: readonly number[], next: readonly number[]): number[] {
  const previousIds = new Set(previous)
  const result = new Set(next)

  for (const id of next) {
    if (previousIds.has(id)) {
      continue
    }

    for (const child of descendants.value.get(id) ?? []) {
      result.add(child)
    }
  }

  for (const id of previous) {
    if (result.has(id)) {
      continue
    }

    for (const child of descendants.value.get(id) ?? []) {
      result.delete(child)
    }
  }

  syncParents(roots.value, result)

  return [...result]
}

function syncParents(nodes: readonly CategoryNode[], selected: Set<number>): void {
  for (const node of nodes) {
    syncParents(node.children, selected)

    if (node.children.length === 0) {
      continue
    }

    if (node.children.every(child => selected.has(child.category.id))) {
      selected.add(node.category.id)
    }
    else {
      selected.delete(node.category.id)
    }
  }
}

function reset(): void {
  controls.value.reset()
  requestSearch()
}
</script>

<template>
  <MudPaper
    class="pa-2"
    data-testid="operations-filter"
  >
    <button
      v-if="isNarrow"
      :aria-expanded="isExpanded"
      :aria-label="toggleLabel"
      class="filter-summary"
      data-testid="operations-filter-toggle"
      type="button"
      @click="isExpanded = !isExpanded"
    >
      <span
        class="filter-summary__period"
        data-testid="operations-filter-period"
      >
        {{ periodRange }}
      </span>

      <MudIcon
        :icon="isExpanded ? mdExpandLess : mdExpandMore"
        color="tertiary"
        size="small"
      />
    </button>

    <MudGrid
      v-show="isPanelOpen"
      :spacing="2"
    >
      <MudItem
        :md="6"
        :xs="12"
      >
        <MudStack
          align-items="center"
          row
          :spacing="0"
          stretch-items="middle"
        >
          <MudTooltip
            :text="previousRangeTooltip"
            arrow
            :delay="100"
            placement="bottom"
          >
            <MudIconButton
              :disabled="!hasInterval"
              :icon="mdChevronLeft"
              aria-label="Предыдущий диапазон"
              color="tertiary"
              data-testid="operations-filter-prev"
              size="medium"
              @click="shift(-1)"
            />
          </MudTooltip>

          <MudToggleGroup
            :model-value="filters.interval"
            aria-label="Период"
            color="tertiary"
            selection-mode="toggle-selection"
            @update:model-value="selectInterval"
          >
            <MudToggleItem
              v-for="option in intervalOptions"
              :key="option.value"
              :aria-label="option.name"
              :data-testid="`operations-filter-interval-${option.value}`"
              :title="option.name"
              :value="option.value"
            >
              <span class="d-none d-sm-inline">{{ option.name }}</span>
              <span class="d-inline d-sm-none">{{ option.shortName }}</span>
            </MudToggleItem>
          </MudToggleGroup>

          <MudTooltip
            :text="nextRangeTooltip"
            arrow
            :delay="100"
            placement="bottom"
          >
            <MudIconButton
              :disabled="!hasInterval"
              :icon="mdChevronRight"
              aria-label="Следующий диапазон"
              color="tertiary"
              data-testid="operations-filter-next"
              size="medium"
              @click="shift(1)"
            />
          </MudTooltip>
        </MudStack>
      </MudItem>

      <MudItem
        :md="6"
        :xs="12"
      >
        <MudStack
          align-items="center"
          class="filter-date-row"
          row
          stretch-items="none"
        >
          <MudDateRangePicker
            v-if="showDateRange"
            v-model:date-range="dateRange"
            adornment="start"
            adornment-color="tertiary"
            clearable
            color="tertiary"
            data-testid="operations-filter-date-range"
            date-format="dd.MM.yyyy"
            editable
            label="Диапазон дат"
            mask="dd.MM.yyyy"
            picker-variant="inline"
            show-week-numbers
          />

          <template v-else>
            <MudDatePicker
              v-model="filters.from"
              :max-date="filters.to"
              adornment="start"
              adornment-color="tertiary"
              anchor-origin="bottom-left"
              clearable
              color="tertiary"
              data-testid="operations-filter-date-from"
              date-format="dd.MM.yyyy"
              editable
              label="Дата с"
              mask="dd.MM.yyyy"
            />

            <MudDatePicker
              v-model="filters.to"
              :min-date="filters.from"
              adornment="start"
              adornment-color="tertiary"
              anchor-origin="bottom-right"
              clearable
              color="tertiary"
              data-testid="operations-filter-date-to"
              date-format="dd.MM.yyyy"
              editable
              label="Дата по"
              mask="dd.MM.yyyy"
            />
          </template>

          <MudIconButton
            v-if="!isNarrow"
            :icon="mdSwapHoriz"
            aria-label="Переключить ввод диапазона"
            color="tertiary"
            data-testid="operations-filter-date-swap"
            size="small"
            @click="showDateRange = !showDateRange"
          />
        </MudStack>

        <MudText
          v-if="rangeError.length > 0"
          class="mt-1"
          data-testid="operations-filter-range-error"
          html-tag="div"
          role="alert"
          typo="caption"
          color="error"
        >
          {{ rangeError }}
        </MudText>
      </MudItem>

      <MudItem
        :md="6"
        :xs="12"
      >
        <SmartPlace
          v-model="place"
          adornment="start"
          data-testid="operations-filter-place"
          @keydown.enter="requestSearch"
        />
      </MudItem>

      <MudItem
        :md="6"
        :xs="12"
      >
        <CategorySelector
          :helper-text="categoriesHelperText"
          :items="items"
          :selected-values="filters.categoryIds"
          selection-mode="multi-selection"
          test-id="operations-filter-category"
          @update:selected-values="selectCategories"
        />
      </MudItem>

      <MudItem
        :md="6"
        :xs="12"
      >
        <MudTextField
          v-model="comment"
          :adornment-icon="mdComment"
          adornment="start"
          adornment-color="tertiary"
          clearable
          data-testid="operations-filter-comment"
          immediate
          label="Комментарий"
          @keydown.enter="requestSearch"
        />
      </MudItem>

      <MudItem
        :md="6"
        :xs="12"
      >
        <MudStack
          align-items="baseline"
          row
          wrap="wrap"
        >
          <MudTooltip
            v-if="isNarrow"
            text="Переключить ввод диапазона"
          >
            <MudIconButton
              :icon="mdSwapHoriz"
              aria-label="Переключить ввод диапазона"
              color="tertiary"
              data-testid="operations-filter-date-swap"
              size="medium"
              variant="filled"
              @click="showDateRange = !showDateRange"
            />
          </MudTooltip>

          <MudTooltip text="Очистить все фильтры">
            <MudIconButton
              :icon="mdDeleteSweep"
              aria-label="Очистить все фильтры"
              color="tertiary"
              data-testid="operations-filter-reset"
              size="medium"
              variant="filled"
              @click="reset"
            />
          </MudTooltip>

          <MudTooltip :text="filters.showEmptyDays ? 'Скрыть пустые дни' : 'Показать пустые дни'">
            <MudToggleIconButton
              v-model:toggled="filters.showEmptyDays"
              :icon="mdStar"
              :toggled-icon="mdStarOutline"
              aria-label="Показывать пустые дни"
              color="tertiary"
              data-testid="operations-filter-empty-days"
              size="medium"
              toggled-variant="outlined"
              variant="filled"
            />
          </MudTooltip>

          <MudTooltip :text="isTransferOpen ? 'Отменить перенос' : 'Перекинуть все отображённые на странице операции в выбранную категорию'">
            <MudToggleIconButton
              v-model:toggled="isTransferOpen"
              :icon="mdMoveToInbox"
              :toggled-icon="mdClose"
              aria-label="Перенести операции в категорию"
              color="tertiary"
              data-testid="operations-filter-transfer"
              size="medium"
              toggled-variant="outlined"
              variant="filled"
            />
          </MudTooltip>

          <MudPopover
            :open="isTransferOpen"
            anchor-origin="bottom-left"
            class="mt-4 mx-4 pa-4"
            data-testid="operations-filter-transfer-panel"
            transform-origin="top-center"
          >
            <CategorySelector
              :helper-text="transferHelperText"
              :items="items"
              :selected-value="transferCategoryId"
              selection-mode="single-selection"
              test-id="operations-filter-transfer-category"
              @update:selected-value="selectTransferCategory"
            />

            <div class="d-flex align-center justify-space-between mt-3">
              <MudButton
                color="error"
                data-testid="operations-filter-transfer-close"
                size="small"
                @click="isTransferOpen = false"
              >
                Закрыть
              </MudButton>

              <MudButton
                :disabled="transferCategoryId === null"
                color="tertiary"
                data-testid="operations-filter-transfer-submit"
                size="small"
                variant="filled"
                @click="confirmTransfer"
              >
                Подтвердить перенос
              </MudButton>
            </div>
          </MudPopover>

          <MudSpacer />

          <MudButton
            :end-icon="mdSearch"
            color="info"
            data-testid="operations-filter-search"
            size="medium"
            variant="filled"
            @click="requestSearch"
          >
            Найти
          </MudButton>
        </MudStack>
      </MudItem>
    </MudGrid>
  </MudPaper>
</template>

<style scoped>
@media (max-width: 599.98px) {
  .filter-summary {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    width: 100%;
    min-height: 44px;
    padding: 0 4px;
    border: none;
    background: none;
    color: inherit;
    font: inherit;
    text-align: left;
    cursor: pointer;
  }

  .filter-summary__period {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .filter-date-row {
    flex-wrap: wrap;
  }

  .filter-date-row > :deep(.mud-picker) {
    flex: 1 1 100%;
    max-width: 100%;
  }
}
</style>
