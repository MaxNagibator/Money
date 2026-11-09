import { describe, expect, it } from 'vitest'
import { Money } from '@/domain/money'

const NBSP = String.fromCharCode(0x00a0)
const NARROW_NBSP = String.fromCharCode(0x202f)

describe('Money.parseInput', () => {
  it.each([
    ['1234,56', '1234.56'],
    ['1234.56', '1234.56'],
    ['1 234,56', '1234.56'],
    [`1${NBSP}234,56`, '1234.56'],
    [`5${NARROW_NBSP}555,00`, '5555'],
    ['0,1', '0.1'],
    ['.5', '0.5'],
    ['-3,25', '-3.25'],
    ['7', '7'],
    ['1,234', '1.234'],
    ['2,999', '2.999'],
    ['1e3', '1000'],
    ['2,5e2', '250'],
    ['1E-2', '0.01'],
  ])('разбирает %j как %s', (input, expected) => {
    expect(Money.parseInput(input)?.toDecimal().toString()).toBe(expected)
  })

  it.each([
    [''],
    ['   '],
    ['abc'],
    ['1..2'],
    ['1,2,3'],
    ['1+2'],
    ['1e'],
    ['1e2,5'],
  ])('отвергает %j', (input) => {
    expect(Money.parseInput(input)).toBeNull()
  })

  it.each([
    ['250,50', 2],
    ['250,5', 1],
    ['250', 0],
    [`5${NARROW_NBSP}555,00`, 2],
    ['2,5e2', null],
  ])('запоминает масштаб введённого %j как %o', (input, scale) => {
    expect(Money.parseInput(input)?.wireScale).toBe(scale)
  })
})

describe('Money.fromWire', () => {
  it.each([
    ['100.00', '100', 2],
    ['100.5', '100.5', 1],
    ['100', '100', 0],
    ['-0.10', '-0.1', 2],
    ['9007199254740993.55', '9007199254740993.55', 2],
    ['1e3', '1000', null],
  ])('читает литерал %j как %s с масштабом %o', (literal, expected, scale) => {
    const value = Money.fromWire(literal)

    expect(value.toDecimal().toString()).toBe(expected)
    expect(value.wireScale).toBe(scale)
  })

  it('берёт масштаб из записи числа, когда литерал не сохранился', () => {
    expect(Money.fromWire(100.5).wireScale).toBe(1)
    expect(Money.fromWire(100).wireScale).toBe(0)
  })

  it('вычисленная сумма масштаб не наследует', () => {
    expect(Money.fromWire('100.00').add(Money.zero).wireScale).toBeNull()
    expect(Money.fromMinorUnits(100).wireScale).toBeNull()
  })
})

describe('Money arithmetic', () => {
  it('складывает без потери точности', () => {
    const left = Money.parseInput('0,1')
    const right = Money.parseInput('0,2')

    expect(left?.add(right!).toDecimal().toString()).toBe('0.3')
  })

  it('не округляет доли копейки', () => {
    expect(Money.fromMinorUnits(1).multiply(1.5).toDecimal().toString()).toBe('0.015')
  })

  it('округляет половину вверх при переводе в копейки', () => {
    expect(Money.fromMinorUnits(1).multiply(1.5).toMinorUnits()).toBe(2)
  })

  it('различает знак', () => {
    expect(Money.fromMinorUnits(-1).isNegative).toBe(true)
    expect(Money.zero.isZero).toBe(true)
  })
})
