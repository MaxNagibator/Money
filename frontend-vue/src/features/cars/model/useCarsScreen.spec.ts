import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { defineComponent } from 'vue'
import {
  createCarEvent,
  deleteCarEvent,
  fetchCarEvents,
  restoreCarEvent,
  updateCarEvent,
} from '@/api/adapters/car-events'
import { createCar, fetchCars } from '@/api/adapters/cars'
import { ApiError } from '@/api/adapters/problem'
import type { Car, CarEvent } from '@/domain/car'
import type { LocalDate } from '@/domain/local-date'
import { parseLocalDate } from '@/domain/local-date'
import type { Clock } from '@/platform/clock'
import type { CarsScreenState } from './useCarsScreen'
import { useCarsScreen } from './useCarsScreen'

vi.mock('@/api/adapters/cars', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/cars')>(),
  fetchCars: vi.fn(),
  createCar: vi.fn(),
}))

vi.mock('@/api/adapters/car-events', async importOriginal => ({
  ...await importOriginal<typeof import('@/api/adapters/car-events')>(),
  fetchCarEvents: vi.fn(),
  createCarEvent: vi.fn(),
  updateCarEvent: vi.fn(),
  deleteCarEvent: vi.fn(),
  restoreCarEvent: vi.fn(),
}))

const fetchCarsMock = vi.mocked(fetchCars)
const createCarMock = vi.mocked(createCar)
const fetchCarEventsMock = vi.mocked(fetchCarEvents)
const createCarEventMock = vi.mocked(createCarEvent)
const updateCarEventMock = vi.mocked(updateCarEvent)
const deleteCarEventMock = vi.mocked(deleteCarEvent)
const restoreCarEventMock = vi.mocked(restoreCarEvent)

const TODAY = localDate('2026-03-01')

const clock: Clock = {
  now: () => new Date('2026-03-01T12:00:00'),
  today: () => TODAY,
}

let storedCars: Car[] = []
let storedEvents: CarEvent[] = []

beforeEach(() => {
  vi.clearAllMocks()

  storedCars = [car(1, 'Нива'), car(2, 'Ока')]
  storedEvents = [
    event(11, 1, '2026-01-10'),
    event(12, 1, '2026-02-20'),
    event(21, 2, '2026-02-01'),
  ]

  fetchCarsMock.mockImplementation(() => Promise.resolve([...storedCars]))
  fetchCarEventsMock.mockImplementation(carId => Promise.resolve(storedEvents.filter(item => item.carId === carId)))
  createCarMock.mockImplementation(name => {
    const created = car(3, name)

    storedCars = [...storedCars, created]

    return Promise.resolve(created.id)
  })
  createCarEventMock.mockResolvedValue(99)
  updateCarEventMock.mockResolvedValue(undefined)
  deleteCarEventMock.mockResolvedValue(undefined)
  restoreCarEventMock.mockResolvedValue(undefined)
})

describe('загрузка экрана', () => {
  it('держит загрузку, пока не пришли авто и события первой вкладки', async () => {
    const cars = defer<Car[]>()
    const events = defer<CarEvent[]>()

    fetchCarsMock.mockReturnValue(cars.promise)
    fetchCarEventsMock.mockReturnValue(events.promise)

    const screen = setup()

    expect(screen.isInitializing.value).toBe(true)

    cars.resolve([...storedCars])
    await flushPromises()

    expect(screen.cars.cars.value).toHaveLength(2)
    expect(screen.cars.activeCarId.value).toBe(1)
    expect(screen.isInitializing.value).toBe(true)

    events.resolve([event(11, 1, '2026-01-10')])
    await flushPromises()

    expect(screen.isInitializing.value).toBe(false)
    expect(screen.events.events.value.map(item => item.id)).toEqual([11])
  })

  it('переход на вкладку без кэша не возвращает экран в загрузку', async () => {
    const screen = setup()

    await settle(screen)

    fetchCarEventsMock.mockReturnValue(defer<CarEvent[]>().promise)
    screen.cars.activeCarId.value = 2
    await flushPromises()

    expect(screen.events.isLoading.value).toBe(true)
    expect(screen.isInitializing.value).toBe(false)
  })

  it('без авто показывает пустое состояние и не спрашивает события', async () => {
    storedCars = []

    const screen = setup()

    await settle(screen)

    expect(screen.cars.isEmpty.value).toBe(true)
    expect(screen.cars.activeCarId.value).toBeNull()
    expect(screen.events.events.value).toEqual([])
    expect(fetchCarEventsMock).not.toHaveBeenCalled()
  })

  it('с авто пустое состояние не показывается', async () => {
    const screen = setup()

    await settle(screen)

    expect(screen.cars.isEmpty.value).toBe(false)
  })
})

