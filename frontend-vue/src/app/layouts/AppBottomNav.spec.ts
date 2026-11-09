import type { VueWrapper } from '@vue/test-utils'
import { mount } from '@vue/test-utils'
import { describe, expect, it, onTestFinished, vi } from 'vitest'
import { nextTick } from 'vue'
import { createMemoryHistory, createRouter } from 'vue-router'
import AppBottomNav from './AppBottomNav.vue'

vi.mock('@/shared/ui/useNarrowViewport', async importOriginal => {
  const actual = await importOriginal<typeof import('@/shared/ui/useNarrowViewport')>()
  const { ref: createRef } = await import('vue')

  return { ...actual, useNarrowViewport: () => createRef(true) }
})

describe('нижняя панель навигации', () => {
  it.each([
    ['/operations', 'bottom-nav-operations'],
    ['/statistics', 'bottom-nav-statistics'],
    ['/debts', 'bottom-nav-debts'],
  ])('на маршруте %s подсвечен раздел %s', async (path, testId) => {
    const wrapper = await setup(path)

    const active = wrapper.findAll('.app-bottom-nav-link[aria-current="page"]')

    expect(active).toHaveLength(1)
    expect(active[0]?.attributes('data-testid')).toBe(testId)
  })

  it.each([
    ['bottom-nav-more', 'more'],
    ['bottom-nav-add', 'add'],
  ])('панель кнопкой %s только просит оболочку о событии %s', async (testId, event) => {
    const wrapper = await setup('/debts')

    await wrapper.get(`[data-testid="${testId}"]`).trigger('click')

    expect(wrapper.emitted(event)).toHaveLength(1)
    expect(wrapper.find('[data-testid="operation-dialog"]').exists()).toBe(false)
  })

  it('фокус в текстовом поле уводит панель, потеря фокуса возвращает', async () => {
    const wrapper = await setup('/operations')
    const field = document.createElement('input')

    document.body.append(field)
    onTestFinished(() => {
      field.remove()
    })

    field.dispatchEvent(new FocusEvent('focusin', { bubbles: true }))
    await nextTick()

    expect(wrapper.get('[data-testid="bottom-nav"]').classes()).toContain('app-bottom-nav-away')

    field.dispatchEvent(new FocusEvent('focusout', { bubbles: true }))
    await nextTick()

    expect(wrapper.get('[data-testid="bottom-nav"]').classes()).not.toContain('app-bottom-nav-away')
  })
})

async function setup(path: string): Promise<VueWrapper> {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: ['/operations', '/statistics', '/debts'].map(route => ({
      path: route,
      component: { render: () => null },
    })),
  })

  await router.push(path)
  await router.isReady()

  const wrapper = mount(AppBottomNav, {
    attachTo: document.body,
    global: {
      plugins: [router],
      stubs: { teleport: true },
    },
  })

  onTestFinished(() => {
    wrapper.unmount()
  })

  return wrapper
}
