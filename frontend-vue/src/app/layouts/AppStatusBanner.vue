<script setup lang="ts">
import { ref } from 'vue'
import { applyUpdate, isConnected, pwaState, reloadApp } from '@/platform/pwa'
import { MudAlert, MudButton } from '@/shared/mud'

const isApplying = ref(false)

async function update(): Promise<void> {
  if (pwaState.updatedElsewhere) {
    reloadApp()

    return
  }

  isApplying.value = true

  try {
    await applyUpdate()
  } finally {
    isApplying.value = false
  }
}
</script>

<template>
  <div
    aria-live="polite"
    role="status"
  >
    <MudAlert
      v-if="!isConnected()"
      class="mb-3"
      data-testid="app-offline-banner"
      dense
      severity="warning"
      variant="filled"
    >
      Нет связи. Показаны сохранённые данные, они могут быть устаревшими. Изменения не отправятся, пока связь
      не вернётся
    </MudAlert>

    <MudAlert
      v-if="pwaState.needRefresh || pwaState.updatedElsewhere"
      class="mb-3"
      data-testid="app-update-banner"
      dense
      severity="info"
      variant="outlined"
    >
      <div class="app-status-row">
        <span>{{ pwaState.updatedElsewhere ? 'Приложение обновилось в другой вкладке' : 'Доступна новая версия' }}</span>
        <MudButton
          color="primary"
          data-testid="app-update-apply"
          :disabled="isApplying"
          size="small"
          variant="filled"
          @click="update()"
        >
          {{ pwaState.updatedElsewhere ? 'Перезагрузить' : 'Обновить' }}
        </MudButton>
      </div>
    </MudAlert>
  </div>
</template>

<style scoped>
.app-status-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 8px 16px;
}
</style>
