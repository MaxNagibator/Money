import '@/app/fonts.css'
import { VueQueryPlugin } from '@tanstack/vue-query'
import { createPinia } from 'pinia'
import { createApp } from 'vue'
import { setAccessTokenProvider, setAccessTokenRefresher } from '@/api/transport/client'
import { setApiBaseUrlSource } from '@/platform/http'
import { removeServiceWorker, startServiceWorker, watchAppEnvironment } from '@/platform/pwa'
import { loadApiBaseUrl } from '@/platform/runtime-config'
import App from '@/app/App.vue'
import { handleAppError } from '@/app/errors'
import { syncMudTheme } from '@/app/providers/mud-theme'
import { ensureMudPopoverContainer } from '@/shared/mud/popover'
import { queryClient, queryOptions } from '@/app/providers/query'
import '@/app/providers/styles'
import { resolveSessionExit } from '@/app/route-guard'
import { router } from '@/app/router'
import { setSessionChangeHandler, useSessionStore } from '@/features/auth'

const pinia = createPinia()

setAccessTokenProvider(() => useSessionStore(pinia).getAccessToken())
setAccessTokenRefresher(staleToken => useSessionStore(pinia).refreshAccessToken(staleToken))
setSessionChangeHandler(change => {
  if (change === 'signedIn') {
    void queryClient.resetQueries()
  } else {
    queryClient.clear()
  }

  const exit = resolveSessionExit(change, router.currentRoute.value)

  if (exit !== null) {
    void router.replace(exit)
  }
})

setApiBaseUrlSource(loadApiBaseUrl())
watchAppEnvironment()

if (import.meta.env.PROD && __PWA_SELF_DESTROY__) {
  void removeServiceWorker()
} else if (import.meta.env.PROD) {
  void startServiceWorker(() => import('virtual:pwa-register').then(module => module.registerSW))
}

syncMudTheme()
ensureMudPopoverContainer()

const app = createApp(App)

app.config.errorHandler = (reason, _instance, info) => {
  handleAppError(reason, info)
}

app
  .use(pinia)
  .use(router)
  .use(VueQueryPlugin, queryOptions)
  .mount('#app')
