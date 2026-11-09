import { fetchDebtOwnerNames } from '@/api/adapters/debts'

const OWNER_COUNT = 10
const CACHE_CAPACITY = 64

const cache = new Map<string, readonly string[]>()
let lastSearchedValue = ''

export interface OwnerSuggestionsState {
  find: (value: string) => Promise<readonly string[]>
  reset: () => void
}

export function useOwnerSuggestions(): OwnerSuggestionsState {
  return { find: findOwners, reset: resetOwners }
}

export async function findOwners(value: string): Promise<readonly string[]> {
  const cached = cache.get(value)

  if (cached !== undefined) {
    return cached
  }

  if (value.length > lastSearchedValue.length
    && value.toLocaleLowerCase().startsWith(lastSearchedValue.toLocaleLowerCase())
    && cache.get(lastSearchedValue)?.length === 0) {
    return []
  }

  const owners = await fetchDebtOwnerNames(0, OWNER_COUNT, value.length === 0 ? null : value)

  addToCache(value, owners)
  lastSearchedValue = value

  return owners
}

export function resetOwners(): void {
  cache.clear()
  lastSearchedValue = ''
}

function addToCache(key: string, value: readonly string[]): void {
  if (cache.has(key)) {
    cache.delete(key)
  } else if (cache.size >= CACHE_CAPACITY) {
    const oldest = cache.keys().next()

    if (oldest.done !== true) {
      cache.delete(oldest.value)
    }
  }

  cache.set(key, value)
}
