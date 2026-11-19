import { describe, expect, it } from 'vitest'
import type { DebtMovement } from '@/domain/debt'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import type { DebtOwnerGroup } from './debt'
import {
  debtCountText,
  debtRoleColor,
  filterDebtGroups,
  groupDebts,
  nextPayableDebt,
  ownerRemainder,
  ownerTotal,
  typeTotal,
} from './debt'

function date(value: string): LocalDate {
  return parseLocalDate(value)!
}

function money(value: string): Money {
  return Money.parseInput(value)!
}

function movement(id: number, extra: Partial<DebtMovement> = {}): DebtMovement {
  return {
    id,
    typeId: 1,
    sum: money('1000'),
    comment: null,
    ownerName: 'Иван',
    date: date('2026-08-22'),
    isDeleted: false,
    ...extra,
  }
}

function owner(debts: readonly DebtMovement[], name = 'Иван'): DebtOwnerGroup {
  return { name, debts: [...debts] }
}

describe('группировка долгов', () => {
  it('собирает всех держателей в один раздел и сортирует по алфавиту', () => {
    const groups = groupDebts([
      movement(1, { typeId: 1, ownerName: 'Иван', date: date('2026-08-20') }),
      movement(2, { typeId: 2, ownerName: 'Пётр', date: date('2026-08-22') }),
      movement(3, { typeId: 1, ownerName: 'Пётр', date: date('2026-08-21') }),
    ])

    expect(groups.map(group => group.typeId)).toEqual([0])
    expect(groups.map(group => group.name)).toEqual(['Держатели'])
    expect(groups[0]?.owners.map(entry => entry.name)).toEqual(['Иван', 'Пётр'])
  })

  it('встречные движения одного держателя взаимно уменьшают отображаемый остаток', () => {
    const groups = groupDebts([
      movement(1, { typeId: 1, sum: money('5000') }),
      movement(2, { typeId: 2, sum: money('2000') }),
    ])

    expect(groups).toHaveLength(1)
    expect(groups[0]?.typeId).toBe(0)
    expect(ownerRemainder(groups[0]!.owners[0]!).toMinorUnits()).toBe(300_000)
  })

  it('нулевой счёт скрывается среди актуальных и остаётся в истории', () => {
    const movements = [
      movement(1, { typeId: 1, sum: money('5000') }),
      movement(2, { typeId: 2, sum: money('5000') }),
    ]

    expect(groupDebts(movements, false)).toEqual([])
    expect(groupDebts(movements, true)).toHaveLength(1)
  })

  it('после перехода через ноль держатель остаётся в общем разделе', () => {
    const groups = groupDebts([
      movement(1, { typeId: 1, sum: money('5000') }),
      movement(2, { typeId: 2, sum: money('6000') }),
    ], false)

    expect(groups).toHaveLength(1)
    expect(groups[0]?.typeId).toBe(0)
    expect(groups[0]?.name).toBe('Держатели')
    expect(ownerRemainder(groups[0]!.owners[0]!).toMinorUnits()).toBe(100_000)
  })

  it('удалённое движение не влияет на текущий баланс', () => {
    const groups = groupDebts([
      movement(1, { typeId: 1, sum: money('5000') }),
      movement(2, { typeId: 2, sum: money('5000'), isDeleted: true }),
    ], false)

    expect(groups[0]?.typeId).toBe(0)
    expect(ownerRemainder(groups[0]!.owners[0]!).toMinorUnits()).toBe(500_000)
  })

  it('долги держателя идут от новых к старым, равные даты сохраняют порядок ответа', () => {
    const groups = groupDebts([
      movement(1, { date: date('2026-08-20') }),
      movement(2, { date: date('2026-08-22') }),
      movement(3, { date: date('2026-08-22') }),
    ])

    expect(groups[0]?.owners[0]?.debts.map(entry => entry.id)).toEqual([2, 3, 1])
  })

  it('держатели с одинаковым именем сливаются в одну карточку', () => {
    const groups = groupDebts([
      movement(1, { ownerName: 'Иван' }),
      movement(2, { ownerName: 'Иван' }),
    ])

    expect(groups[0]?.owners).toHaveLength(1)
    expect(groups[0]?.owners[0]?.debts.map(entry => entry.id)).toEqual([1, 2])
  })

  it('имена, различающиеся регистром, остаются разными держателями', () => {
    const groups = groupDebts([
      movement(1, { ownerName: 'Иван' }),
      movement(2, { ownerName: 'иван' }),
    ])

    expect(groups[0]?.owners.map(entry => entry.name)).toEqual(['Иван', 'иван'])
  })

  it('незнакомый тип не теряется из общего раздела', () => {
    const groups = groupDebts([movement(1, { typeId: 99 })])

    expect(groups[0]?.typeId).toBe(0)
    expect(groups[0]?.name).toBe('Держатели')
  })
})

