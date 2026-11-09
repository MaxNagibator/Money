import { rawNumber } from '@/api/transport/raw-number'
import type { Money } from '@/domain/money'

export function toWireSum(value: Money): number {
  const scale = value.wireScale

  return rawNumber(scale === null ? value.toDecimal().toFixed() : value.toDecimal().toFixed(scale))
}
