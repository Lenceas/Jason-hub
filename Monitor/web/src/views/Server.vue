<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import MetricsCard from '../components/MetricsCard.vue'
import LineChart from '../components/LineChart.vue'

const store = useMonitorStore()
onMounted(() => { store.refreshAll(); store.loadMetricsHistory(24) })

const cpuSeries = computed(() => [{
  name: 'CPU %',
  data: store.metricsHistory.map(m => m.cpu_pct ?? 0),
  color: '#3B82F6',
}])

const memSeries = computed(() => [{
  name: '内存 %',
  data: store.metricsHistory.map(m => m.mem_pct ?? 0),
  color: '#10B981',
}])

const timeLabels = computed(() =>
  store.metricsHistory.map(m => new Date(m.ts).toLocaleTimeString())
)
</script>

<template>
  <div>
    <div class="grid-4">
      <MetricsCard label="CPU" :value="store.metrics?.cpu_pct" unit="%" icon="🔲" />
      <MetricsCard label="内存" :value="store.metrics?.mem_pct" unit="%" icon="🧠" />
      <MetricsCard label="磁盘" :value="store.metrics?.disk_pct" unit="%" icon="💾" />
      <MetricsCard label="网络 (IN)" :value="store.metrics?.net_in" unit="B" icon="📡" />
    </div>
    <div class="charts-grid">
      <div class="card"><LineChart title="CPU 趋势" :x-data="timeLabels" :series="cpuSeries" /></div>
      <div class="card"><LineChart title="内存趋势" :x-data="timeLabels" :series="memSeries" /></div>
    </div>
  </div>
</template>

<style scoped>
.grid-4 { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; }
.charts-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-top: 20px; }
.card { background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius); padding: 20px; box-shadow: var(--card-shadow); }
</style>
