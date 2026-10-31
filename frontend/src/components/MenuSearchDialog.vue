<!--
  菜单搜索组件：提供全局菜单搜索、最近访问和键盘导航。
-->
<template>
  <div>
    <el-tooltip v-if="showTrigger" :content="'菜单搜索'" placement="bottom">
      <button
        type="button"
        class="menu-search-trigger"
        :aria-label="'菜单搜索'"
        aria-haspopup="dialog"
        @click="open"
      >
        <el-icon><AppIcon icon="lucide:search" /></el-icon>
        <span class="menu-search-label">{{ '搜索' }}</span>
        <span class="menu-search-shortcut">Ctrl <kbd>K</kbd></span>
      </button>
    </el-tooltip>
    <el-dialog
      v-model="visible"
      class="menu-search-dialog"
      :title="'菜单搜索'"
      width="min(600px, calc(100vw - 32px))"
      top="8vh"
      append-to-body
      :show-close="false"
      @opened="focusInput"
      @keydown="handleDialogKey"
    >
      <template #header>
        <div class="menu-search-bar">
          <AppIcon icon="lucide:search" :size="20" />
          <input
            ref="input"
            v-model="keyword"
            type="text"
            class="menu-search-input"
            :placeholder="'输入菜单名称关键字搜索'"
            :aria-label="'菜单搜索'"
            role="combobox"
            aria-autocomplete="list"
            aria-expanded="true"
            :aria-controls="listId"
            :aria-activedescendant="
              activeIndex >= 0 ? optionId(activeIndex) : undefined
            "
            autocomplete="off"
            spellcheck="false"
            @compositionstart="composing = true"
            @compositionend="composing = false"
          />
          <button
            type="button"
            class="menu-search-close"
            :aria-label="'关闭'"
            @click="close"
          >
            <AppIcon icon="lucide:x" :size="20" />
          </button>
        </div>
      </template>
      <div ref="scroller" class="menu-search-results" :aria-busy="navigating">
        <div
          v-if="!searching && results.length"
          class="menu-search-history-heading"
        >
          <span>{{ '搜索历史' }}</span>
          <button
            type="button"
            class="menu-search-clear"
            :aria-label="'清空搜索历史'"
            @click="clearRecent"
          >
            <AppIcon icon="lucide:trash-2" :size="16" />
          </button>
        </div>
        <ul
          :id="listId"
          class="menu-search-list"
          role="listbox"
          :aria-label="
            searching
              ? '菜单搜索'
              : '搜索历史'
          "
        >
          <li
            v-for="(item, index) in results"
            :id="optionId(index)"
            :key="item.name"
            class="menu-search-item"
            :class="{ 'is-active': index === activeIndex }"
            role="option"
            :aria-selected="index === activeIndex"
            :aria-label="item.title"
            @pointerenter="pointerEnter(index)"
            @pointermove="pointerMove(index, $event)"
          >
            <button
              type="button"
              class="menu-search-select"
              tabindex="-1"
              :disabled="navigating"
              @click="choose(index)"
            >
              <AppIcon :icon="item.icon" :size="20" />
              <span class="menu-search-item-title">{{ item.title }}</span>
            </button>
            <button
              type="button"
              class="menu-search-remove"
              :aria-label="`${searching ? '移除搜索结果' : '删除'} ${item.title}`"
              @click="remove(index)"
            >
              <AppIcon icon="lucide:x" :size="18" />
            </button>
          </li>
        </ul>
        <div v-if="!results.length" class="menu-search-empty" role="status">
          <AppIcon v-if="searching" icon="lucide:search-x" :size="40" />
          <p>
            {{
(searching ? '没有匹配的菜单' : '暂无搜索历史')
            }}
          </p>
        </div>
      </div>
      <template #footer>
        <div class="menu-search-hints">
          <span
          ><AppIcon icon="lucide:corner-down-left" :size="16" />{{
            '选择'
          }}</span
          >
          <span
          ><AppIcon icon="lucide:arrow-up" :size="16" /><AppIcon
            icon="lucide:arrow-down"
            :size="16"
          />{{ '导航' }}</span
          >
          <span><kbd>ESC</kbd>{{ '关闭' }}</span>
        </div>
      </template>
    </el-dialog>
  </div>
