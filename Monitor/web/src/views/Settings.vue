<script setup lang="ts">
// Settings 页面 — 通知渠道配置（TODO）
</script>

<template>
  <div class="empty-state">
    <div class="empty-icon">⚙️</div>
    <h3>通知渠道配置</h3>
    <p>通知渠道配置功能开发中，后续将支持邮件、企业微信、钉钉等告警通知方式。</p>
  </div>
</template>

<style scoped>
.empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 80px 20px; background: white; border: 1px solid #E2E8F0; border-radius: var(--card-radius); }
.empty-icon { font-size: 48px; margin-bottom: 16px; }
h3 { font-size: 18px; font-weight: 700; margin-bottom: 8px; }
p { color: var(--color-text-secondary); max-width: 400px; text-align: center; }
</style>
