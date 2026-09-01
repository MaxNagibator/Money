import type { components } from '@/api/generated/schema'
import { apiClient } from '@/api/transport/client'
import type { Category } from '@/domain/category'
import { isOperationTypeId } from '@/domain/operation-type'
import { fail } from './problem'

type CategoryDto = components['schemas']['Money.Api.Dto.Categories.CategoryDto']

export const CATEGORIES_QUERY_KEY = ['categories']

export interface CategoryDraft {
  name: string
  color: string | null
  order: number | null
  parentId: number | null
  operationTypeId: number
}

export async function fetchCategories(): Promise<Category[]> {
  const { data, error } = await apiClient.GET('/Categories')

  fail(error, 'Не удалось загрузить категории')

  return (data ?? []).flatMap(toCategory)
}

export async function fetchCategory(id: number): Promise<Category | null> {
  const { data, error } = await apiClient.GET('/Categories/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось загрузить категорию')

  return data === undefined ? null : (toCategory(data)[0] ?? null)
}

export async function createCategory(draft: CategoryDraft): Promise<number> {
  const { data, error } = await apiClient.POST('/Categories', { body: draft })

  fail(error, 'Не удалось создать категорию')

  return data ?? 0
}

export async function updateCategory(id: number, draft: CategoryDraft): Promise<void> {
  const { error } = await apiClient.PUT('/Categories/{id}', {
    params: { path: { id } },
    body: draft,
  })

  fail(error, 'Не удалось сохранить категорию')
}

export async function deleteCategory(id: number): Promise<void> {
  const { error } = await apiClient.DELETE('/Categories/{id}', { params: { path: { id } } })

  fail(error, 'Не удалось удалить категорию')
}

export async function restoreCategory(id: number): Promise<void> {
  const { error } = await apiClient.POST('/Categories/{id}/Restore', { params: { path: { id } } })

  fail(error, 'Не удалось восстановить категорию')
}

function toCategory(dto: CategoryDto): Category[] {
  if (dto.id === undefined || dto.name === null || !isOperationTypeId(dto.operationTypeId)) {
    return []
  }

  return [{
    id: dto.id,
    parentId: dto.parentId ?? null,
    name: dto.name,
    color: dto.color ?? null,
    order: dto.order ?? null,
    operationTypeId: dto.operationTypeId,
    isDeleted: false,
  }]
}
