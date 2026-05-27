import { defineConfig } from 'astro/config';

export default defineConfig({
  site: 'https://lujiesheng.cn',
  devToolbar: {
    enabled: false,
  },
  server: {
    host: true,
    port: 8000,
    open: true,
  },
});
