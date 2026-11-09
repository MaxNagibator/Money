import type { Category } from '@/domain/category'
import type { LocalDate } from '@/domain/local-date'
import { addDays, compareLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { OperationTypeId } from '@/domain/operation-type'

export interface Operation {
  id: number
  categoryId: number
  category: Category | null
  sum: Money
  comment: string | null
  place: string | null
  date: LocalDate
  createdTaskId: number | null
  isDeleted: boolean
}

export interface OperationsDay {
  date: LocalDate
  operations: Operation[]
}

export function groupOperationsByDay(operations: readonly Operation[]): OperationsDay[] {
  const byDate = new Map<LocalDate, Operation[]>()

  for (const operation of operations) {
    const collected = byDate.get(operation.date)

    if (collected === undefined) {
      byDate.set(operation.date, [operation])
      continue
    }

    collected.push(operation)
  }

  return [...byDate]
    .map(([date, dayOperations]) => ({ date, operations: dayOperations }))
    .sort(byDateDescending)
}

export function fillEmptyDays(days: readonly OperationsDay[]): OperationsDay[] {
  const ordered = [...days].sort(byDateDescending)
  const filled: OperationsDay[] = []

  for (const [index, day] of ordered.entries()) {
    filled.push(day)

    const older = ordered[index + 1]

    if (older === undefined) {
      continue
    }

    let gap = addDays(day.date, -1)

    while (compareLocalDate(gap, older.date) > 0) {
      filled.push({ date: gap, operations: [] })
      gap = addDays(gap, -1)
    }
  }

  return filled
}

const dayTotals = new WeakMap<OperationsDay, Map<OperationTypeId, Money>>()

export function calculateDayTotal(day: OperationsDay, operationTypeId: OperationTypeId): Money {
  let totals = dayTotals.get(day)

  if (totals === undefined) {
    totals = new Map()
    dayTotals.set(day, totals)
  }

  const cached = totals.get(operationTypeId)

  if (cached !== undefined) {
    return cached
  }

  const total = day.operations
    .filter(operation => !operation.isDeleted && operation.category?.operationTypeId === operationTypeId)
    .reduce((sum, operation) => sum.add(operation.sum), Money.zero)

  totals.set(operationTypeId, total)

  return total
}

function byDateDescending(left: OperationsDay, right: OperationsDay): number {
  return compareLocalDate(right.date, left.date)
}
