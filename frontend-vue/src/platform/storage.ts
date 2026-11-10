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

export function watchSetting<T>(key: string, fallback: T, onChange: (value: T) => void): () => void {
  if (typeof window === 'undefined') {
    return () => undefined
  }

  const targetKey = PREFIX + key

  const listener = (event: StorageEvent): void => {
    if (event.key !== null && event.key !== targetKey) {
      return
    }

    if (event.newValue === null) {
      onChange(fallback)
      return
    }

    try {
      onChange(JSON.parse(event.newValue) as T)
    } catch {
      onChange(fallback)
    }
  }

  window.addEventListener('storage', listener)

  return () => {
    window.removeEventListener('storage', listener)
  }
}
