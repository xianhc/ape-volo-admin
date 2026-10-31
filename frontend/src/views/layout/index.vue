<template>
  <div class="app-shell ape-volo-bg">
    <el-watermark
      v-if="config.showWatermark"
      :font="font"
      :z-index="9999"
      :gap="[180, 150]"
      class="!absolute !inset-0 !pointer-events-none"
      :content="userStore.userInfo.nickName"
    />
    <div class="layout-root">
      <Navigation
        v-if="showSidebar"
        v-show="
          !workspaceMaximized && (device !== 'mobile' || !sidebarCollapsed)
        "
        id="layout-sidebar"
        class="h-full flex-shrink-0"
      />
      <div class="layout-main">
        <Header v-if="!fullContent" v-show="!workspaceMaximized" />
        <Tabs v-if="!fullContent && (config.showTabs || workspaceMaximized)" />
        <div
          class="layout-content"
          :class="{ 'is-compact': config.contentMode === 'compact' }"
          :style="contentStyle"
        >
          <router-view v-slot="{ Component, route }">
            <div id="base-load-dom" class="layout-route">
              <transition
                mode="out-in"
                :name="transitionName"
                :css="!reloadingView"
              >
                <keep-alive :include="routerStore.keepAliveRouters">
                  <component
                    :is="Component"
                    v-if="reloadFlag"
                    :key="route.matched?.[1]?.name || route.name"
                  />
                </keep-alive>
              </transition>
            </div>
          </router-view>
        </div>
        <footer
          v-if="!fullContent && config.showFooter"
          v-show="!workspaceMaximized"
          class="layout-footer"
        >
          <AppFooter />
        </footer>
      </div>
      <SysSetting :show-trigger="fullContent" :class="{ 'floating-preferences': fullContent }" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import Navigation from '@/views/layout/navigation/index.vue'
  import Header from '@/views/layout/header/index.vue'
  import {
    MenuNavigationType,
    hasSidebarNavigation
  } from '@/enums/MenuNavigationType'
  import useResponsive from '@/composables/useResponsive'
  import Tabs from './tabs/index.vue'
  import AppFooter from '@/components/AppFooter.vue'
  import SysSetting from '@/views/layout/setting/index.vue'
  import { emitter } from '@/utils/bus.js'
  import { computed, onBeforeUnmount, onMounted, nextTick, ref } from 'vue'
  import { useRoute } from 'vue-router'
  import { useRouterStore } from '@/pinia/modules/router'
  import { useUserStore } from '@/pinia/modules/user'
  import { useAppStore } from '@/pinia'
  import { storeToRefs } from 'pinia'
  import '@/style/transition.scss'
  const appStore = useAppStore()
  const { config, isDark, device, sidebarCollapsed, workspaceMaximized } =
    storeToRefs(appStore)

  defineOptions({
    name: 'Layout'
  })

  useResponsive(true)

  const currentRoute = useRoute()
  const routerStore = useRouterStore()
  const userStore = useUserStore()

  const font = computed(() => ({
    color: isDark.value ? 'rgba(255,255,255, .15)' : 'rgba(0, 0, 0, .15)'
  }))

  const transitionName = computed<string>(() => {
    const meta = currentRoute.meta as {
      /** 页面切换动画类型。 */
      transitionType?: string
    }
    return meta.transitionType ?? config.value.transitionType
  })
  const fullContent = computed(
    () =>
      device.value !== 'mobile' &&
      config.value.sideMode === MenuNavigationType.FullContent
  )
  const showSidebar = computed(
    () =>
      !fullContent.value &&
      (device.value === 'mobile' || hasSidebarNavigation(config.value.sideMode))
  )
  const contentStyle = computed(() =>
    config.value.contentMode === 'compact'
      ? { '--app-content-width': `${config.value.contentWidth}px` }
      : undefined
  )

  onMounted(() => {
    // 挂载一些通用的事件
    emitter.on('reload', reload)
  })

  onBeforeUnmount(() => {
    workspaceMaximized.value = false
    emitter.off('reload', reload)
    if (reloadTimer) {
      window.clearTimeout(reloadTimer)
      reloadTimer = null
    }
  })

  const reloadFlag = ref(true)
  const reloadingView = ref(false)
  let reloadTimer: number | null = null
  /**
   * 重新加载布局菜单当前上下文的数据。
   */
  const reload = async () => {
    if (reloadTimer) {
      window.clearTimeout(reloadTimer)
    }
    const requestedPath = currentRoute.fullPath
    reloadTimer = window.setTimeout(async () => {
      reloadTimer = null
      if (currentRoute.fullPath !== requestedPath) return
      const cacheName = routerStore.getKeepAliveComponentName(
        String(currentRoute.name ?? '')
      )
      const cached =
        cacheName && routerStore.keepAliveRouters.includes(cacheName)
      const restoreCache = cached && cacheName ? routerStore.suspendCache(cacheName) : () => {}
      reloadingView.value = true
      try {
        // 只移除当前页面的缓存，保留其他打开页面及其编辑状态。
        reloadFlag.value = false
        await nextTick()
        reloadFlag.value = true
        await nextTick()
      } finally {
        reloadFlag.value = true
        restoreCache()
        reloadingView.value = false
      }
    }, 400)
  }
</script>

<style scoped>
  .app-shell {
    width: 100vw;
    height: 100vh;
    overflow: hidden;
  }

  .layout-root {
    position: relative;
    display: flex;
    box-sizing: border-box;
    width: 100%;
    height: 100%;
    min-height: 0;
  }

  .layout-content.is-compact :deep(.layout-route) {
    width: min(100%, var(--app-content-width));
    margin-inline: auto;
  }

  .floating-preferences {
    position: absolute;
    z-index: 30;
    top: 50%;
    right: 0;
    transform: translateY(-50%);
    padding: 4px;
    border: 1px solid var(--app-border);
    border-right: 0;
    border-radius: var(--ape-volo-radius) 0 0 var(--ape-volo-radius);
    background: var(--app-surface);
    box-shadow: var(--app-shadow-overlay);
  }

  @media (max-width: 991px) {
    #layout-sidebar {
      position: absolute;
      z-index: 20;
      top: var(--app-header-height);
      left: 0;
      height: calc(100% - var(--app-header-height));
      box-shadow: var(--app-shadow-overlay);
    }
  }
</style>
