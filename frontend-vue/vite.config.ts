import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vitest/config'
import { preloadFontsPlugin } from './build/preload-fonts.ts'
import { mudCssPrunePlugin } from './build/prune-mud-css.ts'

export default defineConfig({
  plugins: [vue(), mudCssPrunePlugin(), preloadFontsPlugin()],
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
