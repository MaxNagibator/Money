import type Decimal from 'decimal.js'
import { Money, MoneyDecimal, normalizeNumericInput } from '@/domain/money'

const NUMBER_PATTERN = /^(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?/

export function isMoneyExpression(raw: string): boolean {
  const normalized = normalizeNumericInput(raw)
  return normalized.length > 0 && Money.parseInput(normalized) === null
}

export function evaluateMoneyExpression(raw: string): Money | null {
  const normalized = normalizeNumericInput(raw)

  if (normalized.length === 0) {
    return null
  }

  const value = new ExpressionParser(normalized).parse()
  return value === null ? null : Money.fromDecimal(value)
}

class ExpressionParser {
  private position = 0

  constructor(private readonly input: string) {}

  parse(): Decimal | null {
    const value = this.readSum()

    if (value === null || this.position !== this.input.length || !value.isFinite()) {
      return null
    }

    return value
  }

  private readSum(): Decimal | null {
    let left = this.readProduct()

    while (left !== null) {
      const operator = this.input[this.position]

      if (operator !== '+' && operator !== '-') {
        break
      }

      this.position += 1
      const right = this.readProduct()

      if (right === null) {
        return null
      }

      left = operator === '+' ? left.plus(right) : left.minus(right)
    }

    return left
  }

  private readProduct(): Decimal | null {
    let left = this.readUnary()

    while (left !== null) {
      const operator = this.input[this.position]

      if (operator !== '*' && operator !== '/' && operator !== '%') {
        break
      }

      this.position += 1
      const right = this.readUnary()

      if (right === null || (operator !== '*' && right.isZero())) {
        return null
      }

      if (operator === '*') {
        left = left.times(right)
      } else if (operator === '/') {
        left = left.dividedBy(right)
      } else {
        left = left.modulo(right)
      }
    }

    return left
  }

  private readUnary(): Decimal | null {
    const operator = this.input[this.position]

    if (operator === '+') {
      this.position += 1
      return this.readUnary()
    }

    if (operator === '-') {
      this.position += 1
      const value = this.readUnary()
      return value === null ? null : value.negated()
    }

    return this.readPrimary()
  }

  private readPrimary(): Decimal | null {
    if (this.input[this.position] === '(') {
      this.position += 1
      const value = this.readSum()

      if (value === null || this.input[this.position] !== ')') {
        return null
      }

      this.position += 1
      return value
    }

    return this.readNumber()
  }

  private readNumber(): Decimal | null {
    const match = NUMBER_PATTERN.exec(this.input.slice(this.position))

    if (match === null) {
      return null
    }

    this.position += match[0].length

    try {
      return new MoneyDecimal(match[0])
    } catch {
      return null
    }
  }
}
