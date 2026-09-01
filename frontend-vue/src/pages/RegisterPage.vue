<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { readSafeReturnUrl, RegisterForm } from '@/features/auth'
import { MudGrid, MudItem, MudText } from '@/shared/mud'

const route = useRoute()
const router = useRouter()

async function goBackToRequestedPage(): Promise<void> {
  await router.replace(readSafeReturnUrl(route.query.returnUrl))
}
</script>

<template>
  <MudGrid justify="center">
    <MudItem
      :md="12"
      :xs="12"
    >
      <MudText
        align="center"
        gutter-bottom
        typo="h3"
      >
        Создайте новый аккаунт
      </MudText>
    </MudItem>

    <MudItem
      :md="6"
      :xs="12"
    >
      <RegisterForm @registered="goBackToRequestedPage" />
    </MudItem>
  </MudGrid>
</template>
