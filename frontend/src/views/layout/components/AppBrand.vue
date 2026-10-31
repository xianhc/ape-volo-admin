<template>
  <router-link to="/" class="app-brand" :class="{ 'is-collapsed': collapsed }" :aria-label="setting.appName">
    <img :src="setting.appLogo" alt="" class="app-brand__logo" />
    <span v-if="!collapsed" class="app-brand__name">{{ setting.appName }}</span>
  </router-link>
</template>

<script setup lang="ts">
  /**
   * 侧栏品牌标识组件。
   */
  import setting from '@/setting'

  withDefaults(defineProps<{
    /** 侧栏折叠或保持展开的状态。 */
    collapsed?: boolean
  }>(), { collapsed: false })
</script>

<style scoped>
  .app-brand {
    display: flex;
    flex: none;
    align-items: center;
    gap: 10px;
    box-sizing: border-box;
    height: var(--app-header-height);
    min-width: 0;
    padding-inline: 16px;
    overflow: hidden;
    color: var(--app-text-primary);
    text-decoration: none;
  }

  .app-brand.is-collapsed {
    justify-content: center;
    padding-inline: 8px;
  }

  .app-brand__logo {
    flex: none;
    width: 32px;
    height: 32px;
    object-fit: contain;
  }

  .app-brand__name {
    overflow: hidden;
    font-size: 17px;
    font-weight: 600;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .app-brand:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: -2px;
  }
</style>
