<template>
  <div
    ref="bar"
    class="ape-volo-tabs"
    :class="`tab-style-${app.config.tabStyle}`"
    @keydown.shift.f10.prevent="openKeyboardMenu"
  >
    <el-tabs
      v-model="activeValue"
      type="card"
      class="ape-volo-content"
      @contextmenu.prevent="openContextMenu"
      @tab-change="activate(String($event))"
      @tab-remove="runAction('close', String($event))"
      @auxclick.middle.prevent="middleClose"
    >
      <el-tab-pane
        v-for="item in historys"
        :key="item.name"
        :name="item.name"
        :label="title(item)"
        :closable="canClose(item)"
      >
        <template #label>
          <span
            class="tab-drag-target"
            :class="{ 'is-dragging': draggingTab === item.name }"
            :draggable="app.config.tabDraggable && !item.pinned && !busy"
            @dragstart="startTabDrag(item.name, $event)"
            @dragover.prevent="overTab(item.name, $event)"
            @drop.prevent="dropTab(item.name)"
            @dragend="draggingTab = ''"
          >
            <el-icon
              v-if="app.config.showTabIcon && item.meta.icon"
              size="16"
              class="mr-1"
              ><AppIcon :icon="String(item.meta.icon || '')"
            /></el-icon>
            <span
              class="tab-label"
              :class="{ 'is-active': activeValue === item.name }"
              >{{ title(item) }}</span
            >
            <button
              v-if="item.pinned"
              type="button"
              class="tab-pin"
              :disabled="busy"
              :title="'取消固定'"
              :aria-label="'取消固定'"
              @click.stop="unpinFromTab(item.name, $event)"
              @keydown.enter.stop
              @keydown.space.stop
            >
              <TabPinIcon />
            </button>
          </span>
        </template>
      </el-tab-pane>
    </el-tabs>
    <div class="tab-tools">
      <button
        v-if="app.config.showTabMore"
        ref="moreTrigger"
        type="button"
        class="tab-tool-button"
        :disabled="busy || !historyMap.has(activeValue)"
        :title="'标签页操作'"
        :aria-label="'标签页操作'"
        aria-haspopup="menu"
        :aria-expanded="menuVisible && toolbarMenu"
        :aria-controls="menuId"
        @click="openToolbarMenu"
      >
        <el-icon><AppIcon icon="lucide:layout-grid" /></el-icon>
      </button>
      <button
        v-if="app.config.showTabRefresh"
        type="button"
        class="tab-tool-button"
        :disabled="!enabled('reload', activeValue)"
        :title="'重新加载'"
        :aria-label="'重新加载'"
        @click="runAction('reload', activeValue)"
      >
        <el-icon :class="{ 'is-reloading': reloading }"
          >
<AppIcon icon="lucide:rotate-cw"
        />
</el-icon>
      </button>
      <button
        v-if="app.config.showTabMaximize"
        type="button"
        class="tab-tool-button"
        :disabled="busy || !historyMap.has(activeValue)"
        :title="maximizeLabel"
        :aria-label="maximizeLabel"
        :aria-pressed="app.workspaceMaximized"
        @click="runAction('maximize', activeValue)"
      >
        <el-icon
          >
<AppIcon
            :icon="
              app.workspaceMaximized ? 'lucide:minimize' : 'lucide:maximize'
            "
        />
</el-icon>
      </button>
    </div>
    <Teleport to="body">
      <TabActionsMenu
        v-if="menuVisible"
        :id="menuId"
        ref="menu"
        :entries="entries"
        :label="'标签页操作'"
        :style="{ left: position.left + 'px', top: position.top + 'px' }"
        @action="handleMenuAction"
        @close="dismissMenu"
      />
    </Teleport>
  </div>
</template>

