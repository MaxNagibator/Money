import type { LocalDate } from '@/domain/local-date'
import { compareLocalDate, toParts } from '@/domain/local-date'
import { Money } from '@/domain/money'
import { compareNames, monthNominative } from '@/platform/format'
import type { DebtAccount, DebtMovement } from './debt-accounts'

export type DebtDirection = 'receivable' | 'payable'

export type OwnerTimelineMarkerKind = 'start' | 'reborrowed' | 'settled' | 'payment' | 'debt' | 'today'

export interface PortfolioTimelinePoint {
  date: LocalDate
  receivable: Money
  payable: Money
  net: Money
  isCarryForward: boolean
}

export interface PortfolioTimelineResult {
  points: PortfolioTimelinePoint[]
  undatedPaymentsCount: number
}

export interface OwnerTimelinePoint {
  date: LocalDate
  balanceAfter: Money
  delta: Money
  comments: string[]
  markerKind: OwnerTimelineMarkerKind
}

export interface OwnerTimelineResult {
  points: OwnerTimelinePoint[]
  undatedPaymentsCount: number
}

export interface MonthlyFlowBucket {
  key: string
  year: number
  month: number
  label: string
  fullLabel: string
  issued: Money
  repaid: Money
}

export interface MonthlyFlowsResult {
  months: MonthlyFlowBucket[]
  undatedPaymentsCount: number
}

export interface OwnersBreakdownResult {
  labels: string[]
  values: number[]
  directions: DebtDirection[]
  balances: Money[]
}

export interface DebtCycle {
  ownerName: string
  direction: DebtDirection
  startDate: LocalDate
  endDate: LocalDate
  startOffsetDays: number
  endOffsetDays: number
  durationDays: number
  peakAmount: Money
  eventsCount: number
  status: 'active' | 'closed'
}

export interface DebtCyclesResult {
  minDate: LocalDate | null
  maxDate: LocalDate | null
  totalDays: number
  owners: string[]
  cycles: DebtCycle[]
  undatedPaymentsCount: number
}

interface DatedMovement {
  ownerName: string
  movement: DebtMovement
  date: LocalDate
}

interface PendingCycle {
  ownerName: string
  direction: DebtDirection
  startDate: LocalDate
  endDate: LocalDate
  durationDays: number
  peakAmount: Money
  eventsCount: number
  status: 'active' | 'closed'
}

const MILLISECONDS_IN_DAY = 86_400_000
const DEFAULT_OWNERS_LIMIT = 12

export function diffDays(from: LocalDate, to: LocalDate): number {
  return toUtcDays(to) - toUtcDays(from)
}

export function countUndatedPayments(accounts: readonly DebtAccount[]): number {
  let count = 0

  for (const account of accounts) {
    for (const movement of account.movements) {
      if (!movement.source.isDeleted && movement.date === null) {
        count += 1
      }
    }
  }

  return count
}

export function buildPortfolioTimeline(
  accounts: readonly DebtAccount[],
  today?: LocalDate,
): PortfolioTimelineResult {
  const { dated, undatedPaymentsCount } = collectMovements(accounts)

  if (dated.length === 0) {
    return { points: [], undatedPaymentsCount }
  }

  dated.sort(compareDatedMovements)

  const balances = new Map<string, Money>()
  const points: PortfolioTimelinePoint[] = []
  let index = 0

  while (index < dated.length) {
    const currentDate = dated[index]!.date

    while (index < dated.length && dated[index]!.date === currentDate) {
      const item = dated[index]!
      const previous = balances.get(item.ownerName) ?? Money.zero
      balances.set(item.ownerName, previous.add(item.movement.amount))
      index += 1
    }

    const totals = sumOwnerBalances(balances)
    points.push({
      date: currentDate,
      receivable: totals.receivable,
      payable: totals.payable,
      net: totals.net,
      isCarryForward: false,
    })
  }

  const lastPoint = points.at(-1)
  if (lastPoint !== undefined && today !== undefined && compareLocalDate(today, lastPoint.date) > 0) {
    points.push({
      date: today,
      receivable: lastPoint.receivable,
      payable: lastPoint.payable,
      net: lastPoint.net,
      isCarryForward: true,
    })
  }

  return { points, undatedPaymentsCount }
}

