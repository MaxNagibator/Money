import { useQuery } from '@tanstack/vue-query'
import type { ComputedRef } from 'vue'
import { computed } from 'vue'
import { fetchProfile, PROFILE_QUERY_KEY } from '@/api/adapters/profile'
import { useSessionStore } from './session'

export function useCurrentUserName(): ComputedRef<string | null> {
  const session = useSessionStore()
  const isAuthenticated = computed(() => session.isAuthenticated)

  const query = useQuery({
    queryKey: PROFILE_QUERY_KEY,
    queryFn: fetchProfile,
    enabled: isAuthenticated,
  })

  return computed(() => isAuthenticated.value ? query.data.value?.name ?? null : null)
}
