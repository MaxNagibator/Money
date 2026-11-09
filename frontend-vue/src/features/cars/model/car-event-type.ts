export interface CarEventType {
  id: number
  name: string
  icon: string
}

const ICONS_PATH = '/car_events/'

export const UNKNOWN_CAR_EVENT_TYPE: CarEventType = {
  id: 0,
  name: 'Неизвестный тип',
  icon: `${ICONS_PATH}other_64.png`,
}

export const CAR_EVENT_TYPES: readonly CarEventType[] = [
  { id: 1, name: 'Приобретение', icon: `${ICONS_PATH}sdelka_64.png` },
  { id: 2, name: 'Страховка', icon: `${ICONS_PATH}strahovka_64.png` },
  { id: 3, name: 'Обслуживание', icon: `${ICONS_PATH}service_64.png` },
  { id: 4, name: 'Переобувка', icon: `${ICONS_PATH}pereobyvka_64.png` },
  { id: 99, name: 'Прочее', icon: `${ICONS_PATH}other_64.png` },
]

export function findCarEventType(typeId: number | null): CarEventType | null {
  return CAR_EVENT_TYPES.find(type => type.id === typeId) ?? null
}

export function carEventType(typeId: number): CarEventType {
  return findCarEventType(typeId) ?? UNKNOWN_CAR_EVENT_TYPE
}
