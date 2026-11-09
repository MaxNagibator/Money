export interface VirtualWindow {
  start: number
  end: number
  padStart: number
  padEnd: number
}

export function buildOffsets(sizes: readonly number[]): number[] {
  const offsets = new Array<number>(sizes.length + 1)

  offsets[0] = 0

  for (const [index, size] of sizes.entries()) {
    offsets[index + 1] = (offsets[index] ?? 0) + size
  }

  return offsets
}

export function resolveVirtualWindow(
  offsets: readonly number[],
  top: number,
  bottom: number,
  overscan: number,
): VirtualWindow {
  const count = offsets.length - 1

  if (count <= 0) {
    return { start: 0, end: 0, padStart: 0, padEnd: 0 }
  }

  const first = findFirstVisible(offsets, count, top)
  const last = findLastVisible(offsets, count, bottom, first)

  const start = Math.max(0, first - overscan)
  const end = Math.min(count, last + 1 + overscan)

  return {
    start,
    end,
    padStart: offsets[start] ?? 0,
    padEnd: (offsets[count] ?? 0) - (offsets[end] ?? 0),
  }
}

function findFirstVisible(offsets: readonly number[], count: number, top: number): number {
  let low = 0
  let high = count - 1

  while (low < high) {
    const middle = Math.ceil((low + high) / 2)

    if ((offsets[middle] ?? 0) <= top) {
      low = middle
      continue
    }

    high = middle - 1
  }

  return low
}

function findLastVisible(offsets: readonly number[], count: number, bottom: number, first: number): number {
  let low = first
  let high = count - 1

  while (low < high) {
    const middle = Math.floor((low + high) / 2)

    if ((offsets[middle + 1] ?? 0) >= bottom) {
      high = middle
      continue
    }

    low = middle + 1
  }

  return low
}
