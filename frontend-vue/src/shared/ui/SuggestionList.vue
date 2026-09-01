<script setup lang="ts">
import { MudList, MudListItem, MudPaper } from '@/shared/mud'

defineProps<{
  items: readonly string[]
  selectedIndex: number
  idPrefix: string
  testId: string
}>()

const emit = defineEmits<{
  select: [index: number]
}>()
</script>

<template>
  <MudPaper
    class="autocomplete-suggestions"
    :data-testid="testId"
    :elevation="8"
    style="position: absolute; top: 100%; left: 0; right: 0; max-height: 300px; overflow-y: auto; z-index: 1300; margin-top: 4px;"
  >
    <MudList dense>
      <div
        v-for="(item, index) in items"
        :id="`${idPrefix}-item-${index}`"
        :key="index"
      >
        <MudListItem
          :style="index === selectedIndex ? 'background-color: var(--mud-palette-action-default-hover);' : ''"
          tabindex="-1"
          @click="emit('select', index)"
        >
          {{ item }}
        </MudListItem>
      </div>
    </MudList>
  </MudPaper>
</template>
