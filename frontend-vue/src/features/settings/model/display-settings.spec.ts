import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { AlternateRowStyle, DisplaySettings } from './display-settings'
import {
  alternateRowClass,
  clampMoneyColumnWidth,
  createDisplaySettings,
  displaySettingsDefaults,
  moneyColumnStyle,
  operationsListClasses,
  shouldShowDivider,
} from './display-settings'

const { store, writes } = vi.hoisted(() => ({
  store: new Map<string, unknown>(),
  writes: { count: 0 },
}))

vi.mock('@/platform/storage', () => ({
  readSetting: <T>(key: string, fallback: T): T => (store.has(key) ? store.get(key) as T : fallback),
  writeSetting: (key: string, value: unknown): void => {
    writes.count += 1
    store.set(key, value)
  },
  removeSetting: (key: string): void => {
    store.delete(key)
  },
}))

const KEY = 'settings:display'

function settings(patch: Partial<DisplaySettings> = {}): DisplaySettings {
  return { ...displaySettingsDefaults(), ...patch }
}

beforeEach(() => {
  store.clear()
  writes.count = 0
})

describe('разделители между операциями', () => {
  it('по умолчанию выключены', () => {
    expect(createDisplaySettings().settings.showDividers).toBe(false)
  })

  it.each<[boolean, number, number, boolean]>([
    [true, 0, 3, true],
    [true, 1, 3, true],
    [true, 2, 3, false],
    [true, 0, 1, false],
    [false, 0, 3, false],
  ])('показ %s для строки %i из %i', (showDividers, index, count, expected) => {
    expect(shouldShowDivider(settings({ showDividers }), index, count)).toBe(expected)
  })
})

describe('чередование строк операций', () => {
  it('по умолчанию выключено', () => {
    expect(createDisplaySettings().settings.alternateRowStyle).toBe('none')
  })

  it.each<[AlternateRowStyle, number, string | null]>([
    ['none', 0, null],
    ['none', 1, null],
    ['background', 0, null],
    ['background', 1, 'operation-row-alt'],
    ['border', 1, 'operation-row-alt'],
    ['border', 2, null],
  ])('стиль %s на строке %i', (style, index, expected) => {
    expect(alternateRowClass(style, index)).toBe(expected)
  })

  it.each<[AlternateRowStyle, string[]]>([
    ['none', ['operations-money-align']],
    ['background', ['operations-alt-bg', 'operations-money-align']],
    ['border', ['operations-alt-border', 'operations-money-align']],
  ])('стиль %s задаёт классы списка', (alternateRowStyle, expected) => {
    expect(operationsListClasses(settings({ alternateRowStyle }))).toEqual(expected)
  })
})

describe('выравнивание копеек', () => {
  it('включено по умолчанию и задаёт ширину колонки в ch', () => {
    const { settings: value } = createDisplaySettings()

    expect(value.alignMoneyDecimals).toBe(true)
    expect(moneyColumnStyle(value)).toEqual({ '--money-col-width': '9ch' })
  })

  it('выключенное не задаёт ни стиля, ни класса', () => {
    const value = settings({ alignMoneyDecimals: false })

    expect(moneyColumnStyle(value)).toBeNull()
    expect(operationsListClasses(value)).toEqual([])
  })
})

describe('ширина колонки суммы', () => {
  it('по умолчанию равна девяти символам', () => {
    expect(createDisplaySettings().settings.moneyColumnWidth).toBe(9)
  })

  it.each<[number, number]>([
    [3, 4],
    [4, 4],
    [7, 7],
    [14, 14],
    [15, 14],
    [Number.NaN, 9],
  ])('значение %s зажимается до %i', (value, expected) => {
    expect(clampMoneyColumnWidth(value)).toBe(expected)
  })

  it('зажимает значение, пришедшее из хранилища мимо ползунка', () => {
    store.set(KEY, { ...displaySettingsDefaults(), moneyColumnWidth: 99 })

    expect(createDisplaySettings().settings.moneyColumnWidth).toBe(14)
  })
})

describe('цвета темы на графиках', () => {
  it('включены по умолчанию', () => {
    expect(createDisplaySettings().settings.useChartThemeColors).toBe(true)
  })

  it('сохранённое выключение переживает перезагрузку', () => {
    createDisplaySettings().update({ useChartThemeColors: false })

    expect(createDisplaySettings().settings.useChartThemeColors).toBe(false)
  })
})

describe('сохранение настроек отображения', () => {
  it('пишет изменение сразу, без кнопки', () => {
    const state = createDisplaySettings()

    state.update({ showDividers: true, moneyColumnWidth: 12 })

    expect(store.get(KEY)).toMatchObject({ showDividers: true, moneyColumnWidth: 12 })
  })

  it('не пишет равное значение', () => {
    const state = createDisplaySettings()

    state.update({ showDividers: true })
    const after = writes.count

    state.update({ showDividers: true, alignMoneyDecimals: true, moneyColumnWidth: 9 })

    expect(writes.count).toBe(after)
  })

  it('отбрасывает испорченные значения из хранилища', () => {
    store.set(KEY, { showDividers: 'да', alternateRowStyle: 'полоска', moneyColumnWidth: 'широко' })

    expect(createDisplaySettings().settings).toEqual(displaySettingsDefaults())
  })
})
