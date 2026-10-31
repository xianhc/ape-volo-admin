<template>
  <div>
    <el-tooltip
      v-if="showTrigger"
      class=""
      effect="dark"
      :content="'系统设置'"
      placement="bottom"
    >
      <button
        ref="trigger"
        type="button"
        class="header-tool-button"
        :aria-label="'系统设置'"
        :aria-expanded="isModalVisible"
        aria-haspopup="dialog"
        @click="openSettingModal"
      >
        <el-icon><AppIcon icon="lucide:settings" /></el-icon>
      </button>
    </el-tooltip>
    <el-drawer
      v-model="isModalVisible"
      :title="'系统配置'"
      direction="rtl"
      :size="width"
      :show-close="false"
      :close-on-press-escape="true"
      class="ape-theme-drawer"
      append-to-body
      @opened="focusDrawer"
      @closed="restoreTriggerFocus"
    >
      <template #header>
        <div class="setting-drawer-header">
          <div class="setting-drawer-heading">
            <h2 class="setting-drawer-title">
              {{ '系统配置' }}
            </h2>
            <p class="setting-drawer-subtitle">
              {{ '调整外观、布局、快捷键与通用偏好，变更会自动保存。' }}
            </p>
          </div>
          <div class="setting-header-actions">
            <el-tooltip :content="'设置页签吸顶'" placement="bottom">
              <button
                type="button"
                class="setting-icon-button"
                :class="{ 'is-active': config.settingTabsSticky }"
                :aria-label="'设置页签吸顶'"
                :aria-pressed="config.settingTabsSticky"
                @click="config.settingTabsSticky = !config.settingTabsSticky"
              >
                <AppIcon :icon="config.settingTabsSticky ? 'lucide:pin' : 'lucide:pin-off'" />
              </button>
            </el-tooltip>
            <el-tooltip
              :content="'恢复默认'"
              placement="bottom"
            >
              <button
                type="button"
                class="setting-icon-button"
                :aria-label="'恢复默认'"
                @click="handleResetConfig"
              >
                <AppIcon icon="lucide:rotate-ccw" />
              </button>
            </el-tooltip>
            <button
              ref="closeButton"
              type="button"
              class="setting-icon-button"
              :aria-label="'关闭'"
              @click="isModalVisible = false"
            >
              <AppIcon icon="lucide:x" />
            </button>
          </div>
        </div>
      </template>

      <div class="setting-drawer-scroll">
        <div
          class="setting-tabs-section"
          :class="{ 'is-sticky': config.settingTabsSticky }"
        >
          <div
            class="setting-tabs-wrap"
            :style="{ '--active-tab': tabs.findIndex(tab => tab.key === activeTab) }"
            role="tablist"
            :aria-label="'系统配置'"
          >
            <span class="setting-tab-indicator" aria-hidden="true"></span>
            <button
              v-for="tab in tabs"
              :id="`preference-tab-${tab.key}`"
              :key="tab.key"
              type="button"
              class="setting-tab-trigger"
              :class="{ 'is-active': activeTab === tab.key }"
              role="tab"
              aria-controls="preference-panel"
              :tabindex="activeTab === tab.key ? 0 : -1"
              :aria-selected="activeTab === tab.key"
              @click="activeTab = tab.key"
              @keydown="handleTabKeydown($event, tab.key)"
            >
              <AppIcon :icon="tab.icon" />
              <span>{{ tab.label }}</span>
            </button>
          </div>
        </div>

        <div id="preference-panel" class="setting-tab-content" role="tabpanel" :aria-labelledby="`preference-tab-${activeTab}`">
          <Transition name="preference-tab" mode="out-in">
            <div :key="activeTab">
              <AppearanceSettings v-if="activeTab === 'appearance'" />
              <LayoutSettings v-else-if="activeTab === 'layout'" />
              <ShortcutSettings v-else-if="activeTab === 'shortcuts'" />
              <GeneralSettings v-else :active="isModalVisible" />
            </div>
          </Transition>
        </div>
      </div>

      <template #footer>
        <div class="setting-drawer-footer">
          <div
            class="setting-save-state"
            :class="`is-${saveStatus}`"
            role="status"
          >
            <AppIcon :icon="saveStatusIcon" :spin="saveStatus === 'saving'" />
            <span>{{ saveStatusLabel }}</span>
            <button
              v-if="saveStatus === 'error'"
              type="button"
              class="setting-retry"
              @click="retry"
            >
              {{ '重试' }}
            </button>
          </div>
          <div class="setting-footer-actions">
            <el-button
              type="primary"
              :icon="createIconifyIcon('lucide:copy')"
              @click="copyConfig"
            >
              {{ '复制配置' }}
            </el-button
            >
            <el-button
              :icon="createIconifyIcon('lucide:rotate-ccw')"
              @click="handleResetConfig"
            >
              {{ '恢复默认' }}
            </el-button
            >
          </div>
        </div>
      </template>
    </el-drawer>
  </div>
