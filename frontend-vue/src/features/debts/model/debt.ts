import type { DebtMovement } from '@/domain/debt'
import { compareLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { mdInput, mdOutput } from '@/shared/icons'

export interface DebtType {
  id: number
  name: string
  addLabel: string
  icon: string
}

export interface DebtOwnerGroup {
  name: string
  debts: DebtMovement[]
}

export interface DebtTypeGroup {
  typeId: number
  name: string
  owners: DebtOwnerGroup[]
}

export const DEBT_TYPES: readonly DebtType[] = [
  { id: 1, name: 'Мне должны', addLabel: 'Дал в долг', icon: mdInput },
  { id: 2, name: 'Я должен', addLabel: 'Занял', icon: mdOutput },
]

export const TAKE_TYPE_ID = 1
export const GIVE_TYPE_ID = 2
export const FORGIVABLE_TYPE_ID = TAKE_TYPE_ID
export const UNKNOWN_TYPE_NAME = 'Неизвестный тип'
export const DEBT_OWNERS_GROUP_ID = 0
export const DEBT_OWNERS_GROUP_NAME = 'Держатели'

const DEBT_COUNT_FORMS = ['долгов', 'долг', 'долга', 'долга', 'долга'] as const
const TEEN_FROM = 5
const TEEN_TO = 20
const LAST_FORM = 4

export function debtTypeName(typeId: number): string {
  return DEBT_TYPES.find(type => type.id === typeId)?.name ?? UNKNOWN_TYPE_NAME
}

export function groupDebts(debts: readonly DebtMovement[], includeClosed = true): DebtTypeGroup[] {
  const byOwner = new Map<string, DebtMovement[]>()

  for (const movement of [...debts].sort(byDateDescending)) {
    const collected = byOwner.get(movement.ownerName) ?? []

    collected.push(movement)
    byOwner.set(movement.ownerName, collected)
  }

  const owners: DebtOwnerGroup[] = []

  for (const [name, ownerDebts] of byOwner) {
    const balance = signedBalance(ownerDebts)
    const hasKnownMovement = ownerDebts.some(movement => movement.typeId === TAKE_TYPE_ID || movement.typeId === GIVE_TYPE_ID)

    if (!includeClosed && hasKnownMovement && balance.isZero) {
      continue
    }

    owners.push({ name, debts: ownerDebts })
  }

  owners.sort((left, right) => left.name.localeCompare(right.name, 'ru', { sensitivity: 'base' }))

  return owners.length === 0 ? [] : [{
    typeId: DEBT_OWNERS_GROUP_ID,
    name: DEBT_OWNERS_GROUP_NAME,
    owners,
  }]
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

  return owner.debts.some(movement => contains(movement.comment, term))
}

export function ownerTotal(owner: DebtOwnerGroup): Money {
  return owner.debts.reduce((total, movement) => total.add(movement.sum), Money.zero)
}

export function ownerRemainder(owner: DebtOwnerGroup): Money {
  const balance = signedBalance(owner.debts)

  return balance.isNegative ? balance.negate() : balance
}

export function debtRoleColor(typeId: number): string {
  return typeId === TAKE_TYPE_ID ? 'success' : 'warning'
}

export function debtCountText(count: number): string {
  const tail = count % 100
  const last = count % 10
  const index = tail >= TEEN_FROM && tail < TEEN_TO || last > LAST_FORM ? 0 : last

  return `${count} ${DEBT_COUNT_FORMS[index] ?? DEBT_COUNT_FORMS[0]}`
}

export function debtRemainder(movement: DebtMovement): Money {
  return movement.sum
}

export function nextPayableDebt(owner: DebtOwnerGroup): DebtMovement | null {
  const balance = signedBalance(owner.debts)

  if (balance.isZero) {
    return null
  }

  const payableTypeId = balance.isNegative ? GIVE_TYPE_ID : TAKE_TYPE_ID

  return owner.debts.reduce<DebtMovement | null>((found, movement) => {
    if (movement.typeId !== payableTypeId || movement.isDeleted || movement.sum.compare(Money.zero) <= 0) {
      return found
    }

    return found === null || compareLocalDate(movement.date, found.date) < 0 ? movement : found
  }, null)
}

export function typeTotal(group: DebtTypeGroup): Money {
  return group.owners.reduce((total, owner) => total.add(ownerRemainder(owner)), Money.zero)
}

export function signedBalance(debts: readonly DebtMovement[]): Money {
  return debts.reduce((balance, movement) => {
    if (movement.isDeleted) {
      return balance
    }

    if (movement.typeId === TAKE_TYPE_ID) {
      return balance.add(movement.sum)
    }

    return movement.typeId === GIVE_TYPE_ID ? balance.subtract(movement.sum) : balance
  }, Money.zero)
}
export function canForgiveDebt(movement: DebtMovement): boolean {
  return movement.typeId === FORGIVABLE_TYPE_ID && !movement.isDeleted
}

function contains(value: string | null, term: string): boolean {
  return value !== null && value.toLowerCase().includes(term)
}


function byDateDescending(left: DebtMovement, right: DebtMovement): number {
  return compareLocalDate(right.date, left.date)
}
