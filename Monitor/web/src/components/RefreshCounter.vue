<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  seconds: number
  total?: number
}>()

const max = props.total ?? 10
const progress = computed(() => (max - props.seconds) / max)
const circumference = 2 * Math.PI * 14 // r=14
const offset = computed(() => circumference * (1 - progress.value))
</script>

<template>
  <div class="refresh-counter" :class="{ pulse: seconds <= 1 }" :title="`${seconds}s 后自动刷新`">
    <svg width="36" height="36" viewBox="0 0 36 36">
      <circle
        cx="18" cy="18" r="14"
        fill="none"
        stroke="currentColor"
        stroke-width="3"
        opacity="0.12"
      />
      <circle
        cx="18" cy="18" r="14"
        fill="none"
        stroke="currentColor"
        stroke-width="3"
        stroke-linecap="round"
        :stroke-dasharray="circumference"
        :stroke-dashoffset="offset"
        transform="rotate(-90 18 18)"
        class="ring"
      />
      <text
        x="18" y="19"
        text-anchor="middle"
        dominant-baseline="middle"
        class="count-text"
      >{{ seconds }}</text>
    </svg>
  </div>
</template>

<style scoped>
.refresh-counter {
  display: inline-flex;
  align-items: center;
  color: var(--color-text-secondary);
  transition: color 0.3s;
}
.ring {
  transition: stroke-dashoffset 0.9s linear;
}
.count-text {
  font-family: var(--font-mono);
  font-size: 12px;
  font-weight: 600;
  fill: currentColor;
}
.pulse {
  color: var(--color-primary);
}
.pulse .count-text {
  animation: pop 0.4s ease;
}
@keyframes pop {
  50% { font-size: 15px; }
}
</style>
