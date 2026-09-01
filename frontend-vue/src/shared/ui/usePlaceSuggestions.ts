import { fetchPlaces } from '@/api/adapters/operations'

const PLACE_COUNT = 10

const cache = new Map<string, readonly string[]>()
let lastSearchedValue = ''

export interface PlaceSuggestionsState {
  find: (value: string) => Promise<readonly string[]>
  reset: () => void
}

export function usePlaceSuggestions(): PlaceSuggestionsState {
  return { find: findPlaces, reset: resetPlaces }
}

export async function findPlaces(value: string): Promise<readonly string[]> {
  const cached = cache.get(value)

  if (cached !== undefined) {
    return cached
  }

  const diff = value.length - lastSearchedValue.length

  if (diff > 0
    && value.slice(0, value.length - diff) === lastSearchedValue
    && cache.get(lastSearchedValue)?.length === 0) {
    return []
  }

  const places = await fetchPlaces(0, PLACE_COUNT, value.length === 0 ? null : value)

  cache.set(value, places)
  lastSearchedValue = value

  return places
}

export function resetPlaces(): void {
  cache.clear()
  lastSearchedValue = ''
}
