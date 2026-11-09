import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createMemoryHistory, createRouter, type RouteRecordRaw } from 'vue-router'
import { prefetchRoute, resetPrefetch } from './prefetch'

const stub = { template: '<div />' }

function routerWith(load: () => Promise<typeof stub>) {
  const routes: RouteRecordRaw[] = [
    { path: '/', name: 'home', component: stub },
    { path: '/debts', name: 'debts', component: load },
  ]

  return createRouter({ history: createMemoryHistory(), routes })
}

describe('предзагрузка чанка страницы', () => {
  beforeEach(resetPrefetch)

  it('вызывает загрузчик маршрута', () => {
    const load = vi.fn(() => Promise.resolve(stub))

    prefetchRoute(routerWith(load), '/debts')

    expect(load).toHaveBeenCalledTimes(1)
  })

  it('не повторяет загрузку одного пути', () => {
    const load = vi.fn(() => Promise.resolve(stub))
    const router = routerWith(load)

    prefetchRoute(router, '/debts')
    prefetchRoute(router, '/debts')

    expect(load).toHaveBeenCalledTimes(1)
  })

  it('позволяет повтор после неудачной загрузки', async () => {
    const load = vi.fn(() => Promise.reject(new Error('сеть')))
    const router = routerWith(load)

    prefetchRoute(router, '/debts')
    await Promise.resolve()
    await Promise.resolve()
    prefetchRoute(router, '/debts')

    expect(load).toHaveBeenCalledTimes(2)
  })

  it('молчит на маршруте с уже загруженным компонентом', () => {
    const load = vi.fn(() => Promise.resolve(stub))

    expect(() => prefetchRoute(routerWith(load), '/')).not.toThrow()
    expect(load).not.toHaveBeenCalled()
  })
})
