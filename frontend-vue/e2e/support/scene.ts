import type { Page } from '@playwright/test'
import { json, optionalIdentifier, routeApi } from './api'
import {
  carEvents,
  cars,
  categories,
  debtOwners,
  debts,
  fastOperations,
  operations,
  places,
  regularOperations,
  userInfo,
} from './fixtures'

export async function stubScene(page: Page): Promise<void> {
  await routeApi(page, {
    '/connect/userinfo': ({ route }) => json(route, userInfo),
    '/Categories': ({ route }) => json(route, categories),
    '/Operations': ({ route, path }) => {
      if (path.includes('/GetPlaces')) {
        return json(route, places)
      }

      if (path.includes('/InferCategory')) {
        return json(route, {})
      }

      return json(route, operations)
    },
    '/FastOperations': ({ route }) => json(route, fastOperations),
    '/RegularOperations': ({ route }) => json(route, regularOperations),
    '/Debts': ({ route, path }) => {
      if (path.startsWith('/Debts/Owners/')) {
        return json(route, [])
      }

      if (path === '/Debts/Owners') {
        return json(route, debtOwners)
      }

      return json(route, debts)
    },
    '/Cars': ({ route }) => json(route, cars),
    '/CarEvents': ({ route, path }) => {
      const carId = optionalIdentifier(path)

      return json(route, carEvents.filter(event => event.carId === carId && !event.isDeleted))
    },
    '/Accounts': ({ route }) => json(route, {}),
  })
}