describe('добавление авто', () => {
  it('кнопка «+» открывает и закрывает форму', async () => {
    const screen = setup()

    await settle(screen)

    expect(screen.cars.isAddOpen.value).toBe(false)

    screen.cars.toggleAdd()

    expect(screen.cars.isAddOpen.value).toBe(true)

    screen.cars.toggleAdd()

    expect(screen.cars.isAddOpen.value).toBe(false)
  })

  it.each([
    ['', false],
    ['   ', false],
    ['Ока', true],
  ])('имя %j разрешает создание: %s', async (name, expected) => {
    const screen = setup()

    await settle(screen)

    screen.cars.newCarName.value = name

    expect(screen.cars.canCreate.value).toBe(expected)
  })

  it('пустое имя не отправляется', async () => {
    const screen = setup()

    await settle(screen)
    screen.cars.newCarName.value = '   '
    await screen.createCar()

    expect(createCarMock).not.toHaveBeenCalled()
  })

  it('имя уходит на сервер без обрезки пробелов', async () => {
    const screen = setup()

    await settle(screen)
    screen.cars.newCarName.value = ' Ока '
    await screen.createCar()

    expect(createCarMock).toHaveBeenCalledWith(' Ока ')
  })

  it('успешное создание закрывает форму и делает авто активным', async () => {
    const screen = setup()

    await settle(screen)

    screen.cars.toggleAdd()
    screen.cars.newCarName.value = 'Тойота'
    await screen.createCar()
    await flushPromises()

    expect(screen.cars.cars.value.map(item => item.name)).toEqual(['Нива', 'Ока', 'Тойота'])
    expect(screen.cars.activeCarId.value).toBe(3)
    expect(screen.cars.isAddOpen.value).toBe(false)
    expect(screen.cars.newCarName.value).toBe('')
    expect(screen.notice.value).toEqual({ text: 'Авто успешно сохранено!', type: 'success' })
  })

  it('ошибка создания оставляет форму открытой и не меняет список', async () => {
    const screen = setup()

    await settle(screen)

    createCarMock.mockRejectedValue(new ApiError('Не удалось сохранить авто. Пожалуйста, попробуйте еще раз.'))

    screen.cars.toggleAdd()
    screen.cars.newCarName.value = 'Тойота'
    await screen.createCar()
    await flushPromises()

    expect(screen.notice.value).toEqual({
      text: 'Не удалось сохранить авто. Пожалуйста, попробуйте еще раз.',
      type: 'warning',
    })

    expect(screen.cars.cars.value.map(item => item.name)).toEqual(['Нива', 'Ока'])
    expect(screen.cars.isAddOpen.value).toBe(true)
    expect(screen.cars.newCarName.value).toBe('Тойота')
  })
})

