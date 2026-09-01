import { apiClient } from '@/api/transport/client'
import { fail } from './problem'

export const REGISTER_ERROR = 'Ошибка регистрации аккаунта на сервере.'

export interface AccountRegistration {
  userName: string
  email: string | null
  password: string
}

export async function registerAccount(registration: AccountRegistration): Promise<void> {
  const { error } = await apiClient.POST('/Accounts/Register', {
    body: {
      userName: registration.userName,
      email: registration.email,
      password: registration.password,
    },
  })

  fail(error, REGISTER_ERROR)
}