</template>

<script setup lang="ts">
  import AppIcon from '@/components/AppIcon.vue'
  import { createIconifyIcon } from '@/components/icons'
  import { ref, computed, nextTick, onMounted, onBeforeUnmount } from 'vue'
  import { storeToRefs } from 'pinia'
  import { ElMessage, ElMessageBox } from 'element-plus'
  import { useAppStore } from '@/pinia'
  import { usePreferencesPersistence } from '@/composables/usePreferencesPersistence'
  import AppearanceSettings from './modules/appearance/index.vue'
  import LayoutSettings from './modules/layout/index.vue'
  import GeneralSettings from './modules/general/index.vue'
  import ShortcutSettings from './modules/shortcuts/index.vue'
  import { emitter } from '@/utils/bus'
  defineOptions({
    name: 'SysSetting'
  })
  withDefaults(defineProps<{
    /** 是否显示打开或展开入口。 */
    showTrigger?: boolean
  }>(), { showTrigger: true })

  const appStore = useAppStore()
  const { config, device } = storeToRefs(appStore)
  const { saveStatus, retry } = usePreferencesPersistence()

  /**
   * 应用偏好设置的配置结构或可选布局项。
   */
  type SettingTab = 'appearance' | 'layout' | 'shortcuts' | 'general'
  const activeTab = ref<SettingTab>('appearance')
  const isModalVisible = ref(false)
  const trigger = ref<HTMLButtonElement>()
  let returnFocus: HTMLElement | null = null
  const closeButton = ref<HTMLButtonElement>()

  const tabs = computed(() => [
    {
      key: 'appearance' as const,
      label: '外观',
      icon: 'lucide:palette'
    },
    {
      key: 'layout' as const,
      label: '布局',
      icon: 'lucide:panels-top-left'
    },
    {
      key: 'general' as const,
      label: '通用',
      icon: 'lucide:settings-2'
    },
    {
      key: 'shortcuts' as const,
      label: '快捷键',
      icon: 'lucide:keyboard'
    }
  ])

  const width = computed(() => (device.value === 'mobile' ? '100vw' : '480px'))

  /**
   * 按当前键盘事件执行选择、确认或关闭动作。
   * @param event 触发当前交互的事件。
   * @param key 当前查询、映射或缓存使用的键。
   */
  function handleTabKeydown(event: KeyboardEvent, key: SettingTab): void {
    const index = tabs.value.findIndex(tab => tab.key === key)
    let next: number
    if (event.key === 'ArrowRight') next = (index + 1) % tabs.value.length
    else if (event.key === 'ArrowLeft') next = (index + tabs.value.length - 1) % tabs.value.length
    else if (event.key === 'Home') next = 0
    else if (event.key === 'End') next = tabs.value.length - 1
    else return
    event.preventDefault()
    activeTab.value = tabs.value[next].key
    void nextTick(() => document.getElementById(`preference-tab-${activeTab.value}`)?.focus())
  }
  const saveStatusIcon = computed(
    () =>
      ({
        idle: 'lucide:cloud',
        saving: 'lucide:loader-circle',
        saved: 'lucide:cloud-check',
        error: 'lucide:cloud-alert'
      })[saveStatus.value]
  )
  const saveStatusLabel = computed(() =>
    ({ idle: '', saving: '保存中', saved: '已保存', error: '保存失败' } as Record<string, string>)[saveStatus.value] ?? ''
  )

  /**
   * 打开当前页面的设置面板。
   */
  function openSettingModal() {
    returnFocus = document.activeElement instanceof HTMLElement ? document.activeElement : null
    isModalVisible.value = true
  }

  onMounted(() => emitter.on('openPreferences', openSettingModal))
  onBeforeUnmount(() => emitter.off('openPreferences', openSettingModal))

  /**
   * 将焦点移到当前输入、密码字段或抽屉内容。
   */
  const focusDrawer = (): void => {
    void nextTick(() => closeButton.value?.focus())
  }
  /**
   * 关闭菜单或弹层后将焦点还给触发控件。
   */
  const restoreTriggerFocus = (): void => {
    const target = returnFocus?.isConnected && returnFocus.getClientRects().length ? returnFocus :
      trigger.value ?? document.querySelector<HTMLElement>('[data-toolbar-menu-trigger]')
    target?.focus({ preventScroll: true })
  }

  /**
   * 将当前配置恢复为初始值。
   */
  async function handleResetConfig(): Promise<void> {
    try {
      await ElMessageBox.confirm(
        '确定要恢复默认设置吗？当前个性化配置将被清空，且此操作不可撤销。',
        '恢复默认设置',
        {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        }
      )
      appStore.resetConfig()
      ElMessage.success('已恢复默认设置')
    } catch {
      // Closing the confirmation keeps the current preferences unchanged.
    }
  }

  /**
   * 复制当前配置的可导出表示。
   */
  async function copyConfig(): Promise<void> {
    try {
      await navigator.clipboard.writeText(JSON.stringify(config.value, null, 2))
      ElMessage.success('配置已复制')
    } catch {
      ElMessage.error('无法复制配置')
    }
  }
