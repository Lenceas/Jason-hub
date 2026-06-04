<script setup lang="ts">
defineProps<{
  label: string
  value?: string | number | null
  unit?: string
  color?: string
  icon?: string
}>()
</script>

<template>
  <div class="metrics-card" :style="{ borderTopColor: color ?? 'var(--color-primary)' }">
    <div class="card-header">
      <span v-if="icon" class="card-icon">{{ icon }}</span>
      <span class="card-label">{{ label }}</span>
    </div>
    <div class="card-value">
      <template v-if="value !== null && value !== undefined">
        {{ value }}<span v-if="unit" class="card-unit">{{ unit }}</span>
      </template>
      <span v-else class="text-muted">—</span>
    </div>
  </div>
</template>

<style scoped>
.metrics-card {
  background: var(--color-bg-card);
  border: 1px solid #E2E8F0;
  border-top: 3px solid;
  border-radius: var(--card-radius);
  padding: 16px 20px;
  box-shadow: var(--card-shadow);
  transition: transform 0.2s, box-shadow 0.2s;
}
.metrics-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.12);
}
.card-header {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 8px;
}
.card-icon { font-size: 16px; }
.card-label { font-size: 13px; color: var(--color-text-secondary); }
.card-value { font-size: 28px; font-weight: 700; color: var(--color-text); }
.card-unit { font-size: 14px; font-weight: 400; color: var(--color-text-secondary); margin-left: 4px; }
.text-muted { color: var(--color-text-secondary); font-size: 24px; }
</style>
