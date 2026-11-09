import type { DebtPaymentDraft } from '@/api/adapters/debts'
import { trimToNull } from '@/domain/text'
import type { Debt } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { FIELD_REQUIRED, SUM_UNPARSED } from './debt-form'

export const PAYMENT_SUM_INVALID = 'Недопустимое значение'

export const MIN_PAYMENT_SUM = Money.fromMinorUnits(100)

export interface PaymentFormValue {
  debtId: number
  sum: Money | null
  date: LocalDate | null
  comment: string
}

export interface PaymentFormErrors {
  sum?: string
  date?: string
}

export function newPaymentForm(debt: Debt, today: LocalDate): PaymentFormValue {
  const remainder = debt.sum.subtract(debt.paySum)

  return {
    debtId: debt.id,
    sum: remainder.isNegative ? Money.zero : remainder,
    date: today,
    comment: '',
  }
}

export function validatePaymentForm(value: PaymentFormValue): PaymentFormErrors {
  const errors: PaymentFormErrors = {}

  if (value.sum === null || value.sum.compare(MIN_PAYMENT_SUM) < 0) {
    errors.sum = PAYMENT_SUM_INVALID
  }

  if (value.date === null) {
    errors.date = FIELD_REQUIRED
  }

  return errors
}

export function toPaymentDraft(value: PaymentFormValue): DebtPaymentDraft {
  if (value.sum === null) {
    throw new Error(SUM_UNPARSED)
  }

  if (value.date === null) {
    throw new Error(FIELD_REQUIRED)
  }

  return {
    sum: value.sum,
    comment: trimToNull(value.comment),
    date: value.date,
  }
}
