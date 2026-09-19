import { mount } from '@vue/test-utils'
import { describe, expect, it, onTestFinished } from 'vitest'
import type { Ref } from 'vue'
import { defineComponent } from 'vue'
import { KEYBOARD_VIEWPORT_RATIO, useKeyboardHide } from './useKeyboardHide'

const SCREEN_HEIGHT = window.screen.height
const FULL_HEIGHT = SCREEN_HEIGHT * 0.9
const SHRUNK_HEIGHT = SCREEN_HEIGHT * 0.5

describe('скрытие под экранной клавиатурой, сжатый вьюпорт вместе с фокусом', () => {
  it.each([
    [0.5, true],
    [0.35, true],
    [0.3, true],
    [KEYBOARD_VIEWPORT_RATIO, false],
    [0.1, false],
    [0, false],
  ])('вьюпорт ниже экрана на %s при фокусе в поле – панель спрятана: %s', (share, expected) => {
    const viewport = installViewport()
    const isHidden = setup()

    focusIn(createInput('text'))
    resize(viewport, { height: SCREEN_HEIGHT - SCREEN_HEIGHT * share })

    expect(isHidden.value).toBe(expected)
  })

  it.each<[string, (() => HTMLElement) | null]>([
    ['без фокуса', null],
    ['с фокусом в кнопке', () => createElement('button')],
    ['с фокусом в readonly-поле', () => {
      const field = createInput('text')

      field.readOnly = true

      return field
    }],
  ])('сжатие вьюпорта %s панель не прячет', (_name, create) => {
    const viewport = installViewport()
    const isHidden = setup()

    if (create) {
      focusIn(create())
    }

    resize(viewport, { height: SHRUNK_HEIGHT })

    expect(isHidden.value).toBe(false)
  })

  it('клавиатура закрылась при сохранённом фокусе – панель вернулась', () => {
    const viewport = installViewport()
    const isHidden = setup()

    focusIn(createInput('text'))
    resize(viewport, { height: SHRUNK_HEIGHT })

    expect(isHidden.value).toBe(true)

    resize(viewport, { height: FULL_HEIGHT })

    expect(isHidden.value).toBe(false)
  })

  it('уход фокуса при сжатом вьюпорте возвращает панель', () => {
    const viewport = installViewport()
    const isHidden = setup()
    const field = createInput('text')

    focusIn(field)
    resize(viewport, { height: SHRUNK_HEIGHT })
    focusOut(field, null)

    expect(isHidden.value).toBe(false)
  })

  it('монтирование при уже поднятой клавиатуре прячет панель сразу', () => {
    installViewport(SHRUNK_HEIGHT)

    const field = createInput('text')

    field.focus()

    const isHidden = setup()

    expect(isHidden.value).toBe(true)
  })

  it.each([2, 3])('зум щипком в %s раза панель не прячет', scale => {
    const viewport = installViewport()
    const isHidden = setup()

    focusIn(createInput('text'))
    resize(viewport, { height: FULL_HEIGHT / scale, scale })

    expect(isHidden.value).toBe(false)
  })

  it('зум поверх клавиатуры признак сохраняет', () => {
    const viewport = installViewport()
    const isHidden = setup()

    focusIn(createInput('text'))
    resize(viewport, { height: SHRUNK_HEIGHT / 2, scale: 2 })

    expect(isHidden.value).toBe(true)
  })

  it('прокрутка на iOS двигает offsetTop и панель не прячет', () => {
    const viewport = installViewport()
    const isHidden = setup()

    focusIn(createInput('text'))
    resize(viewport, { offsetTop: 120 })

    expect(isHidden.value).toBe(false)
  })

  it('подписка снимается вместе с компонентом', () => {
    const viewport = installViewport()
    const isHidden = setup(true)

    focusIn(createInput('text'))
    resize(viewport, { height: SHRUNK_HEIGHT })

    expect(isHidden.value).toBe(false)
  })
})

