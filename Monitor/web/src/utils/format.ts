/**
 * 格式化字节数为人类可读的字符串
 * @param bytes 字节数
 * @param decimals 保留小数位数（默认 2）
 */
export function formatBytes(bytes: number | null | undefined, decimals = 2): string {
  if (bytes === null || bytes === undefined) return '—'
  if (bytes === 0) return '0 B'

  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  const idx = Math.min(i, sizes.length - 1)

  return parseFloat((bytes / Math.pow(k, idx)).toFixed(decimals)) + ' ' + sizes[idx]
}
