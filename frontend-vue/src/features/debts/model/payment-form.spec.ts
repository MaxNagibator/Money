import { describe, expect, it } from 'vitest'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { newOwnerPaymentForm, toOwnerPaymentDraft, validatePaymentForm } from './payment-form'

const today = parseLocalDate('2026-11-13')!

function debt(id: number, extra: Partial<Debt> = {}): Debt {
  return {
    id, ownerName: 'Иван', typeId: 1, sum: Money.fromWire('100'), paySum: Money.zero,
    date: today, comment: null, payComment: null, isDeleted: false, ...extra,
  }
}

describe('возврат по долгам человека', () => {
  it('считает весь открытый остаток и исключает удалённые долги', () => {
    const form = newOwnerPaymentForm({ name: 'Иван', debts: [
      debt(1), debt(2, { paySum: Money.fromWire('30') }), debt(3, { isDeleted: true }),
    ] }, today)!

    expect(form.scope).toBe('owner')
    expect(form.ownerName).toBe('Иван')
    expect(form.typeId).toBe(1)
    expect(form.sum?.toDecimal().toString()).toBe('170')
    expect(form.maximum?.toDecimal().toString()).toBe('170')
    expect(validatePaymentForm({ ...form, sum: Money.fromWire('171') }).sum).toContain('170')
    expect(validatePaymentForm({ ...form, sum: Money.fromWire('0.25') })).toEqual({})
    expect(toOwnerPaymentDraft(form)).toMatchObject({ ownerName: 'Иван', typeId: 1 })
  })

  it('не открывает возврат при отсутствии живых долгов', () => {
    expect(newOwnerPaymentForm({ name: 'Иван', debts: [debt(1, { paySum: Money.fromWire('100') })] }, today)).toBeNull()
  })
})