describe('события авто', () => {
  it('грузит события вкладки один раз и берёт их из кэша при возврате', async () => {
    const screen = setup()

    await settle(screen)

    expect(fetchCarEventsMock.mock.calls.map(([carId]) => carId)).toEqual([1])

    screen.cars.activeCarId.value = 2
    await vi.waitFor(() => expect(screen.events.events.value.map(item => item.id)).toEqual([21]))

    expect(fetchCarEventsMock.mock.calls.map(([carId]) => carId)).toEqual([1, 2])

    screen.cars.activeCarId.value = 1
    await flushPromises()

    expect(screen.events.events.value.map(item => item.id)).toEqual([12, 11])
    expect(fetchCarEventsMock).toHaveBeenCalledTimes(2)
  })

  it('удалённое событие остаётся в списке с меткой', async () => {
    const screen = setup()

    await settle(screen)
    await screen.removeEvent(findEvent(screen, 12))

    expect(deleteCarEventMock).toHaveBeenCalledWith(12)
    expect(screen.events.events.value.map(item => item.id)).toEqual([12, 11])
    expect(findEvent(screen, 12).isDeleted).toBe(true)
    expect(fetchCarEventsMock).toHaveBeenCalledTimes(1)
  })

  it('восстановление снимает метку', async () => {
    const screen = setup()

    await settle(screen)
    await screen.removeEvent(findEvent(screen, 12))
    await screen.restoreEvent(findEvent(screen, 12))
    await flushPromises()

    expect(restoreCarEventMock).toHaveBeenCalledWith(12)
    expect(findEvent(screen, 12).isDeleted).toBe(false)
  })

  it('ошибка удаления не меняет метку и показывает сообщение', async () => {
    const screen = setup()

    await settle(screen)

    deleteCarEventMock.mockRejectedValue(new ApiError('Извините, не удалось удалить'))

    await screen.removeEvent(findEvent(screen, 12))

    expect(findEvent(screen, 12).isDeleted).toBe(false)
    expect(screen.notice.value).toEqual({ text: 'Извините, не удалось удалить', type: 'error' })
  })

  it('созданное событие уходит на сервер и попадает в список по общему порядку', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreateEvent()

    expect(screen.form.value).toEqual({
      id: null,
      typeId: null,
      title: '',
      comment: '',
      mileage: null,
      date: TODAY,
    })

    expect(screen.dialogTitle.value).toBe('Создать')

    storedEvents = [...storedEvents, event(99, 1, '2026-03-01')]
    await screen.submitEvent({ ...screen.form.value!, typeId: 3 })
    await flushPromises()

    expect(createCarEventMock).toHaveBeenCalledWith({
      carId: 1,
      typeId: 3,
      title: null,
      comment: null,
      mileage: null,
      date: TODAY,
    })

    expect(screen.form.value).toBeNull()
    expect(screen.notice.value).toEqual({ text: 'Успех!', type: 'success' })
    expect(screen.events.events.value.map(item => item.id)).toEqual([99, 12, 11])
  })

  it('изменение уходит по идентификатору события', async () => {
    const screen = setup()

    await settle(screen)

    screen.startEditEvent(findEvent(screen, 12))

    expect(screen.dialogTitle.value).toBe('Обновить')

    await screen.submitEvent({ ...screen.form.value!, title: 'ТО-2' })
    await flushPromises()

    expect(updateCarEventMock).toHaveBeenCalledWith(12, expect.objectContaining({ carId: 1, title: 'ТО-2' }))
    expect(createCarEventMock).not.toHaveBeenCalled()
    expect(screen.form.value).toBeNull()
  })

  it('ошибка сохранения оставляет диалог открытым и список прежним', async () => {
    const screen = setup()

    await settle(screen)

    createCarEventMock.mockRejectedValue(new ApiError('Извините, неподдерживаемый тип события'))

    screen.startCreateEvent()
    await screen.submitEvent({ ...screen.form.value!, typeId: 3, title: 'ТО' })
    await flushPromises()

    expect(screen.form.value?.title).toBe('')
    expect(screen.isSaving.value).toBe(false)
    expect(screen.notice.value).toEqual({ text: 'Извините, неподдерживаемый тип события', type: 'error' })
    expect(screen.events.events.value.map(item => item.id)).toEqual([12, 11])
  })

  it('незаполненная форма на сервер не уходит', async () => {
    const screen = setup()

    await settle(screen)

    screen.startCreateEvent()
    await screen.submitEvent(screen.form.value!)

    expect(createCarEventMock).not.toHaveBeenCalled()
    expect(screen.form.value).not.toBeNull()
  })
})

function setup(): CarsScreenState {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  let screen!: CarsScreenState

  const wrapper = mount(
    defineComponent({
      setup() {
        screen = useCarsScreen(clock)

        return () => null
      },
    }),
    { global: { plugins: [[VueQueryPlugin, { queryClient }]] } },
  )

  onTestFinished(() => {
    wrapper.unmount()
    queryClient.clear()
  })

  return screen
}

async function settle(screen: CarsScreenState): Promise<void> {
  await vi.waitFor(() => {
    expect(screen.isInitializing.value).toBe(false)
  })
}

function findEvent(screen: CarsScreenState, id: number): CarEvent {
  const found = screen.events.events.value.find(item => item.id === id)

  if (found === undefined) {
    throw new Error(`Событие ${id} не найдено`)
  }

  return found
}

function car(id: number, name: string): Car {
  return { id, name }
}

function event(id: number, carId: number, date: string): CarEvent {
  return {
    id,
    carId,
    typeId: 3,
    title: null,
    comment: null,
    mileage: null,
    date: localDate(date),
    isDeleted: false,
  }
}

function localDate(value: string): LocalDate {
  return parseLocalDate(value)!
}

function defer<T>(): { promise: Promise<T>, resolve: (value: T) => void } {
  let resolve!: (value: T) => void

  const promise = new Promise<T>(settleWith => {
    resolve = settleWith
  })

  return { promise, resolve }
}
