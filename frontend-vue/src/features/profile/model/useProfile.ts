import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import type { ComputedRef, Ref } from 'vue'
import { computed } from 'vue'
import type { PasswordChange, UserProfile } from '@/api/adapters/profile'
import {
  changePassword,
  confirmEmail,
  fetchProfile,
  PROFILE_QUERY_KEY,
  resendConfirmCode,
} from '@/api/adapters/profile'

export interface ProfileState {
  profile: ComputedRef<UserProfile | null>
  isLoading: Ref<boolean>
  loadError: ComputedRef<string | null>
  confirm: (code: string) => Promise<void>
  resend: () => Promise<void>
  changePassword: (change: PasswordChange) => Promise<void>
}

export function useProfile(): ProfileState {
  const queryClient = useQueryClient()

  const query = useQuery({
    queryKey: PROFILE_QUERY_KEY,
    queryFn: fetchProfile,
  })

  const confirmMutation = useMutation({
    mutationFn: (code: string) => confirmEmail(code),
    onSuccess: async () => {
      queryClient.setQueryData<UserProfile>(PROFILE_QUERY_KEY, current =>
        current === undefined ? current : { ...current, isEmailConfirmed: true })

      await queryClient.invalidateQueries({ queryKey: PROFILE_QUERY_KEY })
    },
  })

  const resendMutation = useMutation({ mutationFn: () => resendConfirmCode() })
  const passwordMutation = useMutation({ mutationFn: (change: PasswordChange) => changePassword(change) })

  return {
    profile: computed(() => query.data.value ?? null),
    isLoading: query.isPending,
    loadError: computed(() => {
      const error = query.error.value

      return error === null ? null : error.message
    }),
    confirm: async code => {
      await confirmMutation.mutateAsync(code)
    },
    resend: async () => {
      await resendMutation.mutateAsync()
    },
    changePassword: async change => {
      await passwordMutation.mutateAsync(change)
    },
  }
}
