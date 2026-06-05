<script setup lang="ts">
import { onMounted } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import StatusBadge from '../components/StatusBadge.vue'
import MetricsCard from '../components/MetricsCard.vue'

const store = useMonitorStore()
onMounted(() => store.loadSites())

function latestOk(_siteId: number): boolean | null {
  return true // TODO: 从 store 中获取站点最新状态
}
</script>

<template>
  <div>
    <div class="grid-3">
      <MetricsCard label="监控站点" :value="store.sites.length" icon="🌐" />
      <MetricsCard label="活跃" :value="store.sites.filter(s => s.is_active).length" icon="✅" color="var(--color-success)" />
      <MetricsCard label="已停用" :value="store.sites.filter(s => !s.is_active).length" icon="⏸️" color="var(--color-warning)" />
    </div>
    <div class="card" style="margin-top:20px">
      <h3 class="card-title">站点列表</h3>
      <table v-if="store.sites.length" class="uptime-table">
        <thead>
          <tr>
            <th>名称</th>
            <th>URL</th>
            <th>间隔</th>
            <th>状态</th>
            <th>启用</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="s in store.sites" :key="s.id">
            <td class="cell-name">{{ s.name }}</td>
            <td class="cell-url">{{ s.url }}</td>
            <td>{{ s.interval_sec }}s</td>
            <td><StatusBadge :is-ok="latestOk(s.id)" /></td>
            <td>{{ s.is_active ? '✅' : '⏸️' }}</td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-text">暂无监控站点，请在 API 中添加</div>
    </div>
  </div>
</template>

<style scoped>
.grid-3 { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; }
.card { background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius); padding: 16px; box-shadow: var(--card-shadow); overflow: hidden; }
.card-title { font-size: 15px; font-weight: 600; margin-bottom: 16px; }
.uptime-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.uptime-table th { text-align: left; padding: 10px 12px; color: var(--color-text-secondary); font-weight: 600; border-bottom: 2px solid #E2E8F0; }
.uptime-table td { padding: 10px 12px; border-bottom: 1px solid #F1F5F9; }
.cell-name { font-weight: 600; }
.cell-url { font-family: var(--font-mono); font-size: 12px; color: var(--color-primary); max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.empty-text { padding: 32px; text-align: center; color: var(--color-text-secondary); }

@media (max-width: 768px) {
  .grid-3 { grid-template-columns: repeat(2, 1fr); gap: 8px; }
  .uptime-table th:nth-child(2),
  .uptime-table td:nth-child(2) { display: none; }
}

@media (max-width: 480px) {
  .grid-3 { grid-template-columns: 1fr; gap: 8px; }
  .uptime-table th:nth-child(3),
  .uptime-table td:nth-child(3),
  .uptime-table th:nth-child(5),
  .uptime-table td:nth-child(5) { display: none; }
}
</style>
