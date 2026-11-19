import { computed, ref } from 'vue'
import { Money } from '@/domain/money'
import type { DebtAccount } from './debt-accounts'

export type DebtViewMode = 'accounting' | 'normal' | 'rework' | 'refined'

export function useAccountExpanded(
  controlled: () => boolean | undefined,
  initiallyExpanded: boolean | undefined,
  onChange: (value: boolean) => void,
) {
  const localExpanded = ref(initiallyExpanded ?? false)
  const isExpanded = computed(() => controlled() ?? localExpanded.value)

  function setExpanded(value: boolean): void {
    if (controlled() === undefined) {
      localExpanded.value = value
    }
    onChange(value)
  }

  function toggle(): void {
    setExpanded(!isExpanded.value)
  }

  return { isExpanded, toggle, setExpanded }
}

export function debtSummary(accounts: readonly DebtAccount[]) {
  let receivable = Money.zero
  let payable = Money.zero
  for (const account of accounts) {
    if (account.balance.isNegative) {
      payable = payable.subtract(account.balance)
    } else {
      receivable = receivable.add(account.balance)
    }
  }
  return { receivable, payable, net: receivable.subtract(payable) }
}

export function debtStatement(account: DebtAccount) {
  let balance = Money.zero
  let outgoing = Money.zero
  let incoming = Money.zero
  let count = 0
  const rows = [...account.movements]
    .sort((left, right) => (left.date ?? left.source.date).localeCompare(right.date ?? right.source.date)
      || left.source.id - right.source.id
      || Number(left.kind === 'legacyPayment') - Number(right.kind === 'legacyPayment'))
    .map(movement => {
      if (!movement.source.isDeleted) {
        balance = balance.add(movement.amount)
        count += 1
        if (movement.amount.isNegative) {
          incoming = incoming.subtract(movement.amount)
        } else {
          outgoing = outgoing.add(movement.amount)
        }
      }
      return { movement, balance }
    })
    .reverse()

  return {
    rows, outgoing, incoming, balance, count,
    hasUndatedPayments: account.movements.some(movement => movement.date === null),
  }
}
