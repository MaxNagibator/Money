import { externalAuthUrl } from '../api/auth-api'

export const AUTH_PROVIDER = 'auth'
export const GITHUB_PROVIDER = 'github'
export const CALLBACK_PATH = '/Account/Callback'

export async function startExternalLogin(provider: string): Promise<void> {
  window.location.assign(await externalAuthUrl(provider, `${window.location.origin}${CALLBACK_PATH}`))
}
