<template>
  <div class="sidebar-footer" :class="{ 'is-collapsed': sidebarCollapsed, 'is-mobile': device === 'mobile' }">
    <button
      type="button"
      class="sidebar-footer__button"
      :title="sidebarCollapsed ? '展开侧栏' : '收起侧栏'"
      :aria-label="sidebarCollapsed ? '展开侧栏' : '收起侧栏'"
      :aria-expanded="!sidebarCollapsed"
      aria-controls="layout-sidebar"
      @click="appStore.toggleSidebar"
    >
      <el-icon :size="18">
        <AppIcon v-if="sidebarCollapsed" icon="lucide:chevrons-right" />
        <AppIcon v-else icon="lucide:chevrons-left" />
      </el-icon>
    </button>
    <button
      v-if="device !== 'mobile'"
      type="button"
      class="sidebar-footer__button"
      :class="{ 'is-pinned': sidebarPinned }"
      :title="pinLabel"
      :aria-label="pinLabel"
      :aria-pressed="sidebarPinned"
      @click="appStore.toggleSidebarPinned"
    >
      <el-icon :size="18">
        <AppIcon :icon="sidebarPinned ? 'lucide:pin' : 'lucide:pin-off'" />
      </el-icon>
    </button>
  </div>
</template>

<script setup lang="ts">
  /**
   * 侧栏底部按钮组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { computed } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useAppStore } from '@/pinia'
  const appStore = useAppStore()
  const { device, sidebarCollapsed, sidebarPinned } = storeToRefs(appStore)
  const pinLabel = computed(() => sidebarPinned.value
    ? '启用悬停展开（移入展开，移出收起）'
    : '固定侧栏')
</script>

<style scoped>
  .sidebar-footer {
    position: relative;
    flex: none;
    height: 56px;
    box-sizing: border-box;
    border-top: 1px solid var(--el-border-color-lighter);
    transition: height var(--sidebar-motion-duration) var(--sidebar-motion-easing);
  }

  .sidebar-footer.is-collapsed:not(.is-mobile) {
    height: 88px;
  }

  .sidebar-footer__button:first-child {
    top: 12px;
    left: 8px;
  }

  .sidebar-footer__button:nth-child(2) {
    right: 8px;
    bottom: 12px;
  }

  .sidebar-footer.is-collapsed:not(.is-mobile) .sidebar-footer__button:first-child {
    top: 8px;
  }

  .sidebar-footer.is-collapsed .sidebar-footer__button:first-child {
    left: calc(50% - 16px);
  }

  .sidebar-footer.is-collapsed .sidebar-footer__button:nth-child(2) {
    bottom: 8px;
    right: calc(50% - 16px);
  }

  .sidebar-footer__button {
    position: absolute;
    display: inline-flex;
    flex: none;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    padding: 0;
    border: 1px solid var(--el-border-color-lighter);
    border-radius: var(--ape-volo-radius);
    color: var(--el-text-color-regular);
    background: var(--el-fill-color-light);
    cursor: pointer;
    transition:
      top var(--sidebar-motion-duration) var(--sidebar-motion-easing),
      bottom var(--sidebar-motion-duration) var(--sidebar-motion-easing),
      left var(--sidebar-motion-duration) var(--sidebar-motion-easing),
      right var(--sidebar-motion-duration) var(--sidebar-motion-easing);
  }

  .sidebar-footer__button:hover,
  .sidebar-footer__button.is-pinned {
    color: var(--el-color-primary);
    border-color: var(--el-color-primary-light-5);
  }

  .sidebar-footer__button:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 2px;
  }
  @media (prefers-reduced-motion: reduce) {
    .sidebar-footer,
    .sidebar-footer__button {
      transition: none;
    }
  }
</style>
