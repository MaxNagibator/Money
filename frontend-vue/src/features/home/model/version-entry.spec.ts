import { describe, expect, it } from 'vitest'
import type { LocalDate } from '@/domain/local-date'
import {
  mdAdd,
  mdBugReport,
  mdCircle,
  mdFiberManualRecord,
  mdPalette,
  mdRadioButtonChecked,
  mdSettings,
  mdTrendingUp,
} from '@/shared/icons'
import type { ChangeType, VersionHistoryEntry } from './version-entry'
import { changeAppearance, CHANGE_TYPES, currentVersion, versionChipColor, versionMarker } from './version-entry'

describe('оформление записи истории', () => {
  it.each<[ChangeType, string | null, string]>([
    ['feature', 'success', mdAdd],
    ['bugFix', 'error', mdBugReport],
    ['improvement', 'info', mdTrendingUp],
    ['uiUx', 'warning', mdPalette],
    ['general', null, mdSettings],
  ])('даёт типу правки %s цвет %s и свою иконку', (type, color, icon) => {
    expect(changeAppearance(type)).toEqual({ color, icon })
  })

  it('покрывает все типы правки и не повторяет иконку', () => {
    expect(CHANGE_TYPES).toHaveLength(5)
    expect(new Set(CHANGE_TYPES.map(type => changeAppearance(type).icon)).size).toBe(CHANGE_TYPES.length)
  })

  it('красит чип версии: релизная зелёным, ранняя синим, нулевой минор обычным', () => {
    expect(versionChipColor('1.3.17')).toBe('success')
    expect(versionChipColor('1.2.1.7')).toBe('success')
    expect(versionChipColor('0.8.2')).toBe('primary')
    expect(versionChipColor('0.1.7')).toBe('primary')
    expect(versionChipColor('0.0.1')).toBeNull()
    expect(versionChipColor('1')).toBeNull()
  })

  it.each<[number, string | null, string]>([
    [0, null, mdCircle],
    [1, 'info', mdFiberManualRecord],
    [2, 'primary', mdRadioButtonChecked],
    [3, 'primary', mdRadioButtonChecked],
    [4, 'secondary', mdRadioButtonChecked],
    [10, 'secondary', mdRadioButtonChecked],
  ])('метит версию с %i изменениями цветом %s', (count, color, icon) => {
    expect(versionMarker(entry(count))).toEqual({ color, icon })
  })

  it('берёт текущую версию из первой записи, а без истории отдаёт unknown', () => {
    expect(currentVersion([entry(1, '2.0.0'), entry(1, '1.0.0')])).toBe('2.0.0')
    expect(currentVersion([])).toBe('unknown')
  })
})

function entry(changeCount: number, version = '1.0.0'): VersionHistoryEntry {
  return {
    version,
    date: '2026-05-04' as LocalDate,
    changes: Array.from({ length: changeCount }, () => ({ description: 'правка', type: 'feature' })),
  }
}
