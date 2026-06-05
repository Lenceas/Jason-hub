<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, type Ref } from 'vue'
import * as echarts from 'echarts'

const DEFAULT_HEIGHT = 260

const props = defineProps<{
  title?: string
  data: Array<{ name: string; values: [number, number][]; color?: string }>
  xMin?: number
  xMax?: number
  height?: number
  unit?: string
}>()

const chartRef: Ref<HTMLDivElement | null> = ref(null)
let instance: echarts.ECharts | null = null
let isDisposed = false

function render() {
  if (!chartRef.value || isDisposed) return
  instance ??= echarts.init(chartRef.value)
  instance.setOption({
    tooltip: {
      trigger: 'axis',
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      formatter: (p: any[]) => {
        const u = props.unit ?? ''
        const t = p[0]?.data?.[0]
        const header = t ? new Date(t).toLocaleTimeString() : ''
        const items = p.map(item =>
          `<span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:${item.color};margin-right:6px;"></span>` +
          `${item.seriesName}: <b>${item.data[1]}</b>` +
          `<span style="color:#94A3B8;margin-left:2px;">${u}</span>`
        ).join('<br/>')
        return `${header}<br/>${items}`
      },
    },
    legend: { bottom: -2, textStyle: { fontSize: 12 } },
    grid: { left: 38, right: 8, top: 4, bottom: 44 },
    xAxis: {
      type: 'time',
      min: props.xMin,
      max: props.xMax,
      axisLabel: { fontSize: 11, color: '#94A3B8' },
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: { show: false },
    },
    yAxis: {
      type: 'value',
      splitLine: { lineStyle: { color: '#F1F5F9' } },
      axisLabel: { fontSize: 11, color: '#94A3B8' },
    },
    animation: false,
    series: props.data.map(s => ({
      name: s.name,
      type: 'line',
      data: s.values,
      smooth: true,
      symbol: 'none',
      itemStyle: { color: s.color ?? '#3B82F6' },
      lineStyle: { width: 2, color: s.color ?? '#3B82F6' },
      areaStyle: {
        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: (s.color ?? '#3B82F6') + '30' },
          { offset: 1, color: (s.color ?? '#3B82F6') + '05' },
        ]),
      },
    })),
  })
}

watch(() => [props.data, props.xMin, props.xMax], render, { deep: true })

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
  <div class="chart-wrapper">
    <div v-if="title" class="chart-title">{{ title }}</div>
    <div ref="chartRef" :style="{ height: height ? height + 'px' : DEFAULT_HEIGHT + 'px' }"></div>
  </div>
</template>

<style scoped>
.chart-wrapper { width: 100%; max-width: 100%; overflow: hidden; }
.chart-title { font-size: 14px; font-weight: 600; color: var(--color-text); margin-bottom: 8px; }
</style>
