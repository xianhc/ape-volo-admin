<template>
  <el-popover
    v-model:visible="visible"
    trigger="click"
    placement="bottom-end"
    :width="272"
    :offset="12"
    :show-arrow="false"
    popper-class="user-menu-popper"
    :popper-style="popperStyle"
  >
    <template #reference>
      <button
        ref="trigger"
        type="button"
        class="user-menu-trigger"
        :aria-label="triggerLabel"
        :aria-expanded="visible"
        aria-haspopup="menu"
        data-toolbar-menu-trigger
      >
        <span class="user-menu-trigger__avatar">
          <el-avatar :size="32" :src="avatarSrc" />
          <span class="user-menu__dot user-menu__dot--trigger" aria-hidden="true" />
        </span>
      </button>
    </template>

    <div class="user-menu" role="menu" @keydown.esc.stop="close">
      <div class="user-menu__profile">
        <span class="user-menu__avatar">
          <el-avatar :size="44" :src="avatarSrc" />
          <span class="user-menu__dot" aria-hidden="true" />
        </span>
        <div class="user-menu__identity">
          <p class="user-menu__name" :title="nickName">{{ nickName }}</p>
          <p class="user-menu__email" :title="email">{{ email }}</p>
        </div>
      </div>

      <div class="user-menu__divider" />

      <div class="user-menu__list">
        <button
          v-for="item in primaryItems"
          :key="item.key"
          type="button"
          role="menuitem"
          class="user-menu__item"
          @click="run(item, $event)"
        >
          <el-icon :size="17"><AppIcon :icon="item.icon" /></el-icon>
          <span class="user-menu__label">{{ item.label }}</span>
        </button>

        <template v-if="toolbarItems.length">
          <div class="user-menu__divider user-menu__divider--inset" />
          <button
            v-for="item in toolbarItems"
            :key="item.key"
            type="button"
            role="menuitem"
            class="user-menu__item"
            :disabled="item.disabled"
            @click="run(item, $event)"
          >
            <el-icon :size="17"><AppIcon :icon="item.icon" :spin="item.busy" /></el-icon>
            <span class="user-menu__label">{{ item.label }}</span>
          </button>
        </template>
      </div>
    </div>
  </el-popover>
</template>

<script setup lang="ts">
  /**
   * 顶栏用户菜单：触发器只保留头像，点击后展开身份信息与常用入口。
   */
  import { computed, nextTick, ref } from 'vue'
  import type { CSSProperties } from 'vue'
  import { useRouter } from 'vue-router'
  import { ElMessage } from 'element-plus'
  import AppIcon from '@/components/AppIcon.vue'
  import noAvatar from '@/assets/noAvatar.png'
  import setting from '@/setting'
  import { getStaticBaseUrl } from '@/utils/index'
  import { useUserStore } from '@/pinia/modules/user'
  import type { ToolbarItemKey } from '@/services/appearance/preferences.types'

  /** 用户菜单中的一条入口。 */
  interface UserMenuEntry {
    /** 条目唯一标识。 */
    key: string
    /** 条目文案。 */
    label: string
    /** 条目图标标识。 */
    icon: string
    /** 外部地址，存在时以新标签页打开。 */
    href?: string
    /** 是否禁用。 */
    disabled?: boolean
    /** 关联的工具栏项，点击后交由顶栏处理。 */
    toolbar?: ToolbarItemKey
    /** 是否处于执行中状态。 */
    busy?: boolean
  }

  /** 被放置到用户菜单中的工具栏项。 */
  interface ToolbarEntry {
    /** 工具栏项标识。 */
    key: ToolbarItemKey
    /** 条目文案。 */
    label: string
    /** 条目图标标识。 */
    icon: string
    /** 是否处于执行中状态。 */
    busy?: boolean
    /** 是否禁用。 */
    disabled?: boolean
  }

  const props = defineProps<{
    /** 被放置到用户菜单中的工具栏项。 */
    items: ToolbarEntry[]
    /** 工具栏项的统一处理函数。 */
    activate: (key: ToolbarItemKey, event?: Event) => void
  }>()

  const router = useRouter()
  const userStore = useUserStore()
  const trigger = ref<HTMLButtonElement>()
  const visible = ref(false)

  /** 个人中心路由名，取自后端 sys_menu 的 ComponentName。 */
  const PROFILE_ROUTE_NAME = 'personalCenter'
  /** 个人中心路由未注册时的兜底地址（挂在 /layout 下的一级隐藏菜单）。 */
  const PROFILE_ROUTE_PATH = '/layout/personalCenter'

  const popperStyle: CSSProperties = {
    padding: '0',
    border: '1px solid var(--app-border)',
    borderRadius: '12px',
    boxShadow: 'var(--app-shadow-overlay)'
  }

  const avatarSrc = computed(() => {
    const headerImg = userStore.userInfo.headerImg
    return headerImg ? getStaticBaseUrl() + headerImg : noAvatar
  })
  const nickName = computed(() => userStore.userInfo.nickName || userStore.userInfo.userName || '未登录用户')
  const email = computed(() => userStore.userInfo.email || '未绑定邮箱')
  const triggerLabel = computed(() => `${nickName.value}的账户菜单`)

  const primaryItems = computed<UserMenuEntry[]>(() => [
    { key: 'profile', label: '个人中心', icon: 'lucide:user-round' },
    { key: 'docs', label: '文档', icon: 'lucide:book-open', href: setting.docsUrl },
    { key: 'github', label: 'GitHub', icon: 'lucide:github', href: setting.githubUrl }
  ])
  const toolbarItems = computed<UserMenuEntry[]>(() =>
    props.items.map(item => ({
      key: item.key,
      label: item.label,
      icon: item.icon,
      busy: item.busy,
      disabled: item.disabled,
      toolbar: item.key
    })))

  /**
   * 关闭用户菜单并把焦点还给头像按钮。
   */
  function close(): void {
    visible.value = false
    void nextTick(() => trigger.value?.focus({ preventScroll: true }))
  }

  /**
   * 跳转到个人中心；路由未注册时给出提示而不是静默失败。
   */
  async function openProfile(): Promise<void> {
    const byName = router.hasRoute(PROFILE_ROUTE_NAME)
    const byPath = router.getRoutes().some(route => route.path === PROFILE_ROUTE_PATH)
    if (!byName && !byPath) {
      ElMessage.warning('未找到个人中心页面，请联系管理员分配菜单权限')
      return
    }
    try {
      await router.push(byName ? { name: PROFILE_ROUTE_NAME } : { path: PROFILE_ROUTE_PATH })
    } catch {
      ElMessage.warning('个人中心页面打开失败，请稍后重试')
    }
  }

  /**
   * 执行用户菜单条目的动作并收起菜单。
   * @param item 当前被点击的菜单条目。
   * @param event 触发当前交互的事件。
   */
  function run(item: UserMenuEntry, event: Event): void {
    if (item.disabled) return
    close()
    if (item.href) {
      window.open(item.href, '_blank', 'noopener,noreferrer')
      return
    }
    if (item.toolbar) {
      props.activate(item.toolbar, event)
      return
    }
    void openProfile()
  }
