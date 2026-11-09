<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { readSignedInReturnUrl, SIGN_IN_ERROR, SIGN_IN_FAILURE, useSessionStore } from '@/features/auth'
import { MudAlert, MudProgressCircular } from '@/shared/mud'

const route = useRoute()
const router = useRouter()
const session = useSessionStore()

const errorMessage = ref('')

onMounted(async () => {
  try {
    await session.signInExternal()
  } catch (error) {
    errorMessage.value = `${SIGN_IN_FAILURE} ${error instanceof Error ? error.message : SIGN_IN_ERROR}`
    return
  }

  await router.replace(readSignedInReturnUrl(route.query.returnUrl))
})
</script>

<template>
  <MudAlert
    v-if="errorMessage.length > 0"
    data-testid="callback-error"
    severity="error"
  >
    {{ errorMessage }}
  </MudAlert>

  <MudProgressCircular
    v-else
    color="primary"
    data-testid="callback-progress"
    indeterminate
  />
</template>