</template>
<script setup lang="ts">
  import { nextTick, ref, useId, watch } from 'vue'
  import { useEventListener } from '@vueuse/core'
  import AppIcon from '@/components/AppIcon.vue'
  import { useMenuSearch } from '@/composables/useMenuSearch'
  import { useScreenLockStore } from '@/pinia/modules/screenLock'
  import { useAppStore } from '@/pinia'

  withDefaults(defineProps<{
    /** 是否显示打开或展开入口。 */
    showTrigger?: boolean
  }>(), { showTrigger: true })
  const {
    keyword,
    activeIndex,
    results,
    searching,
    navigating,
    reset,
    move,
    dismiss,
    clearHistory,
    select
  } = useMenuSearch()
  const screenLock = useScreenLockStore()
  const app = useAppStore()
  const visible = ref(false)
  const input = ref<HTMLInputElement>()
  const scroller = ref<HTMLElement>()
  const composing = ref(false)
  const keyboardNavigation = ref(false)
  const listId = 'menu-search-' + useId()
  /**
   * 取得用于候选项或表格行稳定识别的键。
   * @param index 当前项目在集合中的位置。
   * @returns 当前搜索结果对应的可访问性 DOM 标识。
   */
  const optionId = (index: number): string => `${listId}-${index}`
  /**
   * 将焦点移到当前输入、密码字段或抽屉内容。
   */
  const focusInput = (): void => {
    input.value?.focus()
  }
  /**
   * 关闭菜单搜索当前显示的视图。
   */
  const close = (): void => {
    visible.value = false
  }

  /**
   * 打开菜单搜索的目标视图或编辑表单。
   */
  async function open(): Promise<void> {
    if (screenLock.locked) return
    reset()
    composing.value = false
    keyboardNavigation.value = false
    visible.value = true
    await nextTick()
    if (scroller.value) scroller.value.scrollTop = 0
    focusInput()
  }

  /**
   * 将选中的菜单搜索项目应用到当前状态。
   * @param index 当前项目在集合中的位置。
   */
  async function choose(index: number): Promise<void> {
    if (screenLock.locked || navigating.value) return
    if (await select(index)) close()
  }

  /**
   * 移除菜单搜索选中的目标记录。
   * @param index 当前项目在集合中的位置。
   */
  function remove(index: number): void {
    dismiss(index)
    focusInput()
  }

  /**
   * 清除最近使用的候选记录。
   */
  function clearRecent(): void {
    clearHistory()
    focusInput()
  }

  /**
   * 标记指针已进入控件并更新悬浮交互。
   * @param index 当前项目在集合中的位置。
   */
  function pointerEnter(index: number): void {
    if (!keyboardNavigation.value) activeIndex.value = index
  }

  /**
   * 按指针位置更新当前交互状态。
   * @param index 当前项目在集合中的位置。
   * @param event 触发当前交互的事件。
   */
  function pointerMove(index: number, event: PointerEvent): void {
    if (event.movementX || event.movementY) {
      keyboardNavigation.value = false
      activeIndex.value = index
    }
  }

  /**
   * 按当前键盘事件执行选择、确认或关闭动作。
   * @param event 触发当前交互的事件。
   */
  function handleDialogKey(event: KeyboardEvent): void {
    if (event.isComposing || composing.value || screenLock.locked) return
    if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      event.preventDefault()
      keyboardNavigation.value = true
      move(event.key === 'ArrowUp' ? -1 : 1)
    } else if (event.key === 'Enter' && event.target === input.value) {
      event.preventDefault()
      void choose(activeIndex.value)
    } else if (event.key === 'Escape') {
      event.preventDefault()
      close()
    }
  }

  useEventListener(document, 'keydown', (event) => {
    if (
      app.config.enableShortcuts
      &&
      (event.ctrlKey
        || event.metaKey)
      &&
      event.key.toLowerCase() === 'k'
      &&
      !event.isComposing
      &&
      !screenLock.locked
    ) {
      event.preventDefault()
      void open()
    }
  })
  watch(
    () => screenLock.locked,
    (locked) => {
      if (locked) close()
    }
  )
  watch([activeIndex, results], async () => {
    if (!visible.value) return
    await nextTick()
    scroller.value
      ?.querySelector<HTMLElement>(`[id="${optionId(activeIndex.value)}"]`)
      ?.scrollIntoView({ block: 'nearest' })
  })
  defineExpose({ open })
