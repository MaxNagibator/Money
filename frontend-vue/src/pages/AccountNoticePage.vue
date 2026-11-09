<script setup lang="ts">
import { computed } from 'vue'
import { MudAlert, MudText } from '@/shared/mud'
import type { LoginNoticeKey } from '@/shared/ui/login-notice'
import { LOGIN_NOTICES } from '@/shared/ui/login-notice'

const props = defineProps<{ notice: LoginNoticeKey }>()

const notice = computed(() => LOGIN_NOTICES[props.notice])
</script>

<template>
  <MudAlert
    :class="{ 'mb-4': notice.paragraphs.length > 0 }"
    data-testid="account-notice"
    severity="error"
  >
    {{ notice.alert }}
  </MudAlert>

  <MudText
    v-for="paragraph in notice.paragraphs"
    :key="paragraph"
    data-testid="account-notice-paragraph"
  >
    {{ paragraph }}
  </MudText>
</template>
