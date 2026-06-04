<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, type Ref } from 'vue'
import * as echarts from 'echarts'

const props = defineProps<{
  title?: string
  xData?: string[]
  series: Array<{ name: string; data: number[]; color?: string }>
  height?: number
  unit?: string
}>()

const chartRef: Ref<HTMLDivElement | null> = ref(null)
let instance: echarts.ECharts | null = null

function render() {
  if (!chartRef.value) return
  instance ??= echarts.init(chartRef.value)
  instance.setOption({
    tooltip: {
      trigger: 'axis',
      valueFormatter: (v: number) => `${v}${props.unit ?? ''}`,
    },
    legend: { bottom: 0, textStyle: { fontSize: 12 } },
    grid: { left: 50, right: 16, top: 8, bottom: 32 },
    xAxis: {
      type: 'category',
      data: props.xData ?? [],
      axisLabel: { fontSize: 11, color: '#94A3B8' },
      axisLine: { show: false },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value',
      splitLine: { lineStyle: { color: '#F1F5F9' } },
      axisLabel: { fontSize: 11, color: '#94A3B8' },
    },
    series: props.series.map(s => ({
      name: s.name,
      type: 'line',
      data: s.data,
      smooth: true,
      symbol: 'none',
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

watch(() => [props.xData, props.series], render, { deep: true })

onMounted(() => {
  render()
  window.addEventListener('resize', () => instance?.resize())
})

onUnmounted(() => {
  instance?.dispose()
})
</script>

<template>
  <div class="chart-wrapper">
    <div v-if="title" class="chart-title">{{ title }}</div>
    <div ref="chartRef" :style="{ height: height ? height + 'px' : '240px' }"></div>
  </div>
</template>

<style scoped>
.chart-wrapper { width: 100%; }
.chart-title { font-size: 14px; font-weight: 600; color: var(--color-text); margin-bottom: 8px; }
</style>
