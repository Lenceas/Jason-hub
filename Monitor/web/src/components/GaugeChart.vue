<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, type Ref } from 'vue'
import * as echarts from 'echarts'

const props = defineProps<{
  title?: string
  value?: number | null
  max?: number
  color?: string
}>()

const chartRef: Ref<HTMLDivElement | null> = ref(null)
let instance: echarts.ECharts | null = null
let isDisposed = false

function render() {
  if (!chartRef.value || isDisposed) return
  instance ??= echarts.init(chartRef.value)
  const pct = props.value ?? 0
  const max = props.max ?? 100
  instance.setOption({
    series: [{
      type: 'gauge',
      startAngle: 220,
      endAngle: -40,
      center: ['50%', '55%'],
      radius: '90%',
      min: 0,
      max,
      progress: { show: true, width: 8, itemStyle: { color: props.color ?? '#3B82F6' } },
      axisLine: { lineStyle: { width: 8, color: [[1, '#E2E8F0']] } },
      axisTick: { show: false },
      splitLine: { show: false },
      axisLabel: { show: false },
      detail: {
        fontSize: 20,
        fontWeight: 700,
        color: '#1E293B',
        offsetCenter: [0, '40%'],
        formatter: `{value}%`,
      },
      data: [{ value: Math.round(pct * 10) / 10 }],
    }],
  })
}

watch(() => props.value, render)

onMounted(() => {
  render()
  window.addEventListener('resize', () => instance?.resize())
})

onUnmounted(() => {
  isDisposed = true
  instance?.dispose()
  instance = null
})
</script>

<template>
  <div class="gauge-wrapper">
    <div v-if="title" class="gauge-title">{{ title }}</div>
    <div ref="chartRef" style="height:160px"></div>
  </div>
</template>

<style scoped>
.gauge-wrapper { width: 100%; }
.gauge-title { font-size: 14px; font-weight: 600; color: var(--color-text); text-align: center; }
</style>
