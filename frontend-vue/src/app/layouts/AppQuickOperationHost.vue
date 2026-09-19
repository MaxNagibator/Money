<script setup lang="ts">
import type { Component } from 'vue'
import { defineAsyncComponent, onErrorCaptured, ref } from 'vue'
import { logAppError } from '@/app/errors'
import AppQuickOperationError from './AppQuickOperationError.vue'

const QUICK_OPERATION_RETRIES = 1
const QUICK_OPERATION_INFO = 'форма быстрой операции'

const hasFailed = ref(false)
const isFormReady = ref(false)

// TODO: повтор идёт по тому же адресу чанка, а браузер помнит отказ в карте модулей, поэтому после выката форму возвращает только перезагрузка страницы; снять, когда у динамического импорта появится адрес с обходом кэша
const form = defineAsyncComponent<Component>({
  loader: () => import('./AppQuickOperation.vue'),
  onError: (_error, retry, fail, attempts) => {
    if (attempts <= QUICK_OPERATION_RETRIES) {
      retry()
    } else {
      fail()
    }
  },
})

onErrorCaptured(reason => {
  if (isFormReady.value) {
    return true
  }

  hasFailed.value = true
  logAppError(reason, QUICK_OPERATION_INFO)

  return false
})
</script>

<template>
  <AppQuickOperationError v-if="hasFailed" />

  <component
    :is="form"
    v-else
    @vue:mounted="isFormReady = true"
  />
</template>