export function buildOwnerTimeline(
  account: DebtAccount,
  today?: LocalDate,
): OwnerTimelineResult {
  const { dated, undatedPaymentsCount } = collectMovements([account])

  if (dated.length === 0) {
    return { points: [], undatedPaymentsCount }
  }

  dated.sort(compareDatedMovements)

  const points: OwnerTimelinePoint[] = []
  let runningBalance = Money.zero
  let hasStarted = false
  let index = 0

  while (index < dated.length) {
    const currentDate = dated[index]!.date
    const dayMovements: DebtMovement[] = []
    let delta = Money.zero

    while (index < dated.length && dated[index]!.date === currentDate) {
      const movement = dated[index]!.movement
      dayMovements.push(movement)
      delta = delta.add(movement.amount)
      index += 1
    }

    const previousBalance = runningBalance
    runningBalance = runningBalance.add(delta)

    const markerKind = resolveOwnerMarkerKind(previousBalance, runningBalance, hasStarted, dayMovements)
    if (!runningBalance.isZero || !previousBalance.isZero) {
      hasStarted = true
    }

    points.push({
      date: currentDate,
      balanceAfter: runningBalance,
      delta,
      comments: collectComments(dayMovements),
      markerKind,
    })
  }

  const lastPoint = points.at(-1)
  if (lastPoint !== undefined && today !== undefined && compareLocalDate(today, lastPoint.date) > 0) {
    points.push({
      date: today,
      balanceAfter: lastPoint.balanceAfter,
      delta: Money.zero,
      comments: [],
      markerKind: 'today',
    })
  }

  return { points, undatedPaymentsCount }
}

export function buildMonthlyFlows(accounts: readonly DebtAccount[]): MonthlyFlowsResult {
  const buckets = new Map<string, { year: number, month: number, issued: Money, repaid: Money }>()
  let undatedPaymentsCount = 0

  for (const account of accounts) {
    for (const movement of account.movements) {
      if (movement.source.isDeleted) {
        continue
      }

      if (movement.date === null) {
        undatedPaymentsCount += 1
        continue
      }

      const { year, month } = toParts(movement.date)
      const key = `${String(year).padStart(4, '0')}-${String(month).padStart(2, '0')}`
      const bucket = buckets.get(key) ?? { year, month, issued: Money.zero, repaid: Money.zero }
      const magnitude = absoluteMoney(movement.amount)

      if (movement.kind === 'entry') {
        bucket.issued = bucket.issued.add(magnitude)
      } else if (movement.kind === 'payment') {
        bucket.repaid = bucket.repaid.add(magnitude)
      }

      buckets.set(key, bucket)
    }
  }

  if (buckets.size === 0) {
    return { months: [], undatedPaymentsCount }
  }

  let minMonthIndex = Number.POSITIVE_INFINITY
  let maxMonthIndex = Number.NEGATIVE_INFINITY

  for (const bucket of buckets.values()) {
    const index = bucket.year * 12 + (bucket.month - 1)
    if (index < minMonthIndex) {
      minMonthIndex = index
    }
    if (index > maxMonthIndex) {
      maxMonthIndex = index
    }
  }

  const months: MonthlyFlowBucket[] = []

  for (let index = minMonthIndex; index <= maxMonthIndex; index += 1) {
    const year = Math.floor(index / 12)
    const month = index - year * 12 + 1
    const key = `${String(year).padStart(4, '0')}-${String(month).padStart(2, '0')}`
    const bucket = buckets.get(key)

    months.push({
      key,
      year,
      month,
      label: `${monthNominative(month).slice(0, 3)} ${year}`,
      fullLabel: `${monthNominative(month)} ${year}`,
      issued: bucket?.issued ?? Money.zero,
      repaid: bucket?.repaid ?? Money.zero,
    })
  }

  return { months, undatedPaymentsCount }
}

