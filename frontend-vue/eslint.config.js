import js from '@eslint/js'
import pluginVue from 'eslint-plugin-vue'
import pluginA11y from 'eslint-plugin-vuejs-accessibility'
import globals from 'globals'
import tseslint from 'typescript-eslint'
import vueParser from 'vue-eslint-parser'

const platformOnlyGlobals = [
  { name: 'fetch', message: 'Сетевые вызовы идут через src/platform/http.ts.' },
  { name: 'localStorage', message: 'Хранилище доступно только через src/platform/storage.ts.' },
  { name: 'sessionStorage', message: 'Хранилище доступно только через src/platform/storage.ts.' },
]

const platformOnlyProperties = [
  { object: 'window', property: 'fetch', message: 'Сетевые вызовы идут через src/platform/http.ts.' },
  { object: 'window', property: 'localStorage', message: 'Хранилище доступно только через src/platform/storage.ts.' },
  { object: 'window', property: 'sessionStorage', message: 'Хранилище доступно только через src/platform/storage.ts.' },
]

const formattingOnlySyntax = [
  {
    selector: "NewExpression[callee.object.name='Intl']",
    message: 'Форматирование живёт в src/platform/format.ts.',
  },
  {
    selector: "CallExpression[callee.property.name=/^toLocale(String|DateString|TimeString)$/]",
    message: 'Форматирование живёт в src/platform/format.ts.',
  },
]

const boundaryImports = {
  patterns: [
    {
      group: ['@/api/generated', '@/api/generated/*'],
      message: 'Сгенерированные типы доступны только в src/api/adapters.',
    },
    {
      group: ['@/features/*/*'],
      message: 'Фича импортируется только через features/<имя>/index.ts.',
    },
    {
      group: ['@/pages/*'],
      message: 'Страницы подключаются только маршрутизатором.',
    },
  ],
}

const projectRules = {
  'no-restricted-globals': ['error', ...platformOnlyGlobals],
  'no-restricted-properties': ['error', ...platformOnlyProperties],
  'no-restricted-syntax': ['error', ...formattingOnlySyntax],
  '@typescript-eslint/no-restricted-imports': ['error', boundaryImports],
}

export default tseslint.config(
  {
    ignores: ['dist', 'coverage', 'playwright-report', 'test-results', 'src/api/generated', 'src/shared/mud/vendor'],
  },

  js.configs.recommended,
  ...pluginVue.configs['flat/recommended'],
  ...pluginA11y.configs['flat/recommended'],

  {
    files: ['src/**/*.ts', 'e2e/**/*.ts'],
    extends: [tseslint.configs.recommendedTypeChecked],
    languageOptions: {
      globals: globals.browser,
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    rules: projectRules,
  },

  {
    files: ['src/**/*.vue'],
    extends: [tseslint.configs.recommendedTypeChecked],
    languageOptions: {
      globals: globals.browser,
      parser: vueParser,
      parserOptions: {
        parser: tseslint.parser,
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
        extraFileExtensions: ['.vue'],
      },
    },
    rules: projectRules,
  },

  {
    files: ['e2e/support/**/*.ts'],
    rules: {
      'no-restricted-globals': 'off',
    },
  },

  {
    files: ['src/platform/**/*.ts'],
    rules: {
      'no-restricted-globals': 'off',
      'no-restricted-properties': 'off',
      'no-restricted-syntax': 'off',
    },
  },

  {
    files: ['src/api/adapters/**/*.ts', 'src/api/transport/**/*.ts', 'src/app/router.ts', 'src/app/routes.ts'],
    rules: {
      '@typescript-eslint/no-restricted-imports': 'off',
    },
  },

  {
    files: ['src/app/main.ts'],
    rules: {
      '@typescript-eslint/no-unsafe-argument': 'off',
    },
  },

  {
    files: ['src/shared/mud/**/*.vue'],
    rules: {
      'vue/require-default-prop': 'off',
      'vue/no-v-html': 'off',
      'vuejs-accessibility/label-has-for': ['error', { required: 'id' }],
    },
  },

  {
    files: ['*.ts', '*.js', 'scripts/**/*.mjs'],
    languageOptions: {
      globals: globals.node,
    },
  },
)
