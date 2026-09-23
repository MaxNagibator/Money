<script setup lang="ts">
import { computed, ref } from 'vue'
import { mdExpandLess, mdExpandMore, mdHistory } from '@/shared/icons'
import {
  MudButton,
  MudCard,
  MudCardContent,
  MudChip,
  MudCollapse,
  MudIcon,
  MudSpacer,
  MudStack,
  MudText,
  MudTimeline,
  MudTimelineItem,
} from '@/shared/mud'
import { currentVersion, versionMarker } from '../model/version-entry'
import { VERSION_HISTORY } from '../model/version-history'
import VersionHistoryItem from './VersionHistoryItem.vue'

const isVisible = ref(false)

const version = computed(() => currentVersion(VERSION_HISTORY))
</script>

<template>
  <section>
    <MudCard
      class="mt-6"
      :elevation="2"
    >
      <MudCardContent>
        <MudStack
          row
          align-items="center"
          class="history-head"
          :spacing="3"
        >
          <MudIcon
            :icon="mdHistory"
            color="primary"
          />

          <MudText
            color="primary"
            typo="h6"
          >
            История изменений
          </MudText>

          <MudChip
            color="primary"
            size="small"
          >
            v.{{ version }}
          </MudChip>

          <MudSpacer />

          <MudButton
            :end-icon="isVisible ? mdExpandLess : mdExpandMore"
            :aria-expanded="isVisible"
            aria-controls="home-history-panel"
            color="primary"
            data-testid="home-history-toggle"
            size="small"
            variant="text"
            @click="isVisible = !isVisible"
          >
            {{ isVisible ? 'Скрыть' : 'Показать' }}
          </MudButton>
        </MudStack>
      </MudCardContent>
    </MudCard>

    <MudCollapse :expanded="isVisible">
      <MudCard
        id="home-history-panel"
        class="mt-3"
        data-testid="home-history-panel"
        :elevation="1"
      >
        <MudCardContent>
          <MudTimeline
            :reverse="false"
            timeline-orientation="vertical"
            timeline-position="start"
          >
            <MudTimelineItem
              v-for="entry in VERSION_HISTORY"
              :key="entry.version"
              :color="versionMarker(entry).color ?? undefined"
              :icon="versionMarker(entry).icon"
              size="small"
            >
              <VersionHistoryItem :entry="entry" />
            </MudTimelineItem>
          </MudTimeline>
        </MudCardContent>
      </MudCard>
    </MudCollapse>
  </section>
</template>

<style scoped>
@media (max-width: 599.98px) {
  .history-head {
    flex-wrap: wrap;
  }
}
</style>