</script>

<style lang="scss">
  .ape-theme-drawer.el-drawer {
    --preference-radius: 12px;
    display: flex;
    flex-direction: column;
    box-sizing: border-box;
    width: min(480px, 100vw);
    max-width: 100vw;
    // Resize immediately; animating width/max-width briefly pushes the drawer outside the viewport.
    transition-property: transform;
    overflow: hidden;
    background: var(--app-surface);
    font-family: var(--app-font-sans);
    box-shadow: var(--app-shadow-overlay);
  }
  .ape-theme-drawer.el-drawer .preference-choice .preference-preview-frame {
    position: relative;
    overflow: visible;
    border-color: transparent;
    border-radius: var(--preference-radius);
    outline: 1px solid var(--app-border);
    box-shadow: none;
    transition: none;
  }
  .ape-theme-drawer.el-drawer .preference-choice[aria-checked='true'] .preference-preview-frame {
    outline: 2px solid var(--el-color-primary);
  }
  // Like the reference, the outline grows from the center and stays while hovered.
  .ape-theme-drawer.el-drawer .preference-preview-frame::after {
    content: '';
    position: absolute;
    z-index: 1;
    top: 50%;
    left: 50%;
    width: 1px;
    height: 0;
    border-radius: calc(var(--preference-radius) - 2px);
    outline: 2px solid transparent;
    opacity: 0;
    pointer-events: none;
    transition: width 300ms cubic-bezier(0.4, 0, 0.2, 1),
      height 300ms cubic-bezier(0.4, 0, 0.2, 1),
      top 300ms cubic-bezier(0.4, 0, 0.2, 1),
      left 300ms cubic-bezier(0.4, 0, 0.2, 1),
      opacity 300ms cubic-bezier(0.4, 0, 0.2, 1),
      outline-color 300ms cubic-bezier(0.4, 0, 0.2, 1);
  }
  .ape-theme-drawer.el-drawer .preference-choice:not([aria-checked='true']):not([aria-disabled='true']):hover .preference-preview-frame::after,
  .ape-theme-drawer.el-drawer .preference-choice:not([aria-checked='true']):focus-visible .preference-preview-frame::after {
    top: -1px;
    left: -1px;
    width: calc(100% + 2px);
    height: calc(100% + 2px);
    opacity: 1;
    outline-color: var(--el-color-primary);
  }
  .ape-theme-drawer.el-drawer .preference-choice[aria-checked='true'] .preference-preview-frame::after {
    display: none;
  }
  .ape-theme-drawer .el-drawer__header, .ape-theme-drawer .el-drawer__footer {
    flex: none;
    margin: 0;
    padding: 0;
    border: 0;
  }
  .ape-theme-drawer .el-drawer__body {
    flex: 1;
    min-height: 0;
    padding: 0;
    overflow: hidden;
  }
  .ape-theme-drawer .preference-panel {
    display: grid;
    gap: 34px;
  }
  .ape-theme-drawer .preference-section {
    display: grid;
    gap: 16px;
    min-width: 0;
  }
  .ape-theme-drawer .preference-section-heading {
    display: flex;
    align-items: center;
    gap: 9px;
    color: var(--app-text-muted);
  }
  .ape-theme-drawer .preference-section-heading > .app-icon {
    width: 18px;
    height: 18px;
  }
  .ape-theme-drawer .preference-section-heading h3 {
    margin: 0;
    color: var(--app-text-primary);
    font-size: 18px;
    line-height: 1.5;
    font-weight: 650;
  }
  .ape-theme-drawer .preference-group {
    min-width: 0;
  }
  .ape-theme-drawer .ape-theme-setting-item {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    min-height: 58px;
    padding: 10px 0;
  }
  .ape-theme-drawer .ape-theme-setting-label {
    color: var(--app-text-primary);
    font-size: 14px;
    line-height: 1.6;
    font-weight: 500;
  }
  .ape-theme-drawer .setting-help-text, .ape-theme-drawer .setting-description, .ape-theme-drawer .setting-value, .ape-theme-drawer .setting-muted {
    color: var(--app-text-muted);
    font-size: 12px;
    line-height: 1.6;
  }
  .ape-theme-drawer .setting-controls .el-select {
    width: 170px;
  }
  .ape-theme-drawer .setting-controls .el-input-number {
    width: 150px;
  }
  .ape-theme-drawer .setting-controls .el-input__wrapper, .ape-theme-drawer .setting-controls .el-select__wrapper {
    min-height: 36px;
  }
  .ape-theme-drawer .el-switch {
    --el-switch-on-color: var(--el-color-primary);
  }
  .ape-theme-drawer ::-webkit-scrollbar {
    width: 5px;
  }
  .ape-theme-drawer ::-webkit-scrollbar-thumb {
    border-radius: 4px;
    background: var(--app-border-strong);
  }
  .el-drawer-fade-enter-active .ape-theme-drawer {
    transition: transform 500ms cubic-bezier(0.22, 1, 0.36, 1) !important;
  }
  .el-drawer-fade-leave-active .ape-theme-drawer {
    transition: transform 300ms cubic-bezier(0.4, 0, 1, 1) !important;
  }
  @media (max-width: 400px) {
    .ape-theme-drawer .setting-controls .el-select {
      width: 140px;
    }
    .ape-theme-drawer .setting-controls .el-input-number {
      width: 128px;
    }
    .ape-theme-drawer .ape-theme-setting-item {
      gap: 10px;
    }
  }
  @media (prefers-reduced-motion: reduce) {
    .el-drawer-fade-enter-active .ape-theme-drawer, .el-drawer-fade-leave-active .ape-theme-drawer {
      transition: none !important;
    }
    .ape-theme-drawer *, .ape-theme-drawer *::before, .ape-theme-drawer *::after {
      animation: none !important;
      transition: none !important;
    }
  }
