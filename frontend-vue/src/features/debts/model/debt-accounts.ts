import type { Debt } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { parsePaymentHistory, TAKE_TYPE_ID } from './debt'

export interface DebtMovement {
  key: string
  source: Debt
  kind: 'entry' | 'payment' | 'legacyPayment'
  date: LocalDate | null
  amount: Money
  comment: string | null
}

export interface DebtAccount {
  name: string
  balance: Money
  debts: Debt[]
  movements: DebtMovement[]
}

export type DebtAccountSort = 'name' | 'balance'

export function debtAccounts(debts: readonly Debt[]): DebtAccount[] {
  const owners = new Map<string, Debt[]>()

  for (const debt of debts) {
    const rows = owners.get(debt.ownerName) ?? []
    rows.push(debt)
    owners.set(debt.ownerName, rows)
  }

  return [...owners].map(([name, rows]) => {
    const movements = [...rows]
      .sort((left, right) => right.date.localeCompare(left.date) || right.id - left.id)
      .flatMap(toMovements)

    return {
      name,
      debts: rows,
      movements,
      balance: movements.reduce((total, row) => row.source.isDeleted ? total : total.add(row.amount), Money.zero),
    }
  })
}

export function visibleDebtAccounts(
  accounts: readonly DebtAccount[],
  search: string,
  showSettled: boolean,
  sort: DebtAccountSort,
): DebtAccount[] {
  const term = search.trim().toLowerCase()

  return accounts
    .filter(account => showSettled || !account.balance.isZero || account.debts.some(debt => debt.isDeleted))
    .filter(account => account.name.toLowerCase().includes(term)
      || account.movements.some(row => row.comment?.toLowerCase().includes(term)))
    .sort((left, right) => {
      if (sort === 'balance') {
        const byBalance = right.balance.toDecimal().abs().comparedTo(left.balance.toDecimal().abs())
        if (byBalance !== 0) {
          return byBalance
        }
      }

      return left.name.localeCompare(right.name, 'ru')
    })
}

function toMovements(debt: Debt): DebtMovement[] {
  const amount = debt.typeId === TAKE_TYPE_ID ? debt.sum : debt.sum.negate()
  const rows: DebtMovement[] = [{
    key: `debt:${debt.id}`,
    source: debt,
    kind: 'entry',
    date: debt.date,
    amount,
    comment: debt.comment,
  }]

  if (!debt.paySum.isZero) {
    const lines = (debt.payComment ?? '').split(/\r?\n/).map(line => line.trim()).filter(Boolean)
    const payments = parsePaymentHistory(debt.payComment)
    const parsedSum = payments.reduce((total, payment) => total.add(payment.sum), Money.zero)
    if (payments.length > 0 && payments.length === lines.length
      && lines.every(line => /^\d{4}[.-]\d{2}[.-]\d{2} \d+(?:[.,]\d+)?(?: |$)/.test(line))
      && payments.every(payment => payment.date !== null && !payment.sum.isZero && !payment.sum.isNegative)
      && parsedSum.compare(debt.paySum) === 0) {
      return [...rows, ...payments.map((payment, index): DebtMovement => ({
        key: `payment:${debt.id}:${index}`,
        source: debt,
        kind: 'payment',
        date: payment.date,
        amount: debt.typeId === TAKE_TYPE_ID ? payment.sum.negate() : payment.sum,
        comment: payment.comment,
      }))]
    }

    rows.push({
      key: `payment:${debt.id}`,
      source: debt,
      kind: 'legacyPayment',
      date: null,
      amount: debt.typeId === TAKE_TYPE_ID ? debt.paySum.negate() : debt.paySum,
      comment: debt.payComment,
    })
  }

  return rows
}
