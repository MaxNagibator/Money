import { describe, expect, it } from 'vitest'
import type { Debt } from '@/domain/debt'
import { parseLocalDate } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { darkTheme, lightTheme } from '@/shared/ui/theme'
import { debtAccounts } from './debt-accounts'
import {
  buildDebtCycles,
  buildMonthlyFlows,
  buildOwnerTimeline,
  buildOwnersBreakdown,
  buildPortfolioTimeline,
  countUndatedPayments,
  diffDays,
  formatAxisRubles,
} from './debt-charts'
import { resolveDebtChartPalette } from './useDebtChartPalette'

function date(value: string) {
  return parseLocalDate(value)!
}

function debt(id: number, extra: Partial<Debt> = {}): Debt {
  return {
    id,
    ownerName: 'Иван',
    typeId: 1,
    sum: Money.fromWire('1000'),
    paySum: Money.zero,
    comment: null,
    payComment: null,
    date: date('2026-01-10'),
    isDeleted: false,
    ...extra,
  }
}

describe('модель графиков истории долгов', () => {
  it('возвращает пустые структуры на пустом входе', () => {
    const today = date('2026-11-19')

    expect(buildPortfolioTimeline([], today)).toEqual({ points: [], undatedPaymentsCount: 0 })
    expect(buildMonthlyFlows([])).toEqual({ months: [], undatedPaymentsCount: 0 })
    expect(buildOwnersBreakdown([])).toEqual({ labels: [], values: [], directions: [], balances: [] })
    expect(buildDebtCycles([], today)).toEqual({
      minDate: null,
      maxDate: null,
      totalDays: 0,
      owners: [],
      cycles: [],
      undatedPaymentsCount: 0,
    })
    expect(countUndatedPayments([])).toBe(0)
  })

  it('строит портфельную ленту по датам и добавляет точку carry-forward на today', () => {
    const accounts = debtAccounts([
      debt(1, {
        ownerName: 'Анна',
        typeId: 1,
        date: date('2026-01-05'),
        sum: Money.fromWire('1000.50'),
        paySum: Money.fromWire('400.25'),
        payComment: '2026.01.10 400,25 часть',
      }),
      debt(2, {
        ownerName: 'Борис',
        typeId: 2,
        date: date('2026-01-10'),
        sum: Money.fromWire('500.25'),
      }),
    ])

    const timeline = buildPortfolioTimeline(accounts, date('2026-01-20'))

    expect(timeline.undatedPaymentsCount).toBe(0)
    expect(timeline.points).toHaveLength(3)

    expect(timeline.points[0]?.date).toBe('2026-01-05')
    expect(timeline.points[0]?.receivable.equals(Money.fromWire('1000.50'))).toBe(true)
    expect(timeline.points[0]?.payable.isZero).toBe(true)
    expect(timeline.points[0]?.net.equals(Money.fromWire('1000.50'))).toBe(true)
    expect(timeline.points[0]?.isCarryForward).toBe(false)

    expect(timeline.points[1]?.date).toBe('2026-01-10')
    expect(timeline.points[1]?.receivable.equals(Money.fromWire('600.25'))).toBe(true)
    expect(timeline.points[1]?.payable.equals(Money.fromWire('500.25'))).toBe(true)
    expect(timeline.points[1]?.net.equals(Money.fromWire('100'))).toBe(true)
    expect(timeline.points[1]?.isCarryForward).toBe(false)

    expect(timeline.points[2]?.date).toBe('2026-01-20')
    expect(timeline.points[2]?.net.equals(Money.fromWire('100'))).toBe(true)
    expect(timeline.points[2]?.isCarryForward).toBe(true)

    const withoutFuture = buildPortfolioTimeline(accounts, date('2026-01-10'))
    expect(withoutFuture.points).toHaveLength(2)
  })

  it('объединяет несколько долгов и возвратов разных людей на одну дату в одну точку портфеля', () => {
    const accounts = debtAccounts([
      debt(1, {
        ownerName: 'Анна',
        typeId: 1,
        date: date('2026-01-10'),
        sum: Money.fromWire('1000'),
        paySum: Money.fromWire('250'),
        payComment: '2026.01.10 250 возврат в тот же день',
      }),
      debt(2, {
        ownerName: 'Борис',
        typeId: 2,
        date: date('2026-01-10'),
        sum: Money.fromWire('400'),
      }),
      debt(3, {
        ownerName: 'Анна',
        typeId: 1,
        date: date('2026-01-10'),
        sum: Money.fromWire('150'),
      }),
    ])

    const timeline = buildPortfolioTimeline(accounts, date('2026-01-10'))

    expect(timeline.points).toHaveLength(1)
    expect(timeline.points[0]?.date).toBe('2026-01-10')
    expect(timeline.points[0]?.receivable.equals(Money.fromWire('900'))).toBe(true)
    expect(timeline.points[0]?.payable.equals(Money.fromWire('400'))).toBe(true)
    expect(timeline.points[0]?.net.equals(Money.fromWire('500'))).toBe(true)
  })

  it('строит личную ленту с циклами погашения и повторного займа', () => {
    const account = debtAccounts([
      debt(1, {
        date: date('2026-01-01'),
        sum: Money.fromWire('1000'),
        comment: 'Первый заём',
        paySum: Money.fromWire('1000'),
        payComment: '2026.01-05 400 аванс\n2026.01.10 600 остаток',
      }),
      debt(2, {
        date: date('2026-02-01'),
        sum: Money.fromWire('700'),
        comment: 'Второй заём',
      }),
      debt(3, {
        date: date('2026-02-05'),
        sum: Money.fromWire('300'),
        comment: 'Добор',
      }),
    ])[0]!

    const timeline = buildOwnerTimeline(account, date('2026-02-10'))

    expect(timeline.points.map(point => point.markerKind)).toEqual([
      'start',
      'payment',
      'settled',
      'reborrowed',
      'debt',
      'today',
    ])
    expect(timeline.points[2]?.balanceAfter.isZero).toBe(true)
    expect(timeline.points[3]?.delta.equals(Money.fromWire('700'))).toBe(true)
    expect(timeline.points[3]?.comments).toEqual(['Второй заём'])
    expect(timeline.points[4]?.balanceAfter.equals(Money.fromWire('1000'))).toBe(true)
    expect(timeline.points[5]?.date).toBe('2026-02-10')
    expect(timeline.points[5]?.delta.isZero).toBe(true)
  })

  it('исключает удалённые долги и считает legacyPayment без выдуманной даты', () => {
    const accounts = debtAccounts([
      debt(1, {
        ownerName: 'Иван',
        date: date('2026-01-01'),
        sum: Money.fromWire('1000'),
        paySum: Money.fromWire('300'),
        payComment: 'старый возврат',
      }),
      debt(2, {
        ownerName: 'Иван',
        date: date('2026-01-05'),
        sum: Money.fromWire('500'),
        paySum: Money.fromWire('200'),
        payComment: 'удалённый без даты',
        isDeleted: true,
      }),
      debt(3, {
        ownerName: 'Пётр',
        date: date('2026-01-03'),
        sum: Money.fromWire('400'),
        isDeleted: true,
      }),
    ])

    const portfolio = buildPortfolioTimeline(accounts, date('2026-01-10'))
    expect(portfolio.undatedPaymentsCount).toBe(1)
    expect(portfolio.points).toHaveLength(2)
    expect(portfolio.points[0]?.receivable.equals(Money.fromWire('1000'))).toBe(true)

    const owner = buildOwnerTimeline(accounts[0]!)
    expect(owner.undatedPaymentsCount).toBe(1)
    expect(owner.points).toHaveLength(1)
    expect(owner.points[0]?.balanceAfter.equals(Money.fromWire('1000'))).toBe(true)

    const undatedSum = accounts[0]!.movements
      .filter(movement => !movement.source.isDeleted && movement.date === null)
      .reduce((total, movement) => total.add(movement.amount), Money.zero)
    expect(owner.points.at(-1)?.balanceAfter.add(undatedSum).equals(accounts[0]!.balance)).toBe(true)

    const monthly = buildMonthlyFlows(accounts)
    expect(monthly.undatedPaymentsCount).toBe(1)
    expect(monthly.months).toHaveLength(1)
    expect(monthly.months[0]?.issued.equals(Money.fromWire('1000'))).toBe(true)
    expect(monthly.months[0]?.repaid.isZero).toBe(true)

    const breakdown = buildOwnersBreakdown(accounts)
    expect(breakdown.labels).toEqual(['Иван'])
    expect(breakdown.values).toEqual([700])
  })

  it('группирует помесячные потоки строго хронологически через границу года', () => {
    const accounts = debtAccounts([
      debt(1, {
        date: date('2025-11-15'),
        sum: Money.fromWire('1200.50'),
        paySum: Money.fromWire('500.25'),
        payComment: '2025.12.20 300\n2026.01.05 200.25',
      }),
      debt(2, {
        typeId: 2,
        date: date('2026-01-10'),
        sum: Money.fromWire('800'),
        paySum: Money.fromWire('100'),
        payComment: '2026.01.18 100',
      }),
    ])

    const monthly = buildMonthlyFlows(accounts)

    expect(monthly.months.map(month => month.key)).toEqual(['2025-11', '2025-12', '2026-01'])
    expect(monthly.months.map(month => month.label)).toEqual(['ноя 2025', 'дек 2025', 'янв 2026'])

    expect(monthly.months[0]?.issued.equals(Money.fromWire('1200.50'))).toBe(true)
    expect(monthly.months[0]?.repaid.isZero).toBe(true)

    expect(monthly.months[1]?.issued.isZero).toBe(true)
    expect(monthly.months[1]?.repaid.equals(Money.fromWire('300'))).toBe(true)

    expect(monthly.months[2]?.issued.equals(Money.fromWire('800'))).toBe(true)
    expect(monthly.months[2]?.repaid.equals(Money.fromWire('300.25'))).toBe(true)
  })

  it('заполняет пустые промежуточные месяцы нулями между первой и последней датой', () => {
    const accounts = debtAccounts([
      debt(1, { date: date('2025-11-10'), sum: Money.fromWire('500') }),
      debt(2, { typeId: 2, date: date('2026-02-05'), sum: Money.fromWire('200') }),
    ])

    const monthly = buildMonthlyFlows(accounts)

    expect(monthly.months.map(month => month.key)).toEqual(['2025-11', '2025-12', '2026-01', '2026-02'])
    expect(monthly.months[1]?.issued.isZero).toBe(true)
    expect(monthly.months[1]?.repaid.isZero).toBe(true)
    expect(monthly.months[2]?.issued.isZero).toBe(true)
    expect(monthly.months[2]?.repaid.isZero).toBe(true)
    expect(monthly.months[3]?.issued.equals(Money.fromWire('200'))).toBe(true)
  })

  it('сортирует людей по модулю остатка и объединяет хвост в Прочие', () => {
    const debts: Debt[] = [
      debt(1, { ownerName: 'Ноль', sum: Money.fromWire('100'), paySum: Money.fromWire('100'), payComment: '2026.01.11 100' }),
      debt(2, { ownerName: 'Малый плюс', sum: Money.fromWire('50') }),
      debt(3, { ownerName: 'Малый минус', typeId: 2, sum: Money.fromWire('80') }),
      debt(4, { ownerName: 'Средний', sum: Money.fromWire('300') }),
      debt(5, { ownerName: 'Крупный', typeId: 2, sum: Money.fromWire('500') }),
    ]

    const accounts = debtAccounts(debts)
    const breakdown = buildOwnersBreakdown(accounts, 2)

    expect(breakdown.labels).toEqual([
      'Крупный',
      'Средний',
      'Прочие (мне должны)',
      'Прочие (я должен)',
    ])
    expect(breakdown.values).toEqual([-500, 300, 50, -80])
    expect(breakdown.directions).toEqual(['payable', 'receivable', 'receivable', 'payable'])

    const singleDirectionOverflow = buildOwnersBreakdown(
      debtAccounts([
        debt(10, { ownerName: 'Первый', sum: Money.fromWire('500') }),
        debt(11, { ownerName: 'Второй', sum: Money.fromWire('200') }),
        debt(12, { ownerName: 'Третий', sum: Money.fromWire('100') }),
      ]),
      1,
    )

    expect(singleDirectionOverflow.labels).toEqual(['Первый', 'Прочие'])
    expect(singleDirectionOverflow.values).toEqual([500, 300])
  })

  it('строит циклы долгов с закрытием при переходе через ноль и однодневным спаном', () => {
    const accounts = debtAccounts([
      debt(1, {
        ownerName: 'Анна',
        typeId: 1,
        date: date('2026-01-01'),
        sum: Money.fromWire('1000'),
        paySum: Money.fromWire('1000'),
        payComment: '2026.01.01 1000 в тот же день',
      }),
      debt(2, {
        ownerName: 'Анна',
        typeId: 1,
        date: date('2026-01-10'),
        sum: Money.fromWire('500'),
      }),
      debt(3, {
        ownerName: 'Анна',
        typeId: 2,
        date: date('2026-01-15'),
        sum: Money.fromWire('1200'),
      }),
      debt(4, {
        ownerName: 'Борис',
        typeId: 2,
        date: date('2026-01-05'),
        sum: Money.fromWire('400'),
        paySum: Money.fromWire('400'),
        payComment: '2026.01.20 400',
      }),
    ])

    const result = buildDebtCycles(accounts, date('2026-01-25'))

    expect(result.minDate).toBe('2026-01-01')
    expect(result.maxDate).toBe('2026-01-25')
    expect(result.totalDays).toBe(24)
    expect(result.owners).toEqual(['Анна', 'Борис'])
    expect(result.cycles).toHaveLength(4)

    const sameDay = result.cycles[0]!
    expect(sameDay.ownerName).toBe('Анна')
    expect(sameDay.status).toBe('closed')
    expect(sameDay.durationDays).toBe(0)
    expect(sameDay.startOffsetDays).toBe(0)
    expect(sameDay.endOffsetDays).toBe(1)
    expect(sameDay.peakAmount.equals(Money.fromWire('1000'))).toBe(true)
    expect(sameDay.eventsCount).toBe(2)

    const flippedClosed = result.cycles[1]!
    expect(flippedClosed.direction).toBe('receivable')
    expect(flippedClosed.status).toBe('closed')
    expect(flippedClosed.startDate).toBe('2026-01-10')
    expect(flippedClosed.endDate).toBe('2026-01-15')
    expect(flippedClosed.durationDays).toBe(5)
    expect(flippedClosed.eventsCount).toBe(2)

    const flippedActive = result.cycles[2]!
    expect(flippedActive.direction).toBe('payable')
    expect(flippedActive.status).toBe('active')
    expect(flippedActive.startDate).toBe('2026-01-15')
    expect(flippedActive.endDate).toBe('2026-01-25')
    expect(flippedActive.durationDays).toBe(10)
    expect(flippedActive.peakAmount.equals(Money.fromWire('700'))).toBe(true)
    expect(flippedActive.eventsCount).toBe(1)

    const borisCycle = result.cycles[3]!
    expect(borisCycle.ownerName).toBe('Борис')
    expect(borisCycle.direction).toBe('payable')
    expect(borisCycle.status).toBe('closed')
    expect(borisCycle.isEndDateUnknown).toBe(false)
    expect(borisCycle.durationDays).toBe(15)
  })

  it('не показывает закрытый долг с оплатой без даты как активный цикл до today', () => {
    const accounts = debtAccounts([
      debt(1, {
        ownerName: 'Старый',
        typeId: 1,
        date: date('2023-04-10'),
        sum: Money.fromWire('1000'),
        paySum: Money.fromWire('1000'),
        payComment: 'вернул наличными',
      }),
      debt(2, {
        ownerName: 'Текущий',
        typeId: 1,
        date: date('2026-01-05'),
        sum: Money.fromWire('500'),
      }),
    ])

    const result = buildDebtCycles(accounts, date('2026-01-20'))
    const legacy = result.cycles.find(cycle => cycle.ownerName === 'Старый')

    expect(result.undatedPaymentsCount).toBe(1)
    expect(result.owners).toEqual(['Текущий', 'Старый'])
    expect(legacy?.status).toBe('closed')
    expect(legacy?.isEndDateUnknown).toBe(true)
    expect(legacy?.endDate).toBe('2023-04-10')
  })

  it('форматирует подписи осей в рублях и возвращает исходное значение для нечисел', () => {
    expect(formatAxisRubles(1500)).toBe('1\u00A0500\u00A0₽')
    expect(formatAxisRubles(-250.5)).toBe('-250,50\u00A0₽')
    expect(formatAxisRubles('abc')).toBe('abc')
  })

  it('вычисляет разницу календарных дней точно', () => {
    expect(diffDays(date('2026-01-01'), date('2026-01-01'))).toBe(0)
    expect(diffDays(date('2026-02-28'), date('2026-03-01'))).toBe(1)
    expect(diffDays(date('2024-02-28'), date('2024-03-01'))).toBe(2)
  })

  it('возвращает конкретные цвета палитры без CSS-переменных', () => {
    const light = resolveDebtChartPalette(lightTheme, true)
    const dark = resolveDebtChartPalette(darkTheme, true)
    const neutral = resolveDebtChartPalette(lightTheme, false)

    for (const palette of [light, dark, neutral]) {
      for (const value of Object.values(palette) as string[]) {
        expect(value).not.toContain('var(')
        expect(value.length).toBeGreaterThan(0)
      }
    }

    expect(light.text).not.toBe(dark.text)
    expect(neutral.text).toBe('#666666')
    expect(light.repaid).not.toBe(light.receivable)
    expect(dark.repaid).not.toBe(dark.receivable)
    expect(light.repaid).toBe('#00897B')
    expect(dark.repaid).toBe('#4DB6AC')
  })
})
