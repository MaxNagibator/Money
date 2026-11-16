import { describe, expect, it } from 'vitest'
import { Money } from '@/domain/money'
import { formatMoneyEditor, formatRubles } from './format'

describe('рубли для двух представлений долгов', () => {
  it('сохраняет копейки в регистре и скрывает только нулевые в обычном режиме', () => {
    expect(formatRubles(Money.fromWire('10500'), false)).toBe('10 500,00 ₽')
    expect(formatRubles(Money.fromWire('10500'), true)).toBe('10 500 ₽')
    expect(formatRubles(Money.fromWire('10500.50'), true)).toBe('10 500,50 ₽')
  })
})
it('форматирует сумму для правки без потери точности', () => {
  expect(formatMoneyEditor(Money.fromWire('8000.0'))).toBe('8 000')
  expect(formatMoneyEditor(Money.fromWire('8000.0'), true)).toBe('8 000,00')
  expect(formatMoneyEditor(Money.fromWire('3500.123456789'))).toBe('3 500,123456789')
  expect(formatMoneyEditor(Money.fromWire('3500.123456789'), true)).toBe('3 500,123456789')
})
