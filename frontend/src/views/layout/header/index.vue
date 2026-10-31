<template>
  <div
    class="ape-volo-content layout-header"
    :class="{ 'is-semi-dark': config.semiDarkHeader }"
  >
    <div class="header-leading flex items-center min-w-0 flex-1">
      <AppBrand v-if="isMobile || !showSidebarToggle" :collapsed="isMobile" />

      <el-tooltip
        v-if="showSidebarToggle"
        :content="
          sidebarCollapsed
            ? '展开侧栏'
            : '收起侧栏'
        "
        placement="bottom"
      >
        <span class="inline-flex flex-shrink-0 mx-2">
          <button
            type="button"
            class="sidebar-toggle"
            :aria-label="
              sidebarCollapsed
                ? '展开侧栏'
                : '收起侧栏'
            "
            :aria-expanded="!sidebarCollapsed"
            aria-controls="layout-sidebar"
            @click="appStore.toggleSidebar"
            @keydown.enter.stop.prevent="appStore.toggleSidebar"
            @keydown.space.stop.prevent="appStore.toggleSidebar"
          >
            <el-icon :size="20">
              <AppIcon v-if="sidebarCollapsed" icon="lucide:panel-left-open" />
              <AppIcon v-else icon="lucide:panel-left-close" />
            </el-icon>
          </button>
        </span>
      </el-tooltip>

      <el-breadcrumb
        v-show="!isMobile"
        v-if="!showTopMenu && config.showBreadcrumb"
        :separator-icon="createIconifyIcon('lucide:chevron-right')"
        class="header-breadcrumb ml-2 min-w-0"
      >
        <el-breadcrumb-item
          v-for="item in matched.slice(1, matched.length)"
          :key="item.path"
        >
          <span class="breadcrumb-entry">
            <el-icon v-if="config.showBreadcrumbIcon && item.meta.icon"
            ><AppIcon :icon="String(item.meta.icon || '')"
            /></el-icon>
            {{ getItemTitle(item) }}
          </span>
        </el-breadcrumb-item>
      </el-breadcrumb>
      <Aside
        v-if="showTopMenu"
        mode="TopNav"
        class="flex-1 min-w-0 h-full ml-2"
      />
    </div>

    <div class="header-actions">
      <tools class="header-tools">
        <template #user-menu="{ items, activate }">
          <UserMenu :items="items" :activate="activate" />
        </template>
      </tools>
    </div>
  </div>
</template>

<script setup lang="ts">
  /**
   * 顶部栏页面。
   */
  import { createIconifyIcon } from '@/components/icons'
  import AppIcon from '@/components/AppIcon.vue'
  import { menuTitle } from '@/utils/menuTitle'
  import tools from './Tools.vue'
  import UserMenu from './UserMenu.vue'
  import { useRoute } from 'vue-router'
  import { useAppStore } from '@/pinia'
  import { storeToRefs } from 'pinia'
  import { computed } from 'vue'
  import { fmtTitle } from '@/utils/index'
  import Aside from '@/views/layout/navigation/index.vue'
  import {
    hasHeaderNavigation,
    hasSidebarNavigation
  } from '@/enums/MenuNavigationType'
  import AppBrand from '../components/AppBrand.vue'
  import type { RouteRecordNormalized } from 'vue-router'
  const route = useRoute()
  const appStore = useAppStore()
  const { device, config, sidebarCollapsed } = storeToRefs(appStore)
  const isMobile = computed(() => {
    return device.value === 'mobile'
  })
  const showSidebarToggle = computed(
    () => isMobile.value || hasSidebarNavigation(config.value.sideMode)
  )
  const showTopMenu = computed(
    () => !isMobile.value && hasHeaderNavigation(config.value.sideMode)
  )
  const matched = computed(
    () => (route.meta.matched || []) as RouteRecordNormalized[]
  )
  /**
   * 取得当前图标或菜单项的展示名称。
   * @param item 当前待处理的候选项或记录。
   * @returns 当前路由语境中的菜单显示标题。
   */
  const getItemTitle = (item: RouteRecordNormalized) => {
    return fmtTitle(String(menuTitle(item.meta) || ''), route)
  }
</script>

<style scoped>
  .layout-header {
    position: relative;
    flex: none;
    z-index: 10;
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: var(--app-header-height);
    padding-inline: var(--app-space-2);
    border-bottom: 1px solid var(--app-border);
  }

  .layout-header.is-semi-dark {
    --app-surface: #18212c;
    --app-surface-subtle: #253140;
    --app-border: #344252;
    --app-border-strong: #46576a;
    --app-text-primary: #f4f7fa;
    --app-text-secondary: #c4ced8;
    --app-text-muted: #91a0af;
    --el-bg-color: var(--app-surface);
    --el-fill-color-light: var(--app-surface-subtle);
    --el-text-color-primary: var(--app-text-primary);
    --el-text-color-regular: var(--app-text-secondary);
    --el-border-color: var(--app-border);
  }

  .header-leading {
    height: 100%;
  }

  .header-actions {
    display: flex;
    flex: 0 1 auto;
    align-items: center;
    gap: var(--app-space-2);
    min-width: 0;
    max-width: 100%;
    margin-left: var(--app-space-2);
  }

  .header-breadcrumb {
    overflow: hidden;
    white-space: nowrap;
  }

  .breadcrumb-entry {
    display: inline-flex;
    align-items: center;
    gap: 6px;
  }

  .sidebar-toggle {
    display: inline-flex;
    flex: none;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    padding: 0;
    border: 0;
    border-radius: var(--ape-volo-radius);
    color: var(--el-text-color-primary);
    background: transparent;
    cursor: pointer;
  }

  .sidebar-toggle:hover {
    color: var(--el-color-primary);
    background: var(--el-fill-color-light);
  }

  .sidebar-toggle:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 2px;
  }

  @media (max-width: 991px) {
    .header-leading {
      flex: none;
    }

    .header-actions {
      flex: 1;
      min-width: 0;
      justify-content: flex-end;
    }

    .header-tools {
      flex: 0 1 auto;
      min-width: 0;
      margin: 0;
      gap: 8px;
      padding: 4px;
    }

    .header-actions :deep(.user-menu-trigger) {
      flex: none;
    }
  }
</style>
