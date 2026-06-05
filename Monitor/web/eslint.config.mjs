import js from '@eslint/js'
import tseslint from 'typescript-eslint'
import pluginVue from 'eslint-plugin-vue'

export default [
  // 全局忽略
  { ignores: ['dist/**', 'node_modules/**', '*.config.*'] },

  // JS/TS 基础规则
  js.configs.recommended,
  ...tseslint.configs.recommended,

  // Vue 文件
  ...pluginVue.configs['flat/essential'],
  {
    files: ['**/*.vue'],
    languageOptions: {
      parserOptions: { parser: tseslint.parser },
      globals: {
        setInterval: 'readonly',
        clearInterval: 'readonly',
        setTimeout: 'readonly',
        window: 'readonly',
        document: 'readonly',
        HTMLDivElement: 'readonly',
        requestAnimationFrame: 'readonly',
        console: 'readonly',
        fetch: 'readonly',
        Promise: 'readonly',
        ReturnType: 'readonly',
      },
    },
  },

  // 自定义规则
  {
    rules: {
      // 代码质量
      'no-console': 'warn',
      'no-debugger': 'error',
      'no-unused-vars': 'off',
      '@typescript-eslint/no-unused-vars': ['warn', { argsIgnorePattern: '^_' }],
      '@typescript-eslint/no-explicit-any': 'warn',

      // Vue 规范
      'vue/multi-word-component-names': 'off',  // 路由页面可单词
      'vue/no-v-html': 'warn',
      'vue/require-default-prop': 'off',
      'vue/require-explicit-emits': 'off',

      // 注释规范
      'spaced-comment': ['warn', 'always', { markers: ['/'] }],
    },
  },
]
