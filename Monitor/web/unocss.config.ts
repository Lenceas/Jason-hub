import { defineConfig, presetUno, presetAttributify, presetIcons } from 'unocss'

export default defineConfig({
  presets: [presetUno(), presetAttributify(), presetIcons()],
  shortcuts: {
    'card': 'bg-white rounded-xl shadow-sm border border-slate-200 p-5',
    'btn': 'px-4 py-2 rounded-lg font-medium text-sm transition-colors',
    'btn-primary': 'btn bg-blue-500 text-white hover:bg-blue-600',
  },
  rules: [],
})
