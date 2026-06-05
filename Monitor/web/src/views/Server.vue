<script setup lang="ts">
import { onMounted, onUnmounted, computed, ref } from 'vue'
import { useMonitorStore } from '../stores/monitor'
import { formatBytes } from '../utils/format'
import MetricsCard from '../components/MetricsCard.vue'
import LineChart from '../components/LineChart.vue'

const store = useMonitorStore()
const now = ref(Date.now())
let clockTimer: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  store.refreshAll()
  store.loadMetricsHistory()
  clockTimer = setInterval(() => { now.value = Date.now() }, 10_000)
})

onUnmounted(() => {
  if (clockTimer) clearInterval(clockTimer)
})

const rangeOptions = [
  { label: '1h',  value: 1 },
  { label: '6h',  value: 6 },
  { label: '24h', value: 24 },
  { label: '3d',  value: 72 },
  { label: '7d',  value: 168 },
  { label: '30d', value: 720 },
]

const xMin = computed(() => now.value - store.historyRange * 3600_000)
const xMax = computed(() => now.value)

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function toPair(m: any, key: string): [number, number] {
  return [new Date(m.ts).getTime(), m[key] ?? 0]
}

const cpuData = computed(() => [{
  name: 'CPU %', values: store.metricsHistory.map(m => toPair(m, 'cpu_pct')), color: '#3B82F6',
}])

const memData = computed(() => [{
  name: '内存 %', values: store.metricsHistory.map(m => toPair(m, 'mem_pct')), color: '#10B981',
}])

const diskData = computed(() => [{
  name: '磁盘 %', values: store.metricsHistory.map(m => toPair(m, 'disk_pct')), color: '#F59E0B',
}])

const netData = computed(() => {
  const h = store.metricsHistory
  const inValues: [number, number][] = [], outValues: [number, number][] = []
  for (let i = 1; i < h.length; i++) {
    const prev = h[i - 1], curr = h[i]
    const ts = new Date(curr.ts).getTime()
    const sec = (ts - new Date(prev.ts).getTime()) / 1000
    inValues.push([ts, sec > 0 ? Math.round(((curr.net_in ?? 0) - (prev.net_in ?? 0)) / 1024 / sec * 10) / 10 : 0])
    outValues.push([ts, sec > 0 ? Math.round(((curr.net_out ?? 0) - (prev.net_out ?? 0)) / 1024 / sec * 10) / 10 : 0])
  }
  return [
    { name: 'IN',  values: inValues,  color: '#06B6D4' },
    { name: 'OUT', values: outValues, color: '#8B5CF6' },
  ]
})

const memDetail = computed(() => {
  const m = store.metrics
  if (!m?.mem_used || !m?.mem_total) return undefined
  return `${formatBytes(m.mem_used)} / ${formatBytes(m.mem_total)}`
})

const diskDetail = computed(() => {
  const m = store.metrics
  if (!m?.disk_used || !m?.disk_total) return undefined
  return `${formatBytes(m.disk_used)} / ${formatBytes(m.disk_total)}`
})

const loadDetail = computed(() => {
  const m = store.metrics
  if (!m) return undefined
  const l1 = m.load_1m, l5 = m.load_5m, l15 = m.load_15m
  if (l1 == null && l5 == null && l15 == null) return undefined
  return `负载 ${l1 ?? '—'} / ${l5 ?? '—'} / ${l15 ?? '—'}`
})

function thresholdColor(v: number | null | undefined, warn = 60, danger = 80): string {
  if (v == null) return 'var(--color-primary)'
  if (v >= danger) return 'var(--color-danger)'
  if (v >= warn) return 'var(--color-warning)'
  return 'var(--color-success)'
}
</script>

<template>
  <div>
    <div class="grid-4">
      <MetricsCard label="CPU" :value="store.metrics?.cpu_pct" unit="%" :subtitle="loadDetail" icon="🔲" :color="thresholdColor(store.metrics?.cpu_pct, 60, 80)" />
      <MetricsCard label="内存" :value="store.metrics?.mem_pct" unit="%" :subtitle="memDetail" icon="🧠" :color="thresholdColor(store.metrics?.mem_pct, 60, 80)" />
      <MetricsCard label="网络 (IN)" :value="formatBytes(store.metrics?.net_in)" icon="📡" />
      <MetricsCard label="网络 (OUT)" :value="formatBytes(store.metrics?.net_out)" icon="📤" />
      <MetricsCard label="磁盘" :value="store.metrics?.disk_pct" unit="%" :subtitle="diskDetail" icon="💾" :color="thresholdColor(store.metrics?.disk_pct, 70, 85)" />
    </div>
    <div class="range-bar">
      <button
        v-for="o in rangeOptions" :key="o.value"
        class="range-btn"
        :class="{ active: store.historyRange === o.value }"
        @click="store.loadMetricsHistory(o.value)"
      >{{ o.label }}</button>
    </div>
    <div class="charts-grid">
      <div class="card"><LineChart title="CPU 趋势" :data="cpuData" :x-min="xMin" :x-max="xMax" /></div>
      <div class="card"><LineChart title="内存趋势" :data="memData" :x-min="xMin" :x-max="xMax" /></div>
      <div class="card"><LineChart title="网络速率趋势 (KB/s)" :data="netData" :x-min="xMin" :x-max="xMax" unit=" KB/s" /></div>
      <div class="card"><LineChart title="磁盘趋势" :data="diskData" :x-min="xMin" :x-max="xMax" /></div>
    </div>
  </div>
</template>

<style scoped>
.grid-4 { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 10px; margin-bottom: 4px; }
.range-bar { display: flex; align-items: center; gap: 6px; margin-top: 12px; }
.range-btn {
  padding: 4px 12px; border: 1px solid #E2E8F0; border-radius: 6px;
  background: white; font-size: 12px; color: var(--color-text-secondary);
  cursor: pointer; transition: all 0.15s;
}
.range-btn:hover { border-color: var(--color-primary); color: var(--color-primary); }
.range-btn.active { background: var(--color-primary); color: white; border-color: var(--color-primary); }

.charts-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-top: 10px; }
.charts-grid > * { min-width: 0; }
.card { background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius); padding: 12px 16px 8px; box-shadow: var(--card-shadow); overflow: hidden; }

@media (max-width: 768px) {
  .grid-4 { grid-template-columns: repeat(2, 1fr); gap: 8px; }
  .charts-grid { grid-template-columns: 1fr; }
}

@media (max-width: 480px) {
  .grid-4 { grid-template-columns: 1fr; gap: 8px; }
}
</style>
