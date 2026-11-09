import { describe, expect, it } from 'vitest'
import { START_LOCATION } from 'vue-router'
import { resolveNavigation, resolveSessionExit } from './route-guard'

const home = { fullPath: '/', meta: { isPublic: true } }
const operations = { fullPath: '/operations', meta: {} }

describe('resolveNavigation', () => {
  it('первый заход на главную с сессией уводит на операции', () => {
    expect(resolveNavigation(home, START_LOCATION, true)).toBe('/operations')
  })

  it('первый заход на главную без сессии оставляет главную', () => {
    expect(resolveNavigation(home, START_LOCATION, false)).toBe(true)
  })

  it('переход на главную внутри приложения оставляет главную', () => {
    expect(resolveNavigation(home, operations, true)).toBe(true)
  })

  it('главная с запросом или якорем не считается адресом главной', () => {
    expect(resolveNavigation({ fullPath: '/?from=mail', meta: { isPublic: true } }, START_LOCATION, true)).toBe(true)
    expect(resolveNavigation({ fullPath: '/#history', meta: { isPublic: true } }, START_LOCATION, true)).toBe(true)
  })

  it('закрытый маршрут без сессии отправляет на вход с возвратом', () => {
    expect(resolveNavigation(operations, START_LOCATION, false)).toEqual({
      name: 'login',
      query: { returnUrl: '/operations' },
    })
  })
})

describe('resolveSessionExit', () => {
  it('протухшая сессия на закрытом экране уводит на вход с возвратом', () => {
    expect(resolveSessionExit('expired', { fullPath: '/operations?category=1', meta: {} })).toEqual({
      name: 'login',
      query: { returnUrl: '/operations?category=1' },
    })
  })

  it('протухшая сессия на открытом экране оставляет его', () => {
    expect(resolveSessionExit('expired', home)).toBeNull()
    expect(resolveSessionExit('expired', { fullPath: '/login', meta: { isPublic: true } })).toBeNull()
  })

  it.each(['signedOut', 'signedIn'] as const)('смена личности %s уходом с экрана не занимается', change => {
    expect(resolveSessionExit(change, operations)).toBeNull()
  })

  it('чужой адрес в возврате заменяется главной', () => {
    expect(resolveSessionExit('expired', { fullPath: '//evil.test/steal', meta: {} })).toEqual({
      name: 'login',
      query: { returnUrl: '/' },
    })
  })
})
