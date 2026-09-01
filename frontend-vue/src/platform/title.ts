const APP_NAME = 'Филочки'

export function setDocumentTitle(title: string | undefined): void {
  document.title = title ?? APP_NAME
}
