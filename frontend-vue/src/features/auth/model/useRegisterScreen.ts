import type { Ref } from 'vue'
import { ref } from 'vue'
import { REGISTER_ERROR, registerAccount } from '@/api/adapters/accounts'
import type { RegisterFormErrors, RegisterFormValue } from './register-form'
import { newRegisterForm, toRegistration, validateRegisterForm } from './register-form'
import { useSessionStore } from './session'
import { SIGN_IN_ERROR } from './sign-in-messages'

export const REGISTER_FAILURE = 'Ошибка во время регистрации'
export const SIGN_IN_AFTER_REGISTER_FAILURE = 'Регистрация прошла успешно, но войти не удалось:'

export interface RegisterNotice {
  text: string
  type: 'error' | 'warning'
}

export interface RegisterScreenState {
  form: Ref<RegisterFormValue>
  errors: Ref<RegisterFormErrors>
  notice: Ref<RegisterNotice | null>
  isBusy: Ref<boolean>
  submit: () => Promise<boolean>
}

export function useRegisterScreen(): RegisterScreenState {
  const session = useSessionStore()

  const form = ref<RegisterFormValue>(newRegisterForm())
  const errors = ref<RegisterFormErrors>({})
  const notice = ref<RegisterNotice | null>(null)
  const isBusy = ref(false)

  let createdName: string | null = null

  return {
    form,
    errors,
    notice,
    isBusy,
    submit: async () => {
      errors.value = validateRegisterForm(form.value)

      if (Object.keys(errors.value).length > 0) {
        return false
      }

      const registration = toRegistration(form.value)

      isBusy.value = true
      notice.value = null

      try {
        if (createdName !== registration.userName) {
          try {
            await registerAccount(registration)
          } catch (error) {
            notice.value = { text: `${REGISTER_FAILURE} ${messageOf(error, REGISTER_ERROR)}`, type: 'error' }
            return false
          }

          createdName = registration.userName
        }

        try {
          await session.signIn(registration.userName, registration.password)
        } catch (error) {
          notice.value = {
            text: `${SIGN_IN_AFTER_REGISTER_FAILURE} ${messageOf(error, SIGN_IN_ERROR)}`,
            type: 'warning',
          }

          return false
        }

        return true
      } finally {
        isBusy.value = false
      }
    },
  }
}

function messageOf(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}
