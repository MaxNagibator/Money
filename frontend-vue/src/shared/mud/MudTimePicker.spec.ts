import { describe, expect, it } from 'vitest'
import './MudTimePicker.vue'

interface MudTimePickerInterop {
  initPointerEvents: unknown
  destroyPointerEvents: unknown
}

describe('вендорный интероп часов', () => {
  it('ставится побочным эффектом самого компонента', () => {
    const interop = (window as unknown as { mudTimePicker?: MudTimePickerInterop }).mudTimePicker

    expect(typeof interop?.initPointerEvents).toBe('function')
    expect(typeof interop?.destroyPointerEvents).toBe('function')
  })
})
