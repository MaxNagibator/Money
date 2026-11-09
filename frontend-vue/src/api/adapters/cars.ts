import type { components } from '@/api/generated/schema'
import { apiClient } from '@/api/transport/client'
import type { Car } from '@/domain/car'
import { fail } from './problem'

type CarDto = components['schemas']['Money.Api.Dto.Cars.CarDto']

export const CAR_SAVE_ERROR = 'Не удалось сохранить авто. Пожалуйста, попробуйте еще раз.'

export async function fetchCars(): Promise<Car[]> {
  const { data, error } = await apiClient.GET('/Cars')

  fail(error, 'Не удалось загрузить авто')

  return (data ?? []).flatMap(toCar)
}

export async function createCar(name: string): Promise<number> {
  const { data, error } = await apiClient.POST('/Cars', { body: { name } })

  fail(error, CAR_SAVE_ERROR)

  return data ?? 0
}

export function toCar(dto: CarDto): Car[] {
  if (dto.id === undefined || dto.name === null || dto.name === undefined) {
    return []
  }

  return [{ id: dto.id, name: dto.name }]
}
