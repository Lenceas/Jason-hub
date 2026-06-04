<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useMonitorStore } from '../stores/monitor'

const store = useMonitorStore()

onMounted(() => store.startAutoRefresh())
onUnmounted(() => store.stopAutoRefresh())
</script>

<template>
  <div class="dashboard-layout">
    <aside class="sidebar">
      <div class="sidebar-brand">
        <span class="brand-icon">📊</span>
        <span class="brand-text">Monitor</span>
      </div>
      <nav class="sidebar-nav">
        <RouterLink to="/" class="nav-item">📈 概览</RouterLink>
        <RouterLink to="/server" class="nav-item">🖥️ 服务器</RouterLink>
        <RouterLink to="/docker" class="nav-item">🐳 Docker</RouterLink>
        <RouterLink to="/uptime" class="nav-item">🌐 站点</RouterLink>
        <RouterLink to="/health" class="nav-item">❤️ 健康</RouterLink>
        <RouterLink to="/alerts" class="nav-item">🔔 告警</RouterLink>
        <RouterLink to="/settings" class="nav-item">⚙️ 设置</RouterLink>
      </nav>
    </aside>
    <main class="main-content">
      <header class="topbar">
        <h2 class="page-title">
          <RouterView name="title" />
        </h2>
        <div class="topbar-right">
          <span v-if="store.loading" class="loading-dot">⟳</span>
          <span class="update-time">下次更新 {{ 10 }}s</span>
        </div>
      </header>
      <div class="content-area">
        <RouterView />
      </div>
    </main>
  </div>
</template>

<style>
/* 全局重置 */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: var(--color-bg-alt);
  color: var(--color-text);
  -webkit-font-smoothing: antialiased;
}
a { text-decoration: none; color: inherit; }
</style>

<style scoped>
.dashboard-layout { display: flex; min-height: 100vh; }

/* 侧边栏 */
.sidebar {
  width: var(--sidebar-width);
  background: var(--sidebar-bg);
  color: var(--sidebar-text);
  display: flex;
  flex-direction: column;
  position: fixed; left: 0; top: 0; bottom: 0;
  z-index: 100;
}
.sidebar-brand {
  display: flex; align-items: center; gap: 10px;
  padding: 20px; font-size: 18px; font-weight: 700;
  border-bottom: 1px solid rgba(255,255,255,0.08);
}
.brand-icon { font-size: 24px; }
.brand-text { color: white; }
.sidebar-nav { display: flex; flex-direction: column; padding: 12px; gap: 2px; }
.nav-item {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 14px; border-radius: 8px;
  font-size: 14px; transition: background 0.15s;
}
.nav-item:hover { background: rgba(255,255,255,0.08); }
.nav-item.router-link-active { background: var(--sidebar-active); color: white; font-weight: 600; }

/* 主内容区 */
.main-content {
  flex: 1;
  margin-left: var(--sidebar-width);
  display: flex;
  flex-direction: column;
}
.topbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 16px 28px; background: white;
  border-bottom: 1px solid #E2E8F0; position: sticky; top: 0; z-index: 50;
}
.page-title { font-size: 20px; font-weight: 700; }
.topbar-right { display: flex; align-items: center; gap: 12px; }
.loading-dot { animation: spin 1s linear infinite; font-size: 18px; }
@keyframes spin { to { transform: rotate(360deg); } }
.update-time { font-size: 12px; color: var(--color-text-secondary); font-family: var(--font-mono); }
.content-area { padding: 24px 28px; flex: 1; }
</style>
