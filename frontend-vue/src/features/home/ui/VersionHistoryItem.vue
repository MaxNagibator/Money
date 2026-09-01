<script setup lang="ts">
import { computed } from 'vue'
import { formatLocalDate } from '@/platform/format'
import { mdListAlt } from '@/shared/icons'
import { MudChip, MudIcon, MudPaper, MudSpacer, MudStack, MudText } from '@/shared/mud'
import type { VersionHistoryEntry } from '../model/version-entry'
import { changeAppearance, versionChipColor } from '../model/version-entry'

const props = defineProps<{
  entry: VersionHistoryEntry
}>()

const chipColor = computed(() => versionChipColor(props.entry.version) ?? undefined)
</script>

<template>
  <MudPaper
    :data-testid="`home-history-entry-${entry.version}`"
    class="version-entry pa-4 mb-3"
    :elevation="0"
  >
    <MudStack :spacing="2">
      <MudStack
        row
        align-items="center"
        :spacing="2"
        wrap="wrap"
      >
        <MudChip
          :color="chipColor"
          size="small"
        >
          v.{{ entry.version }}
        </MudChip>

        <MudText
          color="secondary"
          typo="body2"
        >
          {{ formatLocalDate(entry.date) }}
        </MudText>

        <MudSpacer />

        <MudChip
          v-if="entry.changes.length > 0"
          :icon="mdListAlt"
          size="small"
        >
          {{ entry.changes.length }} изменений
        </MudChip>
      </MudStack>

      <MudStack
        v-if="entry.changes.length > 0"
        :spacing="1"
      >
        <MudStack
          v-for="(change, index) in entry.changes"
          :key="index"
          row
          align-items="center"
          :spacing="2"
        >
          <MudIcon
            :color="changeAppearance(change.type).color ?? undefined"
            :icon="changeAppearance(change.type).icon"
            size="small"
            style="margin-top: 2px;"
          />

          <MudText
            class="change-description"
            typo="body2"
          >
            {{ change.description }}
          </MudText>
        </MudStack>
      </MudStack>
    </MudStack>
  </MudPaper>
</template>

<style scoped>
.version-entry {
  border-left: 3px solid var(--mud-palette-primary);
}

.change-description {
  line-height: 1.4;
}
</style>
