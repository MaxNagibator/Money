<script setup lang="ts">
import { mdInfo } from '@/shared/icons'
import { MudAlert, MudText } from '@/shared/mud'
import {
  PROMO_CALL_TO_ACTION,
  PROMO_PROBLEMS,
  PROMO_SOLUTION,
  PROMO_SOLUTION_TITLE,
  PROMO_TITLE,
  PROMO_WARNING,
} from '../model/promo'
import PromoCard from './PromoCard.vue'
</script>

<template>
  <section>
    <MudText
      align="center"
      class="mb-4"
      typo="h5"
    >
      {{ PROMO_TITLE }}
    </MudText>

    <MudAlert
      class="mb-4"
      content-alignment="center"
      :icon="mdInfo"
      severity="info"
    >
      <strong>Важно:</strong> {{ PROMO_WARNING }}
    </MudAlert>

    <PromoCard
      v-for="card in PROMO_PROBLEMS"
      :key="card.id"
      :card="card"
    />

    <MudText
      align="center"
      class="my-4"
      typo="h6"
    >
      {{ PROMO_SOLUTION_TITLE }}
    </MudText>

    <PromoCard :card="PROMO_SOLUTION" />

    <MudText
      align="center"
      class="mt-4"
      typo="h6"
    >
      {{ PROMO_CALL_TO_ACTION }}
    </MudText>
  </section>
</template>
