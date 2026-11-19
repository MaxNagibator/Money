import { describe, expect, it } from 'vitest'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { debtAccounts, visibleDebtAccounts } from './debt-accounts'

function debt(id: number, extra: Partial<Debt> = {}): Debt {
  return {
    id,
    ownerName: 'Иван',
    typeId: 1,
    sum: Money.fromWire('1000'),
    paySum: Money.zero,
    comment: null,
    payComment: null,
    date: parseLocalDate('2026-11-10')!,
    isDeleted: false,
    ...extra,
  }
}

describe('счёт человека поверх существующих долгов', () => {
  it('восстанавливает даты отдельных возвратов при совпадении текста и оплаченной суммы', () => {
    const account = debtAccounts([debt(1, {
      paySum: Money.fromWire('350.25'),
      payComment: '2026.11.11 100 Первый\n2026.11.13 250,25 Второй',
    })])[0]!

    const payments = account.movements.filter(row => row.kind === 'payment')
    expect(payments.map(row => row.date)).toEqual(['2026-11-11', '2026-11-13'])
    expect(payments.map(row => row.comment)).toEqual(['Первый', 'Второй'])
    expect(account.balance.equals(Money.fromWire('649.75'))).toBe(true)
  })

  it('не достраивает историю при расхождении текстовых оплат с PaySum', () => {
    const account = debtAccounts([debt(1, {
      paySum: Money.fromWire('350'), payComment: '2026.11.11 100 Первый',
    })])[0]!

    expect(account.movements.filter(row => row.kind === 'legacyPayment')).toHaveLength(1)
    expect(account.movements.find(row => row.kind === 'legacyPayment')?.date).toBeNull()
  })
  it('объединяет оба направления по человеку, сохраняя отдельные записи', () => {
    const accounts = debtAccounts([debt(1), debt(2, { typeId: 2, sum: Money.fromWire('400') })])

    expect(accounts).toHaveLength(1)
    expect(accounts[0]?.balance.equals(Money.fromWire('600'))).toBe(true)
    expect(accounts[0]?.movements.map(row => row.source.id)).toEqual([2, 1])
  })

  it('переплата меняет знак баланса, а исправление возврата пересчитывает его', () => {
    const balance = (returned: string) => debtAccounts([
      debt(1), debt(2, { typeId: 2, sum: Money.fromWire(returned) }),
    ])[0]?.balance

    expect(balance('1200')?.equals(Money.fromWire('-200'))).toBe(true)
    expect(balance('900')?.equals(Money.fromWire('100'))).toBe(true)
  })

  it('сохраняет точность небольших дробных сумм', () => {
    const account = debtAccounts([
      debt(1, { sum: Money.fromWire('0.00000000000000000003') }),
      debt(2, { typeId: 2, sum: Money.fromWire('0.00000000000000000002') }),
    ])[0]!

    expect(account.balance.equals(Money.fromWire('0.00000000000000000001'))).toBe(true)
  })

  it('старую оплату учитывает один раз и сохраняет весь её текст без выдуманной даты', () => {
    const history = '2026.10.01 100 первая часть\nтекст с переносом\n2026.11.01 200 вторая'
    const account = debtAccounts([
      debt(1, { paySum: Money.fromWire('300'), payComment: history }),
      debt(2, { typeId: 2, sum: Money.fromWire('200') }),
    ])[0]!

    expect(account.balance.equals(Money.fromWire('500'))).toBe(true)
    const legacy = account.movements.find(row => row.kind === 'legacyPayment')!

    expect(legacy.amount.equals(Money.fromWire('-300'))).toBe(true)
    expect(legacy.comment).toBe(history)
    expect(legacy.date).toBeNull()
    expect(new Set(account.movements.map(row => row.key)).size).toBe(3)
  })

  it('старое погашение долга другого направления даёт положительное движение', () => {
    const account = debtAccounts([debt(1, { typeId: 2, paySum: Money.fromWire('250') })])[0]!

    expect(account.balance.equals(Money.fromWire('-750'))).toBe(true)
    expect(account.movements.find(row => row.kind === 'legacyPayment')?.amount.equals(Money.fromWire('250'))).toBe(true)
  })

  it('скрывает нулевой счёт по сальдо, не удаляя его историю', () => {
    const accounts = debtAccounts([debt(1), debt(2, { typeId: 2 })])

    expect(visibleDebtAccounts(accounts, '', false, 'name')).toEqual([])
    expect(visibleDebtAccounts(accounts, '', true, 'name')[0]?.movements).toHaveLength(2)
  })

  it('удалённая запись не влияет на баланс и остаётся доступной для восстановления', () => {
    const accounts = debtAccounts([debt(1, { isDeleted: true })])

    expect(accounts[0]?.balance.isZero).toBe(true)
    expect(visibleDebtAccounts(accounts, '', false, 'name')).toHaveLength(1)
    expect(accounts[0]?.movements[0]?.source.isDeleted).toBe(true)
  })

  it('поиск находит человека по истории, сохраняя полный баланс счёта', () => {
    const accounts = debtAccounts([debt(1, { payComment: 'за ремонт', paySum: Money.fromWire('300') }), debt(2, { typeId: 2 })])
    const found = visibleDebtAccounts(accounts, 'РЕМОНТ', false, 'name')

    expect(found).toHaveLength(1)
    expect(found[0]?.balance.equals(Money.fromWire('-300'))).toBe(true)
    expect(found[0]?.movements).toHaveLength(3)
  })

  it('сортирует людей по имени либо по величине остатка, сохраняя разные написания имени', () => {
    const accounts = debtAccounts([debt(1, { ownerName: 'Яна' }), debt(2, { ownerName: 'Анна', typeId: 2, sum: Money.fromWire('2000') })])

    expect(visibleDebtAccounts(accounts, '', false, 'name').map(account => account.name)).toEqual(['Анна', 'Яна'])
    expect(visibleDebtAccounts(accounts, '', false, 'balance').map(account => account.name)).toEqual(['Анна', 'Яна'])
    expect(debtAccounts([debt(1), debt(2, { ownerName: 'иван' })])).toHaveLength(2)
  })
})