</style>

<style lang="scss" scoped>
  .setting-drawer-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 24px;
    border-bottom: 1px solid var(--app-border);
  }
  .setting-drawer-heading {
    min-width: 0;
  }
  .setting-drawer-title {
    margin: 0;
    color: var(--app-text-primary);
    font-size: 20px;
    font-weight: 650;
    line-height: 1.4;
  }
  .setting-drawer-subtitle {
    margin: 6px 0 0;
    font-size: 12px;
    line-height: 1.6;
    color: var(--app-text-muted);
  }
  .setting-header-actions {
    display: flex;
    align-items: center;
    gap: 2px;
    flex: none;
  }
  .setting-icon-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    padding: 0;
    border: 0;
    border-radius: 8px;
    background: transparent;
    color: var(--app-text-secondary);
    font-size: 18px;
    cursor: pointer;
    transition: background-color 180ms, color 180ms;
  }
  .setting-icon-button:hover {
    background: var(--app-surface-subtle);
    color: var(--app-text-primary);
  }
  .setting-icon-button.is-active {
    color: var(--el-color-primary);
  }
  .setting-icon-button:focus-visible, .setting-tab-trigger:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 2px;
  }
  .setting-drawer-scroll {
    height: 100%;
    overflow-y: auto;
    overflow-x: hidden;
    width: 100%;
    overscroll-behavior: contain;
    scrollbar-gutter: stable;
  }
  .setting-tabs-section {
    z-index: 2;
    padding: 18px 24px 8px;
    background: var(--app-surface);
  }
  .setting-tabs-section.is-sticky {
    position: sticky;
    top: 0;
  }
  .setting-tabs-wrap {
    position: relative;
    isolation: isolate;
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    padding: 4px;
    border: 1px solid var(--app-border);
    background: var(--app-surface-subtle);
    border-radius: 12px;
  }
  .setting-tab-indicator {
    position: absolute;
    z-index: -1;
    inset: 4px auto 4px 4px;
    width: calc((100% - 8px) / 4);
    border-radius: 8px;
    background: var(--app-surface);
    box-shadow: 0 1px 4px rgb(0 0 0 / 8%);
    transform: translateX(calc(var(--active-tab) * 100%));
    transition: transform 320ms cubic-bezier(0.22, 1, 0.36, 1);
  }
  .setting-tab-trigger {
    display: flex;
    gap: 6px;
    align-items: center;
    justify-content: center;
    min-width: 0;
    min-height: 40px;
    padding: 0 4px;
    border: 0;
    border-radius: 8px;
    background: transparent;
    color: var(--app-text-secondary);
    font: inherit;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: color 180ms;
  }
  .setting-tab-trigger > .app-icon {
    width: 16px;
    height: 16px;
  }
  .setting-tab-trigger:hover, .setting-tab-trigger.is-active {
    color: var(--el-color-primary);
  }
  .setting-tab-trigger.is-active {
    font-weight: 650;
  }
  .setting-tab-content {
    padding: 24px 24px 40px;
  }
  .preference-tab-enter-active {
    transition: opacity 180ms ease, transform 180ms ease;
  }
  .preference-tab-leave-active {
    transition: opacity 100ms ease;
  }
  .preference-tab-enter-from {
    opacity: 0;
    transform: translateX(8px);
  }
  .preference-tab-leave-to {
    opacity: 0;
  }
  .setting-drawer-footer {
    display: grid;
    gap: 12px;
    padding: 14px 24px 20px;
    border-top: 1px solid var(--app-border);
    background: var(--app-surface);
  }
  .setting-save-state {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    min-height: 18px;
    color: var(--app-text-muted);
    font-size: 12px;
  }
  .setting-save-state.is-error {
    color: var(--el-color-danger);
  }
  .setting-save-state.is-saved {
    color: var(--el-color-success);
  }
  .setting-retry {
    padding: 0;
    border: 0;
    background: transparent;
    color: inherit;
    font: inherit;
    text-decoration: underline;
    cursor: pointer;
  }
  .setting-footer-actions {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
  }
  .setting-footer-actions :deep(.el-button) {
    width: 100%;
    height: 40px;
    margin: 0;
    border-radius: 8px;
  }
  @media (max-width: 400px) {
    .setting-drawer-header {
      padding: 20px 16px;
    }
    .setting-drawer-title {
      font-size: 18px;
    }
    .setting-tabs-section, .setting-tab-content, .setting-drawer-footer {
      padding-inline: 16px;
    }
    .setting-tab-trigger {
      font-size: 13px;
      gap: 4px;
    }
    .setting-tab-trigger > .app-icon {
      width: 14px;
      height: 14px;
    }
  }
</style>
