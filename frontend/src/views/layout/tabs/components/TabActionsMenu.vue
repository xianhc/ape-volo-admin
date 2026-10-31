<template>
  <ul ref="menu" role="menu" class="tab-actions-menu" :aria-label="label" @keydown="handleKey">
    <li v-for="item in entries" :key="item.id" role="none" :class="{ 'has-separator': item.separator }">
      <button type="button" role="menuitem" tabindex="-1" :disabled="item.disabled" @click="emit('action', item.id)">
        <el-icon><AppIcon :icon="String(item.icon || '')" /></el-icon>
        <span>{{ item.label }}</span>
      </button>
    </li>
  </ul>
</template>
<script setup lang="ts">
  /**
   * 页签操作菜单组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { ref } from 'vue'
  import type { TabMenuEntry } from '../tab.types'
  import type { TabAction } from '@/services/navigation/workspaceTabs.types'

  defineProps<{
    /** 当前复合数据分解得到的条目集合。 */
    entries: TabMenuEntry[]
    /** 面向用户显示的标签。 */
    label: string
  }>()
  const emit = defineEmits<{
    /** 当前执行或展示的业务动作。 */
    action: [id: TabAction]
    /** 关闭当前弹层或视图的操作。 */
    close: [restoreFocus: boolean]
  }>()
  const menu = ref<HTMLElement>()
  /**
   * 取得当前可显示且可操作的按钮集合。
   * @returns 当前菜单中未禁用的按钮元素集合。
   */
  const buttons = () => Array.from(menu.value?.querySelectorAll<HTMLButtonElement>('button:not(:disabled)') ?? [])
  defineExpose({
    element: menu,
    /**
     * 将键盘焦点移到首个可用操作项。
     */
    focusFirst: () => buttons()[0]?.focus({ preventScroll: true })
  })

  /**
   * 按当前键盘事件执行选择、确认或关闭动作。
   * @param event 触发当前交互的事件。
   */
  function handleKey(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      event.preventDefault()
      event.stopPropagation()
      emit('close', true)
    } else if (event.key === 'Tab') {
      emit('close', false)
    } else if (['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) {
      event.preventDefault()
      const items = buttons()
      const index = items.findIndex(item => item === document.activeElement)
      let next: number
      if (event.key === 'Home') {
        next = 0
      } else {
        if (event.key === 'End') {
          next = items.length - 1
        } else {
          next = (index + (event.key === 'ArrowDown' ? 1 : -1) + items.length) % items.length
        }
      }
      items[next]?.focus()
    }
  }
</script>
<style scoped>
  .tab-actions-menu {
    position: fixed;
    z-index: 3000;
    box-sizing: border-box;
    width: min(240px, calc(100vw - 16px));
    max-height: calc(100dvh - 16px);
    overflow-y: auto;
    margin: 0;
    padding: 6px;
    list-style: none;
    border: 1px solid var(--app-border);
    border-radius: var(--app-radius-surface);
    background: var(--app-surface-raised);
    box-shadow: var(--app-shadow-overlay);
  }
  .has-separator {
    margin-top: 5px;
    padding-top: 5px;
    border-top: 1px solid var(--app-border);
  }
  button {
    display: flex;
    align-items: center;
    gap: 12px;
    width: 100%;
    padding: 8px 10px;
    border: 0;
    border-radius: var(--ape-volo-radius);
    color: var(--app-text-primary);
    background: transparent;
    font: inherit;
    text-align: left;
    cursor: pointer;
  }
  button .el-icon { flex: none; font-size: 17px; }
  button:disabled { color: var(--app-text-muted); cursor: not-allowed; }
  button:not(:disabled):hover,
  button:focus-visible { background: var(--app-surface-subtle); }
  button:focus-visible { outline: 2px solid var(--el-color-primary); outline-offset: -2px; }
</style>
