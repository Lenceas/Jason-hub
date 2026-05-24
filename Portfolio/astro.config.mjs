import { defineConfig } from 'astro/config';

export default defineConfig({
  site: 'https://jason.dev',
  server: {
    host: true,
    port: 8000,
    open: true,
  },
});
