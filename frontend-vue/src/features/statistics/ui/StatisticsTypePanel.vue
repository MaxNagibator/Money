<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { mdExpandLess, mdExpandMore } from '@/shared/icons'
import {
  MudButton,
  MudExpansionPanel,
  MudGrid,
  MudItem,
  MudStack,
  MudText,
} from '@/shared/mud'
import { collectSumKeys } from '../model/category-sums'
import type { StatisticsPanel } from '../model/useStatistics'
import CategoryBarChart from './CategoryBarChart.vue'
import CategoryPieChart from './CategoryPieChart.vue'
import CategorySumTree from './CategorySumTree.vue'

const props = defineProps<{ panel: StatisticsPanel }>()

const opened = ref<string[]>([])
const expanded = ref(true)

interface ExpansionPanelInstance {
  $el?: HTMLElement
}

const expansionPanelReference = ref<ExpansionPanelInstance>()

onMounted(() => {
  const header = expansionPanelReference.value?.$el?.querySelector<HTMLElement>('.mud-expand-panel-header')
  header?.setAttribute('data-testid', `statistics-panel-toggle-${props.panel.type.id}`)
})

function expandAll(): void {
  opened.value = collectSumKeys(props.panel.tree)
}

function collapseAll(): void {
  opened.value = []
}
</script>

<template>
  <MudExpansionPanel
    ref="expansionPanelReference"
    v-model:expanded="expanded"
    :data-testid="`statistics-panel-${panel.type.id}`"
    :text="panel.type.name"
  >
    <MudText
      v-if="panel.isEmpty"
      :data-testid="`statistics-empty-${panel.type.id}`"
      align="center"
      class="py-6"
      color="secondary"
      html-tag="div"
    >
      За выбранный период операций нет
    </MudText>

    <template v-else>
      <div
        :aria-label="`Суммы по дням, ${panel.type.name}`"
        :data-testid="`statistics-bar-${panel.type.id}`"
        role="img"
      >
        <CategoryBarChart :data="panel.bar" />
      </div>

      <MudGrid>
        <MudItem
          :md="6"
          :xs="12"
        >
          <div
            :aria-label="`Доли категорий, ${panel.type.name}`"
            :data-testid="`statistics-pie-${panel.type.id}`"
            role="img"
          >
            <CategoryPieChart :slices="panel.pie" />
          </div>

          <MudStack
            align-items="center"
            class="mt-2"
            row
          >
            <MudButton
              :data-testid="`statistics-collapse-${panel.type.id}`"
              :end-icon="mdExpandLess"
              full-width
              variant="outlined"
              @click="collapseAll"
            >
              Свернуть
            </MudButton>
            <MudButton
              :data-testid="`statistics-expand-${panel.type.id}`"
              :end-icon="mdExpandMore"
              full-width
              variant="outlined"
              @click="expandAll"
            >
              Развернуть
            </MudButton>
          </MudStack>
        </MudItem>

        <MudItem
          :md="6"
          :xs="12"
        >
          <CategorySumTree
            v-model:opened="opened"
            :nodes="panel.tree"
            :test-id="`statistics-tree-${panel.type.id}`"
          />
        </MudItem>
      </MudGrid>
    </template>
  </MudExpansionPanel>
</template>
