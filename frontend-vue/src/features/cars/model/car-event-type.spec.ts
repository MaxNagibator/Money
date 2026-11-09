import { describe, expect, it } from 'vitest'
import { CAR_EVENT_TYPES, carEventType, findCarEventType, UNKNOWN_CAR_EVENT_TYPE } from './car-event-type'

describe('CAR_EVENT_TYPES', () => {
  it('перечисляет типы событий в порядке экрана', () => {
    expect(CAR_EVENT_TYPES.map(type => [type.id, type.name])).toEqual([
      [1, 'Приобретение'],
      [2, 'Страховка'],
      [3, 'Обслуживание'],
      [4, 'Переобувка'],
      [99, 'Прочее'],
    ])
  })

  it('каждому типу даёт свою иконку', () => {
    const icons = CAR_EVENT_TYPES.map(type => type.icon)

    expect(icons).toEqual([
      '/car_events/sdelka_64.png',
      '/car_events/strahovka_64.png',
      '/car_events/service_64.png',
      '/car_events/pereobyvka_64.png',
      '/car_events/other_64.png',
    ])
  })
})

describe('carEventType', () => {
  it('находит тип по идентификатору', () => {
    expect(carEventType(4).name).toBe('Переобувка')
  })

  it('незнакомый тип не роняет карточку', () => {
    expect(carEventType(777)).toBe(UNKNOWN_CAR_EVENT_TYPE)
    expect(carEventType(777).name).toBe('Неизвестный тип')
  })

  it('незнакомый тип в форме остаётся невыбранным', () => {
    expect(findCarEventType(777)).toBeNull()
    expect(findCarEventType(null)).toBeNull()
  })
})
