import { readFileSync } from 'node:fs'
import { join } from 'node:path'
import { describe, expect, it } from 'vitest'
import { collectUtilityClasses, isPruned, pruneMudCss } from './prune-mud-css'

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

  it('не считает класс внутри отрицания за использование', () => {
    expect(isPruned('.mud-button:not(.mud-chart)')).toBe(false)
    expect(isPruned('.mud-chart-legend:not(.mud-paper)')).toBe(true)
    expect(isPruned('.mud-button-group-horizontal:not(.mud-button-group-rtl)>.mud-button-root:not(:last-child)')).toBe(true)
  })

  it('удаляет утилиту отступа, которой нет в исходниках', () => {
    const used = new Set(['ma-2', 'gap-md-6', 'me-n1'])

    expect(pruneMudCss('.ma-2{margin:8px}.ma-3{margin:12px}', used)).toBe('.ma-2{margin:8px}')
    expect(pruneMudCss('.gap-md-6{gap:24px}.gap-md-8{gap:32px}', used)).toBe('.gap-md-6{gap:24px}')
    expect(pruneMudCss('.me-n1{margin-inline-end:-4px}.me-n2{margin-inline-end:-8px}', used)).toBe('.me-n1{margin-inline-end:-4px}')
  })

  it('оставляет утилиту, попавшую в составной селектор', () => {
    expect(pruneMudCss('.mud-paper .ma-3{margin:12px}', new Set(['ma-2']))).toBe('.mud-paper .ma-3{margin:12px}')
  })

  it('не трогает утилиты без списка использованных', () => {
    expect(pruneMudCss('.ma-3{margin:12px}')).toBe('.ma-3{margin:12px}')
  })

  it('собирает утилиты из исходников по литералам', () => {
    const used = collectUtilityClasses(['<div class="ma-2 gap-md-6 me-n1">', 'const x = "pa-4"'])

    expect([...used].sort()).toEqual(['gap-md-6', 'ma-2', 'me-n1', 'pa-4'])
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

  it('вычищает неиспользуемые компоненты целиком, включая правила с отрицанием', () => {
    for (const name of ['.mud-carousel', '.mud-drop-zone', '.mud-chart', '.mud-data-grid', '.mud-fab-menu', '.mud-button-group']) {
      expect(pruned).not.toContain(name)
    }
  })

  it('оставляет используемым компонентам все их правила', () => {
    for (const name of ['.mud-nav-link', '.mud-picker', '.mud-treeview', '.mud-timeline']) {
      expect(pruned.split(name)).toHaveLength(vendorCss.split(name).length)
    }
  })
})
