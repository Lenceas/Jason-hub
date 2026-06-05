<script setup lang="ts">
import { onMounted } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import StatusBadge from '../components/StatusBadge.vue'
import MetricsCard from '../components/MetricsCard.vue'

const store = useMonitorStore()
onMounted(() => store.loadContainers())
</script>

<template>
  <div>
    <div class="grid-3">
      <MetricsCard label="容器总数" :value="store.containers.length" icon="📦" />
      <MetricsCard label="运行中" :value="store.containers.filter(c => c.status === 'running').length" icon="✅" color="var(--color-success)" />
      <MetricsCard label="已停止" :value="store.containers.filter(c => c.status !== 'running').length" icon="⏹️" color="var(--color-warning)" />
    </div>
    <div class="card" style="margin-top:20px">
      <h3 class="card-title">容器列表</h3>
      <table v-if="store.containers.length" class="docker-table">
        <thead>
          <tr>
            <th>名称</th>
            <th>状态</th>
            <th>CPU</th>
            <th>内存</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in store.containers" :key="c.name">
            <td class="cell-name">{{ c.name }}</td>
            <td><StatusBadge :status="c.status === 'running' ? 'ok' : 'error'" /></td>
            <td>{{ c.cpu_pct ?? '—' }}%</td>
            <td>{{ c.mem_usage ? (c.mem_usage / 1024 / 1024).toFixed(0) + 'MB' : '—' }}</td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-text">暂无容器数据（Agent 可能未启用 Docker 采集）</div>
    </div>
  </div>
</template>

<style scoped>
.grid-3 { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; }
.card { background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius); padding: 16px; box-shadow: var(--card-shadow); overflow: hidden; }
.card-title { font-size: 15px; font-weight: 600; margin-bottom: 16px; }
.docker-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.docker-table th { text-align: left; padding: 10px 12px; color: var(--color-text-secondary); font-weight: 600; border-bottom: 2px solid #E2E8F0; }
.docker-table td { padding: 10px 12px; border-bottom: 1px solid #F1F5F9; }
.docker-table tbody tr:hover { background: #F8FAFC; }
.cell-name { font-family: var(--font-mono); font-size: 12px; font-weight: 600; }
.empty-text { padding: 32px; text-align: center; color: var(--color-text-secondary); }

@media (max-width: 768px) {
  .grid-3 { grid-template-columns: repeat(2, 1fr); gap: 8px; }
}

@media (max-width: 480px) {
  .grid-3 { grid-template-columns: 1fr; gap: 8px; }
  .docker-table th:nth-child(3),
  .docker-table td:nth-child(3),
  .docker-table th:nth-child(4),
  .docker-table td:nth-child(4) { display: none; }
}
</style>
