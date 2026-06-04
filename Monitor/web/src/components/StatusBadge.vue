<script setup lang="ts">
defineProps<{
  status?: string | null
  isOk?: boolean | null
}>()

function statusClass(status?: string | null, isOk?: boolean | null): string {
  if (isOk === true || status === 'ok' || status?.includes('200')) return 'status-ok'
  if (isOk === false || status === 'unreachable' || status?.includes('fail') || status?.includes('500') || status?.includes('503'))
    return 'status-error'
  if (status?.includes('4')) return 'status-warn'
  return 'status-muted'
}

function statusLabel(status?: string | null, isOk?: boolean | null): string {
  if (isOk === true) return '正常'
  if (isOk === false) return '故障'
  if (status === 'unreachable') return '离线'
  if (status?.includes('200')) return '正常'
  if (status?.includes('4')) return '异常'
  if (status?.includes('5')) return '故障'
  return status ?? '未知'
}
</script>

<template>
  <span class="status-badge" :class="statusClass(status, isOk)">
    {{ statusLabel(status, isOk) }}
  </span>
</template>

<style scoped>
.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 2px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  white-space: nowrap;
}
.status-badge::before {
  content: '';
  display: inline-block;
  width: 6px;
  height: 6px;
  border-radius: 50%;
}
.status-ok { background: #D1FAE5; color: #065F46; }
.status-ok::before { background: var(--color-success); }
.status-error { background: #FEE2E2; color: #991B1B; }
.status-error::before { background: var(--color-danger); }
.status-warn { background: #FEF3C7; color: #92400E; }
.status-warn::before { background: var(--color-warning); }
.status-muted { background: #F1F5F9; color: #64748B; }
.status-muted::before { background: #94A3B8; }
</style>
