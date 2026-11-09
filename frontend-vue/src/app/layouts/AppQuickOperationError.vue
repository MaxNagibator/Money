<script setup lang="ts">
import { MudAlert } from '@/shared/mud'
</script>

<template>
  <div class="app-quick-operation-error">
    <MudAlert
      data-testid="quick-operation-error"
      role="alert"
      severity="error"
      variant="filled"
    >
      Не удалось открыть форму операции. Обновите страницу и попробуйте снова.
    </MudAlert>
  </div>
</template>

<style scoped>
.app-quick-operation-error {
  position: fixed;
  right: 24px;
  bottom: calc(24px + var(--app-bottom-nav-height));
  left: 24px;
  z-index: var(--mud-zindex-snackbar);
}
</style>
