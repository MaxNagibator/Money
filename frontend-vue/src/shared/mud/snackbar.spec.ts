import { mount } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import MudSnackbar from './MudSnackbar.vue'

const showDuration = 1000
const visibleDuration = 5000
const hideDuration = 2000

function mountSnackbar() {
  return mount(MudSnackbar, {
    props: {
      message: 'Операция сохранена',
      modelValue: true,
      'onUpdate:modelValue': (value: boolean) => wrapper.setProps({ modelValue: value }),
    },
    attachTo: document.body,
  })
}

let wrapper: ReturnType<typeof mountSnackbar>

function bar() {
  return wrapper.get('.mud-snackbar')
}

function style() {
  return bar().attributes('style') ?? ''
}

async function advance(milliseconds: number): Promise<void> {
  vi.advanceTimersByTime(milliseconds)
  await nextTick()
}

beforeEach(() => {
  vi.useFakeTimers()
  wrapper = mountSnackbar()
})

afterEach(() => {
  wrapper.unmount()
  vi.useRealTimers()
})

describe('появление и уход уведомления', () => {
  it('проявляется, держится и гаснет по длительностям MudBlazor', async () => {
    expect(style()).toContain('mud-snackbar-showing')
    expect(style()).toContain(`${showDuration}ms`)

    await advance(showDuration)

    expect(style()).toContain('opacity: 95%')
    expect(style()).not.toContain('animation')

    await advance(visibleDuration - 1)

    expect(wrapper.find('.mud-snackbar').exists()).toBe(true)

    await advance(1)

    expect(style()).toContain('mud-snackbar-hiding')
    expect(style()).toContain(`${hideDuration}ms`)

    await advance(hideDuration)

    expect(wrapper.find('.mud-snackbar').exists()).toBe(false)
    expect(wrapper.props('modelValue')).toBe(false)
  })
})

describe('пауза и закрытие уведомления', () => {
  it('указатель над уведомлением отменяет уход и возвращает его после ухода указателя', async () => {
    await advance(showDuration + visibleDuration)

    expect(style()).toContain('mud-snackbar-hiding')

    await bar().trigger('pointerenter')

    expect(style()).toContain('opacity: 95%')
    expect(style()).not.toContain('animation')

    await advance(hideDuration * 2)

    expect(wrapper.find('.mud-snackbar').exists()).toBe(true)

    await bar().trigger('pointerleave')

    expect(style()).toContain('mud-snackbar-hiding')

    await advance(hideDuration)

    expect(wrapper.find('.mud-snackbar').exists()).toBe(false)
  })

  it('кнопка закрытия запускает уход, который наведением уже не отменить', async () => {
    await advance(showDuration)

    await wrapper.get('.mud-snackbar-close-button').trigger('click')

    expect(style()).toContain('mud-snackbar-hiding')

    await bar().trigger('pointerenter')

    expect(style()).toContain('mud-snackbar-hiding')

    await advance(hideDuration)

    expect(wrapper.find('.mud-snackbar').exists()).toBe(false)
  })
})
