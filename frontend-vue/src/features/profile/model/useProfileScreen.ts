import type { ComputedRef, Ref } from 'vue'
import { computed, onScopeDispose, ref } from 'vue'
import {
  ANONYMOUS_NAME,
  CHANGE_PASSWORD_ERROR,
  CONFIRM_EMAIL_ERROR,
  RESEND_CODE_ERROR,
} from '@/api/adapters/profile'
import type { Clock } from '@/platform/clock'
import { systemClock } from '@/platform/clock'
import type { ConfirmFormErrors, ConfirmFormValue } from './confirm-form'
import { newConfirmForm, toConfirmCode, validateConfirmForm } from './confirm-form'
import type { PasswordFormErrors, PasswordFormValue } from './password-form'
import { newPasswordForm, toPasswordChange, validatePasswordForm } from './password-form'
import type { ProfileState } from './useProfile'
import { useProfile } from './useProfile'

export const RESEND_COOLDOWN_SECONDS = 30
export const RESEND_LABEL = 'Отправить код'
export const EMAIL_CONFIRMED_NOTICE = 'Почта успешно подтверждена!'
export const CODE_SENT_NOTICE = 'Новый код отправлен на вашу почту'
export const PASSWORD_CHANGED_NOTICE = 'Пароль успешно поменян!'

export interface ProfileNotice {
  text: string
  type: 'success' | 'warning'
}

export interface ProfileScreenState {
  profile: ProfileState
  name: ComputedRef<string>
  email: ComputedRef<string | null>
  isEmailConfirmed: ComputedRef<boolean>
  notice: Ref<ProfileNotice | null>
  confirmForm: Ref<ConfirmFormValue>
  confirmErrors: Ref<ConfirmFormErrors>
  isConfirming: Ref<boolean>
  passwordForm: Ref<PasswordFormValue>
  passwordErrors: Ref<PasswordFormErrors>
  isChangingPassword: Ref<boolean>
  isPasswordVisible: Ref<boolean>
  isResending: Ref<boolean>
  resendSecondsLeft: Ref<number>
  resendLabel: ComputedRef<string>
  canResend: ComputedRef<boolean>
  submitConfirm: () => Promise<void>
  resendCode: () => Promise<void>
  submitPassword: () => Promise<void>
}

export function useProfileScreen(clock: Clock = systemClock): ProfileScreenState {
  const profile = useProfile()

  const notice = ref<ProfileNotice | null>(null)

  const confirmForm = ref<ConfirmFormValue>(newConfirmForm())
  const confirmErrors = ref<ConfirmFormErrors>({})
  const isConfirming = ref(false)

  const passwordForm = ref<PasswordFormValue>(newPasswordForm())
  const passwordErrors = ref<PasswordFormErrors>({})
  const isChangingPassword = ref(false)
  const isPasswordVisible = ref(false)

  const isResending = ref(false)
  const resendSecondsLeft = ref(0)

  let deadline: number | null = null
  let ticker: ReturnType<typeof setInterval> | null = null

  function stopCooldown(): void {
    deadline = null
    resendSecondsLeft.value = 0

    if (ticker !== null) {
      clearInterval(ticker)
      ticker = null
    }
  }

  function refreshCooldown(): void {
    if (deadline === null) {
      return
    }

    const left = Math.max(0, Math.ceil((deadline - clock.now().getTime()) / 1000))

    resendSecondsLeft.value = left

    if (left === 0) {
      stopCooldown()
    }
  }

  function startCooldown(): void {
    deadline = clock.now().getTime() + RESEND_COOLDOWN_SECONDS * 1000
    resendSecondsLeft.value = RESEND_COOLDOWN_SECONDS
    ticker ??= setInterval(refreshCooldown, 1000)
  }

  onScopeDispose(stopCooldown)

  return {
    profile,
    name: computed(() => profile.profile.value?.name ?? ANONYMOUS_NAME),
    email: computed(() => profile.profile.value?.email ?? null),
    isEmailConfirmed: computed(() => profile.profile.value?.isEmailConfirmed ?? false),
    notice,
    confirmForm,
    confirmErrors,
    isConfirming,
    passwordForm,
    passwordErrors,
    isChangingPassword,
    isPasswordVisible,
    isResending,
    resendSecondsLeft,
    resendLabel: computed(() => resendSecondsLeft.value > 0
      ? `${RESEND_LABEL} (${resendSecondsLeft.value} сек)`
      : RESEND_LABEL),
    canResend: computed(() => resendSecondsLeft.value === 0 && !isResending.value),
    submitConfirm: async () => {
      confirmErrors.value = validateConfirmForm(confirmForm.value)

      if (Object.keys(confirmErrors.value).length > 0) {
        return
      }

      isConfirming.value = true

      try {
        await profile.confirm(toConfirmCode(confirmForm.value))
        stopCooldown()
        notice.value = { text: EMAIL_CONFIRMED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, CONFIRM_EMAIL_ERROR), type: 'warning' }
      } finally {
        isConfirming.value = false
      }
    },
    resendCode: async () => {
      isResending.value = true

      try {
        await profile.resend()
        startCooldown()
        notice.value = { text: CODE_SENT_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, RESEND_CODE_ERROR), type: 'warning' }
      } finally {
        isResending.value = false
      }
    },
    submitPassword: async () => {
      passwordErrors.value = validatePasswordForm(passwordForm.value)

      if (Object.keys(passwordErrors.value).length > 0) {
        return
      }

      isChangingPassword.value = true

      try {
        await profile.changePassword(toPasswordChange(passwordForm.value))
        passwordForm.value = newPasswordForm()
        notice.value = { text: PASSWORD_CHANGED_NOTICE, type: 'success' }
      } catch (error) {
        notice.value = { text: messageOf(error, CHANGE_PASSWORD_ERROR), type: 'warning' }
      } finally {
        isChangingPassword.value = false
      }
    },
  }
}

function messageOf(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}
