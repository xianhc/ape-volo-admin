<template>
  <div
    :class="{
      'navigation-shell': isSidebar,
      'navigation-shell--floating': isSidebar && sidebarAutoHide
    }"
    :style="isSidebar ? { width: sidebarReservedWidth + 'px' } : undefined"
    @pointerover="handlePointerEnter"
    @pointerleave="handlePointerLeave"
    @focusin="handleFocusIn"
    @focusout="handleFocusOut"
  >
    <div
      class="navigation-panel h-full"
      :class="{
        'navigation-panel--floating': isSidebar && sidebarAutoHide,
        'is-semi-dark': isSidebar && config.semiDarkSidebar
      }"
      :style="isSidebar ? { width: panelWidth + 'px' } : undefined"
    >
      <AppBrand
        v-if="isSidebar && !isMobile"
        :collapsed="sidebarCollapsed"
        class="navigation-brand"
      />
      <div class="navigation-menu">
        <SidebarNav v-if="showSidebarNav" />
        <TopNav v-if="showTopNav" />
        <DualSidebarNav v-if="showDualSidebarNav" />
        <CombinationNav v-if="showCombinationNav" :mode="mode" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import SidebarNav from './SidebarNav.vue'
  import TopNav from './TopNav.vue'
  import DualSidebarNav from './DualSidebarNav.vue'
  import CombinationNav from './CombinationNav.vue'
  import AppBrand from '../components/AppBrand.vue'
  import {
    MenuNavigationType,
    hasDualSidebarNavigation
  } from '@/enums/MenuNavigationType'

  import { storeToRefs } from 'pinia'
  import { useAppStore } from '@/pinia'
  import { computed } from 'vue'
  const { mode = 'SidebarNav' } = defineProps<{
    /** Redis 或日志队列当前运行模式。 */
    mode?: 'SidebarNav' | 'TopNav'
  }>()
  const appStore = useAppStore()
  const {
    config,
    device,
    sidebarCollapsed,
    sidebarAutoHide,
    sidebarReservedWidth
  } = storeToRefs(appStore)

  const isSidebar = computed(() => mode === 'SidebarNav')
  /**
   * 更新指针进入控件时的交互状态。
   * @param event 触发当前交互的事件。
   */
  const handlePointerEnter = (event: PointerEvent) => {
    // 底部按钮不触发悬停展开，避免按钮在点击前移动。
    if (
      event.target instanceof Element &&
      event.target.closest('.sidebar-footer')
    )
      return
    if (
      isSidebar.value &&
      sidebarAutoHide.value &&
      event.pointerType === 'mouse'
    ) {
      appStore.setSidebarHover(true)
    }
  }
  /**
   * 结束控件的指针悬浮状态。
   * @param event 触发当前交互的事件。
   */
  const handlePointerLeave = (event: PointerEvent) => {
    if (isSidebar.value && event.pointerType === 'mouse')
      appStore.setSidebarHover(false)
  }
  /**
   * 更新控件内部焦点状态。
   * @param event 触发当前交互的事件。
   */
  const handleFocusIn = (event: FocusEvent) => {
    if (isSidebar.value && event.target instanceof Element) {
      appStore.setSidebarFocus(event.target.matches(':focus-visible'))
    }
  }
  /**
   * 更新控件内部焦点状态。
   * @param event 触发当前交互的事件。
   */
  const handleFocusOut = (event: FocusEvent) => {
    if (!isSidebar.value) return
    const container = event.currentTarget
    if (
      container instanceof Element &&
      (!(event.relatedTarget instanceof Node) ||
        !container.contains(event.relatedTarget))
    ) {
      appStore.setSidebarFocus(false)
    }
  }

  const isMobile = computed(() => device.value === 'mobile')
  const effectiveMode = computed(() =>
    isMobile.value ? MenuNavigationType.SidebarNav : config.value.sideMode
  )
  const panelWidth = computed(() => {
    const menuWidth = sidebarCollapsed.value
      ? config.value.layoutSideCollapsedWidth
      : config.value.layoutSideWidth
    const primaryWidth =
      !isMobile.value && hasDualSidebarNavigation(effectiveMode.value)
        ? config.value.layoutSideCollapsedWidth
        : 0
    return menuWidth + primaryWidth
  })

  const showSidebarNav = computed(() => {
    return (
      isSidebar.value &&
      (effectiveMode.value === MenuNavigationType.SidebarNav ||
        effectiveMode.value === MenuNavigationType.MixedNav)
    )
  })

  const showTopNav = computed(() => {
    return (
      !isSidebar.value &&
      (effectiveMode.value === MenuNavigationType.TopNav ||
        effectiveMode.value === MenuNavigationType.MixedNav)
    )
  })

  const showDualSidebarNav = computed(() => {
    return isSidebar.value && hasDualSidebarNavigation(effectiveMode.value)
  })

  const showCombinationNav = computed(() => {
    return (
      (isSidebar.value &&
        effectiveMode.value === MenuNavigationType.CombinationNav) ||
      (!isSidebar.value &&
        (effectiveMode.value === MenuNavigationType.CombinationNav ||
          effectiveMode.value === MenuNavigationType.HeaderMixedNav))
    )
  })
