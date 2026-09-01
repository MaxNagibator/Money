export interface CategoryDto {
  id: number
  name: string
  parentId: number | null
  order: number | null
  color: string | null
  operationTypeId: number
}

export interface OperationDto {
  id: number
  categoryId: number
  sum: number
  comment: string | null
  place: string | null
  date: string
  createdTaskId: number | null
}

export interface FastOperationDto {
  id: number
  name: string
  order: number | null
  categoryId: number
  sum: number
  comment: string | null
  place: string | null
}

export interface RegularOperationDto {
  id: number
  name: string
  categoryId: number
  sum: number
  comment: string | null
  place: string | null
  timeTypeId: number
  timeValue: number | null
  dateFrom: string
  dateTo: string | null
  runTime: string | null
}

export interface DebtDto {
  id: number
  typeId: number
  sum: number
  paySum: number
  comment: string | null
  ownerName: string
  date: string
  payComment: string | null
  isDeleted: boolean
}

export interface DebtOwnerDto {
  id: number
  name: string
}

export interface CarDto {
  id: number
  name: string
}

export interface CarEventDto {
  id: number
  typeId: number
  title: string | null
  comment: string | null
  mileage: number | null
  date: string
}

export interface StoredCarEvent extends CarEventDto {
  carId: number
  isDeleted: boolean
}

export type UserInfo = Record<string, string>

export const FIXTURE_TODAY = '2026-02-03T10:00:00'

export const FIXTURE_RANGE = { from: '2026-02-01', to: '2026-02-28' }

export const STUB_USER_NAME = 'ivan'

export const userInfo: UserInfo = {
  name: STUB_USER_NAME,
  email: 'ivan@money.test',
  email_verified: 'true',
}

export const categories: CategoryDto[] = [
  { id: 1, name: 'Еда', parentId: null, order: null, color: '#ff0000', operationTypeId: 1 },
  { id: 2, name: 'Кафе', parentId: 1, order: null, color: null, operationTypeId: 1 },
  { id: 3, name: 'Зарплата', parentId: null, order: null, color: null, operationTypeId: 2 },
]

export const operations: OperationDto[] = [
  { id: 10, categoryId: 2, sum: 100.5, comment: 'обед', place: 'Кафе у дома', date: '2026-02-05T00:00:00', createdTaskId: null },
  { id: 11, categoryId: 1, sum: 200, comment: null, place: null, date: '2026-02-01T00:00:00', createdTaskId: null },
  { id: 12, categoryId: 3, sum: 5000, comment: null, place: null, date: '2026-02-01T00:00:00', createdTaskId: null },
]

export const fastOperations: FastOperationDto[] = [
  { id: 10, name: 'Кофе', order: 1, categoryId: 2, sum: 250.5, comment: 'утро', place: 'Пекарня' },
  { id: 11, name: 'Аванс', order: null, categoryId: 3, sum: 5000, comment: null, place: null },
]

export const regularOperations: RegularOperationDto[] = [
  {
    id: 1,
    name: 'Аренда',
    categoryId: 1,
    sum: 1000,
    comment: null,
    place: null,
    timeTypeId: 3,
    timeValue: 5,
    dateFrom: '2026-01-01T00:00:00',
    dateTo: null,
    runTime: '2026-02-05T00:00:00',
  },
  {
    id: 2,
    name: 'Подписка',
    categoryId: 2,
    sum: 299,
    comment: 'музыка',
    place: 'Онлайн',
    timeTypeId: 1,
    timeValue: null,
    dateFrom: '2026-01-10T00:00:00',
    dateTo: null,
    runTime: '2026-02-10T00:00:00',
  },
]

export const debtOwners: DebtOwnerDto[] = [
  { id: 10, name: 'Пётр' },
  { id: 20, name: 'Анна' },
]

export const debts: DebtDto[] = [
  { id: 1, typeId: 1, sum: 1000, paySum: 250, comment: 'за билеты', ownerName: 'Пётр', date: '2026-01-20T00:00:00', payComment: '2026-01-25 250 первый взнос', isDeleted: false },
  { id: 2, typeId: 2, sum: 500, paySum: 0, comment: null, ownerName: 'Анна', date: '2026-01-10T00:00:00', payComment: null, isDeleted: false },
  { id: 3, typeId: 1, sum: 300, paySum: 300, comment: null, ownerName: 'Пётр', date: '2026-01-05T00:00:00', payComment: null, isDeleted: false },
]

export const cars: CarDto[] = [
  { id: 1, name: 'Ласточка' },
  { id: 2, name: 'Пикап' },
]

export const carEvents: StoredCarEvent[] = [
  { id: 12, carId: 1, typeId: 4, title: 'Переобувка', comment: 'зима', mileage: 99500, date: '2026-02-01T00:00:00', isDeleted: false },
  { id: 11, carId: 1, typeId: 2, title: 'Страховка ОСАГО', comment: null, mileage: 98000, date: '2026-01-20T00:00:00', isDeleted: false },
  { id: 21, carId: 2, typeId: 1, title: 'Покупка', comment: null, mileage: 0, date: '2025-11-11T00:00:00', isDeleted: false },
]

export const places: string[] = ['Пекарня', 'Пекарня на углу']
