import '@fontsource/roboto/300.css'
import '@fontsource/roboto/400.css'
import '@fontsource/roboto/500.css'
import '@fontsource/roboto/700.css'
import { VueQueryPlugin } from '@tanstack/vue-query'
import { createPinia } from 'pinia'
import { createApp } from 'vue'
import { setAccessTokenProvider, setAccessTokenRefresher } from '@/api/transport/client'
import { setApiBaseUrlSource } from '@/platform/http'
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
