import { describe, expect, it } from 'vitest'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { debtAccounts } from './debt-accounts'
import { debtStatement, debtSummary } from './debt-presentation'

function entry(id: number, sum: string, typeId = 1, extra: Partial<Debt> = {}): Debt {
  return {
    id, sum: Money.fromWire(sum), typeId, ownerName: 'Иван', date: parseLocalDate(`2026-11-${String(id).padStart(2, '0')}`)!,
    paySum: Money.zero, payComment: null, comment: null, isDeleted: false, ...extra,
  }
}

describe('представление расчётов', () => {
  it('считает сальдо от старого события к новому и показывает новые строки сверху', () => {
    const account = debtAccounts([entry(1, '1000'), entry(2, '400', 2), entry(3, '200')])[0]!
    const statement = debtStatement(account)

    expect(statement.rows.map(row => row.balance.toDecimal().toString())).toEqual(['800', '600', '1000'])
    expect(statement.outgoing.equals(Money.fromWire('1200'))).toBe(true)
    expect(statement.incoming.equals(Money.fromWire('400'))).toBe(true)
    expect(statement.balance.equals(account.balance)).toBe(true)
  })

  it('сохраняет неизвестную дату и текст старых оплат, группируя их после исходной выдачи', () => {
    const text = '2026.09.15 1000,0\nВторая строка комментария'
    const account = debtAccounts([entry(1, '3000', 1, { paySum: Money.fromWire('1000'), payComment: text }), entry(2, '500', 2)])[0]!
    const statement = debtStatement(account)

    expect(statement.hasUndatedPayments).toBe(true)
    expect(statement.rows.map(row => row.balance.toDecimal().toString())).toEqual(['1500', '2000', '3000'])
    expect(statement.rows[1]?.movement.date).toBeNull()
    expect(statement.rows[1]?.movement.comment).toBe(text)
  })

  it('удалённые строки не влияют на обороты и сальдо', () => {
    const account = debtAccounts([entry(1, '1000'), entry(2, '400', 2, { isDeleted: true })])[0]!
    const statement = debtStatement(account)

    expect(statement.incoming.isZero).toBe(true)
    expect(statement.balance.equals(Money.fromWire('1000'))).toBe(true)
    expect(statement.count).toBe(1)
  })

  it('сводка разделяет остатки по людям после взаимозачёта внутри каждого счёта', () => {
    const accounts = debtAccounts([
      entry(1, '1000'), entry(2, '400', 2), entry(3, '200', 2, { ownerName: 'Анна' }),
    ])
    const summary = debtSummary(accounts)

    expect(summary.receivable.equals(Money.fromWire('600'))).toBe(true)
    expect(summary.payable.equals(Money.fromWire('200'))).toBe(true)
    expect(summary.net.equals(Money.fromWire('400'))).toBe(true)
  })
})
