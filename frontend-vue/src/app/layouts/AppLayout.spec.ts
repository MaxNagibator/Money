import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query'
import type { VueWrapper } from '@vue/test-utils'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { ref } from 'vue'
import { createMemoryHistory, createRouter } from 'vue-router'
import AppLayout from './AppLayout.vue'

const { useNarrowViewportMock, auth } = vi.hoisted(() => ({
  useNarrowViewportMock: vi.fn(),
  auth: {} as { session: { isAuthenticated: boolean, signOut: () => Promise<void> } },
}))

vi.mock('@/shared/ui/useNarrowViewport', async importOriginal => ({
  ...await importOriginal<typeof import('@/shared/ui/useNarrowViewport')>(),
  useNarrowViewport: useNarrowViewportMock,
}))

vi.mock('@/features/auth', async importOriginal => {
  const { computed, reactive } = await import('vue')

  auth.session = reactive({ isAuthenticated: true, signOut: () => Promise.resolve() })

  return {
    ...await importOriginal<typeof import('@/features/auth')>(),
    useCurrentUserName: () => computed(() => 'demo'),
    useSessionStore: () => auth.session,
  }
})

vi.mock('@/features/categories', async () => {
  const { computed } = await import('vue')

  return {
    useCategories: () => ({
      categories: computed(() => []),
      isLoading: computed(() => false),
      loadError: computed(() => null),
    }),
  }
})

const narrow = ref(true)

beforeEach(() => {
  narrow.value = true
  auth.session.isAuthenticated = true
  useNarrowViewportMock.mockReturnValue(narrow)
})

describe('оболочка приложения', () => {
  it('«Ещё» нижней панели открывает меню оболочки, выбор раздела его закрывает', async () => {
    const wrapper = await setup()

    await wrapper.get('[data-testid="nav-operations"]').trigger('click')
    expect(wrapper.get('.mud-drawer').classes()).toContain('mud-drawer--closed')

    await wrapper.get('[data-testid="bottom-nav-more"]').trigger('click')

    expect(wrapper.get('.mud-drawer').classes()).toContain('mud-drawer--open')
  })

  it('на телефоне шапки нет, а заголовок раздела идёт первой строкой содержимого', async () => {
    const wrapper = await setup('/operations')

    expect(wrapper.find('[aria-label="Открыть меню"]').exists()).toBe(false)
    expect(wrapper.get('[data-testid="app-page-title"]').text()).toBe('Операции')
  })

  it('заголовка от оболочки нет там, где его рисует сама страница', async () => {
    const wrapper = await setup('/debts')

    expect(wrapper.find('[data-testid="app-page-title"]').exists()).toBe(false)
  })

  it('главная на телефоне получает заголовок от оболочки: свой она прячет на узком', async () => {
    const wrapper = await setup('/')

    expect(wrapper.get('[data-testid="app-page-title"]').text()).toBe('Проект «Филочки»')
  })

  it('выход из аккаунта закрывает меню телефона', async () => {
    const wrapper = await setup()

    await wrapper.get('[data-testid="nav-sign-out"]').trigger('click')
    await wrapper.vm.$nextTick()

    expect(wrapper.get('.mud-drawer').classes()).toContain('mud-drawer--closed')
    expect(document.body.classList).not.toContain('app-drawer-locked')
  })

  it('без сессии шапка на телефоне остаётся: нижней панели там нет', async () => {
    auth.session.isAuthenticated = false

    const wrapper = await setup()

    expect(wrapper.find('[aria-label="Открыть меню"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="bottom-nav"]').exists()).toBe(false)
  })

  it('на широком экране шапка остаётся', async () => {
    narrow.value = false

    const wrapper = await setup()

    expect(wrapper.find('[aria-label="Открыть меню"]').exists()).toBe(true)
  })

  it('тема переключается из меню, и меню остаётся открытым', async () => {
    const wrapper = await setup()

    const themeSwitch = wrapper.get('[data-testid="nav-theme-switch"]')
    const before = themeSwitch.text()

    await themeSwitch.trigger('click')

    expect(wrapper.get('[data-testid="nav-theme-switch"]').text()).not.toBe(before)
    expect(wrapper.get('.mud-drawer').classes()).toContain('mud-drawer--open')

    await wrapper.get('[data-testid="nav-theme-switch"]').trigger('click')
  })

  it('«+» открывает форму операции, и сохранение ей отдаёт оболочка', async () => {
    const wrapper = await setup()

    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(false)

    await wrapper.get('[data-testid="bottom-nav-add"]').trigger('click')
    await openedDialog(wrapper)

    expect(wrapper.get('[data-testid="operation-dialog-title"]').text()).toBe('Новый расход')
  })

  it('пока панель видна, оболочка помечает body для подъёма плавающих слоёв', async () => {
    const wrapper = await setup()

    expect(document.body.classList).toContain('app-bottom-nav-shown')

    narrow.value = false
    await wrapper.vm.$nextTick()

    expect(document.body.classList).not.toContain('app-bottom-nav-shown')
  })

  it('выход из аккаунта уносит открытую форму операции', async () => {
    const wrapper = await setup()

    await wrapper.get('[data-testid="bottom-nav-add"]').trigger('click')
    await openedDialog(wrapper)

    auth.session.isAuthenticated = false
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(false)
  })

  it('поворот телефона в альбомный не уносит открытую форму', async () => {
    const wrapper = await setup()

    await wrapper.get('[data-testid="bottom-nav-add"]').trigger('click')
    await openedDialog(wrapper)

    narrow.value = false
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="bottom-nav"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(true)
  })
})

async function setup(path = '/debts'): Promise<VueWrapper> {
  const page = { render: () => null }

  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', name: 'home', meta: { title: 'Проект «Филочки»' }, component: page },
      { path: '/operations', meta: { title: 'Операции' }, component: page },
      { path: '/statistics', meta: { title: 'Статистика' }, component: page },
      { path: '/debts', meta: { ownHeading: true, title: 'Долги' }, component: page },
    ],
  })

  await router.push(path)
  await router.isReady()

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  const wrapper = mount(AppLayout, {
    attachTo: document.body,
    global: {
      plugins: [router, createPinia(), [VueQueryPlugin, { queryClient }]],
      stubs: { teleport: true },
    },
  })

  onTestFinished(() => {
    wrapper.unmount()
  })

  return wrapper
}

async function openedDialog(wrapper: VueWrapper): Promise<void> {
  await vi.waitFor(() => {
    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(true)
  })
}
