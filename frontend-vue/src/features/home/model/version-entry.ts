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

export type ChangeType = 'feature' | 'bugFix' | 'improvement' | 'uiUx' | 'general'

export type ThemeColor = 'primary' | 'secondary' | 'tertiary' | 'success' | 'error' | 'warning' | 'info'

export interface VersionChange {
  description: string
  type: ChangeType
}

export interface VersionHistoryEntry {
  version: string
  date: LocalDate
  changes: readonly VersionChange[]
}

export interface Appearance {
  color: ThemeColor | null
  icon: string
}

export const CHANGE_TYPES: readonly ChangeType[] = ['feature', 'bugFix', 'improvement', 'uiUx', 'general']

const CHANGE_TYPE_APPEARANCE: Readonly<Record<ChangeType, Appearance>> = {
  feature: { color: 'success', icon: mdAdd },
  bugFix: { color: 'error', icon: mdBugReport },
  improvement: { color: 'info', icon: mdTrendingUp },
  uiUx: { color: 'warning', icon: mdPalette },
  general: { color: null, icon: mdSettings },
}

export function changeAppearance(type: ChangeType): Appearance {
  return CHANGE_TYPE_APPEARANCE[type]
}

export function versionMarker(entry: VersionHistoryEntry): Appearance {
  const count = entry.changes.length

  if (count === 0) {
    return { color: null, icon: mdCircle }
  }

  if (count === 1) {
    return { color: 'info', icon: mdFiberManualRecord }
  }

  return { color: count <= 3 ? 'primary' : 'secondary', icon: mdRadioButtonChecked }
}

export function versionChipColor(version: string): ThemeColor | null {
  const parts = version.split('.')

  if (parts.length < 2) {
    return null
  }

  if (parts[0] !== '0') {
    return 'success'
  }

  return parts[1] === '0' ? null : 'primary'
}

export function currentVersion(history: readonly VersionHistoryEntry[]): string {
  return history[0]?.version ?? 'unknown'
}

export function compareVersions(left: string, right: string): number {
  const leftParts = left.split('.').map(Number)
  const rightParts = right.split('.').map(Number)
  const length = Math.max(leftParts.length, rightParts.length)

  for (let index = 0; index < length; index += 1) {
    const difference = (leftParts[index] ?? 0) - (rightParts[index] ?? 0)

    if (difference !== 0) {
      return difference < 0 ? -1 : 1
    }
  }

  return 0
}
