<template>
  <div class="flex h-full">
    <!-- 一级菜单常驻侧边栏 -->
    <div
      class="navigation-surface dual-primary-column relative h-full box-border"
      :style="{
        width: config.layoutSideCollapsedWidth + 'px'
      }"
    >
      <el-scrollbar>
        <el-menu
          :default-active="topActive"
          class="border-r-0 w-full dual-top-menu"
          unique-opened
          @select="selectTopMenuItem"
        >
          <template v-for="item in routerStore.asyncRouters[0]?.children || []">
            <el-menu-item
              v-if="
                !item.hidden && (!item.children || item.children.length === 0)
              "
              :key="item.name"
              :index="item.name"
              class="overflow-hidden dual-top-menu-item"
              :style="{
                height: firstLevelMenuItemHeight + 'px'
              }"
            >
              <div class="dual-top-menu-item__content">
                <el-icon v-if="item.meta.icon" class="dual-top-menu-item__icon">
                  <AppIcon :icon="String(item.meta.icon || '')" />
                </el-icon>
                <span v-else class="dual-top-menu-item__fallback">
                  {{ menuTitle(item.meta)[0] }}
                </span>
                <span class="dual-top-menu-item__label">
                  {{ menuTitle(item.meta) }}
                </span>
              </div>
            </el-menu-item>
            <template v-else-if="!item.hidden">
              <el-menu-item
                :key="item.name"
                :index="item.name"
                :class="{ 'is-active': topActive === item.name }"
                class="overflow-hidden dual-top-menu-item"
                :style="{
                  height: firstLevelMenuItemHeight + 'px'
                }"
              >
                <div class="dual-top-menu-item__content">
                  <el-icon
                    v-if="item.meta.icon"
                    class="dual-top-menu-item__icon"
                  >
                    <AppIcon :icon="String(item.meta.icon || '')" />
                  </el-icon>
                  <span v-else class="dual-top-menu-item__fallback">
                    {{ menuTitle(item.meta)[0] }}
                  </span>
                  <span class="dual-top-menu-item__label">
                    {{ menuTitle(item.meta) }}
                  </span>
                </div>
              </el-menu-item>
            </template>
          </template>
        </el-menu>
      </el-scrollbar>
    </div>

    <!-- 二级菜单并列显示 -->
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
          <template v-for="item in secondLevelMenus">
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

