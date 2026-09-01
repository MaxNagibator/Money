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
      <div class="d-flex align-center gap-6">
        <MudIcon
          :color="card.color"
          :icon="card.icon"
          size="large"
        />

        <MudText
          align="justify"
          typo="body1"
        >
          {{ card.text }}
        </MudText>
      </div>
    </MudCardContent>
  </MudCard>
</template>
