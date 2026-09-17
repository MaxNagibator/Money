import { mount } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h, nextTick } from 'vue'
import { useTransitionsAfterMount } from './useTransitionsAfterMount'

let frames: FrameRequestCallback[] = []
let cancelled: number[] = []
let readEnabled: () => boolean = () => false

const host = defineComponent({
  setup() {
    const enabled = useTransitionsAfterMount()

    readEnabled = () => enabled.value

    return () => h('div')
  },
})

beforeEach(() => {
  frames = []
  cancelled = []
  vi.stubGlobal('requestAnimationFrame', (callback: FrameRequestCallback) => frames.push(callback))
  vi.stubGlobal('cancelAnimationFrame', (handle: number) => cancelled.push(handle))
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useTransitionsAfterMount', () => {
  it('держит переходы выключенными в кадре монтирования', async () => {
    mount(host)

    await nextTick()

    expect(readEnabled()).toBe(false)
  })

  it('включает переходы следующим кадром', async () => {
    mount(host)

    for (const frame of frames) {
      frame(0)
    }

    await nextTick()

    expect(readEnabled()).toBe(true)
  })

  it('снимает отложенный кадр при размонтировании', () => {
    const wrapper = mount(host)

    wrapper.unmount()

    expect(cancelled).toHaveLength(1)
  })
})