</script>
<style scoped lang="scss">
  .menu-search-trigger {
    display: inline-flex;
    flex: none;
    align-items: center;
    justify-content: center;
    gap: 10px;
    height: 36px;
    padding: 0 12px;
    border: 0;
    border-radius: 999px;
    background: var(--app-surface-subtle);
    color: var(--app-text-secondary);
    font: inherit;
    cursor: pointer;
  }
  .menu-search-trigger .el-icon {
    font-size: 20px;
  }
  .menu-search-trigger:hover {
    color: var(--app-text-primary);
  }
  .menu-search-trigger:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 2px;
  }
  .menu-search-label {
    font-size: 14px;
  }
  .menu-search-shortcut {
    padding: 4px 7px;
    border-radius: 5px;
    background: var(--app-surface);
    color: var(--app-text-secondary);
    font-size: 12px;
    line-height: 1;
  }
  kbd {
    font: inherit;
  }
  .menu-search-bar {
    display: flex;
    align-items: center;
    gap: 12px;
    min-height: 52px;
    padding: 0 20px;
    color: var(--app-text-secondary);
  }
  .menu-search-input {
    flex: 1;
    min-width: 0;
    height: 52px;
    padding: 0;
    border: 0;
    outline: none;
    background: transparent;
    color: var(--app-text-primary);
    font: inherit;
    font-size: 16px;
    font-weight: 500;
  }
  .menu-search-input::placeholder {
    color: var(--app-text-muted);
    font-weight: 400;
  }
  .menu-search-close,
  .menu-search-remove,
  .menu-search-clear {
    display: inline-flex;
    flex: none;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    padding: 0;
    border: 0;
    border-radius: 50%;
    background: transparent;
    color: inherit;
    cursor: pointer;
  }
  .menu-search-close:hover,
  .menu-search-clear:hover {
    background: var(--app-surface-subtle);
    color: var(--app-text-primary);
  }
  button:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 2px;
  }
  .menu-search-results {
    box-sizing: border-box;
    min-height: 160px;
    max-height: min(450px, calc(100dvh - 200px));
    overflow-y: auto;
    padding: 12px 20px;
    overscroll-behavior: contain;
    scrollbar-width: thin;
    scrollbar-color: var(--app-border-strong) transparent;
  }
  .menu-search-results::-webkit-scrollbar {
    display: block;
    width: 6px;
  }
  .menu-search-results::-webkit-scrollbar-thumb {
    border-radius: 6px;
    background: var(--app-border-strong);
  }
  .menu-search-history-heading {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 8px;
    color: var(--app-text-secondary);
    font-size: 12px;
  }
  .menu-search-list {
    display: grid;
    gap: 12px;
    margin: 0;
    padding: 0;
    list-style: none;
  }
  .menu-search-item {
    display: flex;
    align-items: center;
    min-width: 0;
    min-height: 56px;
    padding-right: 14px;
    border-radius: 8px;
    background: var(--app-surface-subtle);
    color: var(--app-text-primary);
  }
  .menu-search-item.is-active {
    background: var(--el-color-primary);
    color: var(--el-color-white);
  }
  .menu-search-select {
    display: flex;
    flex: 1;
    align-items: center;
    gap: 10px;
    min-width: 0;
    min-height: 56px;
    padding: 14px 16px;
    border: 0;
    border-radius: inherit;
    background: transparent;
    color: inherit;
    font: inherit;
    font-size: 16px;
    font-weight: 500;
    text-align: left;
    cursor: pointer;
  }
  .menu-search-item-title {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  .menu-search-remove:hover {
    background: color-mix(in srgb, currentColor 14%, transparent);
  }
  .menu-search-item.is-active button:focus-visible {
    outline-color: var(--el-color-white);
  }
  .menu-search-item:hover .menu-search-select .app-icon {
    animation: app-icon-pop 300ms cubic-bezier(0.4, 0, 0.2, 1);
  }
  .menu-search-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 136px;
    gap: 12px;
    color: var(--app-text-muted);
    font-size: 14px;
  }
  .menu-search-empty p {
    margin: 0;
  }
  .menu-search-hints,
  .menu-search-hints > span {
    display: flex;
    align-items: center;
  }
  .menu-search-hints {
    flex-wrap: wrap;
    gap: 12px;
    color: var(--app-text-secondary);
    font-size: 12px;
  }
  .menu-search-hints > span {
    gap: 4px;
  }
  .menu-search-hints kbd {
    font-size: 10px;
    font-weight: 600;
  }
  :global(.el-dialog.menu-search-dialog) {
    overflow: hidden;
    padding: 0;
    border: 1px solid var(--app-border);
    border-radius: 12px;
    background: var(--app-surface);
  }
  :global(.menu-search-dialog .el-dialog__header) {
    padding: 0;
    margin: 0;
    border-bottom: 1px solid var(--app-border);
  }
  :global(.menu-search-dialog .el-dialog__body) {
    padding: 0;
  }
  :global(.menu-search-dialog .el-dialog__footer) {
    padding: 10px 12px;
    border-top: 1px solid var(--app-border);
    text-align: left;
  }
  @media (max-width: 991px) {
    .menu-search-trigger {
      width: 32px;
      height: 32px;
      padding: 0;
    }
    .menu-search-label,
    .menu-search-shortcut {
      display: none;
    }
  }
  @media (max-width: 480px) {
    .menu-search-bar {
      padding-inline: 14px;
    }
    .menu-search-results {
      padding-inline: 12px;
    }
  }
  @media (prefers-reduced-motion: reduce) {
    .menu-search-item:hover .menu-search-select .app-icon {
      animation: none;
    }
  }
</style>
