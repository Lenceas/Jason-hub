import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { ServerMetrics, Site, ContainerSnapshot, HealthRecord, AlertRule, AlertEvent } from '../types'
import * as api from '../api'

export const useMonitorStore = defineStore('monitor', () => {
  const metrics = ref<ServerMetrics | null>(null)
  const metricsHistory = ref<ServerMetrics[]>([])
  const sites = ref<Site[]>([])
  const containers = ref<ContainerSnapshot[]>([])
  const healthRecords = ref<HealthRecord[]>([])
  const alertRules = ref<AlertRule[]>([])
  const alertEvents = ref<AlertEvent[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const nextRefresh = ref(10)
  const historyRange = ref(1)

  let timer: ReturnType<typeof setInterval> | null = null

  async function loadMetrics() {
    const result = await api.fetchLatestMetrics()
    if (result !== null) metrics.value = result
  }

  /** 全量加载历史（切换范围时用） */
  async function loadMetricsHistory(range?: number) {
    const r = range ?? historyRange.value
    try {
      const res = await api.fetchMetricsHistory(r)
      if (res && res.metrics.length) {
        const filtered = res.metrics.filter(m => m.cpu_pct != null || m.mem_pct != null)
        if (filtered.length > 0) {
          metricsHistory.value = filtered
          historyRange.value = r
        }
      }
    } catch {
      // API 失败时保留旧数据
    }
  }

  async function loadSites() {
    const result = await api.fetchSites()
    if (result !== null) sites.value = result
  }

  async function loadContainers() {
    const result = await api.fetchContainers()
    if (result !== null) containers.value = result
  }

  async function loadHealth() {
    const result = await api.fetchHealthRecords()
    if (result !== null) healthRecords.value = result
  }

  async function loadAlertRules() {
    const result = await api.fetchAlertRules()
    if (result !== null) alertRules.value = result
  }

  async function loadAlertEvents(limit = 50) {
    const result = await api.fetchAlertEvents(limit)
    if (result !== null) alertEvents.value = result
  }

  async function refreshAll() {
    loading.value = true
    error.value = null
    try {
      await Promise.allSettled([
        loadMetrics(),
        loadSites(),
        loadContainers(),
        loadHealth(),
        loadAlertRules(),
        loadAlertEvents(),
      ])
    } catch {
      error.value = '数据加载失败'
    } finally {
      loading.value = false
    }
  }

  const COUNT = 10

  let trendTick = 0

  function startAutoRefresh(_intervalMs = 10000) {
    stopAutoRefresh()
    Promise.all([refreshAll(), loadMetricsHistory()])
    nextRefresh.value = COUNT
    trendTick = 0

    timer = setInterval(() => {
      if (nextRefresh.value > 1) {
        nextRefresh.value--
      } else {
        refreshAll()
        // 每 30s（3 个周期）全量刷新趋势
        if (++trendTick % 3 === 0) loadMetricsHistory()
        nextRefresh.value = COUNT
      }
    }, 1000)
  }

  function stopAutoRefresh() {
    if (timer) {
      clearInterval(timer)
      timer = null
    }
  }

  return {
    metrics, metricsHistory, sites, containers, healthRecords,
    alertRules, alertEvents, loading, error, nextRefresh, historyRange,
    loadMetrics, loadMetricsHistory, loadSites, loadContainers,
    loadHealth, loadAlertRules, loadAlertEvents,
    refreshAll, startAutoRefresh, stopAutoRefresh,
  }
})
