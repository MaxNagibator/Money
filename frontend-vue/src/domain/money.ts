import Decimal from 'decimal.js'

export const MoneyDecimal = Decimal.clone({
  precision: 28,
  rounding: Decimal.ROUND_HALF_UP,
  toExpNeg: -28,
  toExpPos: 28,
})

const MINOR_UNITS_IN_MAJOR = 100
const INPUT_PATTERN = /^-?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?$/
const SPACES_PATTERN = new RegExp(String.raw`[\s\u00A0\u202F]`, 'g')

export function normalizeNumericInput(raw: string): string {
  return raw.replace(SPACES_PATTERN, '').replaceAll(',', '.')
}

export class Money {
  private constructor(private readonly amount: Decimal) {}

  static readonly zero: Money = new Money(new MoneyDecimal(0))

  static fromMinorUnits(minorUnits: number): Money {
    if (!Number.isInteger(minorUnits)) {
      throw new RangeError('Копейки задаются целым числом')
    }

    return new Money(new MoneyDecimal(minorUnits).dividedBy(MINOR_UNITS_IN_MAJOR))
  }

  static fromDecimal(value: Decimal): Money {
    return new Money(value)
  }

  static parseInput(raw: string): Money | null {
    const normalized = normalizeNumericInput(raw)

    if (normalized.length === 0 || !INPUT_PATTERN.test(normalized)) {
      return null
    }

    return new Money(new MoneyDecimal(normalized))
  }

  add(other: Money): Money {
    return new Money(this.amount.plus(other.amount))
  }

  subtract(other: Money): Money {
    return new Money(this.amount.minus(other.amount))
  }

  multiply(factor: number): Money {
    return new Money(this.amount.times(factor))
  }

  negate(): Money {
    return new Money(this.amount.negated())
  }

  compare(other: Money): number {
    return this.amount.comparedTo(other.amount)
  }

  equals(other: Money): boolean {
    return this.amount.equals(other.amount)
  }

  get isZero(): boolean {
    return this.amount.isZero()
  }

  get isNegative(): boolean {
    return this.amount.isNegative()
  }

  toMinorUnits(): number {
    return this.amount
      .times(MINOR_UNITS_IN_MAJOR)
      .toDecimalPlaces(0, Decimal.ROUND_HALF_UP)
      .toNumber()
  }

  toDecimal(): Decimal {
    return this.amount
  }
}
