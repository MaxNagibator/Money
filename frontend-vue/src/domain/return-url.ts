export const DEFAULT_RETURN_URL = '/'
export const SIGNED_IN_RETURN_URL = '/operations'

export function readSignedInReturnUrl(value: unknown): string {
  return value === undefined ? SIGNED_IN_RETURN_URL : readSafeReturnUrl(value)
}

export function readSafeReturnUrl(value: unknown, fallback: string = DEFAULT_RETURN_URL): string {
  if (typeof value !== 'string' || !value.startsWith('/')) {
    return fallback
  }

  const next = value.charAt(1)

  return next === '/' || next === '\\' ? fallback : value
}
