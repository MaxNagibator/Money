import { afterAll } from 'vitest'

type TimerId = ReturnType<typeof globalThis.setTimeout>

const nativeSetTimeout = globalThis.setTimeout
const nativeClearTimeout = globalThis.clearTimeout
const nativeSetInterval = globalThis.setInterval
const nativeClearInterval = globalThis.clearInterval

const pendingTimeouts = new Set<TimerId>()
const pendingIntervals = new Set<TimerId>()

function trackedSetTimeout(
  handler: TimerHandler,
  timeout?: number,
  ...args: unknown[]
): TimerId {
  if (typeof handler !== 'function') {
    const id = nativeSetTimeout(handler, timeout)
    pendingTimeouts.add(id)
    return id
  }

  const callback = handler as (...callbackArgs: unknown[]) => void
  const id = nativeSetTimeout((...callbackArgs: unknown[]) => {
    pendingTimeouts.delete(id)
    callback(...callbackArgs)
  }, timeout, ...args)
  pendingTimeouts.add(id)
  return id
}

function trackedClearTimeout(id?: TimerId): void {
  if (id !== undefined) {
    pendingTimeouts.delete(id)
  }
  nativeClearTimeout(id)
}

function trackedSetInterval(
  handler: TimerHandler,
  timeout?: number,
  ...args: unknown[]
): TimerId {
  const id = typeof handler === 'function'
    ? nativeSetInterval(handler, timeout, ...args)
    : nativeSetInterval(handler, timeout)
  pendingIntervals.add(id)
  return id
}

function trackedClearInterval(id?: TimerId): void {
  if (id !== undefined) {
    pendingIntervals.delete(id)
  }
  nativeClearInterval(id)
}

globalThis.setTimeout = trackedSetTimeout
globalThis.clearTimeout = trackedClearTimeout
globalThis.setInterval = trackedSetInterval
globalThis.clearInterval = trackedClearInterval

export function pendingTimerCount(): number {
  return pendingTimeouts.size + pendingIntervals.size
}

export function clearPendingTimers(): void {
  for (const id of pendingTimeouts) {
    nativeClearTimeout(id)
  }
  pendingTimeouts.clear()

  for (const id of pendingIntervals) {
    nativeClearInterval(id)
  }
  pendingIntervals.clear()
}

afterAll(clearPendingTimers)
