import { describe, expect, it } from 'vitest'
import { mdAttachMoney } from '@/shared/icons'
import type { NavEntry } from './navigation'
import { buildBottomNavigation, buildNavigation, isNavGroup } from './navigation'

function titles(entries: NavEntry[]): string[] {
  return entries.map(entry => entry.title)
}

function paths(entries: NavEntry[]): string[] {
  return entries.flatMap(entry => isNavGroup(entry) ? entry.items.map(item => item.to) : [entry.to])
}

describe('buildNavigation', () => {
  it('без сессии видны только регистрация и вход', () => {
    const entries = buildNavigation({ isAuthenticated: false, userName: null })

    expect(titles(entries)).toEqual(['Регистрация', 'Вход'])
    expect(paths(entries)).toEqual(['/register', '/login'])
  })

  it('с сессией видны разделы без регистрации и входа', () => {
    const entries = buildNavigation({ isAuthenticated: true, userName: 'ivan' })

    expect(titles(entries)).toEqual(['Операции', 'Долги', 'Авто', 'Настройки', 'ivan'])
    expect(paths(entries)).not.toContain('/register')
    expect(paths(entries)).not.toContain('/login')
  })

  it('песочницы нет в меню ни с сессией, ни без', () => {
    expect(paths(buildNavigation({ isAuthenticated: true, userName: 'ivan' }))).not.toContain('/sandbox')
    expect(paths(buildNavigation({ isAuthenticated: false, userName: null }))).not.toContain('/sandbox')
  })

  it('имя пользователя стоит названием профиля', () => {
    const entries = buildNavigation({ isAuthenticated: true, userName: 'ivan' })

    expect(entries.at(-1)).toEqual({
      title: 'ivan',
      to: '/profile',
      icon: expect.any(String) as string,
      testId: 'nav-profile',
    })
  })

  it('до ответа профиля пункт называется общим словом', () => {
    const entries = buildNavigation({ isAuthenticated: true, userName: null })

    expect(entries.at(-1)?.title).toBe('Профиль')
  })

  it('долги собраны в группу со списком и отчётами', () => {
    const entries = buildNavigation({ isAuthenticated: true, userName: 'ivan' })
    const debts = entries.find(entry => entry.title === 'Долги')

    expect(debts).toBeDefined()
    expect(debts !== undefined && isNavGroup(debts)).toBe(true)
    if (debts === undefined || !isNavGroup(debts)) {
      return
    }

    expect(debts.items.map(item => ({ title: item.title, to: item.to, testId: item.testId, match: item.match }))).toEqual([
      { title: 'Список', to: '/debts', testId: 'nav-debts', match: 'all' },
      { title: 'Отчёты', to: '/debts/accounts', testId: 'nav-debt-accounts', match: undefined },
    ])
  })

  it('в нижней навигации у долгов остаётся денежная иконка', () => {
    const items = buildBottomNavigation()
    const debts = items.find(item => item.to === '/debts')

    expect(debts).toMatchObject({
      title: 'Долги',
      to: '/debts',
      icon: mdAttachMoney,
      testId: 'bottom-nav-debts',
    })
  })
})
