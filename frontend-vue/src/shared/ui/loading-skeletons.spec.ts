import { describe, expect, it } from 'vitest'
import {
  MAX_SKELETON_COUNT,
  MAX_SKELETON_ROWS,
  MIN_SKELETON_COUNT,
  resolveGridSkeletonCount,
  resolveSkeletonCount,
} from './loading-skeletons'

describe('resolveSkeletonCount', () => {
  it.each([
    [900, 5],
    [1080, 5],
    [660, 3],
  ])('покрывает видимую область при высоте %i', (viewportHeight, expected) => {
    expect(resolveSkeletonCount(viewportHeight, 220)).toBe(expected)
  })

  it.each([
    [320],
    [0],
    [Number.NaN],
  ])('не опускается ниже минимума при высоте %s', viewportHeight => {
    expect(resolveSkeletonCount(viewportHeight, 220)).toBe(MIN_SKELETON_COUNT)
  })

  it('не поднимается выше потолка на очень высоком окне', () => {
    expect(resolveSkeletonCount(4000, 220)).toBe(MAX_SKELETON_COUNT)
  })
})

describe('resolveGridSkeletonCount', () => {
  it.each([
    [599, 923, 2],
    [960, 923, 6],
    [1280, 923, 8],
    [1920, 923, 8],
  ])('заполняет сетку по ширине %i и высоте %i', (viewportWidth, viewportHeight, expected) => {
    expect(resolveGridSkeletonCount(viewportWidth, viewportHeight, 212)).toBe(expected)
  })

  it.each([
    [0, 0],
    [Number.NaN, Number.NaN],
  ])('держит потолок строк при вырожденных размерах %s и %s', (viewportWidth, viewportHeight) => {
    expect(resolveGridSkeletonCount(viewportWidth, viewportHeight, 212)).toBe(MAX_SKELETON_ROWS)
  })

  it('не выходит за окно на узком низком экране', () => {
    expect(resolveGridSkeletonCount(900, 400, 212)).toBe(2)
  })

  it('держит потолок в две строки на очень высоком окне', () => {
    expect(resolveGridSkeletonCount(1280, 4000, 212)).toBe(MAX_SKELETON_COUNT)
  })

  it('оставляет одну строку, когда вторая не попадает в окно', () => {
    expect(resolveGridSkeletonCount(1280, 200, 212)).toBe(4)
  })
})
