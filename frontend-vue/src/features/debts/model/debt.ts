import Decimal from 'decimal.js'
import type { Debt } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { compareLocalDate, parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'

export interface DebtType {
  id: number
  name: string
  addLabel: string
}

export interface DebtOwnerGroup {
  name: string
  debts: Debt[]
}

export interface DebtTypeGroup {
  typeId: number
  name: string
  owners: DebtOwnerGroup[]
}

export interface DebtPaymentRecord {
  date: LocalDate | null
  sum: Money
  comment: string
}

export type DebtStatus = 'paid' | 'inProgress' | 'notStarted'

export const DEBT_TYPES: readonly DebtType[] = [
  { id: 1, name: 'Нужно забрать', addLabel: 'Забрать своё' },
  { id: 2, name: 'Нужно отдать', addLabel: 'Отдать чужое' },
]

export const FORGIVABLE_TYPE_ID = 1
export const UNKNOWN_TYPE_NAME = 'Неизвестный тип'
export const NO_PAYMENT_MARK = '∞'

export const DEBT_STATUS_TEXT: Record<DebtStatus, string> = {
  paid: 'Погашено',
  inProgress: 'В процессе',
  notStarted: 'Не начато',
}

const PERCENT_SCALE = 2
const FULL_PROGRESS = 100
const ENTRY_PARTS = 3

export function debtTypeName(typeId: number): string {
  return DEBT_TYPES.find(type => type.id === typeId)?.name ?? UNKNOWN_TYPE_NAME
}

export function groupDebts(debts: readonly Debt[]): DebtTypeGroup[] {
  const byType = new Map<number, Map<string, Debt[]>>()

  for (const debt of [...debts].sort(byDateDescending)) {
    const owners = byType.get(debt.typeId) ?? new Map<string, Debt[]>()
    const collected = owners.get(debt.ownerName) ?? []

    collected.push(debt)
    owners.set(debt.ownerName, collected)
    byType.set(debt.typeId, owners)
  }

  return [...byType]
    .map(([typeId, owners]) => ({
      typeId,
      name: debtTypeName(typeId),
      owners: [...owners].map(([name, ownerDebts]) => ({ name, debts: ownerDebts })),
    }))
    .sort((left, right) => right.typeId - left.typeId)
}

export function filterDebtGroups(groups: readonly DebtTypeGroup[], query: string): DebtTypeGroup[] {
  const term = query.trim().toLowerCase()

  if (term.length === 0) {
    return [...groups]
  }

  return groups.flatMap(group => {
    const owners = group.owners.filter(owner => matchesSearch(owner, term))

    return owners.length === 0 ? [] : [{ ...group, owners }]
  })
}

export function matchesSearch(owner: DebtOwnerGroup, query: string): boolean {
  const term = query.trim().toLowerCase()

  if (term.length === 0) {
    return true
  }

  if (owner.name.toLowerCase().includes(term)) {
    return true
  }

  return owner.debts.some(debt => contains(debt.comment, term) || contains(debt.payComment, term))
}

export function ownerTotal(owner: DebtOwnerGroup): Money {
  return owner.debts.reduce((total, debt) => total.add(debt.sum), Money.zero)
}

export function ownerPaid(owner: DebtOwnerGroup): Money {
  return owner.debts.reduce((total, debt) => total.add(debt.paySum), Money.zero)
}

export function ownerRemainder(owner: DebtOwnerGroup): Money {
  return owner.debts.reduce((total, debt) => total.add(debt.sum).subtract(debt.paySum), Money.zero)
}

export function ownerPercent(owner: DebtOwnerGroup): number {
  const total = ownerTotal(owner)

  if (total.isZero || total.isNegative) {
    return 0
  }

  return ownerPaid(owner)
    .toDecimal()
    .dividedBy(total.toDecimal())
    .toDecimalPlaces(PERCENT_SCALE, Decimal.ROUND_HALF_EVEN)
    .times(FULL_PROGRESS)
    .toNumber()
}

export function typeTotal(group: DebtTypeGroup): Money {
  return group.owners.reduce((total, owner) => total.add(ownerTotal(owner)), Money.zero)
}

export function lastPaymentDate(owner: DebtOwnerGroup): LocalDate | null {
  const newest = owner.debts.reduce<Debt | null>(
    (found, debt) => found === null || compareLocalDate(debt.date, found.date) > 0 ? debt : found,
    null,
  )

  const lines = splitLines(newest?.payComment ?? null)
  const last = lines.at(-1)

  if (last === undefined) {
    return null
  }

  const space = last.indexOf(' ')

  return parsePaymentDate(space < 0 ? last : last.slice(0, space))
}

export function parsePaymentHistory(payComment: string | null): DebtPaymentRecord[] {
  return splitLines(payComment).flatMap(line => {
    const [rawDate, rawSum, rawComment] = splitEntry(line)

    if (rawDate === undefined || rawSum === undefined) {
      return []
    }

    return [{
      date: parsePaymentDate(rawDate),
      sum: Money.parseInput(rawSum) ?? Money.zero,
      comment: rawComment ?? '',
    }]
  })
}

export function hasPaymentHistory(debt: Debt): boolean {
  return debt.paySum.compare(Money.zero) > 0 && splitLines(debt.payComment).length > 0
}

export function paymentProgress(debt: Debt): number {
  if (debt.sum.isZero) {
    return 0
  }

  const progress = debt.paySum
    .toDecimal()
    .dividedBy(debt.sum.toDecimal())
    .times(FULL_PROGRESS)
    .toNumber()

  return Math.trunc(Math.min(progress, FULL_PROGRESS))
}

export function debtStatus(debt: Debt): DebtStatus {
  const progress = paymentProgress(debt)

  if (progress >= FULL_PROGRESS) {
    return 'paid'
  }

  return progress > 0 ? 'inProgress' : 'notStarted'
}

export function canForgiveDebt(debt: Debt): boolean {
  return debt.typeId === FORGIVABLE_TYPE_ID && !debt.isDeleted && debt.paySum.compare(debt.sum) < 0
}

function contains(value: string | null, term: string): boolean {
  return value !== null && value.toLowerCase().includes(term)
}

function splitLines(value: string | null): string[] {
  if (value === null) {
    return []
  }

  return value
    .split(/\r?\n/)
    .map(line => line.trim())
    .filter(line => line.length > 0)
}

function splitEntry(line: string): string[] {
  const parts: string[] = []
  let rest = line.trim()

  while (parts.length < ENTRY_PARTS - 1 && rest.length > 0) {
    const space = rest.indexOf(' ')

    if (space < 0) {
      parts.push(rest)
      rest = ''
      break
    }

    parts.push(rest.slice(0, space))
    rest = rest.slice(space + 1).trim()
  }

  if (rest.length > 0) {
    parts.push(rest)
  }

  return parts
}

function parsePaymentDate(value: string): LocalDate | null {
  return parseLocalDate(value.replaceAll('.', '-'))
}

function byDateDescending(left: Debt, right: Debt): number {
  return compareLocalDate(right.date, left.date)
}
