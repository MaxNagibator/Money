import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { applyUpdate, reloadApp } from '@/platform/pwa'
import AppStatusBanner from './AppStatusBanner.vue'

const { state } = await vi.hoisted(async () => {
  const { reactive } = await import('vue')

  return {
    state: reactive({ online: true, serverReachable: true, needRefresh: false, updatedElsewhere: false }),
  }
})

vi.mock('@/platform/pwa', () => ({
  pwaState: state,
  isConnected: () => state.online && state.serverReachable,
  applyUpdate: vi.fn(() => Promise.resolve()),
  reloadApp: vi.fn(),
}))

beforeEach(() => {
  vi.clearAllMocks()
  Object.assign(state, { online: true, serverReachable: true, needRefresh: false, updatedElsewhere: false })
})

describe('плашка состояния приложения', () => {
  it('при живой сети и без обновлений ничего не показывает', () => {
    const wrapper = mount(AppStatusBanner)

    expect(wrapper.text()).toBe('')
  })

  it.each([
    ['браузер знает, что сети нет', { online: false }],
    ['браузер считает сеть живой, а ответ пришёл из сохранённого', { serverReachable: false }],
  ])('%s: предупреждает, что данные могут быть устаревшими и запись не уйдёт', (_case, change) => {
    Object.assign(state, change)

    const wrapper = mount(AppStatusBanner)
    const banner = wrapper.get('[data-testid="app-offline-banner"]').text()

    expect(banner).toContain('Нет связи')
    expect(banner).toContain('могут быть устаревшими')
    expect(banner).toContain('Изменения не отправятся')
  })

  it('новую версию ставит только по нажатию', async () => {
    state.needRefresh = true

    const wrapper = mount(AppStatusBanner)

    expect(vi.mocked(applyUpdate)).not.toHaveBeenCalled()

    await wrapper.get('[data-testid="app-update-apply"]').trigger('click')

    expect(vi.mocked(applyUpdate)).toHaveBeenCalledTimes(1)
    expect(vi.mocked(reloadApp)).not.toHaveBeenCalled()
  })

  it('после обновления в другой вкладке предлагает перезагрузку, а не перезагружает сама', async () => {
    state.updatedElsewhere = true

    const wrapper = mount(AppStatusBanner)

    expect(vi.mocked(reloadApp)).not.toHaveBeenCalled()
    expect(wrapper.get('[data-testid="app-update-apply"]').text()).toBe('Перезагрузить')

    await wrapper.get('[data-testid="app-update-apply"]').trigger('click')

    expect(vi.mocked(reloadApp)).toHaveBeenCalledTimes(1)
    expect(vi.mocked(applyUpdate)).not.toHaveBeenCalled()
  })
})