describe('агрегаты держателя', () => {
  it('считает оборот и чистый остаток по движениям', () => {
    const entry = owner([
      movement(1, { typeId: 1, sum: money('1000') }),
      movement(2, { typeId: 2, sum: money('250') }),
      movement(3, { typeId: 1, sum: money('500') }),
    ])

    expect(entry.debts).toHaveLength(3)
    expect(ownerTotal(entry).toMinorUnits()).toBe(175_000)
    expect(ownerRemainder(entry).toMinorUnits()).toBe(125_000)
  })

  it('итог типа складывается из сумм держателей', () => {
    const groups = groupDebts([
      movement(1, { ownerName: 'Иван', sum: money('1000') }),
      movement(2, { ownerName: 'Пётр', sum: money('500') }),
    ])

    expect(typeTotal(groups[0]!).toMinorUnits()).toBe(150_000)
  })
})

describe('поиск по держателям', () => {
  const groups = groupDebts([
    movement(1, { typeId: 1, ownerName: 'Иван', comment: 'за обед' }),
    movement(2, { typeId: 1, ownerName: 'Иван', comment: null }),
    movement(3, { typeId: 1, ownerName: 'Пётр', comment: 'бензин' }),
    movement(4, { typeId: 2, ownerName: 'Мария', comment: 'книга' }),
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

  it('поиск оставляет в общем разделе только совпавшего держателя', () => {
    const filtered = filterDebtGroups(groups, 'книга')

    expect(filtered.map(group => group.typeId)).toEqual([0])
    expect(filtered[0]?.owners.map(owner => owner.name)).toEqual(['Мария'])
  })

  it.each(['', '   '])('запрос %j отключает фильтр', query => {
    expect(filterDebtGroups(groups, query)).toHaveLength(1)
  })
})

describe('книжная карточка на узком экране', () => {
  it.each([
    [1, 'success'],
    [2, 'warning'],
  ])('тип %i красится в %s', (typeId, expected) => {
    expect(debtRoleColor(typeId)).toBe(expected)
  })

  it.each([
    [0, '0 долгов'],
    [1, '1 долг'],
    [2, '2 долга'],
    [4, '4 долга'],
    [5, '5 долгов'],
    [11, '11 долгов'],
    [14, '14 долгов'],
    [21, '21 долг'],
    [22, '22 долга'],
    [25, '25 долгов'],
    [111, '111 долгов'],
  ])('%i долгов пишется как «%s»', (count, expected) => {
    expect(debtCountText(count)).toBe(expected)
  })

  it('к обратному движению берётся самое раннее активное движение', () => {
    const group = owner([
      movement(1, { date: date('2026-08-22') }),
      movement(2, { date: date('2026-08-20') }),
    ])

    expect(nextPayableDebt(group)?.id).toBe(2)
  })

  it('удалённый долг к оплате не предлагается', () => {
    const group = owner([
      movement(1, { date: date('2026-08-18'), isDeleted: true }),
      movement(2, { date: date('2026-08-20') }),
    ])

    expect(nextPayableDebt(group)?.id).toBe(2)
  })

  it('у держателя с нулевым балансом платить нечего', () => {
    expect(nextPayableDebt(owner([
      movement(1, { typeId: 1 }),
      movement(2, { typeId: 2 }),
    ]))).toBeNull()
  })
})
