import type { VueWrapper } from '@vue/test-utils'
import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, onTestFinished, vi } from 'vitest'
import { defineComponent, h, shallowRef } from 'vue'
import type { Router } from 'vue-router'
import { createMemoryHistory, createRouter } from 'vue-router'
import { captureRenderError, clearRenderError, handleAppError } from '@/app/errors'
import type { ErrorBoundaryState } from './useErrorBoundary'
import { useErrorBoundary } from './useErrorBoundary'

const BROKEN_MESSAGE = 'разметка сломалась'
const CONTENT = '[data-testid="fragile-content"]'

const thrown = shallowRef<Error | null>(null)

const Fragile = {
  setup: () => () => {
    if (thrown.value !== null) {
      throw thrown.value
    }

    return h('div', { 'data-testid': 'fragile-content' }, 'страница жива')
  },
}

beforeEach(() => {
  thrown.value = null
  clearRenderError()
})

describe('перехват ошибки отрисовки', () => {
  it('убирает сломанное содержимое, а не всю страницу', async () => {
    const { wrapper, boundary } = await mountBoundary()

    await breakRender(new Error(BROKEN_MESSAGE))

    expect(boundary.error.value?.message).toBe(BROKEN_MESSAGE)
    expect(wrapper.find(CONTENT).exists()).toBe(false)
    expect(wrapper.find('[data-testid="app-error"]').exists()).toBe(true)
  })

  it('не глушит ошибку, она доходит до консоли', async () => {
    const log = vi.spyOn(console, 'error').mockImplementation(() => undefined)

    await mountBoundary()
    await breakRender(new Error(BROKEN_MESSAGE))

    expect(log.mock.calls.flat()).toContainEqual(expect.objectContaining({ message: BROKEN_MESSAGE }))
  })

  it('брошенный не Error доходит до границы понятным сообщением', async () => {
    const { boundary } = await mountBoundary()

    captureRenderError({ code: 500 })
    await flushPromises()

    expect(boundary.error.value?.message).toBe('Неизвестная ошибка')
  })

  it('ловит сама, не полагаясь на обработчик приложения', async () => {
    const { boundary } = await mountBoundary({ isAppHandlerSilent: true })

    await breakRender(new Error(BROKEN_MESSAGE))

    expect(boundary.error.value?.message).toBe(BROKEN_MESSAGE)
  })
})

describe('возврат к работе', () => {
  it('«Повторить» снимает ошибку и возвращает содержимое', async () => {
    const { wrapper, boundary } = await mountBoundary()

    await breakRender(new Error(BROKEN_MESSAGE))

    thrown.value = null
    boundary.recover()
    await flushPromises()

    expect(boundary.error.value).toBeNull()
    expect(wrapper.get(CONTENT).text()).toBe('страница жива')
  })

  it('переход по маршруту снимает ошибку', async () => {
    const { wrapper, boundary, router } = await mountBoundary()

    await breakRender(new Error(BROKEN_MESSAGE))

    thrown.value = null
    await router.push('/settings')
    await flushPromises()

    expect(boundary.error.value).toBeNull()
    expect(wrapper.get(CONTENT).text()).toBe('страница жива')
  })
})

async function breakRender(reason: Error): Promise<void> {
  thrown.value = reason
  await flushPromises()
}

async function mountBoundary(options: { isAppHandlerSilent?: boolean } = {}): Promise<{
  wrapper: VueWrapper
  boundary: ErrorBoundaryState
  router: Router
}> {
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: Fragile },
      { path: '/settings', component: Fragile },
    ],
  })

  await router.push('/')
  await router.isReady()

  let boundary!: ErrorBoundaryState

  const wrapper = mount(
    defineComponent({
      setup() {
        boundary = useErrorBoundary()

        return () => boundary.error.value === null
          ? h(Fragile)
          : h('div', { 'data-testid': 'app-error' }, 'ошибка')
      },
    }),
    {
      global: {
        plugins: [router],
        config: {
          errorHandler: (reason: unknown, _instance: unknown, info: string) => {
            if (options.isAppHandlerSilent !== true) {
              handleAppError(reason, info)
            }
          },
        },
      },
    },
  )

  onTestFinished(() => {
    wrapper.unmount()
  })

  await flushPromises()

  return { wrapper, boundary, router }
}
