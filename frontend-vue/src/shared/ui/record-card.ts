import type { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'
import { operationTypeById } from '@/domain/operation-type'
import { formatMoney } from '@/platform/format'

const ROLE_VARIABLE = '--record-card-role'
const ROLE_EDGE_VARIABLE = '--record-card-role-edge'
const UNKNOWN_ROLE = 'warning'

export type RecordCardRole = Record<string, string>

export function roleVariables(role: string): RecordCardRole {
  return {
    [ROLE_VARIABLE]: `var(--mud-palette-${role}-rgb)`,
    [ROLE_EDGE_VARIABLE]: `var(--mud-palette-${role}-darken)`,
  }
}

export function operationRoleVariables(operationTypeId: OperationTypeId | null): RecordCardRole {
  return roleVariables(operationTypeId === null ? UNKNOWN_ROLE : operationTypeById(operationTypeId).color)
}

export function recordSumLabel(operationTypeId: OperationTypeId | null, sum: Money): string {
  const kind = operationTypeId === null ? 'сумма' : operationTypeById(operationTypeId).shortAddLabel.toLowerCase()

  return `${kind} ${formatMoney(sum)}`
}

export function recordRowName(label: string, sumLabel: string | null, deletedMark: string | null = null): string {
  return [label, sumLabel, deletedMark].filter(part => part !== null).join(', ')
}
