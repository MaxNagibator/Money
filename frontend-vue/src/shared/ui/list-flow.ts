interface HostSnapshot {
  hostRect: DOMRect
  borderTop: number
  borderLeft: number
  scrollTop: number
  scrollLeft: number
  rects: Map<Element, DOMRect>
}

const snapshots = new WeakMap<HTMLElement, HostSnapshot>()

function readSnapshot(host: HTMLElement): HostSnapshot {
  const known = snapshots.get(host)

  if (known !== undefined) {
    return known
  }

  const style = getComputedStyle(host)
  const rects = new Map<Element, DOMRect>()

  for (const child of Array.from(host.children)) {
    rects.set(child, child.getBoundingClientRect())
  }

  const snapshot: HostSnapshot = {
    hostRect: host.getBoundingClientRect(),
    borderTop: Number.parseFloat(style.borderTopWidth) || 0,
    borderLeft: Number.parseFloat(style.borderLeftWidth) || 0,
    scrollTop: host.scrollTop,
    scrollLeft: host.scrollLeft,
    rects,
  }

  snapshots.set(host, snapshot)
  requestAnimationFrame(() => snapshots.delete(host))

  return snapshot
}

export function pinLeavingItem(element: Element): void {
  if (!(element instanceof HTMLElement)) {
    return
  }

  const host = element.parentElement

  if (host === null) {
    return
  }

  const snapshot = readSnapshot(host)
  const rect = snapshot.rects.get(element) ?? element.getBoundingClientRect()

  element.style.width = `${rect.width}px`
  element.style.height = `${rect.height}px`
  element.style.top = `${rect.top - snapshot.hostRect.top - snapshot.borderTop + snapshot.scrollTop}px`
  element.style.left = `${rect.left - snapshot.hostRect.left - snapshot.borderLeft + snapshot.scrollLeft}px`
}

export function unpinItem(element: Element): void {
  if (!(element instanceof HTMLElement)) {
    return
  }

  element.style.width = ''
  element.style.height = ''
  element.style.top = ''
  element.style.left = ''
}
