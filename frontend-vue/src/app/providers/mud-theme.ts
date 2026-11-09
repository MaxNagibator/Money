import { watch, watchEffect } from 'vue'
import { router } from '@/app/router'
import { useThemeSettings } from '@/features/settings'
import { tryWriteSetting } from '@/platform/storage'
import { EFFECTIVE_THEME_KEY } from '@/shared/ui/useAppTheme'

export function syncMudTheme(): void {
  const theme = useThemeSettings()

  watchEffect(() => {
    document.documentElement.dataset.mudTheme = theme.effective.value
    tryWriteSetting(EFFECTIVE_THEME_KEY, theme.effective.value)
  })

  watch(router.currentRoute, () => {
    theme.recheckOnNavigation()
  })
}
