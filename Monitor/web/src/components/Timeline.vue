<script setup lang="ts">
defineProps<{
  items: Array<{ time: string; title: string; description?: string; color?: string }>
}>()
</script>

<template>
  <div class="timeline">
    <div v-for="(item, idx) in items" :key="idx" class="timeline-item">
      <div class="timeline-dot" :style="{ background: item.color ?? 'var(--color-primary)' }"></div>
      <div class="timeline-content">
        <div class="timeline-time">{{ item.time }}</div>
        <div class="timeline-title">{{ item.title }}</div>
        <div v-if="item.description" class="timeline-desc">{{ item.description }}</div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.timeline { position: relative; padding-left: 20px; }
.timeline::before {
  content: '';
  position: absolute;
  left: 7px; top: 4px; bottom: 4px;
  width: 2px;
  background: #E2E8F0;
}
.timeline-item { position: relative; padding-bottom: 20px; }
.timeline-dot {
  position: absolute; left: -16px; top: 4px;
  width: 10px; height: 10px; border-radius: 50%;
  border: 2px solid white;
  box-shadow: 0 0 0 2px #E2E8F0;
}
.timeline-time { font-size: 12px; color: var(--color-text-secondary); font-family: var(--font-mono); }
.timeline-title { font-size: 14px; font-weight: 600; color: var(--color-text); margin: 2px 0; }
.timeline-desc { font-size: 13px; color: var(--color-text-secondary); }
</style>
