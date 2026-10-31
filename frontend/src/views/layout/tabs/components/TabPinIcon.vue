<template>
  <AppIcon :icon="slashed ? 'lucide:pin-off' : 'lucide:pin'" />
</template>
<script setup lang="ts">
  /**
   * 页签固定图标组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  withDefaults(defineProps<{
    /** 图标是否附加斜线标记。 */
    slashed?: boolean
  }>(), { slashed: false })
</script>
