import type { LocalDate } from '@/domain/local-date'

export interface Car {
  id: number
  name: string
}

export interface CarEvent {
  id: number
  carId: number
  typeId: number
  title: string | null
  comment: string | null
  mileage: number | null
  date: LocalDate
  isDeleted: boolean
}

export function sortCarEvents(events: readonly CarEvent[]): CarEvent[] {
  return [...events].sort((left, right) => {
    if (left.date !== right.date) {
      return left.date < right.date ? 1 : -1
    }

    return right.id - left.id
  })
}

export function maxMileage(events: readonly CarEvent[]): number {
  return events.reduce((top, event) => Math.max(top, event.mileage ?? 0), 0)
}
