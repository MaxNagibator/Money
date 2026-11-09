import type { RouteLocation, RouteRecordRaw } from 'vue-router'
import { describe, expect, it } from 'vitest'
import { buildRoutes, toAccountRedirect } from './routes'

const sandbox: RouteRecordRaw = { path: '/sandbox', redirect: '/' }

function paths(routes: RouteRecordRaw[]): string[] {
  return routes.map(route => route.path)
}

describe('buildRoutes', () => {
  it('песочницы нет среди маршрутов сборки', () => {
    expect(paths(buildRoutes([]))).not.toContain('/sandbox')
  })

  it('маршруты разработки встают перед перехватом неизвестного пути', () => {
    const routes = paths(buildRoutes([sandbox]))

    expect(routes.at(-2)).toBe('/sandbox')
    expect(routes.at(-1)).toBe('/:pathMatch(.*)*')
  })

  it('открыты без входа только главная, настройки, вход, регистрация, страницы Account и страница не найдена', () => {
    const open = buildRoutes([]).filter(route => route.meta?.isPublic === true)

    expect(paths(open)).toEqual([
      '/',
      '/settings',
      '/login',
      '/register',
      '/Account/Logout',
      '/Account/Lockout',
      '/Account/AccessDenied',
      '/Account/InvalidUser',
      '/Account/Callback',
      '/:pathMatch(.*)*',
    ])
  })

  it('настройки открыты без входа', () => {
    expect(buildRoutes([]).find(route => route.path === '/settings')?.meta?.isPublic).toBe(true)
  })

  it('прежние адреса входа и регистрации переводят на свои страницы', () => {
    expect(paths(buildRoutes([]))).toEqual(expect.arrayContaining(['/Account/Login', '/Account/Register', '/Account/Logout']))
  })
})

describe('toAccountRedirect', () => {
  function redirect(name: string, query: Record<string, string>): { name: string, query: Record<string, unknown> } {
    return toAccountRedirect(name)({ query } as unknown as RouteLocation)
  }

  it('переносит адрес возврата в прежнем написании', () => {
    expect(redirect('login', { ReturnUrl: '/debts' }).query.returnUrl).toBe('/debts')
  })

  it('оставляет собственное написание адреса возврата', () => {
    expect(redirect('login', { returnUrl: '/cars' }).query.returnUrl).toBe('/cars')
  })

  it('без адреса возврата не выдумывает его', () => {
    expect(redirect('register', {}).query.returnUrl).toBeUndefined()
  })
})
