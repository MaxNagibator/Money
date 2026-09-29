import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import { applyUpdate, checkForUpdates, clearOfflineData, estimateStorage, promptInstall } from '@/platform/pwa'
import AppInfoCard from './AppInfoCard.vue'

const { initial, state } = await vi.hoisted(async () => {
  const { reactive } = await import('vue')
  const initial = {
    online: true,
    serverReachable: true,
    offlineSupport: 'ready',
    canCheckUpdates: true,
    needRefresh: false,
    updatedElsewhere: false,
    updateCheck: 'idle',
    install: 'manual',
  }

  return { initial, state: reactive({ ...initial }) }
})

vi.mock('@/platform/pwa', () => ({
  pwaState: state,
  isConnected: () => state.online && state.serverReachable,
  applyUpdate: vi.fn(() => Promise.resolve()),
  checkForUpdates: vi.fn(() => Promise.resolve()),
  clearOfflineData: vi.fn(() => Promise.resolve(true)),
  estimateStorage: vi.fn(() => Promise.resolve({ usage: 1_048_576, quota: 1_073_741_824 })),
  promptInstall: vi.fn(() => Promise.resolve()),
}))

beforeEach(() => {
  vi.clearAllMocks()
  Object.assign(state, initial)
})

async function setup(): Promise<ReturnType<typeof mount>> {
  const wrapper = mount(AppInfoCard, { props: { version: '1.3.17' } })

  await flushPromises()

  return wrapper
}

function text(wrapper: ReturnType<typeof mount>, testId: string): string {
  return wrapper.get(`[data-testid="${testId}"]`).text()
}

describe('карточка «Приложение»', () => {
  it('показывает версию, сеть, готовность к офлайну и занятое место', async () => {
    const wrapper = await setup()

    expect(text(wrapper, 'settings-app-version')).toBe('Версия 1.3.17')
    expect(text(wrapper, 'settings-app-network')).toBe('Связь есть')
    expect(text(wrapper, 'settings-app-offline')).toContain('откроется и без сети')
    expect(text(wrapper, 'settings-app-storage')).toMatch(/^Занято 1 МБ из доступных 1 ГБ$/u)
  })

  it('кнопка установки есть только там, где браузер её предлагает', async () => {
    const wrapper = await setup()

    expect(wrapper.find('[data-testid="settings-app-install-button"]').exists()).toBe(false)

    state.install = 'available'
    await nextTick()
    await wrapper.get('[data-testid="settings-app-install-button"]').trigger('click')

    expect(vi.mocked(promptInstall)).toHaveBeenCalledTimes(1)
  })

  it('новая версия предлагается кнопкой и не ставится сама', async () => {
    const wrapper = await setup()

    expect(wrapper.find('[data-testid="settings-app-apply-update"]').exists()).toBe(false)

    state.needRefresh = true
    await nextTick()

    expect(text(wrapper, 'settings-app-update-status')).toBe('Доступна новая версия')
    expect(vi.mocked(applyUpdate)).not.toHaveBeenCalled()

    await wrapper.get('[data-testid="settings-app-apply-update"]').trigger('click')

    expect(vi.mocked(applyUpdate)).toHaveBeenCalledTimes(1)
  })

  it('без зарегистрированного воркера проверять обновления нечем', async () => {
    state.canCheckUpdates = false
    state.offlineSupport = 'unsupported'

    const wrapper = await setup()

    expect(wrapper.get('[data-testid="settings-app-check-update"]').attributes('disabled')).toBeDefined()
    expect(text(wrapper, 'settings-app-offline')).toBe('Этот браузер не умеет открывать приложение без сети')

    await wrapper.get('[data-testid="settings-app-check-update"]').trigger('click')

    expect(vi.mocked(checkForUpdates)).not.toHaveBeenCalled()
  })

  it('очистка кэша сообщает результат и пересчитывает занятое место', async () => {
    const wrapper = await setup()

    vi.mocked(estimateStorage).mockResolvedValue({ usage: 0, quota: 1_073_741_824 })
    await wrapper.get('[data-testid="settings-app-clear-cache"]').trigger('click')
    await flushPromises()

    expect(vi.mocked(clearOfflineData)).toHaveBeenCalledTimes(1)
    expect(text(wrapper, 'settings-app-clear-result')).toContain('удалены')
    expect(text(wrapper, 'settings-app-storage')).toMatch(/^Занято 0 Б/u)
  })

  it('неудачная очистка не выдаётся за успех', async () => {
    vi.mocked(clearOfflineData).mockResolvedValue(false)

    const wrapper = await setup()

    await wrapper.get('[data-testid="settings-app-clear-cache"]').trigger('click')
    await flushPromises()

    expect(text(wrapper, 'settings-app-clear-result')).toBe('Не удалось удалить сохранённые данные. Попробуйте позже')
  })
})
