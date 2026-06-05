<script setup lang="ts">
import { onMounted } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import AlertTable from '../components/AlertTable.vue'
import MetricsCard from '../components/MetricsCard.vue'

const store = useMonitorStore()
onMounted(() => { store.loadAlertRules(); store.loadAlertEvents() })
</script>

<template>
  <div>
    <div class="grid-3">
      <MetricsCard label="告警规则" :value="store.alertRules.length" icon="🔔" />
      <MetricsCard label="已启用" :value="store.alertRules.filter(r => r.enabled).length" icon="✅" color="var(--color-success)" />
      <MetricsCard label="近期事件" :value="store.alertEvents.length" icon="📋" color="var(--color-warning)" />
    </div>
    <div class="card" style="margin-top:20px">
      <h3 class="card-title">告警规则</h3>
      <table v-if="store.alertRules.length" class="rules-table">
        <thead>
          <tr>
            <th>名称</th>
            <th>指标</th>
            <th>触发条件</th>
            <th>持续</th>
            <th>状态</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in store.alertRules" :key="r.id">
            <td class="cell-name">{{ r.name }}</td>
            <td><code>{{ r.metric }}</code></td>
            <td>{{ r.operator }} {{ r.threshold }}</td>
            <td>{{ r.duration_sec }}s</td>
            <td>{{ r.enabled ? '✅ 启用' : '⏸️ 停用' }}</td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-text">暂无告警规则</div>
    </div>
    <div class="card" style="margin-top:20px">
      <h3 class="card-title">告警事件</h3>
      <AlertTable :events="store.alertEvents" />
    </div>
  </div>
</template>

<style scoped>
.grid-3 { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; }
.card { background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius); padding: 16px; box-shadow: var(--card-shadow); overflow: hidden; }
.card-title { font-size: 15px; font-weight: 600; margin-bottom: 16px; }
.rules-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.rules-table th { text-align: left; padding: 10px 12px; color: var(--color-text-secondary); font-weight: 600; border-bottom: 2px solid #E2E8F0; }
.rules-table td { padding: 10px 12px; border-bottom: 1px solid #F1F5F9; }
.rules-table code { font-family: var(--font-mono); font-size: 12px; background: #F1F5F9; padding: 2px 6px; border-radius: 4px; }
.cell-name { font-weight: 600; }
.empty-text { padding: 32px; text-align: center; color: var(--color-text-secondary); }

@media (max-width: 768px) {
  .grid-3 { grid-template-columns: repeat(2, 1fr); gap: 8px; }
}

@media (max-width: 480px) {
  .grid-3 { grid-template-columns: 1fr; gap: 8px; }
  .rules-table th:nth-child(2),
  .rules-table td:nth-child(2),
  .rules-table th:nth-child(4),
  .rules-table td:nth-child(4) { display: none; }
}
</style>