<script setup>
  import AppIcon from '@/components/AppIcon.vue'
  import { menuTitle } from '@/utils/menuTitle'
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
    name: 'DualSidebarNav'
  })

  const route = useRoute()
  const router = useRouter()
  const routerStore = useRouterStore()
  const active = ref('')
  const topActive = ref('')
  const secondLevelMenus = ref([])
  const firstLevelMenuItemHeight = computed(() => {
    return Math.max(Number(config.value.layoutSideItemHeight) || 0, 72)
  })

  const layoutSideWidth = computed(() => {
    if (!isCollapse.value) {
      return config.value.layoutSideWidth
    } else {
      return config.value.layoutSideCollapsedWidth
    }
  })

  provide('isCollapse', isCollapse)

  /**
   * 更新二级菜单
   * @param menuName 需要展开第二级菜单的顶级菜单标识。
   */
  const updateSecondLevelMenus = (menuName) => {
    const menu = routerStore.asyncRouters[0]?.children.find(
      (item) => item.name === menuName
    )
    if (menu && menu.children && menu.children.length > 0) {
      secondLevelMenus.value = menu.children
    }
  }

  /**
   * 选择一级菜单
   * @param index 当前条目、菜单或字段在所属集合中的索引。
   */
  const selectTopMenuItem = (index) => {
    topActive.value = index

    // 获取选中的菜单项
    const menu = routerStore.asyncRouters[0]?.children.find(
      (item) => item.name === index
    )

    // 只有当选中的菜单有子菜单时，才更新二级菜单区域
    if (menu && menu.children && menu.children.length > 0) {
      updateSecondLevelMenus(index)

      // 导航到第一个可见的子菜单
      const firstVisibleChild = menu.children.find((child) => !child.hidden)
      if (firstVisibleChild) {
        void navigateToMenu(router, firstVisibleChild)
      }
    } else {
      // 如果没有子菜单，直接导航到该菜单，但不更新二级菜单区域
      const menu = routerStore.routeMap[index]
      if (menu) void navigateToMenu(router, menu)
    }
  }

  /**
   * 选择二级或更深层级的菜单
   * @param index 当前条目、菜单或字段在所属集合中的索引。
   */
  const selectMenuItem = (index) => {
    const menu = routerStore.routeMap[index]
    if (menu) void navigateToMenu(router, menu)
  }

  // 导航到指定菜单
  watchEffect(() => {
    active.value = route.name

    /**
     * 找到当前路由所属的一级菜单
     * @returns 当前路由所属顶级菜单名；无对应父级时由实现回退到当前路由名。
     */
    const findParentMenu = () => {
      // 首先检查当前路由是否就是一级菜单
      const isTopMenu = routerStore.asyncRouters[0]?.children.some(
        (item) => !item.hidden && item.name === route.name
      )

      if (isTopMenu) {
        return route.name
      }

      for (const topMenu of routerStore.asyncRouters[0]?.children || []) {
        if (topMenu.hidden) continue

        // 检查当前路由是否是这个一级菜单的子菜单
        if (
          topMenu.children &&
          topMenu.children.some((child) => child.name === route.name)
        ) {
          return topMenu.name
        }

        /**
         * 递归检查更深层级
         * @param items 待操作的业务记录或包含标识和并发版本的目标集合。
         * @returns 当前菜单子树中包含目标路由时为 true。
         */
        const checkChildren = (items) => {
          for (const item of items || []) {
            if (item.name === route.name) {
              return true
            }
            if (item.children && checkChildren(item.children)) {
              return true
            }
          }
          return false
        }

        if (topMenu.children && checkChildren(topMenu.children)) {
          return topMenu.name
        }
      }
      return null
    }

    const parentMenu = findParentMenu()
    if (parentMenu) {
      topActive.value = parentMenu

      // 只有当父菜单有子菜单时，才更新二级菜单区域
      const menu = routerStore.asyncRouters[0]?.children.find(
        (item) => item.name === parentMenu
      )
      if (menu && menu.children && menu.children.length > 0) {
        updateSecondLevelMenus(parentMenu)
      } else {
        // 如果找到的父菜单没有子菜单，保持当前一级菜单高亮，但需要显示一些二级菜单
        // 寻找第一个有子菜单的一级菜单来显示其子菜单
        const firstMenuWithChildren = routerStore.asyncRouters[0].children.find(
          (item) => !item.hidden && item.children && item.children.length > 0
        )

        if (firstMenuWithChildren) {
          // 只更新二级菜单区域，但保持当前一级菜单的高亮状态
          updateSecondLevelMenus(firstMenuWithChildren.name)
        }
      }
    } else if (routerStore.asyncRouters[0]?.children?.length > 0) {
      // 如果没有找到父菜单，保持当前路由名称作为高亮，但需要显示一些二级菜单
      // 寻找第一个有子菜单的一级菜单来显示其子菜单
      const firstMenuWithChildren = routerStore.asyncRouters[0].children.find(
        (item) => !item.hidden && item.children && item.children.length > 0
      )

      if (firstMenuWithChildren) {
        // 只更新二级菜单区域，高亮状态保持为当前路由
        topActive.value = route.name
        secondLevelMenus.value = firstMenuWithChildren.children
      }
    }
  })
</script>

<style scoped>
  .dual-top-menu-item {
    padding: 6px 4px;
    line-height: normal;
    justify-content: center;
  }

  .dual-top-menu-item__content {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 6px;
    width: 100%;
    min-height: 100%;
    text-align: center;
  }

  .dual-top-menu-item__icon {
    font-size: 18px;
    margin: 0;
  }

  .dual-top-menu-item__fallback {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 22px;
    height: 22px;
    border-radius: 9999px;
    font-size: 12px;
    font-weight: 600;
    background: color-mix(in srgb, var(--app-text-secondary) 16%, transparent);
  }

  .dual-top-menu-item__label {
    display: -webkit-box;
    overflow: hidden;
    font-size: 11px;
    line-height: 1.2;
    text-align: center;
    word-break: break-all;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
  }

  .dual-top-menu :deep(.el-menu-item) {
    margin: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 6px 0 !important;
    text-align: center;
  }

  .dual-top-menu :deep(.el-menu-item .el-icon) {
    width: auto;
    margin-right: 0;
  }

  .dual-top-menu :deep(.el-menu-item > *) {
    margin: 0 auto;
  }

  .dual-top-menu :deep(.el-menu-item.is-active) .dual-top-menu-item__label,
  .dual-top-menu :deep(.el-menu-item.is-active) .dual-top-menu-item__icon,
  .dual-top-menu :deep(.el-menu-item.is-active) .dual-top-menu-item__fallback {
    color: var(--app-primary-on);
  }

  .dual-top-menu :deep(.el-menu-item.is-active) .dual-top-menu-item__fallback {
    background: color-mix(in srgb, var(--app-primary-on) 18%, transparent);
  }
</style>
