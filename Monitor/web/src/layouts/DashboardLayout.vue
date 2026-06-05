<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import RefreshCounter from '../components/RefreshCounter.vue'
import FlipClock from '../components/FlipClock.vue'

const store = useMonitorStore()
const sidebarOpen = ref(false)

onMounted(() => store.startAutoRefresh())
onUnmounted(() => store.stopAutoRefresh())
</script>

<template>
  <div class="dashboard-layout">
    <!-- 手机遮罩层 -->
    <div v-if="sidebarOpen" class="sidebar-overlay" @click="sidebarOpen = false" />

    <aside class="sidebar" :class="{ open: sidebarOpen }">
      <nav class="sidebar-nav">
        <RouterLink to="/overview" class="nav-item" @click="sidebarOpen = false">📈 概览</RouterLink>
        <RouterLink to="/server" class="nav-item" @click="sidebarOpen = false">🖥️ 服务器</RouterLink>
        <RouterLink to="/docker" class="nav-item" @click="sidebarOpen = false">🐳 Docker</RouterLink>
        <RouterLink to="/uptime" class="nav-item" @click="sidebarOpen = false">🌐 站点</RouterLink>
        <RouterLink to="/health" class="nav-item" @click="sidebarOpen = false">❤️ 健康</RouterLink>
        <RouterLink to="/alerts" class="nav-item" @click="sidebarOpen = false">🔔 告警</RouterLink>
        <RouterLink to="/settings" class="nav-item" @click="sidebarOpen = false">⚙️ 设置</RouterLink>
      </nav>
    </aside>

    <main class="main-content">
      <header class="topbar">
        <button class="hamburger" @click="sidebarOpen = !sidebarOpen">
          <span />
          <span />
          <span />
        </button>
        <h2 class="page-title">
          <RouterView name="title" />
        </h2>
        <div class="clock-center"><FlipClock /></div>
        <div class="topbar-right" style="margin-left:auto">
          <span v-if="store.loading" class="loading-dot">⟳</span>
          <RefreshCounter :seconds="store.nextRefresh" />
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

/* —— 侧边栏 —— */
.sidebar {
  width: var(--sidebar-width);
  min-width: var(--sidebar-width);
  background: var(--sidebar-bg);
  color: var(--sidebar-text);
  display: flex;
  flex-direction: column;
  position: sticky; top: 0;
  height: 100vh;
  z-index: 200;
  transition: transform 0.25s ease;
}
.sidebar-nav { display: flex; flex-direction: column; padding: 0 12px 8px; gap: 2px; flex: 1; }
.sidebar-nav::before {
  content: '监控导航';
  display: flex; align-items: center;
  height: 48px;
  font-size: 11px; font-weight: 600; color: #94A3B8;
  text-transform: uppercase; letter-spacing: 1px;
  padding: 0 4px; margin-bottom: 4px;
  border-bottom: 1px solid #D4D8DD;
}
.nav-item {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 12px; border-radius: 8px;
  font-size: 14px; transition: background 0.15s;
}
.nav-item:hover { background: #D4D8DD; }
.nav-item.router-link-active { background: var(--sidebar-active); color: white; font-weight: 600; }

/* 手机：侧边栏默认隐藏，点击后 overlay 滑入 */
@media (max-width: 768px) {
  .sidebar {
    transform: translateX(-100%);
    position: fixed; left: 0; top: 0; bottom: 0;
    width: 200px;
    min-width: 200px;
    box-shadow: 4px 0 16px rgba(0,0,0,0.3);
  }
  .sidebar.open { transform: translateX(0); }
}

/* 遮罩 */
.sidebar-overlay {
  display: none;
  position: fixed; inset: 0; background: rgba(0,0,0,0.4);
  z-index: 150;
}
@media (max-width: 768px) {
  .sidebar-overlay { display: block; }
}

/* —— 主内容区 —— */
.main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

@media (max-width: 768px) {
  .main-content { width: 100%; }
}

/* 顶栏 */
.topbar {
  display: flex; align-items: center; gap: 12px;
  height: 48px; padding: 0 20px; background: white;
  border-bottom: 1px solid #E2E8F0; position: sticky; top: 0; z-index: 50;
}
.hamburger {
  display: none; flex-direction: column; gap: 4px;
  background: none; border: none; padding: 6px; cursor: pointer;
}
.hamburger span {
  display: block; width: 20px; height: 2px;
  background: var(--color-text); border-radius: 1px;
}
@media (max-width: 768px) {
  .hamburger { display: flex; }
  .flip-clock { display: none; }
}

.page-title { font-size: 20px; font-weight: 700; }
.clock-center { flex: 1; display: flex; justify-content: center; }
.topbar-right { display: flex; align-items: center; gap: 12px; white-space: nowrap; }
.loading-dot { animation: spin 1s linear infinite; font-size: 18px; }
@keyframes spin { to { transform: rotate(360deg); } }

.content-area { padding: 16px; flex: 1; min-width: 0; overflow-x: hidden; }

@media (max-width: 768px) {
  .content-area { padding: 12px; }
  .page-title { font-size: 16px; }
}

@media (max-width: 480px) {
  .topbar { padding: 10px 12px; }
  .page-title { font-size: 14px; }
  .content-area { padding: 8px; }
}
</style>
