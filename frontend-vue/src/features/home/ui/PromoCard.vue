<script setup lang="ts">
import { MudCard, MudCardContent, MudIcon, MudText } from '@/shared/mud'
import type { PromoCard } from '../model/promo'

defineProps<{
  card: PromoCard
}>()
</script>

<template>
  <MudCard
    :data-testid="`home-promo-${card.id}`"
    class="mb-4"
  >
    <MudCardContent>
      <div class="d-flex align-center gap-6 promo-layout">
        <MudIcon
          :color="card.color"
          :icon="card.icon"
          size="large"
        />

        <MudText
          align="justify"
          class="promo-text"
          typo="body1"
        >
          {{ card.text }}
        </MudText>
      </div>
    </MudCardContent>
  </MudCard>
</template>

<style scoped>
@media (max-width: 599.98px) {
  .promo-layout {
    flex-direction: column !important;
    align-items: flex-start !important;
    gap: 8px !important;
  }

  .promo-text {
    text-align: left;
  }
}
</style>
