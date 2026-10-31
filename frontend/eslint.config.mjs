import js from '@eslint/js'
import pluginVue from 'eslint-plugin-vue'
import globals from 'globals'
import tsParser from '@typescript-eslint/parser'
import tsPlugin from '@typescript-eslint/eslint-plugin'

export default [
  js.configs.recommended,
  ...pluginVue.configs['flat/recommended'],
  {
    name: 'app/files-to-lint',
    files: ['**/*.{js,mjs,jsx,ts,tsx}'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: globals.browser,
      parser: tsParser,
      parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module',
        ecmaFeatures: {
          jsx: true
        }
      }
    },
    plugins: {
      '@typescript-eslint': tsPlugin
    },
    rules: {
      'no-lone-blocks': 'off',
      'no-extend-native': 'off',
      'no-unused-vars': 'off',
      '@typescript-eslint/no-unused-vars': [
        'error',
        { argsIgnorePattern: '^_' }
      ],
      'no-debugger': 'off',
      '@typescript-eslint/no-explicit-any': 'off'
    }
  },
  {
    // TypeScript 自身已做未定义标识符检查，no-undef 会把类型位置（如 as RsaHashedKeyAlgorithm）误判为运行时引用。
    name: 'app/typescript-no-undef',
    files: ['**/*.{ts,tsx}'],
    rules: {
      'no-undef': 'off'
    }
  },
  {
    name: 'app/vue-files',
    files: ['**/*.vue'],
    languageOptions: {
      globals: globals.browser,
      parserOptions: {
        parser: {
          js: 'espree',
          ts: tsParser,
          '<template>': 'espree'
        },
        ecmaVersion: 'latest',
        sourceType: 'module'
      }
    },
    plugins: {
      '@typescript-eslint': tsPlugin
    },
    rules: {
      'vue/max-attributes-per-line': 'off',
      'vue/no-v-model-argument': 'off',
      'vue/multi-word-component-names': 'off',
      'vue/singleline-html-element-content-newline': 'off',
      'vue/html-self-closing': 'off',
      'vue/html-indent': 'off',
      'vue/html-closing-bracket-newline': 'off',
      // Vue 的 script setup 使用 TypeScript，由 vue-tsc 检查类型位置和未定义标识符。
      'no-undef': 'off',
      // 与 app/files-to-lint 保持一致：TS 语义的检查交给 @typescript-eslint 版本。
      // 基础版 no-unused-vars 不认识 TS 类型声明，(row: T) => void 里的参数名会被误判为未使用。
      'no-unused-vars': 'off',
      '@typescript-eslint/no-unused-vars': [
        'error',
        { argsIgnorePattern: '^_' }
      ],
      'no-debugger': 'off',
      '@typescript-eslint/no-explicit-any': 'off'
    }
  },
  {
    // 构建期脚本运行在 Node，需要 node 全局；src 下的应用代码仍按浏览器环境校验。
    name: 'app/node-scripts',
    files: ['*.{js,mjs,ts}', '*.config.{js,mjs,ts}'],
    languageOptions: {
      globals: globals.node
    }
  },
  {
    name: 'app/files-to-ignore',
    ignores: ['**/dist/**', '**/build/*.js', '**/src/assets/**', '**/public/**', '**/src/types/auto-imports.d.ts']
  }
]