</script>

<style scoped>
  .user-menu-trigger {
    display: inline-flex;
    flex: none;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 0;
    border-radius: 50%;
    background: transparent;
    cursor: pointer;
  }

  .user-menu-trigger__avatar {
    position: relative;
    display: inline-flex;
    border-radius: 50%;
    transition: box-shadow 0.15s ease;
  }

  .user-menu-trigger:hover .user-menu-trigger__avatar {
    box-shadow: 0 0 0 2px color-mix(in srgb, var(--el-color-primary) 32%, transparent);
  }

  .user-menu-trigger:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 3px;
  }

  .user-menu__avatar {
    position: relative;
    display: inline-flex;
    flex: none;
    border-radius: 50%;
  }

  .user-menu__dot {
    position: absolute;
    right: 0;
    bottom: 1px;
    width: 11px;
    height: 11px;
    border: 2px solid var(--el-bg-color-overlay);
    border-radius: 50%;
    background: #22c55e;
  }

  .user-menu__dot--trigger {
    right: -1px;
    bottom: 0;
    width: 9px;
    height: 9px;
    border-color: var(--app-surface);
  }

  .user-menu {
    display: flex;
    flex-direction: column;
    color: var(--app-text-primary);
    font-size: 14px;
  }

  .user-menu__profile {
    display: flex;
    align-items: center;
    gap: var(--app-space-3);
    min-width: 0;
    padding: 16px 16px 14px;
  }

  .user-menu__identity {
    min-width: 0;
  }

  .user-menu__name {
    margin: 0;
    overflow: hidden;
    color: var(--app-text-primary);
    font-size: 15px;
    font-weight: 600;
    line-height: 1.4;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .user-menu__email {
    margin: 2px 0 0;
    overflow: hidden;
    color: var(--app-text-muted);
    font-size: 12.5px;
    line-height: 1.4;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .user-menu__divider {
    height: 1px;
    background: var(--app-border);
  }

  .user-menu__divider--inset {
    margin: 4px 6px;
  }

  .user-menu__list {
    display: flex;
    flex-direction: column;
    padding: 6px;
  }

  .user-menu__item {
    display: flex;
    align-items: center;
    gap: 10px;
    width: 100%;
    height: 40px;
    padding: 0 10px;
    border: 0;
    border-radius: 8px;
    background: transparent;
    color: var(--app-text-primary);
    font: inherit;
    text-align: left;
    cursor: pointer;
  }

  .user-menu__item:hover:not(:disabled) {
    background: var(--app-surface-subtle);
  }

  .user-menu__item:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: -2px;
  }

  .user-menu__item:disabled {
    color: var(--app-text-muted);
    cursor: not-allowed;
  }

  .user-menu__item > .el-icon {
    flex: none;
    color: var(--app-text-secondary);
  }

  .user-menu__label {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  :global(.user-menu-popper.el-popper) {
    overflow: hidden;
  }
</style>
