<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { readSafeReturnUrl, useSessionStore } from '@/features/auth'
import { MudProgressCircular } from '@/shared/mud'

const route = useRoute()
const router = useRouter()
const session = useSessionStore()

onMounted(async () => {
  await session.signOut()
  await router.replace(readSafeReturnUrl(route.query.returnUrl ?? route.query.ReturnUrl))
})
</script>

<template>
  <MudProgressCircular
    color="primary"
    data-testid="logout-progress"
    indeterminate
  />
</template>
