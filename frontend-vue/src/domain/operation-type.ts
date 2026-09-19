import { mdArrowCircleDown, mdArrowCircleUp } from '@/shared/icons'

export type OperationTypeId = 1 | 2

export interface OperationType {
  id: OperationTypeId
  name: string
  addLabel: string
  shortAddLabel: string
  newTitle: string
  editTitle: string
  color: string
  icon: string
  sign: string
}

export const OPERATION_TYPES: readonly OperationType[] = [
  {
    id: 1,
    name: 'Расходы',
    addLabel: 'Добавить расходы',
    shortAddLabel: 'Расход',
    newTitle: 'Новый расход',
    editTitle: 'Правка расхода',
    color: 'warning',
    icon: mdArrowCircleDown,
    sign: '−',
  },
  {
    id: 2,
    name: 'Доходы',
    addLabel: 'Добавить доходы',
    shortAddLabel: 'Доход',
    newTitle: 'Новый доход',
    editTitle: 'Правка дохода',
    color: 'success',
    icon: mdArrowCircleUp,
    sign: '+',
  },
]

export function isOperationTypeId(value: number): value is OperationTypeId {
  return OPERATION_TYPES.some(type => type.id === value)
}

export function operationTypeById(id: OperationTypeId): OperationType {
  const type = OPERATION_TYPES.find(item => item.id === id)

  if (type === undefined) {
    throw new Error(`Неизвестный тип операции: ${id}`)
  }

  return type
}
