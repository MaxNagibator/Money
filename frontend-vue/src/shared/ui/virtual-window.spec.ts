import { describe, expect, it } from 'vitest'
import { buildOffsets, resolveVirtualWindow } from './virtual-window'

const uniform = buildOffsets(Array.from({ length: 100 }, () => 100))

describe('buildOffsets', () => {
  it('копит высоты нарастающим итогом', () => {
    expect(buildOffsets([10, 20, 30])).toStrictEqual([0, 10, 30, 60])
  })

  it('на пустом списке отдаёт одну границу', () => {
    expect(buildOffsets([])).toStrictEqual([0])
  })
})

describe('resolveVirtualWindow', () => {
  it('на пустом списке не рисует ничего', () => {
    expect(resolveVirtualWindow([0], 0, 800, 4)).toStrictEqual({ start: 0, end: 0, padStart: 0, padEnd: 0 })
  })

  it.each([
    [0, 800, 0, 12],
    [1000, 1800, 6, 22],
    [9200, 10_000, 88, 100],
  ])('накрывает видимое окно с запасом: %i .. %i', (top, bottom, start, end) => {
    const frame = resolveVirtualWindow(uniform, top, bottom, 4)

    expect(frame.start).toBe(start)
    expect(frame.end).toBe(end)
  })

  it('держит суммарную высоту неизменной', () => {
    const frame = resolveVirtualWindow(uniform, 3000, 3800, 4)
    const rendered = (uniform[frame.end] ?? 0) - (uniform[frame.start] ?? 0)

    expect(frame.padStart + rendered + frame.padEnd).toBe(10_000)
  })

  it('на разной высоте элементов берёт границы по фактическим смещениям', () => {
    const offsets = buildOffsets([500, 40, 40, 40, 500])

    expect(resolveVirtualWindow(offsets, 520, 600, 0)).toStrictEqual({
      start: 1,
      end: 4,
      padStart: 500,
      padEnd: 500,
    })
  })

  it('список выше окна оставляет отрисованным хвост', () => {
    const frame = resolveVirtualWindow(uniform, 20_000, 20_800, 0)

    expect(frame).toStrictEqual({ start: 99, end: 100, padStart: 9900, padEnd: 0 })
  })

  it('список ниже окна оставляет отрисованной голову', () => {
    const frame = resolveVirtualWindow(uniform, -5000, -4200, 0)

    expect(frame).toStrictEqual({ start: 0, end: 1, padStart: 0, padEnd: 9900 })
  })
})