<script setup lang="ts">
  /**
   * 多页签栏页面。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { computed, nextTick, ref, useId, watch } from 'vue'
  import { useRoute } from 'vue-router'
  import { useEventListener, onClickOutside } from '@vueuse/core'
  import { useAppStore } from '@/pinia'
  import { useWorkspaceTabs } from '@/composables/useWorkspaceTabs'
  import TabActionsMenu from './components/TabActionsMenu.vue'
  import TabPinIcon from './components/TabPinIcon.vue'
  import type { TabMenuEntry } from './tab.types'
  import type { TabAction } from '@/services/navigation/workspaceTabs.types'

  defineOptions({ name: 'HistoryComponent' })
  const app = useAppStore()
  const route = useRoute()
  const {
    historys,
    activeValue,
    busy,
    reloading,
    historyMap,
    activate,
    enabled,
    canClose,
    runAction,
    reorderTabs,
    title
  } = useWorkspaceTabs()
  const bar = ref<HTMLElement>()
  const moreTrigger = ref<HTMLButtonElement>()
  const menu = ref<InstanceType<typeof TabActionsMenu>>()
  const menuElement = computed(() => menu.value?.element)
  const menuVisible = ref(false)
  const toolbarMenu = ref(false)
  const menuTarget = ref('')
  const position = ref({ left: 0, top: 0 })
  const menuId = 'tab-actions-' + useId()
  let origin: HTMLElement | undefined
  const draggingTab = ref('')
  const maximizeLabel = computed(() =>
    (app.workspaceMaximized ? '还原' : '最大化')
  )

  const entries = computed<TabMenuEntry[]>(() => {
    const pinned = historyMap.value.get(menuTarget.value)?.pinned
    const actions: Omit<TabMenuEntry, 'disabled'>[] = [
      { id: 'close', label: '关闭', icon: 'lucide:x' },
      {
        id: 'pin',
        label: (pinned ? '取消固定' : '固定'),
        icon: pinned ? 'lucide:pin-off' : 'lucide:pin'
      },
      {
        id: 'maximize',
        label: maximizeLabel.value,
        icon: app.workspaceMaximized ? 'lucide:minimize' : 'lucide:maximize'
      },
      {
        id: 'reload',
        label: '重新加载',
        icon: 'lucide:rotate-cw'
      },
      {
        id: 'openWindow',
        label: '在新窗口打开',
        icon: 'lucide:arrow-up-right'
      },
      {
        id: 'closeLeft',
        label: '关闭左侧标签页',
        icon: 'lucide:arrow-left',
        separator: true
      },
      {
        id: 'closeRight',
        label: '关闭右侧标签页',
        icon: 'lucide:arrow-right'
      },
      {
        id: 'closeOthers',
        label: '关闭其他标签页',
        icon: 'lucide:copy-x',
        separator: true
      },
      { id: 'closeAll', label: '关闭全部标签页', icon: 'lucide:circle-x' }
    ]
    return actions.map((item) => ({
      ...item,
      disabled: !enabled(item.id, menuTarget.value)
    }))
  })

  /**
   * 取得拖动或焦点操作对应的标签元素。
   * @param event 触发当前交互的事件。
   * @returns 目标标签对应的 DOM 元素；找不到时为 undefined。
   */
  function tabElement(event: Event) {
    const element =
      event.target instanceof Element
        ? event.target.closest('[id^="tab-"]')
        : null
    return element instanceof HTMLElement ? element : undefined
  }

  /**
   * 关闭当前菜单并恢复触发器焦点。
   * @param restoreFocus 关闭后是否恢复触发器焦点。
   */
  function dismissMenu(restoreFocus = false) {
    menuVisible.value = false
    if (restoreFocus) {
      void nextTick(() => {
        const activeTab = Array.from(
          bar.value?.querySelectorAll<HTMLElement>('.el-tabs__item') ?? []
        ).find((element) => element.id === 'tab-' + activeValue.value)
        const current = origin?.isConnected
          ? origin
          : activeTab || moreTrigger.value
        current?.focus({ preventScroll: true })
      })
    }
  }

  /**
   * 打开菜单并记录触发位置与焦点来源。
   * @param name 待解析或匹配的名称。
   * @param element 当前待解析或渲染的 DOM 或编辑器元素。
   * @param left 比较时的左侧值。
   * @param top 当前顶部菜单选择。
   * @param toolbar 当前工具栏项目配置。
   */
  async function showMenu(
    name: string,
    element: HTMLElement,
    left: number,
    top: number,
    toolbar: boolean
  ) {
    if (!historyMap.value.has(name) || busy.value) return
    menuTarget.value = name
    origin = element
    toolbarMenu.value = toolbar
    position.value = { left, top }
    menuVisible.value = true
    await nextTick()
    const bounds = menu.value?.element?.getBoundingClientRect()
    if (bounds)
      position.value = {
        left: Math.min(
          Math.max(8, left),
          Math.max(8, window.innerWidth - bounds.width - 8)
        ),
        top: Math.min(
          Math.max(8, top),
          Math.max(8, window.innerHeight - bounds.height - 8)
        )
      }
    menu.value?.focusFirst()
  }

  /**
   * 打开菜单并记录触发位置与焦点来源。
   * @param event 触发当前交互的事件。
   */
  function openContextMenu(event: MouseEvent) {
    const element = tabElement(event)
    if (element)
      void showMenu(
        element.id.slice(4),
        element,
        event.clientX,
        element.getBoundingClientRect().bottom + 4,
        false
      )
  }

  /**
   * 打开菜单并记录触发位置与焦点来源。
   * @param event 触发当前交互的事件。
   */
  function openKeyboardMenu(event: KeyboardEvent) {
    const element = tabElement(event)
    if (element) {
      const bounds = element.getBoundingClientRect()
      void showMenu(
        element.id.slice(4),
        element,
        bounds.left,
        bounds.bottom + 4,
        false
      )
    }
  }

  /**
   * 打开菜单并记录触发位置与焦点来源。
   */
  function openToolbarMenu() {
    if (menuVisible.value && toolbarMenu.value) {
      dismissMenu(true)
      return
    }
    const element = moreTrigger.value
    if (element) {
      const bounds = element.getBoundingClientRect()
      void showMenu(
        activeValue.value,
        element,
        bounds.right - 240,
        bounds.bottom + 4,
        true
      )
    }
  }

  /**
   * 执行当前菜单或按钮对应的业务动作。
   * @param action 当前执行或展示的业务动作。
   */
  async function handleMenuAction(action: TabAction) {
    const name = menuTarget.value
    dismissMenu()
    await runAction(action, name)
    dismissMenu(true)
  }

  /**
   * 响应鼠标中键关闭允许关闭的标签。
   * @param event 触发当前交互的事件。
   */
  function middleClose(event: MouseEvent) {
    const element = tabElement(event)
    if (element) void runAction('close', element.id.slice(4))
  }

  /**
   * 移除目标标签的固定状态。
   * @param name 待解析或匹配的名称。
   * @param event 触发当前交互的事件。
   */
  async function unpinFromTab(name: string, event: MouseEvent) {
    const element = tabElement(event)
    await runAction('pin', name)
    await nextTick()
    element?.focus({ preventScroll: true })
  }

  /**
   * 记录拖动起点与当前被拖动的目标。
   * @param name 待解析或匹配的名称。
   * @param event 触发当前交互的事件。
   */
  function startTabDrag(name: string, event: DragEvent): void {
    if (!app.config.tabDraggable || busy.value) {
      event.preventDefault()
      return
    }
    draggingTab.value = name
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move'
      event.dataTransfer.setData('text/plain', name)
    }
  }

  /**
   * 取得拖动或焦点操作对应的标签元素。
   * @param name 待解析或匹配的名称。
   * @param event 触发当前交互的事件。
   */
  function overTab(name: string, event: DragEvent): void {
    const target = historyMap.value.get(name)
    if (!draggingTab.value || target?.pinned) {
      if (event.dataTransfer) event.dataTransfer.dropEffect = 'none'
      return
    }
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move'
  }

  /**
   * 提交当前拖动得到的顺序变化。
   * @param name 待解析或匹配的名称。
   */
  function dropTab(name: string): void {
    if (draggingTab.value) reorderTabs(draggingTab.value, name)
    draggingTab.value = ''
  }

  onClickOutside(menuElement, () => dismissMenu(), { ignore: [moreTrigger] })
  useEventListener(window, 'resize', () => dismissMenu())
  useEventListener(
    window,
    'scroll',
    (event) => {
      if (
        !(event.target instanceof Node) ||
        !menu.value?.element?.contains(event.target)
      )
        dismissMenu()
    },
    { capture: true }
  )
  watch(
    () => route.fullPath,
    () => dismissMenu()
  )
