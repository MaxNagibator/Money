import { mount } from '@vue/test-utils'
import { describe, expect, it, onTestFinished } from 'vitest'
import { defineComponent } from 'vue'
import { KEYBOARD_INSET_VARIABLE, useViewportVariables, VIEWPORT_HEIGHT_VARIABLE } from './useViewportVariables'

const LAYOUT_HEIGHT = 844
const KEYBOARD_HEIGHT = 336

describe('переменные вьюпорта для листов снизу', () => {
  it('замер снимается при монтировании', () => {
    installLayoutHeight()
    installViewport()
    setup()

    expect(readVariables()).toEqual({ height: '844px', inset: '0px' })
  })

  it('поднятая клавиатура пересчитывает обе величины', () => {
    installLayoutHeight()

    const viewport = installViewport()

    setup()
    dispatch(viewport, 'resize', { height: LAYOUT_HEIGHT - KEYBOARD_HEIGHT })

    expect(readVariables()).toEqual({ height: '508px', inset: '336px' })
  })

  it('прокрутка сдвигает offsetTop и перекрытие считается от него', () => {
    installLayoutHeight()

    const viewport = installViewport(LAYOUT_HEIGHT - KEYBOARD_HEIGHT)

    setup()
    dispatch(viewport, 'scroll', { offsetTop: 100 })

    expect(readVariables()).toEqual({ height: '508px', inset: '236px' })
  })

  it('отрицательное перекрытие обрезается нулём', () => {
    installLayoutHeight()

    const viewport = installViewport()

    setup()
    dispatch(viewport, 'resize', { height: LAYOUT_HEIGHT + 60 })

    expect(readVariables()).toEqual({ height: '904px', inset: '0px' })
  })

  it('без `visualViewport` переменные не выставляются', () => {
    installLayoutHeight()
    setup()

    expect(readVariables()).toEqual({ height: '', inset: '' })
  })

  it('размонтирование снимает переменные и отписывает от вьюпорта', () => {
    installLayoutHeight()

    const viewport = installViewport()

    setup(true)
    dispatch(viewport, 'resize', { height: LAYOUT_HEIGHT - KEYBOARD_HEIGHT })

    expect(readVariables()).toEqual({ height: '', inset: '' })
  })
})

class ViewportStub extends EventTarget implements VisualViewport {
  public offsetLeft = 0
  public offsetTop = 0
  public pageLeft = 0
  public pageTop = 0
  public width = 0
  public height = 0
  public scale = 1
  public onresize: VisualViewport['onresize'] = null
  public onscroll: VisualViewport['onscroll'] = null
}

function installViewport(height = LAYOUT_HEIGHT): ViewportStub {
  const viewport = new ViewportStub()

  viewport.height = height

  Object.defineProperty(window, 'visualViewport', { configurable: true, value: viewport })

  onTestFinished(() => {
    Reflect.deleteProperty(window, 'visualViewport')
  })

  return viewport
}

function installLayoutHeight(height = LAYOUT_HEIGHT): void {
  Object.defineProperty(document.documentElement, 'clientHeight', { configurable: true, value: height })

  onTestFinished(() => {
    Reflect.deleteProperty(document.documentElement, 'clientHeight')

    document.documentElement.style.removeProperty(VIEWPORT_HEIGHT_VARIABLE)
    document.documentElement.style.removeProperty(KEYBOARD_INSET_VARIABLE)
  })
}

function dispatch(viewport: ViewportStub, event: 'resize' | 'scroll', change: Partial<VisualViewport>): void {
  Object.assign(viewport, change)

  viewport.dispatchEvent(new Event(event))
}

function readVariables(): { height: string, inset: string } {
  const style = document.documentElement.style

  return {
    height: style.getPropertyValue(VIEWPORT_HEIGHT_VARIABLE),
    inset: style.getPropertyValue(KEYBOARD_INSET_VARIABLE),
  }
}

function setup(unmountNow = false): void {
  const wrapper = mount(defineComponent({
    setup() {
      useViewportVariables()

      return () => null
    },
  }))

  if (unmountNow) {
    wrapper.unmount()

    return
  }

  onTestFinished(() => {
    wrapper.unmount()
  })
}
