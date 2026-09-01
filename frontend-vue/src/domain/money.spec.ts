import { describe, expect, it } from 'vitest'
import { Money } from '@/domain/money'

const NBSP = String.fromCharCode(0x00a0)
const NARROW_NBSP = String.fromCharCode(0x202f)

describe('Money.parseInput', () => {
  it.each([
    ['1234,56', 123_456],
    ['1234.56', 123_456],
    ['1 234,56', 123_456],
    [`1${NBSP}234,56`, 123_456],
    [`5${NARROW_NBSP}555,00`, 555_500],
    ['0,1', 10],
    ['.5', 50],
    ['-3,25', -325],
    ['7', 700],
    ['1,234', 123],
    ['2,999', 300],
    ['1e3', 100_000],
    ['2,5e2', 25_000],
    ['1E-2', 1],
  ])('разбирает %j как %i копеек', (input, expected) => {
    expect(Money.parseInput(input)?.toMinorUnits()).toBe(expected)
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
})

describe('Money arithmetic', () => {
  it('складывает без потери точности', () => {
    const left = Money.parseInput('0,1')
    const right = Money.parseInput('0,2')

    expect(left?.add(right!).toMinorUnits()).toBe(30)
  })

  it('округляет половину вверх при переводе в копейки', () => {
    expect(Money.fromMinorUnits(1).multiply(1.5).toMinorUnits()).toBe(2)
  })

  it('различает знак', () => {
    expect(Money.fromMinorUnits(-1).isNegative).toBe(true)
    expect(Money.zero.isZero).toBe(true)
  })
})
