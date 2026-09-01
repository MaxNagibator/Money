import type { CategoryDraft } from '@/api/adapters/categories'
import type { Category } from '@/domain/category'
import type { OperationTypeId } from '@/domain/operation-type'

export interface CategoryFormValue {
  id: number | null
  name: string
  order: number | null
  color: string | null
  parentId: number | null
  operationTypeId: OperationTypeId
}

export function newCategoryForm(operationTypeId: OperationTypeId, parentId: number | null): CategoryFormValue {
  return {
    id: null,
    name: '',
    order: null,
    color: randomColor(),
    parentId,
    operationTypeId,
  }
}

export function editCategoryForm(category: Category): CategoryFormValue {
  return {
    id: category.id,
    name: category.name,
    order: category.order,
    color: category.color,
    parentId: category.parentId,
    operationTypeId: category.operationTypeId,
  }
}

export function toDraft(value: CategoryFormValue): CategoryDraft {
  return {
    name: value.name.trim(),
    order: value.order,
    color: value.color,
    parentId: value.parentId,
    operationTypeId: value.operationTypeId,
  }
}

function randomColor(): string {
  const channels = Array.from({ length: 3 }, () => Math.floor(Math.random() * 256))

  return `#${channels.map(channel => channel.toString(16).padStart(2, '0')).join('')}`
}