</script>

<style lang="scss" scoped>
  .ape-volo-tabs {
    display: flex;
    align-items: center;
    flex: none;
    height: var(--app-tabs-height);
    min-width: 0;
    border-bottom: 1px solid var(--app-border);
    background: var(--app-surface);

    > :deep(.el-tabs) {
      flex: 1;
      width: 0;
      min-width: 0;
      height: 100%;
    }

    ::v-deep(.el-tabs--card > .el-tabs__header) {
      height: calc(100% - 6px);
      margin: 6px 2px 0;
      border: none;
    }
    ::v-deep(.el-tabs__nav-scroll) {
      height: 100%;
      padding-inline: var(--app-space-1);
    }

    ::v-deep(.el-tabs__nav-wrap) {
      height: 100%;

      &.is-scrollable {
        padding-inline: 36px;
      }
    }

    ::v-deep(.el-tabs__nav-next),
    ::v-deep(.el-tabs__nav-prev) {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      box-sizing: border-box;
      width: 36px;
      height: 100%;
      border-inline: 1px solid var(--app-border);
      background: var(--app-surface);
      color: var(--app-text-primary);
      transition:
        color 150ms ease,
        background-color 150ms ease,
        opacity 150ms ease;

      .el-icon {
        display: none;
      }

      &::before {
        content: '';
        width: 20px;
        height: 20px;
        background-color: currentColor;
        mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M11 6 5 12l6 6M19 6l-6 6 6 6' fill='none' stroke='black' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E")
          center / contain no-repeat;
      }

      &:not(.is-disabled):hover {
        background: var(--app-surface-subtle);
        color: var(--el-color-primary);
      }

      &.is-disabled {
        color: var(--app-text-muted);
        opacity: 0.5;
        cursor: default;
        pointer-events: none;
      }
    }

    ::v-deep(.el-tabs__nav-next)::before {
      transform: rotate(180deg);
    }

    ::v-deep(.el-tabs__nav) {
      height: 100%;
      border: 0;
    }

    ::v-deep(.el-tabs__header) {
      border-bottom: 0;
    }
    ::v-deep(.el-tabs__item) {
      box-sizing: border-box;
      height: 100%;
      margin: 0;
      padding-inline: var(--app-space-3) !important;
      border: 0;
      border-bottom: 0;
      border-radius: 6px 6px 0 0 !important;
      color: var(--app-text-secondary);
      transition:
        color var(--el-transition-duration-fast),
        background-color var(--el-transition-duration-fast) !important;

      &:hover {
        color: var(--app-text-primary);
        background: var(--app-surface-subtle);
      }

      &.is-active {
        color: var(--app-tab-active-text);
        background: var(--app-tab-active-bg);
      }
    }
    ::v-deep(.el-tabs__item):first-child {
      border-left: 0;
    }
    :deep(.el-tabs__item.is-closable .is-icon-close) {
      width: 14px;
    }
  }

  .tab-label {
    display: inline-flex;
    align-items: center;
    min-width: 0;
    color: inherit;
  }

  .tab-drag-target {
    display: inline-flex;
    align-items: center;
    min-width: 0;
    height: 100%;
  }
  .tab-drag-target[draggable='true'] {
    cursor: grab;
  }
  .tab-drag-target.is-dragging {
    opacity: 0.45;
    cursor: grabbing;
  }

  .ape-volo-tabs.tab-style-chrome {
    ::v-deep(.el-tabs__item) {
      margin-right: 3px;
      border-radius: 9px 9px 0 0 !important;
    }
    ::v-deep(.el-tabs__item.is-active) {
      box-shadow:
        inset 0 1px 0 var(--app-border),
        inset 1px 0 0 var(--app-border),
        inset -1px 0 0 var(--app-border);
    }
  }

  .ape-volo-tabs.tab-style-plain {
    ::v-deep(.el-tabs__item) {
      border-radius: 0 !important;
      background: transparent;
    }
    ::v-deep(.el-tabs__item.is-active) {
      border-bottom: 2px solid var(--el-color-primary);
      background: transparent;
    }
  }

  .ape-volo-tabs.tab-style-brisk {
    ::v-deep(.el-tabs__item) {
      border-radius: 0 !important;
      background: transparent;
    }
    ::v-deep(.el-tabs__item.is-active) {
      box-shadow: inset 0 2px 0 var(--el-color-primary);
      background: var(--app-surface-subtle);
    }
  }

  .tab-label.is-active {
    color: var(--app-tab-active-text);
  }

  @media (prefers-reduced-motion: reduce) {
    .ape-volo-tabs :deep(.el-tabs__item),
    .ape-volo-tabs :deep(.el-tabs__nav-prev),
    .ape-volo-tabs :deep(.el-tabs__nav-next) {
      transition: none !important;
    }
  }
  .tab-tools {
    display: flex;
    flex: none;
    height: 100%;
    border-left: 1px solid var(--app-border);
  }
  .tab-tool-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    padding: 0;
    border: 0;
    border-left: 1px solid var(--app-border);
    background: transparent;
    color: var(--app-text-secondary);
    cursor: pointer;
    font-size: 17px;
  }
  .tab-tool-button:first-child {
    border-left: 0;
  }
  .tab-tool-button:not(:disabled):hover {
    background: var(--app-surface-subtle);
    color: var(--app-text-primary);
  }
  .tab-tool-button:disabled {
    color: var(--app-text-muted);
    cursor: not-allowed;
  }
  .tab-tool-button:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: -2px;
  }
  .tab-pin {
    display: inline-flex;
    flex: none;
    width: 14px;
    height: 14px;
    margin-left: 5px;
    padding: 0;
    border: 0;
    background: transparent;
    color: inherit;
    cursor: pointer;
  }
  .tab-pin:focus-visible {
    outline: 2px solid var(--el-color-primary);
  }
  .is-reloading {
    animation: tab-refresh-spin 1s linear infinite;
  }
  @keyframes tab-refresh-spin {
    to {
      transform: rotate(360deg);
    }
  }
  @media (prefers-reduced-motion: reduce) {
    .is-reloading {
      animation: none;
    }
  }
</style>
