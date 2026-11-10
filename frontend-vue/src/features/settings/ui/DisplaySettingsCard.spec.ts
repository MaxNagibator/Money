import { mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it } from 'vitest'
import { displaySettingsDefaults, useDisplaySettings } from '../model/display-settings'
import DisplaySettingsCard from './DisplaySettingsCard.vue'

beforeEach(() => {
  useDisplaySettings().update(displaySettingsDefaults())
})

describe('карточка «Настройки отображения»', () => {
  it('переключатель фиксации нижней панели обновляет настройку', async () => {
    const wrapper = mount(DisplaySettingsCard)
    const { settings } = useDisplaySettings()
    const toggle = wrapper.get('[data-testid="settings-pin-bottom-nav"] [role="switch"]')

    expect(settings.pinBottomNav).toBe(false)
    expect(toggle.attributes('aria-checked')).toBe('false')

    await toggle.setValue(true)

    expect(settings.pinBottomNav).toBe(true)
    expect(toggle.attributes('aria-checked')).toBe('true')
  })
})
