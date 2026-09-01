<script setup lang="ts">
import { mdError, mdRefresh } from '@/shared/icons'
import { MudAlert, MudButton, MudDivider } from '@/shared/mud'
import { useErrorBoundary } from './useErrorBoundary'

const isDevelopment = import.meta.env.DEV
const boundary = useErrorBoundary()
</script>

<template>
  <template v-if="boundary.error.value === null">
    <slot />
  </template>

  <div
    v-else
    data-testid="app-error"
  >
    <template v-if="isDevelopment">
      <p data-testid="app-error-message">
        {{ boundary.error.value?.message }}
      </p>

      <br>
      <MudDivider class="my-3" />

      <p class="error-stack">
        {{ boundary.error.value?.stack }}
      </p>
    </template>

    <MudAlert
      :icon="mdError"
      data-testid="app-error-alert"
      severity="error"
    >
      Извините, произошла ошибка. Пожалуйста, попробуйте снова.
    </MudAlert>

    <MudButton
      :start-icon="mdRefresh"
      class="mt-3"
      color="primary"
      data-testid="app-error-retry"
      variant="filled"
      @click="boundary.recover()"
    >
      Повторить
    </MudButton>
  </div>
</template>

<style scoped>
.error-stack {
  white-space: pre-wrap;
}
</style>
