import { describe, expect, it } from 'vitest'
import type { Debt } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { DebtOwnerGroup } from './debt'
import {
  canForgiveDebt,
  debtStatus,
  filterDebtGroups,
  groupDebts,
  hasPaymentHistory,
  lastPaymentDate,
  ownerPercent,
  ownerRemainder,
  ownerTotal,
  parsePaymentHistory,
  paymentProgress,
  typeTotal,
} from './debt'

function date(value: string): LocalDate {
  return parseLocalDate(value)!
}

function money(value: string): Money {
  return Money.parseInput(value)!
}

function debt(id: number, extra: Partial<Debt> = {}): Debt {
  return {
    id,
    typeId: 1,
    sum: money('1000'),
    paySum: Money.zero,
    comment: null,
    ownerName: 'Иван',
    date: date('2026-08-22'),
    payComment: null,
    isDeleted: false,
    ...extra,
  }
}

function owner(debts: readonly Debt[], name = 'Иван'): DebtOwnerGroup {
  return { name, debts: [...debts] }
}

describe('группировка долгов', () => {
  it('типы идут по идентификатору убыв., держатели по первому появлению в списке от новых к старым', () => {
    const groups = groupDebts([
      debt(1, { typeId: 1, ownerName: 'Иван', date: date('2026-08-20') }),
      debt(2, { typeId: 2, ownerName: 'Пётр', date: date('2026-08-22') }),
      debt(3, { typeId: 1, ownerName: 'Пётр', date: date('2026-08-21') }),
    ])

    expect(groups.map(group => group.typeId)).toEqual([2, 1])
    expect(groups.map(group => group.name)).toEqual(['Нужно отдать', 'Нужно забрать'])
    expect(groups[1]?.owners.map(entry => entry.name)).toEqual(['Пётр', 'Иван'])
  })

  it('долги держателя идут от новых к старым, равные даты сохраняют порядок ответа', () => {
    const groups = groupDebts([
      debt(1, { date: date('2026-08-20') }),
      debt(2, { date: date('2026-08-22') }),
      debt(3, { date: date('2026-08-22') }),
    ])

    expect(groups[0]?.owners[0]?.debts.map(entry => entry.id)).toEqual([2, 3, 1])
  })

  it('держатели с одинаковым именем сливаются в одну карточку', () => {
    const groups = groupDebts([
      debt(1, { ownerName: 'Иван' }),
      debt(2, { ownerName: 'Иван' }),
    ])

    expect(groups[0]?.owners).toHaveLength(1)
    expect(groups[0]?.owners[0]?.debts.map(entry => entry.id)).toEqual([1, 2])
  })

  it('имена, различающиеся регистром, остаются разными держателями', () => {
    const groups = groupDebts([
      debt(1, { ownerName: 'Иван' }),
      debt(2, { ownerName: 'иван' }),
    ])

    expect(groups[0]?.owners.map(entry => entry.name)).toEqual(['Иван', 'иван'])
  })

  it('незнакомый тип не теряется, а получает имя-заглушку', () => {
    const groups = groupDebts([debt(1, { typeId: 99 })])

    expect(groups[0]?.typeId).toBe(99)
    expect(groups[0]?.name).toBe('Неизвестный тип')
  })
})

describe('агрегаты держателя', () => {
  it('считает сумму, остаток и процент погашения с округлением до двух знаков', () => {
    const entry = owner([
      debt(1, { sum: money('1000'), paySum: money('250') }),
      debt(2, { sum: money('500') }),
    ])

    expect(entry.debts).toHaveLength(2)
    expect(ownerTotal(entry).toMinorUnits()).toBe(150_000)
    expect(ownerRemainder(entry).toMinorUnits()).toBe(125_000)
    expect(ownerPercent(entry)).toBe(17)
  })

  it.each([
    ['197', '200', 98],
    ['25', '200', 12],
    ['250', '1000', 25],
  ])('ровная половина в проценте округляется к чётному: %s из %s', (paySum, sum, expected) => {
    expect(ownerPercent(owner([debt(1, { sum: money(sum), paySum: money(paySum) })]))).toBe(expected)
  })

  it('нулевая сумма долгов даёт нулевой процент', () => {
    expect(ownerPercent(owner([debt(1, { sum: Money.zero, paySum: Money.zero })]))).toBe(0)
  })

  it('итог типа складывается из сумм держателей', () => {
    const groups = groupDebts([
      debt(1, { ownerName: 'Иван', sum: money('1000') }),
      debt(2, { ownerName: 'Пётр', sum: money('500') }),
    ])

    expect(typeTotal(groups[0]!).toMinorUnits()).toBe(150_000)
  })
})

