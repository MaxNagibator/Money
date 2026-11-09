import type { LocalDate } from '@/domain/local-date'
import { fromDate } from '@/domain/local-date'

export interface Clock {
  now: () => Date
  today: () => LocalDate
}

export const systemClock: Clock = {
  now: () => new Date(),
  today: () => fromDate(new Date()),
}
