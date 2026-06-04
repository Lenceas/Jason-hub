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

  let refreshTimer: ReturnType<typeof setInterval> | null = null

  async function loadMetrics() {
    metrics.value = await api.fetchLatestMetrics()
  }

  async function loadMetricsHistory(range = 24) {
    const res = await api.fetchMetricsHistory(range)
    metricsHistory.value = res.metrics
  }

  async function loadSites() {
    sites.value = await api.fetchSites()
  }

  async function loadContainers() {
    containers.value = await api.fetchContainers()
  }

  async function loadHealth() {
    healthRecords.value = await api.fetchHealthRecords()
  }

  async function loadAlertRules() {
    alertRules.value = await api.fetchAlertRules()
  }

  async function loadAlertEvents(limit = 50) {
    alertEvents.value = await api.fetchAlertEvents(limit)
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
    } catch (e) {
      error.value = '数据加载失败'
    } finally {
      loading.value = false
    }
  }

  function startAutoRefresh(intervalMs = 10000) {
    stopAutoRefresh()
    refreshAll()
    refreshTimer = setInterval(refreshAll, intervalMs)
  }

  function stopAutoRefresh() {
    if (refreshTimer) {
      clearInterval(refreshTimer)
      refreshTimer = null
    }
  }

  return {
    metrics, metricsHistory, sites, containers, healthRecords,
    alertRules, alertEvents, loading, error,
    loadMetrics, loadMetricsHistory, loadSites, loadContainers,
    loadHealth, loadAlertRules, loadAlertEvents,
    refreshAll, startAutoRefresh, stopAutoRefresh,
  }
})
