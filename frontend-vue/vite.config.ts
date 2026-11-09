import { execFileSync } from 'node:child_process'
import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vitest/config'
import { preloadFontsPlugin } from './build/preload-fonts.ts'
import { mudCssPrunePlugin } from './build/prune-mud-css.ts'
import { pwaPlugin, pwaSelfDestroy } from './build/pwa.ts'

function readBuild() {
  try {
    const [commit, builtAt] = execFileSync('git', ['log', '-1', '--format=%h%n%cI'], { encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] }).trim().split('\n')

    return { commit, builtAt }
  } catch {
    return { commit: process.env.GITHUB_SHA?.slice(0, 7) ?? '', builtAt: new Date().toISOString() }
  }
}

export default defineConfig({
  plugins: [vue(), mudCssPrunePlugin(), preloadFontsPlugin(), pwaPlugin()],
  define: {
    __APP_BUILD__: JSON.stringify(readBuild()),
    __PWA_SELF_DESTROY__: JSON.stringify(pwaSelfDestroy),
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    host: '127.0.0.1',
    port: 5173,
    warmup: {
      clientFiles: ['./src/app/main.ts', './src/pages/*.vue'],
    },
    watch: {
      ignored: ['**/dist/**', '**/test-results/**', '**/playwright-report/**'],
    },
    proxy: {
      '/api': {
        target: process.env.VITE_API_PROXY_TARGET ?? 'https://localhost:7124',
        changeOrigin: true,
        secure: false,
        rewrite: path => path.replace(/^\/api/, ''),
      },
    },
  },
  test: {
    environment: 'happy-dom',
    include: ['src/**/*.spec.ts', 'build/**/*.spec.ts'],
    restoreMocks: true,
    setupFiles: ['./src/platform/network-guard.setup.ts'],
  },
})
