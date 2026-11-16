import { describe, expect, it, vi } from 'vitest'
import { clearPendingTimers, pendingTimerCount } from './timer-guard.setup'

describe('защита от утечки таймеров после закрытия happy-dom', () => {
  it('отменяет незавершённые setTimeout и setInterval при завершении файла', async () => {
    const onTimeout = vi.fn()
    const onInterval = vi.fn()
    const before = pendingTimerCount()

    setTimeout(onTimeout, 10)
    setInterval(onInterval, 10)
    expect(pendingTimerCount()).toBe(before + 2)

    clearPendingTimers()
    expect(pendingTimerCount()).toBe(0)

    await new Promise(resolve => {
      setTimeout(resolve, 30)
    })

    expect(onTimeout).not.toHaveBeenCalled()
    expect(onInterval).not.toHaveBeenCalled()
  })

  it('удаляет завершённый и явно отменённый таймер без влияния на штатную работу', async () => {
    const completed = vi.fn()
    const cancelled = vi.fn()
    const interval = vi.fn()
    const before = pendingTimerCount()

    setTimeout(completed, 5)
    const cancelledId = setTimeout(cancelled, 5)
    const intervalId = setInterval(interval, 50)
    expect(pendingTimerCount()).toBe(before + 3)

    clearTimeout(cancelledId)
    clearInterval(intervalId)
    expect(pendingTimerCount()).toBe(before + 1)

    await new Promise(resolve => {
      setTimeout(resolve, 20)
    })

    expect(completed).toHaveBeenCalledTimes(1)
    expect(cancelled).not.toHaveBeenCalled()
    expect(interval).not.toHaveBeenCalled()
    expect(pendingTimerCount()).toBe(before)
  })
})