describe('скрытие под экранной клавиатурой, фолбэк по фокусу без visualViewport', () => {
  it.each([
    'text',
    'search',
    'number',
    'tel',
    'email',
    'url',
    'password',
    'date',
    'datetime-local',
    'month',
    'week',
    'time',
  ])('поле input[type=%s] поднимает клавиатуру и прячет панель', type => {
    const isHidden = setup()

    focusIn(createInput(type))

    expect(isHidden.value).toBe(true)
  })

  it.each([
    'checkbox',
    'radio',
    'button',
    'submit',
    'reset',
    'file',
    'range',
    'color',
  ])('поле input[type=%s] клавиатуру не поднимает и панель оставляет', type => {
    const isHidden = setup()

    focusIn(createInput(type))

    expect(isHidden.value).toBe(false)
  })

  it.each([
    ['textarea', () => createElement('textarea')],
    ['contenteditable', () => {
      const element = createElement('div')

      element.setAttribute('contenteditable', 'true')

      return element
    }],
  ])('%s прячет панель', (_name, create) => {
    const isHidden = setup()

    focusIn(create())

    expect(isHidden.value).toBe(true)
  })

  it.each([
    ['input', () => createInput('text')],
    ['textarea', () => createElement('textarea')],
  ])('%s с readonly клавиатуру не поднимает и панель оставляет', (_name, create) => {
    const isHidden = setup()
    const field = create()

    field.readOnly = true

    focusIn(field)

    expect(isHidden.value).toBe(false)
  })

  it('кнопка панель не прячет', () => {
    const isHidden = setup()

    focusIn(createElement('button'))

    expect(isHidden.value).toBe(false)
  })

  it('уход фокуса с поля возвращает панель', () => {
    const isHidden = setup()
    const field = createInput('text')

    focusIn(field)
    focusOut(field, null)

    expect(isHidden.value).toBe(false)
  })

  it('переход между полями панель не возвращает', () => {
    const isHidden = setup()
    const field = createInput('text')
    const next = createInput('number')

    focusIn(field)
    focusOut(field, next)

    expect(isHidden.value).toBe(true)
  })

  it('подписка снимается вместе с компонентом', () => {
    const isHidden = setup(true)

    focusIn(createInput('text'))

    expect(isHidden.value).toBe(false)
  })

  it('монтирование при уже сфокусированном поле прячет панель сразу', () => {
    createInput('text').focus()

    const isHidden = setup()

    expect(isHidden.value).toBe(true)
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

function installViewport(height = FULL_HEIGHT): ViewportStub {
  const viewport = new ViewportStub()

  viewport.height = height

  Object.defineProperty(window, 'visualViewport', { configurable: true, value: viewport })

  onTestFinished(() => {
    Reflect.deleteProperty(window, 'visualViewport')
  })

  return viewport
}

function resize(viewport: ViewportStub, change: Partial<VisualViewport>): void {
  Object.assign(viewport, change)

  viewport.dispatchEvent(new Event('resize'))
}

function setup(unmountNow = false): Readonly<Ref<boolean>> {
  let isHidden!: Readonly<Ref<boolean>>

  const wrapper = mount(defineComponent({
    setup() {
      isHidden = useKeyboardHide()

      return () => null
    },
  }))

  if (unmountNow) {
    wrapper.unmount()

    return isHidden
  }

  onTestFinished(() => {
    wrapper.unmount()
  })

  return isHidden
}

function createInput(type: string): HTMLInputElement {
  const input = createElement('input')

  input.type = type

  return input
}

function createElement<K extends keyof HTMLElementTagNameMap>(tag: K): HTMLElementTagNameMap[K] {
  const element = document.createElement(tag)

  document.body.append(element)

  onTestFinished(() => {
    element.remove()
  })

  return element
}

function focusIn(target: HTMLElement): void {
  target.dispatchEvent(new FocusEvent('focusin', { bubbles: true }))
}

function focusOut(target: HTMLElement, relatedTarget: HTMLElement | null): void {
  target.dispatchEvent(new FocusEvent('focusout', { bubbles: true, relatedTarget }))
}
