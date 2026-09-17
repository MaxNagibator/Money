import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { pinLeavingItem, unpinItem } from './list-flow'

interface Box {
  top: number
  left: number
  width: number
  height: number
}

let frames: FrameRequestCallback[] = []
let computedStyle: ReturnType<typeof vi.fn>

function setBox(element: Element, box: Box): void {
  element.getBoundingClientRect = () => ({
    ...box,
    x: box.left,
    y: box.top,
    right: box.left + box.width,
    bottom: box.top + box.height,
    toJSON: () => box,
  })
}

function createHost(border: number, scroll: number): HTMLElement {
  const host = document.createElement('div')

  setBox(host, { top: 100, left: 20, width: 600, height: 400 })
  Object.defineProperty(host, 'scrollTop', { value: scroll, configurable: true })
  Object.defineProperty(host, 'scrollLeft', { value: scroll, configurable: true })
  computedStyle.mockReturnValue({ borderTopWidth: `${border}px`, borderLeftWidth: `${border}px` })
  document.body.append(host)

  return host
}

function addItem(host: HTMLElement, box: Box): HTMLElement {
  const item = document.createElement('div')

  setBox(item, box)
  host.append(item)

  return item
}

function runFrames(): void {
  const pending = frames

  frames = []

  for (const frame of pending) {
    frame(0)
  }
}

beforeEach(() => {
  frames = []
  computedStyle = vi.fn(() => ({ borderTopWidth: '0px', borderLeftWidth: '0px' }))
  vi.stubGlobal('getComputedStyle', computedStyle)
  vi.stubGlobal('requestAnimationFrame', (callback: FrameRequestCallback) => frames.push(callback))
})

afterEach(() => {
  vi.unstubAllGlobals()
  document.body.innerHTML = ''
})

describe('pinLeavingItem', () => {
  it.each([
    { name: 'без бордюра и прокрутки', border: 0, scroll: 0, top: '80px', left: '10px' },
    { name: 'с бордюром хоста', border: 2, scroll: 0, top: '78px', left: '8px' },
    { name: 'с прокруткой хоста', border: 0, scroll: 30, top: '110px', left: '40px' },
    { name: 'с бордюром и прокруткой', border: 3, scroll: 30, top: '107px', left: '37px' },
  ])('закрепляет элемент относительно хоста: $name', ({ border, scroll, top, left }) => {
    const host = createHost(border, scroll)
    const item = addItem(host, { top: 180, left: 30, width: 200, height: 40 })

    pinLeavingItem(item)

    expect(item.style.width).toBe('200px')
    expect(item.style.height).toBe('40px')
    expect(item.style.top).toBe(top)
    expect(item.style.left).toBe(left)
  })

  it('при уходе нескольких элементов берёт их исходные места', () => {
    const host = createHost(0, 0)
    const first = addItem(host, { top: 180, left: 20, width: 200, height: 40 })
    const second = addItem(host, { top: 230, left: 20, width: 200, height: 40 })
    const third = addItem(host, { top: 280, left: 20, width: 200, height: 40 })

    pinLeavingItem(first)

    setBox(second, { top: 180, left: 20, width: 200, height: 40 })
    setBox(third, { top: 230, left: 20, width: 200, height: 40 })

    pinLeavingItem(second)
    pinLeavingItem(third)

    expect(second.style.top).toBe('130px')
    expect(third.style.top).toBe('180px')
  })

  it('к следующему кадру снимает геометрию заново', () => {
    const host = createHost(0, 0)
    const item = addItem(host, { top: 180, left: 20, width: 200, height: 40 })

    pinLeavingItem(item)
    runFrames()

    setBox(item, { top: 230, left: 20, width: 200, height: 40 })
    pinLeavingItem(item)

    expect(item.style.top).toBe('130px')
  })

  it('не трогает элемент без родителя', () => {
    const item = document.createElement('div')

    setBox(item, { top: 180, left: 20, width: 200, height: 40 })
    pinLeavingItem(item)

    expect(item.style.top).toBe('')
    expect(computedStyle).not.toHaveBeenCalled()
  })
})

describe('unpinItem', () => {
  it('снимает инлайновые стили закрепления', () => {
    const host = createHost(0, 0)
    const item = addItem(host, { top: 180, left: 30, width: 200, height: 40 })

    item.style.opacity = '0.5'
    pinLeavingItem(item)
    unpinItem(item)

    expect(item.style.width).toBe('')
    expect(item.style.height).toBe('')
    expect(item.style.top).toBe('')
    expect(item.style.left).toBe('')
    expect(item.style.opacity).toBe('0.5')
  })
})

describe('узел без раскладки', () => {
  it.each([
    { name: 'pinLeavingItem', run: pinLeavingItem },
    { name: 'unpinItem', run: unpinItem },
  ])('$name выходит на узле, который не HTMLElement', ({ run }) => {
    const host = createHost(0, 0)
    const node = document.createElementNS('http://www.w3.org/2000/svg', 'svg')

    host.append(node)

    expect(() => run(node)).not.toThrow()
    expect(computedStyle).not.toHaveBeenCalled()
    expect(node.getAttribute('style')).toBeNull()
  })
})
