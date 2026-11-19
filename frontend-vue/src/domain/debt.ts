import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'

export interface DebtMovement {
  id: number
  typeId: number
  sum: Money
  comment: string | null
  ownerName: string
  date: LocalDate
  isDeleted: boolean
}

export interface DebtOwner {
  id: number
  name: string
}
