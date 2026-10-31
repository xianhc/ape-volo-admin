<template>
  <div class="toolbar-host">
    <div class="header-tools-list">
      <el-tooltip v-for="item in headerItems" :key="item.key" :content="item.label" placement="bottom">
        <button type="button" class="header-tool-button" :class="{ 'header-tool-button--search': item.key === 'menuSearch' }" :aria-label="item.label" :disabled="item.disabled" @click="activate(item.key, $event)">
          <el-icon><AppIcon :icon="item.icon" :spin="item.busy" /></el-icon>
          <span v-if="item.key === 'menuSearch'" class="toolbar-search-label">{{ '搜索' }} <kbd>Ctrl K</kbd></span>
        </button>
      </el-tooltip>
    </div>
    <slot name="user-menu" :items="userItems" :activate="activate"></slot>
    <div class="toolbar-controllers">
      <MenuSearchDialog ref="menuSearch" :show-trigger="false" />
      <ScreenLockControl ref="screenLock" :show-trigger="false" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import AppIcon from '@/components/AppIcon.vue'
  import { useAppStore } from '@/pinia'
  import { computed, ref, onBeforeUnmount, onMounted } from 'vue'
  import { useFullscreen } from '@vueuse/core'
  import { emitter } from '@/utils/bus'
  import MenuSearchDialog from '@/components/MenuSearchDialog.vue'
  import ScreenLockControl from './ScreenLockControl.vue'
  import { useSignOut } from '@/composables/useSignOut'
  import { TOOLBAR_ITEMS } from '@/services/appearance/toolbar'
  import type { ToolbarItemKey } from '@/services/appearance/preferences.types'
  const app = useAppStore()
  const { signingOut, signOut } = useSignOut()
  const { isFullscreen, toggle: toggleFullscreen } = useFullscreen()
  const menuSearch = ref<InstanceType<typeof MenuSearchDialog>>()
  const screenLock = ref<InstanceType<typeof ScreenLockControl>>()
  const refreshing = ref(false)
  let refreshTimer: ReturnType<typeof setTimeout> | undefined

  const items = computed(() => app.config.toolbarOrder.map(key => {
    let label: string
    if (key === 'theme') {
      label = (app.isDark ? '切换为浅色' : '切换为深色')
    } else {
      if (key === 'fullscreen') {
        label = (isFullscreen.value ? '退出全屏' : '进入全屏')
      } else {
        label = TOOLBAR_ITEMS[key].label
      }
    }
    let icon: string
    if (key === 'theme') {
      if (app.isDark) {
        icon = 'lucide:sun'
      } else {
        icon = 'lucide:moon'
      }
    } else {
      if (key === 'fullscreen' && isFullscreen.value) {
        icon = 'lucide:minimize'
      } else {
        icon = TOOLBAR_ITEMS[key].icon
      }
    }
    return {
      key,
      label: label,
      icon: icon,
      busy: (key === 'refresh' && refreshing.value) || (key === 'logout' && signingOut.value),
      disabled: (key === 'refresh' && refreshing.value) ||
        (key === 'theme' && app.themeTransitioning) || (key === 'logout' && signingOut.value)
    }
  }))
  const headerItems = computed(() => items.value.filter(item => app.config.toolbarPlacements[item.key] === 'header'))
  const userItems = computed(() => items.value.filter(item => app.config.toolbarPlacements[item.key] === 'userDropdown'))

  /**
   * 执行选中工具项对应的搜索、主题、刷新、通知或会话动作。
   * @param key 当前查询、映射或缓存使用的键。
   * @param event 触发当前交互的事件。
   * @param anchor 浮层或主题动画的定位元素。
   */
  function activate(key: ToolbarItemKey, event?: Event): void {
    if (items.value.find(item => item.key === key)?.disabled) return
    switch (key) {
      case 'menuSearch':

        void menuSearch.value?.open()
        break
      case 'preferences':

        emitter.emit('openPreferences')
        break
      case 'theme':

        void app.toggleTheme(!app.isDark, event)
        break
      case 'fullscreen':

        void toggleFullscreen()
        break
      case 'refresh':


        refreshing.value = true
        emitter.emit('reload')
        refreshTimer = setTimeout(() => {
          refreshing.value = false
        }, 1000)
        break
      case 'lockScreen':

        screenLock.value?.open()
        break
      case 'logout':

        void signOut()
        break
    }
  }

  onMounted(() => emitter.on('requestSignOut', signOut))
  onBeforeUnmount(() => {
    if (refreshTimer) clearTimeout(refreshTimer)
    emitter.off('requestSignOut', signOut)
  })
</script>

<style scoped>
  .toolbar-host { display: flex; align-items: center; gap: var(--app-space-2); min-width: 0; max-width: 100%; }
  .toolbar-controllers { display: none; }
  .header-tools-list { display: flex; align-items: center; gap: var(--app-space-1); min-width: 0; overflow-x: auto; scrollbar-width: none; }
  .header-tools-list:empty { display: none; }
  .header-tools-list::-webkit-scrollbar { display: none; }
  .header-tool-button { flex: none; }
  /* Search includes text and a shortcut, so it needs more than an icon-button width. */
  .header-tool-button.header-tool-button--search {
    width: auto;
    gap: 10px;
    padding-inline: 12px;
    border-radius: 999px;
    background: var(--app-surface-subtle);
    color: var(--app-text-secondary);
    font-size: 14px;
  }
  .header-tool-button--search > .el-icon {
    flex: none;
  }
  .toolbar-search-label {
    display: inline-flex;
    flex: none;
    align-items: center;
    gap: 10px;
    white-space: nowrap;
  }
  .toolbar-search-label kbd {
    padding: 4px 7px;
    border-radius: 5px;
    background: var(--app-surface);
    color: var(--app-text-muted);
    font: inherit;
    font-size: 12px;
    line-height: 1;
  }
  @media (max-width: 1199px) {
    .toolbar-search-label { display: none; }
    .header-tool-button.header-tool-button--search {
      width: 36px;
      padding: 0;
      border-radius: 50%;
      background: transparent;
    }
  }
  @media (max-width: 991px) {
    .header-tools-list { gap: 2px; }
    .toolbar-host { gap: 4px; }
    .header-tool-button.header-tool-button--search { width: 32px; }
  }
</style>