describe('последний платёж держателя', () => {
  it('берётся из комментария самого свежего долга', () => {
    const entry = owner([
      debt(1, { date: date('2026-08-20'), payComment: '2026.08.19 100 старый' }),
      debt(2, { date: date('2026-08-22'), payComment: '2026.08.01 50 первый\n2026.08.21 50 второй' }),
    ])

    expect(lastPaymentDate(entry)).toBe('2026-08-21')
  })

  it('строка платежа без пробела не роняет разбор', () => {
    expect(lastPaymentDate(owner([debt(1, { payComment: '2026.08.21' })]))).toBe('2026-08-21')
    expect(lastPaymentDate(owner([debt(1, { payComment: 'мусор' })]))).toBeNull()
  })

  it('строки режутся и по CRLF, и по LF', () => {
    const entry = owner([debt(1, { payComment: '2026.08.01 50 первый\r\n2026.08.21 50 второй\r\n' })])

    expect(lastPaymentDate(entry)).toBe('2026-08-21')
  })

  it('без платежей даты нет', () => {
    expect(lastPaymentDate(owner([debt(1)]))).toBeNull()
    expect(lastPaymentDate(owner([debt(1, { payComment: '   ' })]))).toBeNull()
  })
})

describe('поиск по держателям', () => {
  const groups = groupDebts([
    debt(1, { typeId: 1, ownerName: 'Иван', comment: 'за обед' }),
    debt(2, { typeId: 1, ownerName: 'Иван', comment: null }),
    debt(3, { typeId: 1, ownerName: 'Пётр', payComment: '2026.08.21 50 бензин' }),
    debt(4, { typeId: 2, ownerName: 'Мария', comment: 'книга' }),
  ])

  it.each([
    ['иВаН', ['Иван']],
    ['ОБЕД', ['Иван']],
    ['бензин', ['Пётр']],
  ])('запрос %s оставляет держателей %j', (query, expected) => {
    const filtered = filterDebtGroups(groups, query)

    expect(filtered[0]?.owners.map(entry => entry.name)).toEqual(expected)
  })

  it('у совпавшего держателя остаются все его долги', () => {
    const filtered = filterDebtGroups(groups, 'обед')

    expect(filtered[0]?.owners[0]?.debts.map(entry => entry.id)).toEqual([1, 2])
  })

  it('тип без совпавших держателей исчезает целиком', () => {
    const filtered = filterDebtGroups(groups, 'книга')

    expect(filtered.map(group => group.typeId)).toEqual([2])
  })

  it.each(['', '   '])('запрос %j отключает фильтр', query => {
    expect(filterDebtGroups(groups, query)).toHaveLength(2)
  })
})

describe('прогресс и статус долга', () => {
  it.each([
    ['1000', '1000', 100, 'Погашено'],
    ['1000', '997', 99, 'В процессе'],
    ['1000', '0', 0, 'Не начато'],
    ['0', '0', 0, 'Не начато'],
    ['1000', '1200', 100, 'Погашено'],
  ])('долг %s с оплатой %s даёт прогресс %i', (sum, paySum, expected, text) => {
    const entry = debt(1, { sum: money(sum), paySum: money(paySum) })
    const statuses = { paid: 'Погашено', inProgress: 'В процессе', notStarted: 'Не начато' }

    expect(paymentProgress(entry)).toBe(expected)
    expect(statuses[debtStatus(entry)]).toBe(text)
  })

  it.each([
    [{ typeId: 1 }, true],
    [{ typeId: 2 }, false],
    [{ typeId: 1, isDeleted: true }, false],
    [{ typeId: 1, paySum: money('1000') }, false],
  ])('прощаемость долга %j равна %s', (extra, expected) => {
    expect(canForgiveDebt(debt(1, extra))).toBe(expected)
  })
})

describe('история погашений', () => {
  it('разбирает строки сервера и оставляет комментарий целиком', () => {
    const records = parsePaymentHistory('2026.08.21 1234,56 за обед в кафе\r\n2026.08.22 100 ')

    expect(records).toHaveLength(2)
    expect(records[0]?.date).toBe('2026-08-21')
    expect(records[0]?.sum.toMinorUnits()).toBe(123_456)
    expect(records[0]?.comment).toBe('за обед в кафе')
    expect(records[1]?.comment).toBe('')
  })

  it('строка меньше чем из двух частей отбрасывается', () => {
    expect(parsePaymentHistory('2026.08.21\n\n2026.08.22 100 взнос')).toHaveLength(1)
  })

  it('нераспознанные дата и сумма не роняют строку', () => {
    const records = parsePaymentHistory('мусор ещё мусор')

    expect(records[0]?.date).toBeNull()
    expect(records[0]?.sum.isZero).toBe(true)
  })

  it.each([
    [{ paySum: Money.zero, payComment: '2026.08.21 100 взнос' }, false],
    [{ paySum: money('100'), payComment: null }, false],
    [{ paySum: money('100'), payComment: '   ' }, false],
    [{ paySum: money('100'), payComment: '2026.08.21 100 взнос' }, true],
  ])('таблица истории для %j показывается: %s', (extra, expected) => {
    expect(hasPaymentHistory(debt(1, extra))).toBe(expected)
  })
})
