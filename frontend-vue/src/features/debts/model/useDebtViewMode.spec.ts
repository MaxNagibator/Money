import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { effectScope, nextTick } from 'vue'
import { readSetting, tryWriteSetting } from '@/platform/storage'
import { useDebtViewMode } from './useDebtViewMode'

vi.mock('@/platform/storage', () => ({ readSetting: vi.fn(), tryWriteSetting: vi.fn() }))
const scopes: ReturnType<typeof effectScope>[] = []

beforeEach(() => vi.resetAllMocks())
afterEach(() => scopes.splice(0).forEach(scope => scope.stop()))

function setup() {
  const scope = effectScope()
  scopes.push(scope)
  return scope.run(() => useDebtViewMode())!
}

describe('режим представления долгов', () => {
  it('восстанавливает бухгалтерский режим и сохраняет смену выбора', async () => {
    vi.mocked(readSetting).mockReturnValue('accounting')
    const mode = setup()
    expect(mode.value).toBe('accounting')
    mode.value = 'normal'
    await nextTick()
    expect(tryWriteSetting).toHaveBeenCalledWith('debts:view-mode', 'normal')
  })

  it('восстанавливает режимы rework и refined и сохраняет выбор', async () => {
    vi.mocked(readSetting).mockReturnValue('rework')
    const mode = setup()
    expect(mode.value).toBe('rework')
    mode.value = 'refined'
    await nextTick()
    expect(tryWriteSetting).toHaveBeenCalledWith('debts:view-mode', 'refined')
  })

  it('не принимает повреждённую настройку и работает без доступа к хранилищу', async () => {
    vi.mocked(readSetting).mockReturnValue({ mode: 'unknown' })
    expect(setup().value).toBe('normal')
    vi.mocked(readSetting).mockImplementationOnce(() => { throw new Error('Storage blocked') })
    const mode = setup()
    mode.value = 'accounting'
    await nextTick()
    expect(mode.value).toBe('accounting')
  })
})
