<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

const Y = ref(''), M = ref(''), D = ref('')
const H = ref(''), m = ref(''), S = ref('')
const W = ref('')

const WDAYS = ['周日', '周一', '周二', '周三', '周四', '周五', '周六']

let timer: ReturnType<typeof setInterval> | null = null
let prevS = ''

const pad = (n: number) => String(n).padStart(2, '0')

function tick() {
  const now = new Date()
  W.value = WDAYS[now.getDay()]
  Y.value = String(now.getFullYear())
  M.value = pad(now.getMonth() + 1)
  D.value = pad(now.getDate())
  H.value = pad(now.getHours())
  m.value = pad(now.getMinutes())
  const s = pad(now.getSeconds())
  if (s !== prevS) {
    // 触发翻牌动画：先清空再赋值
    S.value = ''
    requestAnimationFrame(() => { S.value = s })
    prevS = s
  } else {
    S.value = s
  }
}

onMounted(() => { tick(); timer = setInterval(tick, 1000) })
onUnmounted(() => { if (timer) clearInterval(timer) })
</script>

<template>
  <div class="flip-clock">
    <div class="flip-group">
      <div class="flip-card">{{ Y }}</div>
    </div>
    <span class="flip-sep">-</span>
    <div class="flip-group">
      <div class="flip-card">{{ M }}</div>
    </div>
    <span class="flip-sep">-</span>
    <div class="flip-group">
      <div class="flip-card">{{ D }}</div>
    </div>
    <span class="flip-sep-v" />
    <div class="flip-group">
      <div class="flip-card">{{ H }}</div>
    </div>
    <span class="flip-sep pulse">:</span>
    <div class="flip-group">
      <div class="flip-card">{{ m }}</div>
    </div>
    <span class="flip-sep pulse">:</span>
    <div class="flip-group">
      <div class="flip-card second" :key="S">{{ S }}</div>
    </div>
    <span class="flip-sep-v" />
    <div class="flip-card weekday">{{ W }}</div>
  </div>
</template>

<style scoped>
.flip-clock {
  display: flex; align-items: center; gap: 3px;
  padding: 8px 0;
  user-select: none;
}

.flip-card.weekday {
  min-width: 38px; font-size: 15px;
}

.flip-group {
  display: flex; gap: 0;
}

.flip-card {
  display: flex; align-items: center; justify-content: center;
  min-width: 26px; height: 34px;
  background: #F1F5F9;
  color: #1E293B;
  border: 1px solid #E2E8F0;
  padding: 0 3px;
  font-family: var(--font-mono);
  font-size: 17px; font-weight: 700;
  border-radius: 4px;
  transition: transform 0.15s ease;
}

.flip-card.second {
  animation: flipIn 0.3s ease;
}

@keyframes flipIn {
  0%   { transform: rotateX(90deg); opacity: 0; }
  100% { transform: rotateX(0deg); opacity: 1; }
}

.flip-sep {
  color: #94A3B8; font-size: 14px; font-weight: 600;
}
.flip-sep-v {
  width: 8px;
}
.flip-sep.pulse {
  animation: pulse 1s step-end infinite;
}
@keyframes pulse {
  50% { opacity: 0.3; }
}
</style>
