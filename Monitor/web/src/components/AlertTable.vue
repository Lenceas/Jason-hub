<script setup lang="ts">
import type { AlertEvent } from '../types'
import StatusBadge from './StatusBadge.vue'

defineProps<{
  events: AlertEvent[]
}>()
</script>

<template>
  <div class="alert-table-wrapper">
    <table v-if="events.length" class="alert-table">
      <thead>
        <tr>
          <th>时间</th>
          <th>规则</th>
          <th>消息</th>
          <th>级别</th>
          <th>状态</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="evt in events" :key="evt.id">
          <td class="cell-time">{{ new Date(evt.triggered_at).toLocaleString() }}</td>
          <td>{{ evt.rule_name ?? '—' }}</td>
          <td class="cell-msg">{{ evt.message ?? '—' }}</td>
          <td>
            <StatusBadge :status="evt.severity === 'critical' ? 'error' : 'warn'" />
          </td>
          <td>
            <StatusBadge :is-ok="!evt.resolved_at" :status="evt.resolved_at ? 'resolved' : 'triggering'" />
          </td>
        </tr>
      </tbody>
    </table>
    <div v-else class="empty">暂无告警事件</div>
  </div>
</template>

<style scoped>
.alert-table-wrapper { overflow-x: auto; }
.alert-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.alert-table th {
  text-align: left; padding: 10px 12px;
  color: var(--color-text-secondary); font-weight: 600;
  border-bottom: 2px solid #E2E8F0;
}
.alert-table td { padding: 10px 12px; border-bottom: 1px solid #F1F5F9; }
.alert-table tbody tr:hover { background: #F8FAFC; }
.cell-time { white-space: nowrap; font-family: var(--font-mono); font-size: 12px; }
.cell-msg { max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.empty { padding: 32px; text-align: center; color: var(--color-text-secondary); }
</style>
