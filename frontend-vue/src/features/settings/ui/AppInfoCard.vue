<script setup lang="ts">
import { computed, onMounted, ref, shallowRef } from 'vue'
import type { StorageUsage } from '@/platform/pwa'
import {
  applyUpdate,
  checkForUpdates,
  clearOfflineData,
  estimateStorage,
  isConnected,
  promptInstall,
  pwaState,
} from '@/platform/pwa'
import {
  MudButton,
  MudCard,
  MudCardContent,
  MudCardHeader,
  MudDivider,
  MudStack,
  MudText,
} from '@/shared/mud'
import type { CacheClearResult } from '../model/app-info'
import {
  CACHE_CLEAR_TEXT,
  describeBuild,
  describeNetwork,
  describeStorage,
  describeUpdate,
  INSTALL_TEXT,
  OFFLINE_SUPPORT_TEXT,
} from '../model/app-info'
import './settings-card.css'

const { version } = defineProps<{
  version: string
}>()

const build = describeBuild(__APP_BUILD__)

const storage = shallowRef<StorageUsage | null | undefined>(undefined)
const isClearing = ref(false)
const clearResult = ref<CacheClearResult>('idle')
const isApplying = ref(false)

const updateText = computed(() => describeUpdate(pwaState.needRefresh, pwaState.updateCheck))
const isCheckDisabled = computed(() =>
  !pwaState.canCheckUpdates || pwaState.needRefresh || pwaState.updateCheck === 'checking',
)

onMounted(() => {
  void refreshStorage()
})

let storageRequest = 0

async function refreshStorage(): Promise<void> {
  const request = ++storageRequest
  const estimate = await estimateStorage()

  if (request === storageRequest) {
    storage.value = estimate
  }
}

async function clearCache(): Promise<void> {
  isClearing.value = true

  try {
    clearResult.value = await clearOfflineData() ? 'done' : 'failed'
    await refreshStorage()
  } finally {
    isClearing.value = false
  }
}

async function update(): Promise<void> {
  isApplying.value = true

  try {
    await applyUpdate()
  } finally {
    isApplying.value = false
  }
}
</script>

<template>
  <MudCard
    class="mb-6 settings-card"
    data-testid="settings-app"
  >
    <MudCardHeader>
      <template #card-header-content>
        <MudText typo="h6">
          Приложение
        </MudText>
      </template>
    </MudCardHeader>

    <MudCardContent>
      <MudText
        data-testid="settings-app-version"
        typo="body1"
      >
        Версия {{ version }}
      </MudText>
      <MudText
        v-if="build !== ''"
        class="mud-text-secondary"
        data-testid="settings-app-build"
        typo="body2"
      >
        Сборка {{ build }}
      </MudText>

      <div
        aria-live="polite"
        role="status"
      >
        <MudText
          v-if="updateText !== null"
          class="mt-3"
          data-testid="settings-app-update-status"
          typo="body2"
        >
          {{ updateText }}
        </MudText>
      </div>

      <MudStack
        class="mt-3"
        row
        wrap="wrap"
        :spacing="2"
      >
        <MudButton
          v-if="pwaState.needRefresh"
          color="primary"
          data-testid="settings-app-apply-update"
          :disabled="isApplying"
          variant="filled"
          @click="update()"
        >
          Обновить
        </MudButton>
        <MudButton
          color="primary"
          data-testid="settings-app-check-update"
          :disabled="isCheckDisabled"
          variant="outlined"
          @click="checkForUpdates()"
        >
          Проверить обновления
        </MudButton>
      </MudStack>

      <MudDivider class="my-4" />

      <MudText
        data-testid="settings-app-install"
        typo="body2"
      >
        {{ INSTALL_TEXT[pwaState.install] }}
      </MudText>
      <MudButton
        v-if="pwaState.install === 'available'"
        class="mt-3"
        color="primary"
        data-testid="settings-app-install-button"
        variant="filled"
        @click="promptInstall()"
      >
        Установить
      </MudButton>

      <MudDivider class="my-4" />

      <MudText
        data-testid="settings-app-network"
        typo="body2"
      >
        {{ describeNetwork(isConnected()) }}
      </MudText>
      <MudText
        class="mt-1"
        data-testid="settings-app-offline"
        typo="body2"
      >
        {{ OFFLINE_SUPPORT_TEXT[pwaState.offlineSupport] }}
      </MudText>

      <MudDivider class="my-4" />

      <MudText
        data-testid="settings-app-storage"
        typo="body2"
      >
        {{ describeStorage(storage) }}
      </MudText>
      <MudButton
        class="mt-3"
        color="primary"
        data-testid="settings-app-clear-cache"
        :disabled="isClearing"
        variant="outlined"
        @click="clearCache()"
      >
        Очистить офлайн-кэш
      </MudButton>
      <div
        aria-live="polite"
        role="status"
      >
        <MudText
          v-if="CACHE_CLEAR_TEXT[clearResult] !== null"
          class="mt-2"
          data-testid="settings-app-clear-result"
          typo="body2"
        >
          {{ CACHE_CLEAR_TEXT[clearResult] }}
        </MudText>
      </div>
    </MudCardContent>
  </MudCard>
</template>