export function buildOwnersBreakdown(
  accounts: readonly DebtAccount[],
  limit = DEFAULT_OWNERS_LIMIT,
): OwnersBreakdownResult {
  const nonZero = accounts
    .filter(account => !account.balance.isZero)
    .sort((left, right) => {
      const byAbs = right.balance.toDecimal().abs().comparedTo(left.balance.toDecimal().abs())
      return byAbs !== 0 ? byAbs : compareNames(left.name, right.name)
    })

  const effectiveLimit = Math.max(0, limit)
  const head = nonZero.slice(0, effectiveLimit)
  const overflow = nonZero.slice(effectiveLimit)

  const entries: { label: string, balance: Money }[] = head.map(account => ({
    label: account.name,
    balance: account.balance,
  }))

  if (overflow.length > 0) {
    let otherReceivable = Money.zero
    let otherPayable = Money.zero

    for (const account of overflow) {
      if (account.balance.isNegative) {
        otherPayable = otherPayable.add(account.balance)
      } else {
        otherReceivable = otherReceivable.add(account.balance)
      }
    }

    if (!otherReceivable.isZero && !otherPayable.isZero) {
      entries.push({ label: 'Прочие (мне должны)', balance: otherReceivable })
      entries.push({ label: 'Прочие (я должен)', balance: otherPayable })
    } else if (!otherReceivable.isZero) {
      entries.push({ label: 'Прочие', balance: otherReceivable })
    } else if (!otherPayable.isZero) {
      entries.push({ label: 'Прочие', balance: otherPayable })
    }
  }

  return {
    labels: entries.map(entry => entry.label),
    values: entries.map(entry => entry.balance.toDecimal().toNumber()),
    directions: entries.map(entry => directionOf(entry.balance)),
    balances: entries.map(entry => entry.balance),
  }
}

export function buildDebtCycles(
  accounts: readonly DebtAccount[],
  today: LocalDate,
): DebtCyclesResult {
  let undatedPaymentsCount = 0
  const pendingCycles: PendingCycle[] = []
  const ownerStats = new Map<string, { latestActivity: LocalDate, count: number }>()

  for (const account of accounts) {
    const dated: DatedMovement[] = []

    for (const movement of account.movements) {
      if (movement.source.isDeleted) {
        continue
      }

      if (movement.date === null) {
        undatedPaymentsCount += 1
        continue
      }

      dated.push({ ownerName: account.name, movement, date: movement.date })
    }

    if (dated.length === 0) {
      continue
    }

    dated.sort(compareDatedMovements)

    const ownerCycles = extractOwnerCycles(account.name, dated, today)
    if (ownerCycles.length === 0) {
      continue
    }

    pendingCycles.push(...ownerCycles)

    const latestActivity = ownerCycles.reduce<LocalDate>(
      (latest, cycle) => compareLocalDate(cycle.endDate, latest) > 0 ? cycle.endDate : latest,
      ownerCycles[0]!.endDate,
    )

    const existingStats = ownerStats.get(account.name)
    if (existingStats === undefined) {
      ownerStats.set(account.name, { latestActivity, count: ownerCycles.length })
    } else {
      ownerStats.set(account.name, {
        latestActivity: compareLocalDate(latestActivity, existingStats.latestActivity) > 0
          ? latestActivity
          : existingStats.latestActivity,
        count: existingStats.count + ownerCycles.length,
      })
    }
  }

  if (pendingCycles.length === 0) {
    return {
      minDate: null,
      maxDate: null,
      totalDays: 0,
      owners: [],
      cycles: [],
      undatedPaymentsCount,
    }
  }

  let minDate = pendingCycles[0]!.startDate
  let maxDate = pendingCycles[0]!.endDate

  for (const cycle of pendingCycles) {
    if (compareLocalDate(cycle.startDate, minDate) < 0) {
      minDate = cycle.startDate
    }

    if (compareLocalDate(cycle.endDate, maxDate) > 0) {
      maxDate = cycle.endDate
    }
  }

  const cycles: DebtCycle[] = pendingCycles.map(cycle => {
    const startOffsetDays = diffDays(minDate, cycle.startDate)
    const endOffsetDays = Math.max(startOffsetDays + 1, diffDays(minDate, cycle.endDate))

    return {
      ...cycle,
      startOffsetDays,
      endOffsetDays,
    }
  })

  const totalDays = cycles.reduce((max, cycle) => Math.max(max, cycle.endOffsetDays), 1)
  const owners = [...ownerStats.entries()]
    .sort(([leftName, leftStats], [rightName, rightStats]) => {
      const byLatest = compareLocalDate(rightStats.latestActivity, leftStats.latestActivity)
      if (byLatest !== 0) {
        return byLatest
      }

      const byCount = rightStats.count - leftStats.count
      return byCount !== 0 ? byCount : compareNames(leftName, rightName)
    })
    .map(([name]) => name)

  return {
    minDate,
    maxDate,
    totalDays,
    owners,
    cycles,
    undatedPaymentsCount,
  }
}

