const SHEET_CLASS = 'form-sheet-open'

let openSheets = 0

export function markFormSheetOpen(): void {
  openSheets += 1
  document.body.classList.add(SHEET_CLASS)
}

export function markFormSheetClosed(): void {
  if (openSheets === 0) {
    return
  }

  openSheets -= 1

  if (openSheets === 0) {
    document.body.classList.remove(SHEET_CLASS)
  }
}
