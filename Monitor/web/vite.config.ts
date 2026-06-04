import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import UnoCSS from 'unocss/vite'

export default defineConfig({
  plugins: [vue(), UnoCSS()],
  server: {
    port: 8001,
    proxy: {
      '/api': {
        target: 'http://localhost:8051',
        changeOrigin: true,
      },
    },
  },
})
