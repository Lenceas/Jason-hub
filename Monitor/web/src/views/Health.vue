<script setup lang="ts">
import { onMounted } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import StatusBadge from '../components/StatusBadge.vue'

const store = useMonitorStore()
onMounted(() => store.loadHealth())
</script>

<template>
  <div class="card">
    <h3 class="card-title">服务健康状态</h3>
    <table v-if="store.healthRecords.length" class="health-table">
      <thead>
        <tr>
          <th>服务</th>
          <th>状态</th>
          <th>延迟</th>
          <th>检查时间</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="h in store.healthRecords" :key="h.service">
          <td class="cell-name">{{ h.service }}</td>
          <td><StatusBadge :status="h.status" /></td>
          <td class="cell-latency">{{ h.latency_ms ?? '—' }}ms</td>
          <td class="cell-time">{{ new Date(h.ts).toLocaleString() }}</td>
        </tr>
      </tbody>
    </table>
    <div v-else class="empty-text">暂无健康检查数据</div>
  </div>
</template>

<style scoped>
.card { background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius); padding: 20px; box-shadow: var(--card-shadow); }
.card-title { font-size: 15px; font-weight: 600; margin-bottom: 16px; }
.health-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.health-table th { text-align: left; padding: 10px 12px; color: var(--color-text-secondary); font-weight: 600; border-bottom: 2px solid #E2E8F0; }
.health-table td { padding: 10px 12px; border-bottom: 1px solid #F1F5F9; }
.health-table tbody tr:hover { background: #F8FAFC; }
.cell-name { font-weight: 600; }
.cell-latency { font-family: var(--font-mono); font-size: 12px; }
.cell-time { font-family: var(--font-mono); font-size: 12px; color: var(--color-text-secondary); }
.empty-text { padding: 32px; text-align: center; color: var(--color-text-secondary); }
</style>
