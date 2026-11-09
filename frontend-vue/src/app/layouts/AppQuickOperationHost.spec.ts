import type { VueWrapper } from '@vue/test-utils'
import { flushPromises, mount } from '@vue/test-utils'
import { beforeAll, describe, expect, it, vi } from 'vitest'
import type { Component } from 'vue'

const { chunk, loads } = vi.hoisted(() => ({ chunk: { fails: true }, loads: { count: 0 } }))

vi.mock('./AppQuickOperation.vue', async () => {
  loads.count += 1

  if (chunk.fails) {
    throw new Error('чанк формы не загрузился')
  }

  const { defineComponent, h } = await import('vue')

  return {
    __esModule: true,
    default: defineComponent({
      setup: () => () => h('button', {
        'data-testid': 'quick-operation-crash',
        'onClick': () => {
          throw new Error('форма сломалась после сохранения')
        },
      }),
    }),
  }
})

describe('ленивая форма быстрой операции', () => {
  beforeAll(async () => {
    await import('./AppQuickOperationHost.vue')
  }, 30_000)

  it('отказ загрузки объясняется пользователю и не доходит до глобального обработчика', async () => {
    const logged = vi.spyOn(console, 'error').mockImplementation(() => {})
    const errorHandler = vi.fn()

    const wrapper = await mountHost(errorHandler)

    expect(wrapper.get('[data-testid="quick-operation-error"]').text())
      .toBe('Не удалось открыть форму операции. Обновите страницу и попробуйте снова.')
    expect(errorHandler).not.toHaveBeenCalled()
    expect(logged).toHaveBeenCalledOnce()
    expect(loads.count).toBe(2)
  })

  it('поломка загруженной формы идёт в общий обработчик, а не в сообщение о незагрузке', async () => {
    chunk.fails = false

    const errorHandler = vi.fn()
    const wrapper = await mountHost(errorHandler)

    await wrapper.get('[data-testid="quick-operation-crash"]').trigger('click')

    expect(errorHandler).toHaveBeenCalledOnce()
    expect(wrapper.find('[data-testid="quick-operation-error"]').exists()).toBe(false)
  })
})

async function mountHost(errorHandler: () => void): Promise<VueWrapper> {
  vi.resetModules()

  const host = await import('./AppQuickOperationHost.vue') as { default: Component }
  const wrapper = mount(host.default, { global: { config: { errorHandler } } })

  await flushPromises()

  return wrapper
}
