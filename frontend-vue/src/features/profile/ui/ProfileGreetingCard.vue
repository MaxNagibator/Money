<script setup lang="ts">
import { computed } from 'vue'
import { mdAccountCircle, mdClosedCaption, mdVerified } from '@/shared/icons'
import { MudAlert, MudCard, MudCardContent, MudCardHeader, MudIcon, MudText } from '@/shared/mud'

const props = defineProps<{
  name: string
  email: string | null
  isEmailConfirmed: boolean
}>()

const emailText = computed(() => {
  if (!props.isEmailConfirmed) {
    return 'Email не подтверждён'
  }

  return props.email === null ? 'Email подтверждён' : `Email подтверждён: ${props.email}`
})
</script>

<template>
  <MudCard
    class="mb-6"
    data-testid="profile-greeting"
  >
    <MudCardHeader>
      <template #card-header-avatar>
        <MudIcon
          :icon="mdAccountCircle"
          size="medium"
        />
      </template>
      <template #card-header-content>
        <MudText typo="h6">
          Добро пожаловать, {{ name }}!
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <MudAlert
        :icon="isEmailConfirmed ? mdVerified : mdClosedCaption"
        :severity="isEmailConfirmed ? 'success' : 'warning'"
        data-testid="profile-email-status"
        variant="outlined"
      >
        {{ emailText }}
      </MudAlert>
    </MudCardContent>
  </MudCard>
</template>
