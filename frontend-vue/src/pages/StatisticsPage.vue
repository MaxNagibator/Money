<script setup lang="ts">
import { useCategories } from '@/features/categories'
import { OperationFiltersPanel, OperationsPeriodSummary, useOperationFiltersStore, useOperations } from '@/features/operations'
import { StatisticsTypePanel, useStatistics } from '@/features/statistics'
import { MudAlert, MudExpansionPanels, MudSkeleton, MudStack } from '@/shared/mud'

const categoriesState = useCategories()
const filtersState = useOperationFiltersStore()

filtersState.apply()

const operations = useOperations(filtersState.applied, categoriesState.categories)
const statistics = useStatistics(operations.days, filtersState.applied, categoriesState.categories)

async function search(): Promise<void> {
  filtersState.apply()
  await operations.search()
}
</script>

<template>
  <OperationFiltersPanel
    :categories="categoriesState.categories.value"
    :state="filtersState"
    @search="search"
  />

  <OperationsPeriodSummary
    :from="filtersState.applied.from"
    :to="filtersState.applied.to"
    :totals="operations.totals.value"
  />

  <MudAlert
    v-if="categoriesState.loadError.value !== null"
    class="mb-4"
    data-testid="statistics-categories-error"
    severity="warning"
    variant="text"
  >
    {{ categoriesState.loadError.value }}
  </MudAlert>

  <MudAlert
    v-if="operations.errorMessage.value !== null"
    class="mb-4"
    data-testid="statistics-error"
    severity="error"
    variant="text"
  >
    {{ operations.errorMessage.value }}
  </MudAlert>

  <MudStack
    v-if="operations.isLoading.value"
    data-testid="statistics-loading"
    :spacing="2"
  >
    <MudSkeleton
      height="32px"
      skeleton-type="text"
      width="40%"
    />
    <MudSkeleton
      height="240px"
      skeleton-type="rectangle"
    />
    <MudSkeleton
      v-for="index in 3"
      :key="index"
      height="48px"
      skeleton-type="rectangle"
    />
  </MudStack>

  <MudExpansionPanels
    v-else
    multi-expansion
  >
    <StatisticsTypePanel
      v-for="panel in statistics.panels.value"
      :key="panel.type.id"
      :panel="panel"
    />
  </MudExpansionPanels>
</template>
