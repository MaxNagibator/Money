import { inferCategory } from '@/api/adapters/operations'
import type { OperationTypeId } from '@/domain/operation-type'

const cache = new Map<string, number | null>()

export async function inferCategoryCached(place: string, typeId: OperationTypeId): Promise<number | null> {
  const key = `${typeId}:${place}`
  const cached = cache.get(key)

  if (cached !== undefined) {
    return cached
  }

  let suggestion: number | null

  try {
    suggestion = await inferCategory(place, typeId)
  } catch {
    suggestion = null
  }

  cache.set(key, suggestion)

  return suggestion
}

export function resetCategoryInference(): void {
  cache.clear()
}
