import type { DebtDraft } from '@/api/adapters/debts'
import { trimToNull } from '@/domain/text'
import type { Debt } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { DATE_UNPARSED, SUM_UNPARSED } from '@/shared/ui/field-messages'

export { DATE_UNPARSED, SUM_UNPARSED }

export const FIELD_REQUIRED = 'Заполни меня'

export interface DebtFormValue {
  id: number | null
  typeId: number | null
  ownerName: string
  sum: Money | null
  date: LocalDate | null
  comment: string
}

export interface DebtFormErrors {
  typeId?: string
  ownerName?: string
  date?: string
}

export function newDebtForm(date: LocalDate, typeId: number | null = null): DebtFormValue {
  return {
    id: null,
    typeId,
    ownerName: '',
    sum: Money.zero,
    date,
    comment: '',
  }
}

export function editDebtForm(debt: Debt): DebtFormValue {
  return {
    id: debt.id,
    typeId: debt.typeId,
    ownerName: debt.ownerName,
    sum: debt.sum,
    date: debt.date,
    comment: debt.comment ?? '',
  }
}

export function dateFieldError(shown: LocalDate | null, parsed: LocalDate | null): string {
  if (parsed !== null) {
    return ''
  }

  return shown === null ? FIELD_REQUIRED : DATE_UNPARSED
}

export function shouldResetOwners(previous: string, next: string): boolean {
  return next.trim().length > 0 && next !== previous
}

export function validateDebtForm(value: DebtFormValue): DebtFormErrors {
  const errors: DebtFormErrors = {}

  if (value.typeId === null) {
    errors.typeId = FIELD_REQUIRED
  }

  if (value.ownerName.trim().length === 0) {
    errors.ownerName = FIELD_REQUIRED
  }

  if (value.date === null) {
    errors.date = FIELD_REQUIRED
  }

  return errors
}

export function toDebtDraft(value: DebtFormValue): DebtDraft {
  const ownerName = value.ownerName.trim()

  if (value.typeId === null || ownerName.length === 0 || value.date === null) {
    throw new Error(FIELD_REQUIRED)
  }

  if (value.sum === null) {
    throw new Error(SUM_UNPARSED)
  }

  return {
    typeId: value.typeId,
    sum: value.sum,
    comment: trimToNull(value.comment),
    ownerName,
    date: value.date,
  }
}
