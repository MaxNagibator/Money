import type { OperationTypeId } from '@/domain/operation-type'
import { operationTypeById } from '@/domain/operation-type'

const ROLE_VARIABLE = '--record-card-role'
const UNKNOWN_ROLE = 'warning'

export type RecordCardRole = Record<string, string>

export function roleVariables(role: string): RecordCardRole {
  return { [ROLE_VARIABLE]: `var(--mud-palette-${role}-rgb)` }
}

export function operationRoleVariables(operationTypeId: OperationTypeId | null): RecordCardRole {
  return roleVariables(operationTypeId === null ? UNKNOWN_ROLE : operationTypeById(operationTypeId).color)
}
