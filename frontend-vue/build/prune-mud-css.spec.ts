import { readFileSync } from 'node:fs'
import { join } from 'node:path'
import { describe, expect, it } from 'vitest'
import { isPruned, pruneMudCss } from './prune-mud-css'

const vendorCss = readFileSync(join(import.meta.dirname, '..', 'src', 'shared', 'mud', 'vendor', 'MudBlazor.min.css'), 'utf8')

function countBraces(css: string): { open: number, close: number } {
  return {
    open: (css.match(/\{/g) ?? []).length,
    close: (css.match(/\}/g) ?? []).length,
  }
}

describe('обрезка вендорённого CSS', () => {
  it('убирает правило неиспользуемого компонента', () => {
    expect(pruneMudCss('.mud-chart{color:red}.mud-paper{color:blue}')).toBe('.mud-paper{color:blue}')
  })

  it('оставляет группе только уцелевшие селекторы', () => {
    expect(pruneMudCss('.mud-chart,.mud-paper{color:red}')).toBe('.mud-paper{color:red}')
  })

  it('не трогает компонент с похожим началом имени', () => {
    expect(pruneMudCss('.mud-input-helper-text{color:red}.mud-form-helpertext{color:blue}'))
      .toBe('.mud-input-helper-text{color:red}')
  })

  it('чистит содержимое медиазапроса и снимает опустевший', () => {
    expect(pruneMudCss('@media (min-width:600px){.mud-chart{color:red}.mud-paper{color:blue}}'))
      .toBe('@media (min-width:600px){.mud-paper{color:blue}}')

    expect(pruneMudCss('@media (min-width:600px){.mud-chart{color:red}}')).toBe('')
  })

  it('оставляет правила без классов MudBlazor', () => {
    expect(pruneMudCss('body{margin:0}:root{--mud-palette-primary:red}')).toBe('body{margin:0}:root{--mud-palette-primary:red}')
  })

  it('оставляет at-правила без вложенных селекторов', () => {
    expect(pruneMudCss('@font-face{font-family:Roboto}@charset "utf-8";')).toBe('@font-face{font-family:Roboto}@charset "utf-8";')
  })

  it('не удаляет селектор с отрицанием', () => {
    expect(isPruned('.mud-button:not(.mud-chart)')).toBe(false)
  })

  it('удаляет селектор, которому нужен неиспользуемый предок', () => {
    expect(isPruned('.mud-chart .mud-chart-legend')).toBe(true)
    expect(isPruned('.mud-chart .mud-paper')).toBe(true)
    expect(isPruned('.mud-paper .mud-input-control')).toBe(false)
  })

  it('не принимает скобку внутри строки за границу правила', () => {
    expect(pruneMudCss('.mud-paper::after{content:"}"}.mud-chart{color:red}')).toBe('.mud-paper::after{content:"}"}')
  })

  it('не принимает скобку внутри комментария за границу правила', () => {
    expect(pruneMudCss('/* } */.mud-paper{color:red}')).toBe('/* } */.mud-paper{color:red}')
  })

  it('не режет селектор по запятой внутри строки', () => {
    expect(pruneMudCss('[data-x=","].mud-paper{color:red}')).toBe('[data-x=","].mud-paper{color:red}')
  })
})

describe('обрезка вендорённого CSS на реальном файле', () => {
  const pruned = pruneMudCss(vendorCss)

  it('оставляет скобки сбалансированными и не растёт в размере', () => {
    const before = countBraces(vendorCss)
    const after = countBraces(pruned)

    expect(after.open).toBe(after.close)
    expect(after.open).toBeLessThan(before.open)
    expect(pruned.length).toBeLessThan(vendorCss.length)
  })

  it('сохраняет анимации и шрифты, а медиазапросы только теряет', () => {
    for (const rule of ['@font-face', '@keyframes', '@supports']) {
      expect(pruned.split(rule)).toHaveLength(vendorCss.split(rule).length)
    }

    expect(pruned.split('@media').length).toBeLessThanOrEqual(vendorCss.split('@media').length)
  })

  it('сохраняет правила используемых компонентов', () => {
    for (const name of ['.mud-input-control', '.mud-nav-link', '.mud-table', '.mud-treeview', '.mud-timeline', '.mud-picker']) {
      expect(pruned).toContain(name)
    }
  })

  it('вычищает неиспользуемые компоненты целиком', () => {
    for (const name of ['.mud-carousel', '.mud-drop-zone']) {
      expect(pruned).not.toContain(name)
    }
  })

  it('оставляет неиспользуемому компоненту только правила с отрицанием', () => {
    for (const name of ['.mud-chart', '.mud-data-grid', '.mud-fab-menu']) {
      const survivors = pruned.split(name).length - 1

      expect(survivors).toBeLessThan((vendorCss.split(name).length - 1) / 2)
    }
  })

  it('оставляет используемым компонентам все их правила', () => {
    for (const name of ['.mud-nav-link', '.mud-picker', '.mud-treeview', '.mud-timeline']) {
      expect(pruned.split(name)).toHaveLength(vendorCss.split(name).length)
    }
  })
})
