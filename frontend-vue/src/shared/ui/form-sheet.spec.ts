import { afterEach, describe, expect, it } from 'vitest'
import { markFormSheetClosed, markFormSheetOpen } from './form-sheet'

afterEach(() => {
  document.body.className = ''
})

describe('пометка открытого листа формы', () => {
  it('ставит класс на время открытого листа', () => {
    markFormSheetOpen()

    expect(document.body.classList.contains('form-sheet-open')).toBe(true)

    markFormSheetClosed()

    expect(document.body.classList.contains('form-sheet-open')).toBe(false)
  })

  it('держит класс, пока открыт хотя бы один лист', () => {
    markFormSheetOpen()
    markFormSheetOpen()
    markFormSheetClosed()

    expect(document.body.classList.contains('form-sheet-open')).toBe(true)

    markFormSheetClosed()

    expect(document.body.classList.contains('form-sheet-open')).toBe(false)
  })

  it('не уходит в минус на лишнем закрытии', () => {
    markFormSheetClosed()
    markFormSheetOpen()

    expect(document.body.classList.contains('form-sheet-open')).toBe(true)

    markFormSheetClosed()

    expect(document.body.classList.contains('form-sheet-open')).toBe(false)
  })
})
