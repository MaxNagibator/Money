import { describe, expect, it } from 'vitest'
import { parse } from 'yaml'
import specText from '../../spec/date-alias.yml?raw'
import { parseDateInput } from '@/domain/date-input'
import type { LocalDate } from '@/domain/local-date'

interface DateCase {
  input: string
  expected: string | null
}

const spec = parse(specText) as { today: string, cases: DateCase[] }
const today = spec.today as LocalDate

describe('parseDateInput по таблице', () => {
  it.each(spec.cases.map(item => [item.input, item.expected]))(
    '%j даёт %j',
    (input, expected) => {
      expect(parseDateInput(String(input), today)).toBe(expected)
    },
  )
})
