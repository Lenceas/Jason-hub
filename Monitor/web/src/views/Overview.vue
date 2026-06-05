<script setup lang="ts">
import { onMounted } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import MetricsCard from '../components/MetricsCard.vue'
import StatusBadge from '../components/StatusBadge.vue'
import GaugeChart from '../components/GaugeChart.vue'

const store = useMonitorStore()
onMounted(() => store.refreshAll())

function thresholdColor(v: number | null | undefined, warn = 60, danger = 80): string {
  if (v == null) return 'var(--color-primary)'
  if (v >= danger) return 'var(--color-danger)'
  if (v >= warn) return 'var(--color-warning)'
  return 'var(--color-success)'
}
</script>

<template>
  <div class="overview">
    <div class="grid-4">
      <MetricsCard
        label="CPU"
        :value="store.metrics?.cpu_pct"
        unit="%"
        :color="thresholdColor(store.metrics?.cpu_pct, 60, 80)"
        icon="🔲"
      />
      <MetricsCard
        label="内存"
        :value="store.metrics?.mem_pct"
        unit="%"
        :color="thresholdColor(store.metrics?.mem_pct, 60, 80)"
        icon="🧠"
      />
      <MetricsCard
        label="磁盘"
        :value="store.metrics?.disk_pct"
        unit="%"
        :color="thresholdColor(store.metrics?.disk_pct, 70, 85)"
        icon="💾"
      />
      <MetricsCard
        label="负载 (1m)"
        :value="store.metrics?.load_1m"
        unit=""
        :color="(store.metrics?.load_1m ?? 0) > 2 ? 'var(--color-danger)' : 'var(--color-info)'"
        icon="📊"
      />
    </div>

    <div class="grid-2" style="margin-top:24px">
      <div class="card">
        <h3 class="card-title">实时状态</h3>
        <div class="gauges-row">
          <GaugeChart title="CPU" :value="store.metrics?.cpu_pct" />
          <GaugeChart title="内存" :value="store.metrics?.mem_pct" />
          <GaugeChart title="磁盘" :value="store.metrics?.disk_pct" />
        </div>
      </div>
      <div class="card">
        <h3 class="card-title">服务健康</h3>
        <div class="services-list">
          <div v-for="h in store.healthRecords" :key="h.service" class="service-row">
            <span class="service-name">{{ h.service }}</span>
            <StatusBadge :status="h.status" />
            <span class="service-latency">{{ h.latency_ms }}ms</span>
          </div>
          <div v-if="!store.healthRecords.length" class="empty-text">暂无数据</div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.grid-4 { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 12px; }
.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.card {
  background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius);
  padding: 16px; box-shadow: var(--card-shadow); overflow: hidden; min-width: 0;
}
.card-title { font-size: 15px; font-weight: 600; margin-bottom: 16px; }
.gauges-row { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 8px; }
.services-list { display: flex; flex-direction: column; gap: 12px; }
.service-row { display: flex; align-items: center; gap: 12px; }
.service-name { flex: 1; font-weight: 600; font-size: 14px; }
.service-latency { font-family: var(--font-mono); font-size: 12px; color: var(--color-text-secondary); }
.empty-text { color: var(--color-text-secondary); padding: 24px; text-align: center; }

@media (max-width: 768px) {
  .grid-4 { grid-template-columns: repeat(2, 1fr); gap: 8px; }
  .grid-2 { grid-template-columns: 1fr; }
  .gauges-row { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 480px) {
  .grid-4 { grid-template-columns: 1fr; gap: 8px; }
  .gauges-row { grid-template-columns: 1fr; }
}
</style>
