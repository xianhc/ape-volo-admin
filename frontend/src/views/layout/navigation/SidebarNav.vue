<template>
  <div
    class="navigation-surface sidebar-column relative h-full flex flex-col min-h-0 box-border"
    :class="{ 'is-collapsed': isCollapse }"
    :style="{
      width: layoutSideWidth + 'px'
    }"
  >
    <el-scrollbar class="flex-1 min-h-0">
      <el-menu
        :collapse="isCollapse"
        :collapse-transition="false"
        :default-active="active"
        class="border-r-0 w-full"
        unique-opened
        @select="selectMenuItem"
      >
        <template v-for="item in routerStore.asyncRouters[0]?.children || []">
          <menu-node v-if="!item.hidden" :key="item.name" :router-info="item" />
        </template>
      </el-menu>
    </el-scrollbar>
    <SidebarFooter />
  </div>
</template>

<script setup>
  /**
   * 侧栏导航组件。
   */
  import SidebarFooter from './SidebarFooter.vue'
  import menuNode from '@/views/layout/navigation/MenuNode.vue'
  import { ref, provide, watchEffect, computed } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useRouterStore } from '@/pinia/modules/router'
  import { useAppStore } from '@/pinia'
  import { storeToRefs } from 'pinia'
  import { navigateToMenu } from '@/services/navigation/menuNavigation'
  const appStore = useAppStore()
  const { config, sidebarCollapsed: isCollapse } = storeToRefs(appStore)

  defineOptions({
    name: 'SidebarNav'
  })
  const route = useRoute()
  const router = useRouter()
  const routerStore = useRouterStore()
  const active = ref('')
  const layoutSideWidth = computed(() => {
    if (!isCollapse.value) {
      return config.value.layoutSideWidth
    } else {
      return config.value.layoutSideCollapsedWidth
    }
  })
  watchEffect(() => {
    active.value = route.name
  })

  provide('isCollapse', isCollapse)

  /**
   * 执行当前菜单项并按规则收起菜单。
   * @param index 当前项目在集合中的位置。
   */
  const selectMenuItem = (index) => {
    const menu = routerStore.routeMap[index]
    if (menu) void navigateToMenu(router, menu)
  }
</script>
