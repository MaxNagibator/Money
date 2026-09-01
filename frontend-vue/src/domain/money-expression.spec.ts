import { describe, expect, it } from 'vitest'
import { parse } from 'yaml'
import specText from '../../spec/money-expression.yml?raw'
import { evaluateMoneyExpression, isMoneyExpression } from '@/domain/money-expression'

interface ExpressionCase {
  input: string
  expected: string | null
}

const spec = parse(specText) as { cases: ExpressionCase[] }

describe('evaluateMoneyExpression по таблице', () => {
  it.each(spec.cases.map(item => [item.input, item.expected]))(
    '%j даёт %j',
    (input, expected) => {
      const result = evaluateMoneyExpression(input)

      if (expected === null) {
        expect(result).toBeNull()
        return
      }

      expect(result?.toDecimal().toString()).toBe(expected)
    },
  )
})

describe('isMoneyExpression', () => {
  it.each([
    ['1+2', true],
    ['(1)', true],
    ['1234,56', false],
    ['1 234,56', false],
    ['-5', false],
    ['1e3', false],
    ['', false],
    ['abc', true],
  ])('%j это выражение: %s', (input, expected) => {
    expect(isMoneyExpression(input)).toBe(expected)
  })
})
