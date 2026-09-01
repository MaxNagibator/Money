import type { ComputedRef, Ref } from 'vue'
import { computed } from 'vue'
import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import type { Operation, OperationsDay } from '@/domain/operation'
import type { OperationType } from '@/domain/operation-type'
import { OPERATION_TYPES } from '@/domain/operation-type'
import type { OperationFilters } from '@/features/operations'
import type { Clock } from '@/platform/clock'
import { systemClock } from '@/platform/clock'
import type { BarChartData } from './bar-series'
import { buildBarChartData } from './bar-series'
import type { PieSlice, SumTreeNode } from './category-sums'
import { buildCategorySums, buildPieSlices, buildSumTree } from './category-sums'

export interface StatisticsPanel {
  type: OperationType
  bar: BarChartData
  pie: PieSlice[]
  tree: SumTreeNode[]
  isEmpty: boolean
}

export interface StatisticsState {
  panels: ComputedRef<StatisticsPanel[]>
}

export function useStatistics(
  days: Ref<readonly OperationsDay[]> | ComputedRef<readonly OperationsDay[]>,
  filters: OperationFilters,
  categories: Ref<readonly Category[]> | ComputedRef<readonly Category[]>,
  clock: Clock = systemClock,
): StatisticsState {
  const loaded = computed(() => days.value
    .flatMap(day => day.operations)
    .filter(operation => !operation.isDeleted))

  const panels = computed(() => OPERATION_TYPES.map(type => buildPanel(
    type,
    loaded.value,
    categories.value,
    { from: filters.from, to: filters.to },
    clock.today(),
  )))

  return { panels }
}

function buildPanel(
  type: OperationType,
  operations: readonly Operation[],
  categories: readonly Category[],
  range: { from: LocalDate | null, to: LocalDate | null },
  today: LocalDate,
): StatisticsPanel {
  const scoped = operations.filter(operation => operation.category?.operationTypeId === type.id)
  const sums = buildCategorySums(categories, scoped, type.id)

  return {
    type,
    bar: buildBarChartData(scoped, range, today, operations),
    pie: buildPieSlices(sums),
    tree: buildSumTree(sums),
    isEmpty: scoped.length === 0,
  }
}
