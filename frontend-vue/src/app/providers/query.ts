import type { VueQueryPluginOptions } from '@tanstack/vue-query'
import { QueryClient } from '@tanstack/vue-query'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
      staleTime: 30_000,
      networkMode: 'offlineFirst',
    },
    mutations: {
      retry: false,
      networkMode: 'always',
    },
  },
})

export const queryOptions: VueQueryPluginOptions = { queryClient }
