import { ref, watch } from 'vue'
import { readSetting, tryWriteSetting } from '@/platform/storage'
import type { DebtViewMode } from './debt-presentation'

const STORAGE_KEY = 'debts:view-mode'
const VALID_MODES: readonly DebtViewMode[] = ['accounting', 'normal', 'rework', 'refined']

function initialMode(): DebtViewMode {
  try {
    const saved = readSetting<unknown>(STORAGE_KEY, 'normal')
    return typeof saved === 'string' && VALID_MODES.includes(saved as DebtViewMode)
      ? saved as DebtViewMode
      : 'normal'
  } catch {
    return 'normal'
  }
}

export function useDebtViewMode() {
  const mode = ref<DebtViewMode>(initialMode())
  watch(mode, value => { tryWriteSetting(STORAGE_KEY, value) })
  return mode
}
