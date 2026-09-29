import type { PluginOption } from 'vite'
import { VitePWA } from 'vite-plugin-pwa'

function resolveSelfDestroy(value: string | undefined): boolean {
  const raw = value?.trim() ?? ''

  if (raw === '' || raw === '0') {
    return false
  }

  if (raw === '1') {
    return true
  }

  throw new Error(`MONEY_PWA_SELF_DESTROY = '${raw}' не годится: 1 снимает воркер у клиентов, 0 или пусто – обычная сборка`)
}

export const pwaSelfDestroy = resolveSelfDestroy(process.env.MONEY_PWA_SELF_DESTROY)

export function pwaPlugin(): PluginOption {
  return VitePWA({
    strategies: 'injectManifest',
    srcDir: 'src/service-worker',
    filename: 'service-worker.ts',
    registerType: 'prompt',
    injectRegister: false,
    selfDestroying: pwaSelfDestroy,
    manifestFilename: 'manifest.webmanifest',
    includeManifestIcons: false,
    manifest: {
      id: '/',
      name: 'Филочки',
      short_name: 'Филочки',
      description: 'Личный учёт доходов и расходов: операции, категории, статистика, долги и регулярные платежи.',
      lang: 'ru',
      start_url: '/',
      scope: '/',
      display: 'standalone',
      theme_color: '#594ae2',
      background_color: '#ffffff',
      icons: [
        { src: '/icon-192.png', sizes: '192x192', type: 'image/png', purpose: 'any' },
        { src: '/icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'any' },
        { src: '/icon-maskable-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
      ],
    },
    injectManifest: {
      globPatterns: ['**/*.{js,css,html,woff2,png}'],
      globIgnores: ['icon-512.png', 'icon-maskable-512.png'],
      rollupFormat: 'iife',
    },
  })
}
