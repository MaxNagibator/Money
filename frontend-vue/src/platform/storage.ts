const PREFIX = 'money:'

export function readSetting<T>(key: string, fallback: T): T {
  const raw = localStorage.getItem(PREFIX + key)

  if (raw === null) {
    return fallback
  }

  try {
    return JSON.parse(raw) as T
  } catch {
    return fallback
  }
}

export function writeSetting<T>(key: string, value: T): void {
  localStorage.setItem(PREFIX + key, JSON.stringify(value))
}

export function tryWriteSetting<T>(key: string, value: T): boolean {
  try {
    writeSetting(key, value)

    return true
  } catch {
    return false
  }
}

export function removeSetting(key: string): void {
  localStorage.removeItem(PREFIX + key)
}
