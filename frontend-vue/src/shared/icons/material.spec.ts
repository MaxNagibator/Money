import { describe, expect, it } from 'vitest'
import * as icons from './index'

const SHAPE_PATTERN = /<(?<tag>path|circle|rect|polygon|polyline|ellipse|line|g)\b/g
const FILLED_SHAPE_PATTERN = /<(?:path|circle|rect|polygon|polyline|ellipse)\b(?![^>]*fill="none")[^>]*>/
const ICON_REFERENCE_PATTERN = /\bmd[A-Z]\w*/g

const sources = import.meta.glob<string>('../../**/*.{vue,ts}', {
  query: '?raw',
  import: 'default',
  eager: true,
})

const glyphs = Object.entries(icons) as [string, string][]

describe('набор иконок', () => {
  it('каждый глиф разбирается в фигуры и не пуст', () => {
    const broken = glyphs
      .filter(([, glyph]) => glyph.trim().length === 0
        || glyph.match(SHAPE_PATTERN) === null
        || !FILLED_SHAPE_PATTERN.test(glyph))
      .map(([name]) => name)

    expect(broken).toEqual([])
  })

  it('каждая иконка экранов объявлена непустым фрагментом', () => {
    const declared = new Set(glyphs.filter(([, glyph]) => glyph.trim().length > 0).map(([name]) => name))

    const referenced = new Set(
      Object.entries(sources)
        .filter(([path]) => !path.includes('/icons/'))
        .flatMap(([, source]) => source.match(ICON_REFERENCE_PATTERN) ?? []),
    )

    const missing = [...referenced].filter(name => !declared.has(name)).sort()

    expect(missing).toEqual([])
  })
})
