import { createApp, h } from 'vue'
import { createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
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

/** 渲染纯文本标题的函数式组件 */
const pageTitle = (text: string) => ({ render: () => h('span', text) })

const routes = [
  {
    path: '/',
    component: DashboardLayout,
    children: [
      { path: '', redirect: '/overview' },
      { path: 'overview', components: { default: Overview, title: pageTitle('📈 概览大盘') } },
      { path: 'server',  components: { default: Server,  title: pageTitle('🖥️ 服务器监控') } },
      { path: 'docker',  components: { default: Docker,  title: pageTitle('🐳 Docker 容器') } },
      { path: 'uptime',  components: { default: Uptime,  title: pageTitle('🌐 站点可用性') } },
      { path: 'health',  components: { default: Health,  title: pageTitle('❤️ 应用健康') } },
      { path: 'alerts',  components: { default: Alerts,  title: pageTitle('🔔 告警中心') } },
      { path: 'settings',components: { default: Settings,title: pageTitle('⚙️ 系统设置') } },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
