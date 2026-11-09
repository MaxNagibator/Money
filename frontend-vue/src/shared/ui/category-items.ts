import type { Category } from '@/domain/category'
import { buildCategoryTree } from '@/domain/category'
import type { CategoryNode } from '@/domain/category'
import { OPERATION_TYPES } from '@/domain/operation-type'
import type { MudColor, MudTreeItemData } from '@/shared/mud'

function toItem(node: CategoryNode): MudTreeItemData {
  const type = OPERATION_TYPES.find(candidate => candidate.id === node.category.operationTypeId)

  const base: MudTreeItemData = {
    value: node.category.id,
    text: node.category.name,
    endIcon: type?.icon,
    endIconColor: type?.color as MudColor | undefined,
  }

  return node.children.length === 0 ? base : { ...base, children: node.children.map(toItem) }
}

export function categoryTreeItems(categories: readonly Category[]): MudTreeItemData[] {
  return OPERATION_TYPES.flatMap(type => buildCategoryTree(categories, type.id)).map(toItem)
}
