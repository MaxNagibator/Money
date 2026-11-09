import type { LocalDate } from '@/domain/local-date'
import type { Money } from '@/domain/money'

export interface Debt {
  id: number
  typeId: number
  sum: Money
  paySum: Money
  comment: string | null
  ownerName: string
  date: LocalDate
  payComment: string | null
  isDeleted: boolean
}

export interface DebtOwner {
  id: number
  name: string
}
