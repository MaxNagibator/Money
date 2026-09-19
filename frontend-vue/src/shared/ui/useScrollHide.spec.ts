import { mount } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { Ref } from 'vue'
import { defineComponent, nextTick } from 'vue'
import { useScrollHide } from './useScrollHide'

const SCROLL_HIDE_THRESHOLD = 24
const SCROLL_HIDE_DELAY = 150

describe('скрытие при прокрутке вниз', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    window.scrollTo(0, 0)
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it.each([
    ['меньше порога', SCROLL_HIDE_THRESHOLD - 1, false],
    ['ровно порог', SCROLL_HIDE_THRESHOLD, true],
  ])('движение вниз на %s прячет панель: %s', async (_name, shift, expected) => {
    const { isHidden } = setup()

    await scroll(shift)
    vi.advanceTimersByTime(SCROLL_HIDE_DELAY)

    expect(isHidden.value).toBe(expected)
  })

  it('панель прячется не раньше задержки', async () => {
    const { isHidden } = setup()

    await scroll(SCROLL_HIDE_THRESHOLD)
    vi.advanceTimersByTime(SCROLL_HIDE_DELAY - 1)

    expect(isHidden.value).toBe(false)

    vi.advanceTimersByTime(1)

    expect(isHidden.value).toBe(true)
  })

  it('движение вверх возвращает панель сразу', async () => {
    const { isHidden } = setup()

    await scroll(SCROLL_HIDE_THRESHOLD)
    vi.advanceTimersByTime(SCROLL_HIDE_DELAY)

    await scroll(-SCROLL_HIDE_THRESHOLD)

    expect(isHidden.value).toBe(false)
  })

  it('короткий откат вверх не возвращает спрятанную панель', async () => {
    const { isHidden } = setup()

    await scroll(SCROLL_HIDE_THRESHOLD)
    vi.advanceTimersByTime(SCROLL_HIDE_DELAY)

    await scroll(-(SCROLL_HIDE_THRESHOLD - 1))

    expect(isHidden.value).toBe(true)
  })

  it('выключенный композабл панель не прячет', async () => {
    const { isHidden } = setup(false)

    await scroll(SCROLL_HIDE_THRESHOLD * 2)
    vi.advanceTimersByTime(SCROLL_HIDE_DELAY)

    expect(isHidden.value).toBe(false)
  })
})

function setup(isEnabled = true): { isHidden: Readonly<Ref<boolean>> } {
  let isHidden!: Readonly<Ref<boolean>>

  mount(defineComponent({
    setup() {
      isHidden = useScrollHide(isEnabled)

      return () => null
    },
  }))

  return { isHidden }
}

async function scroll(shift: number): Promise<void> {
  window.scrollTo(0, window.scrollY + shift)
  window.dispatchEvent(new Event('scroll'))

  await nextTick()
}
