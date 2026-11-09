import { describe, expect, it } from 'vitest'
import { toWireSum } from '@/api/adapters/money-wire'
import { serializeBody } from '@/api/transport/raw-number'
import { rewriteWireJson } from '@/api/transport/wire-json'
import { Money } from '@/domain/money'
import { formatMoneyPlain } from '@/platform/format'

function wire(value: Money): string {
  return serializeBody({ sum: toWireSum(value) }) as string
}

function typed(input: string): Money {
  const value = Money.parseInput(input)

  if (value === null) {
    throw new Error(`Не разобрана сумма ${input}`)
  }

  return value
}

describe('toWireSum', () => {
  it.each([
    ['250,50', '250.50'],
    ['250,5', '250.5'],
    ['250', '250'],
    ['0,015', '0.015'],
    ['-0,10', '-0.10'],
  ])('отправляет введённое %j как %s', (input, expected) => {
    expect(wire(typed(input))).toBe(`{"sum":${expected}}`)
  })

  it.each([
    ['вычисленную', typed('250,50').add(Money.zero)],
    ['из выражения', Money.fromMinorUnits(25_050)],
  ])('%s сумму отправляет без хвостовых нулей', (_case, value) => {
    expect(wire(value)).toBe('{"sum":250.5}')
  })

  it('масштаб введённой суммы переживает круг до карточки', () => {
    const body = wire(typed('250,50'))
    const received = JSON.parse(rewriteWireJson(body)!) as { sum: string }

    expect(body).toBe('{"sum":250.50}')
    expect(formatMoneyPlain(Money.fromWire(received.sum))).toBe('250,50')
  })
})