</script>

<style scoped>
  .navigation-panel {
    display: flex;
    flex-direction: column;
    min-height: 0;
    background: var(--app-surface);
  }

  .navigation-panel.is-semi-dark {
    --app-surface: #18212c;
    --app-surface-subtle: #253140;
    --app-border: #344252;
    --app-border-strong: #46576a;
    --app-text-primary: #f4f7fa;
    --app-text-secondary: #c4ced8;
    --app-text-muted: #91a0af;
    --app-menu-active-bg: color-mix(
      in srgb,
      var(--el-color-primary) 38%,
      #18212c
    );
    --app-menu-active-text: #ffffff;
    --el-menu-bg-color: var(--app-surface);
    --el-menu-text-color: var(--app-text-secondary);
    --el-menu-hover-bg-color: var(--app-surface-subtle);
    --el-text-color-primary: var(--app-text-primary);
    --el-text-color-regular: var(--app-text-secondary);
  }

  .navigation-brand {
    border-right: 1px solid var(--app-border);
  }

  .navigation-menu {
    flex: 1;
    min-height: 0;
  }

  .navigation-shell {
    --sidebar-motion-duration: 420ms;
    --sidebar-motion-easing: cubic-bezier(0.22, 1, 0.36, 1);
    position: relative;
    height: 100%;
    transition: width var(--sidebar-motion-duration)
      var(--sidebar-motion-easing);
  }

  .navigation-shell :deep(.sidebar-column) {
    overflow: hidden;
    transition:
      width var(--sidebar-motion-duration) var(--sidebar-motion-easing),
      padding-left var(--sidebar-motion-duration) var(--sidebar-motion-easing),
      padding-right var(--sidebar-motion-duration) var(--sidebar-motion-easing);
  }

  .navigation-shell :deep(.sidebar-column:not(.is-collapsed) > .el-scrollbar) {
    animation: sidebar-menu-reveal var(--sidebar-motion-duration)
      var(--sidebar-motion-easing);
  }

  .navigation-shell :deep(.sidebar-column.is-collapsed > .el-scrollbar) {
    animation: sidebar-menu-settle var(--sidebar-motion-duration)
      var(--sidebar-motion-easing);
  }

  @keyframes sidebar-menu-reveal {
    from {
      opacity: 0;
      transform: translateX(-6px);
    }
    to {
      opacity: 1;
      transform: translateX(0);
    }
  }

  @keyframes sidebar-menu-settle {
    from {
      opacity: 0.55;
    }
    to {
      opacity: 1;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .navigation-shell,
    .navigation-shell :deep(.sidebar-column) {
      transition: none;
    }
    .navigation-shell :deep(.sidebar-column > .el-scrollbar) {
      animation: none;
    }
  }

  .navigation-shell--floating {
    z-index: 8;
  }

  .navigation-panel--floating {
    position: absolute;
    top: 0;
    left: 0;
    width: max-content;
    height: 100%;
  }
</style>
