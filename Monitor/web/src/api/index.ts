import axios from 'axios'
import type {
  ServerMetrics,
  Site,
  UptimeHistory,
  ContainerSnapshot,
  HealthRecord,
  AlertRule,
  AlertEvent
} from '../types'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE ?? '/api/v1',
  timeout: 10000,
})

// ======== 服务器监控 ========

export async function fetchLatestMetrics(): Promise<ServerMetrics | null> {
  try {
    const { data } = await api.get<ServerMetrics>('/server/metrics')
    return data
  } catch {
    return null
  }
}

export async function fetchMetricsHistory(range = 24): Promise<{ range_hours: number; metrics: ServerMetrics[] }> {
  const { data } = await api.get('/server/history', { params: { range } })
  return data
}

// ======== 站点监控 ========

export async function fetchSites(): Promise<Site[]> {
  const { data } = await api.get<Site[]>('/uptime/sites')
  return data
}

export async function createSite(payload: { name: string; url: string }): Promise<Site> {
  const { data } = await api.post<Site>('/uptime/sites', payload)
  return data
}

export async function deleteSite(id: number): Promise<void> {
  await api.delete(`/uptime/sites/${id}`)
}

export async function fetchSiteUptime(siteId: number): Promise<UptimeHistory> {
  const { data } = await api.get<UptimeHistory>(`/uptime/sites/${siteId}/history`)
  return data
}

// ======== 容器 ========

export async function fetchContainers(): Promise<ContainerSnapshot[]> {
  const { data } = await api.get<ContainerSnapshot[]>('/docker/containers')
  return data
}

// ======== 健康检查 ========

export async function fetchHealthRecords(): Promise<HealthRecord[]> {
  const { data } = await api.get<HealthRecord[]>('/health/services')
  return data
}

// ======== 告警规则 ========

export async function fetchAlertRules(): Promise<AlertRule[]> {
  const { data } = await api.get<AlertRule[]>('/alerts/rules')
  return data
}

export async function createAlertRule(payload: {
  name: string; metric: string; operator: string; threshold: number
}): Promise<AlertRule> {
  const { data } = await api.post<AlertRule>('/alerts/rules', payload)
  return data
}

export async function deleteAlertRule(id: number): Promise<void> {
  await api.delete(`/alerts/rules/${id}`)
}

// ======== 告警事件 ========

export async function fetchAlertEvents(limit = 50): Promise<AlertEvent[]> {
  const { data } = await api.get<AlertEvent[]>('/alerts/history', { params: { limit } })
  return data
}
