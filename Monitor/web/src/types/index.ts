export interface ServerMetrics {
  cpu_pct: number | null
  mem_pct: number | null
  mem_used: number | null
  mem_total: number | null
  disk_pct: number | null
  disk_used: number | null
  disk_total: number | null
  net_in: number | null
  net_out: number | null
  load_1m: number | null
  load_5m: number | null
  load_15m: number | null
  ts: string
}

export interface Site {
  id: number
  name: string
  url: string
  interval_sec: number
  timeout_ms: number
  is_active: boolean
  created_at: string
}

export interface UptimeRecord {
  id: number
  status_code: number | null
  response_ms: number | null
  is_ok: boolean
  checked_at: string
}

export interface UptimeHistory {
  site_id: number
  uptime_pct: number
  total_checks: number
  ok_checks: number
  records: UptimeRecord[]
}

export interface ContainerSnapshot {
  name: string
  status: string | null
  cpu_pct: number | null
  mem_usage: number | null
  mem_limit: number | null
  ts: string
}

export interface HealthRecord {
  service: string
  status: string | null
  latency_ms: number | null
  ts: string
}

export interface AlertRule {
  id: number
  name: string
  metric: string
  operator: string
  threshold: number
  duration_sec: number
  enabled: boolean
  created_at: string
}

export interface AlertEvent {
  id: number
  rule_name: string | null
  triggered_at: string
  resolved_at: string | null
  message: string | null
  severity: string
}
