import type { VueQueryPluginOptions } from '@tanstack/vue-query'
import { QueryClient } from '@tanstack/vue-query'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
      staleTime: 30_000,
    },
    mutations: {
      retry: false,
    },
  },
})

export const queryOptions: VueQueryPluginOptions = { queryClient }
