import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { createRouter, createWebHashHistory } from 'vue-router'
import App from './App.vue'
import DashboardLayout from './layouts/DashboardLayout.vue'
import './styles/variables.css'
import 'virtual:uno.css'

import Overview from './views/Overview.vue'
import Server from './views/Server.vue'
import Docker from './views/Docker.vue'
import Uptime from './views/Uptime.vue'
import Health from './views/Health.vue'
import Alerts from './views/Alerts.vue'
import Settings from './views/Settings.vue'

const routes = [
  {
    path: '/',
    component: DashboardLayout,
    children: [
      {
        path: '',
        components: { default: Overview, title: { template: '📈 概览大盘' } },
      },
      {
        path: 'server',
        components: { default: Server, title: { template: '🖥️ 服务器监控' } },
      },
      {
        path: 'docker',
        components: { default: Docker, title: { template: '🐳 Docker 容器' } },
      },
      {
        path: 'uptime',
        components: { default: Uptime, title: { template: '🌐 站点可用性' } },
      },
      {
        path: 'health',
        components: { default: Health, title: { template: '❤️ 应用健康' } },
      },
      {
        path: 'alerts',
        components: { default: Alerts, title: { template: '🔔 告警中心' } },
      },
      {
        path: 'settings',
        components: { default: Settings, title: { template: '⚙️ 系统设置' } },
      },
    ],
  },
]

const router = createRouter({
  history: createWebHashHistory(),
  routes,
})

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
