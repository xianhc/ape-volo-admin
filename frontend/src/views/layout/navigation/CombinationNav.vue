<template>
  <div class="h-full">
    <div
      v-if="mode === 'TopNav'"
      class="navigation-surface top-navigation h-full min-w-0 flex items-center"
    >
      <el-menu
        :default-active="routerStore.topActive"
        mode="horizontal"
        class="!border-r-0 border-b-0 w-full flex gap-1 items-center box-border h-[calc(100%-1px)]"
        unique-opened
        @select="handleTopSelect"
      >
        <template v-for="item in routerStore.topMenu">
          <menu-node
            v-if="!item.hidden"
            :key="item.name"
            :router-info="item"
            mode="horizontal"
          />
        </template>
      </el-menu>
    </div>
    <div
      v-if="mode === 'SidebarNav'"
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
          class="!border-r-0 w-full"
          unique-opened
          @select="handleLeftSelect"
        >
          <template v-for="item in routerStore.leftMenu">
            <menu-node
              v-if="!item.hidden"
              :key="item.name"
              :router-info="item"
            />
          </template>
        </el-menu>
      </el-scrollbar>
      <SidebarFooter />
    </div>
  </div>
</template>
<script setup lang="ts">
  /**
   * 混合导航组件：顶部与侧栏联动。
   */
  import SidebarFooter from './SidebarFooter.vue'
  import MenuNode from '@/views/layout/navigation/MenuNode.vue'
  import { ref, provide, watchEffect, computed } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { useRouterStore } from '@/pinia/modules/router'
  import { useAppStore } from '@/pinia'
  import { storeToRefs } from 'pinia'
  import { navigateToMenu } from '@/services/navigation/menuNavigation'
  const appStore = useAppStore()
  const { config, sidebarCollapsed: isCollapse } = storeToRefs(appStore)

  defineOptions({
    name: 'CombinationNav'
  })

  const props = withDefaults(
    defineProps<{
      /** Redis 或日志队列当前运行模式。 */
      mode?: string
    }>(),
    {
      mode: 'SidebarNav'
    }
  )

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
    active.value = String(route.name || '')
  })

  provide(
    'isCollapse',
    computed(() => props.mode === 'SidebarNav' && isCollapse.value)
  )

  /**
   * 执行当前菜单项并按规则收起菜单。
   * @param index 当前项目在集合中的位置。
   * @param _ 框架传入但此回调不使用的参数。
   * @param __ 框架传入但此回调不使用的参数。
   * @param top 当前顶部菜单选择。
   */
  const selectMenuItem = (
    index: string,
    _: unknown,
    __: unknown,
    top: boolean
  ) => {
    if (!top) {
      const menu = routerStore.routeMap[index]
      if (menu) void navigateToMenu(router, menu)
      return
    }
    const leftMenu = routerStore.setLeftMenu(index)
    if (!leftMenu) {
      const menu = routerStore.routeMap[index]
      if (menu) void navigateToMenu(router, menu)
      return
    }
    const firstMenu = leftMenu.find(
      (item) =>
        !item.hidden &&
        item.path.indexOf('http://') === -1 &&
        item.path.indexOf('https://') === -1
    )
    if (firstMenu?.name) {
      void navigateToMenu(router, firstMenu)
    }
  }

  /**
   * 切换顶部或侧栏菜单并进入目标路由。
   * @param index 当前项目在集合中的位置。   */
  const handleTopSelect = (index: string) =>
    selectMenuItem(index, undefined, undefined, true)
  /**
   * 切换顶部或侧栏菜单并进入目标路由。
   * @param index 当前项目在集合中的位置。   */
  const handleLeftSelect = (index: string) =>
    selectMenuItem(index, undefined, undefined, false)
</script>