function extractOwnerCycles(
  ownerName: string,
  dated: readonly DatedMovement[],
  today: LocalDate,
): PendingCycle[] {
  const cycles: PendingCycle[] = []
  let balance = Money.zero
  let active: {
    direction: DebtDirection
    startDate: LocalDate
    peakAmount: Money
    eventsCount: number
  } | null = null

  for (const item of dated) {
    balance = balance.add(item.movement.amount)

    if (balance.isZero) {
      if (active !== null) {
        cycles.push({
          ownerName,
          direction: active.direction,
          startDate: active.startDate,
          endDate: item.date,
          durationDays: diffDays(active.startDate, item.date),
          peakAmount: active.peakAmount,
          eventsCount: active.eventsCount + 1,
          status: 'closed',
        })
        active = null
      }
      continue
    }

    const nextDirection = directionOf(balance)
    const magnitude = absoluteMoney(balance)

    if (active === null) {
      active = {
        direction: nextDirection,
        startDate: item.date,
        peakAmount: magnitude,
        eventsCount: 1,
      }
    } else if (active.direction === nextDirection) {
      active.eventsCount += 1
      if (magnitude.compare(active.peakAmount) > 0) {
        active.peakAmount = magnitude
      }
    } else {
      cycles.push({
        ownerName,
        direction: active.direction,
        startDate: active.startDate,
        endDate: item.date,
        durationDays: diffDays(active.startDate, item.date),
        peakAmount: active.peakAmount,
        eventsCount: active.eventsCount + 1,
        status: 'closed',
      })
      active = {
        direction: nextDirection,
        startDate: item.date,
        peakAmount: magnitude,
        eventsCount: 1,
      }
    }
  }

  if (active !== null) {
    const lastEventDate = dated.at(-1)!.date
    const endDate = compareLocalDate(today, lastEventDate) >= 0 ? today : lastEventDate
    cycles.push({
      ownerName,
      direction: active.direction,
      startDate: active.startDate,
      endDate,
      durationDays: diffDays(active.startDate, endDate),
      peakAmount: active.peakAmount,
      eventsCount: active.eventsCount,
      status: 'active',
    })
  }

  return cycles
}

function collectMovements(accounts: readonly DebtAccount[]): {
  dated: DatedMovement[]
  undatedPaymentsCount: number
} {
  const dated: DatedMovement[] = []
  let undatedPaymentsCount = 0

  for (const account of accounts) {
    for (const movement of account.movements) {
      if (movement.source.isDeleted) {
        continue
      }

      if (movement.date === null) {
        undatedPaymentsCount += 1
        continue
      }

      dated.push({ ownerName: account.name, movement, date: movement.date })
    }
  }

  return { dated, undatedPaymentsCount }
}

function compareDatedMovements(left: DatedMovement, right: DatedMovement): number {
  const byDate = compareLocalDate(left.date, right.date)
  if (byDate !== 0) {
    return byDate
  }

  const byId = left.movement.source.id - right.movement.source.id
  if (byId !== 0) {
    return byId
  }

  return Number(left.movement.kind === 'payment') - Number(right.movement.kind === 'payment')
}

function sumOwnerBalances(balances: ReadonlyMap<string, Money>): {
  receivable: Money
  payable: Money
  net: Money
} {
  let receivable = Money.zero
  let payable = Money.zero

  for (const balance of balances.values()) {
    if (balance.isNegative) {
      payable = payable.subtract(balance)
    } else if (!balance.isZero) {
      receivable = receivable.add(balance)
    }
  }

  return {
    receivable,
    payable,
    net: receivable.subtract(payable),
  }
}

function resolveOwnerMarkerKind(
  previousBalance: Money,
  runningBalance: Money,
  hasStarted: boolean,
  dayMovements: readonly DebtMovement[],
): OwnerTimelineMarkerKind {
  if (runningBalance.isZero) {
    return 'settled'
  }

  if (previousBalance.isZero) {
    return hasStarted ? 'reborrowed' : 'start'
  }

  return dayMovements.every(movement => movement.kind === 'payment') ? 'payment' : 'debt'
}

function collectComments(movements: readonly DebtMovement[]): string[] {
  const comments: string[] = []

  for (const movement of movements) {
    const trimmed = movement.comment?.trim()
    if (trimmed !== undefined && trimmed.length > 0) {
      comments.push(trimmed)
    }
  }

  return comments
}

function directionOf(balance: Money): DebtDirection {
  return balance.isNegative ? 'payable' : 'receivable'
}

function absoluteMoney(value: Money): Money {
  return value.isNegative ? value.negate() : value
}

function toUtcDays(date: LocalDate): number {
  const { year, month, day } = toParts(date)
  return Math.floor(Date.UTC(year, month - 1, day) / MILLISECONDS_IN_DAY)
}
