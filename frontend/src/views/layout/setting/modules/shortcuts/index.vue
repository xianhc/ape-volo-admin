<template>
  <div class="preference-panel">
    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:keyboard" />
        <h3>{{ '快捷键' }}</h3>
      </div>
      <div class="preference-group">
        <PreferenceItem
          :label="'启用快捷键'"
          :description="'统一控制当前已接入的全局快捷键'"
        >
          <el-switch v-model="config.enableShortcuts" />
        </PreferenceItem>
        <PreferenceItem
          :label="'菜单搜索'"
          :description="'在任意页面快速打开菜单搜索'"
        >
          <span class="shortcut-keys"
          ><kbd>Ctrl</kbd><span>/</span><kbd>⌘</kbd><span>+</span
          ><kbd>K</kbd></span
          >
        </PreferenceItem>
        <PreferenceItem :label="'偏好设置（Ctrl/⌘+Shift+P）'">
          <el-switch
            v-model="config.shortcutPreferences"
            :disabled="!config.enableShortcuts"
          />
        </PreferenceItem>
        <PreferenceItem :label="'锁屏（Ctrl/⌘+Shift+L）'">
          <el-switch
            v-model="config.shortcutLockScreen"
            :disabled="!config.enableShortcuts"
          />
        </PreferenceItem>
        <PreferenceItem :label="'退出（Ctrl/⌘+Shift+Q）'">
          <el-switch
            v-model="config.shortcutLogout"
            :disabled="!config.enableShortcuts"
          />
        </PreferenceItem>
        <PreferenceItem :label="'Esc 退出内容最大化'">
          <el-switch
            v-model="config.shortcutEscape"
            :disabled="!config.enableShortcuts"
          />
        </PreferenceItem>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:list-tree" />
        <h3>{{ '搜索内导航' }}</h3>
      </div>
      <div class="shortcut-hints">
        <span><kbd>↑</kbd><kbd>↓</kbd>{{ '导航' }}</span>
        <span><kbd>Enter</kbd>{{ '选择' }}</span>
        <span><kbd>Esc</kbd>{{ '关闭' }}</span>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
  /**
   * 快捷键设置页。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { useAppStore } from '@/pinia'
  import { storeToRefs } from 'pinia'
  import PreferenceItem from '../../components/PreferenceItem.vue'

  defineOptions({ name: 'ShortcutSettings' })
  const { config } = storeToRefs(useAppStore())
</script>

<style scoped>
  .shortcut-keys,
  .shortcut-hints,
  .shortcut-hints span {
    display: flex;
    align-items: center;
  }
  .shortcut-keys {
    gap: 4px;
    color: var(--app-text-muted);
    font-size: 12px;
  }
  kbd {
    min-width: 24px;
    padding: 3px 6px;
    border: 1px solid var(--app-border);
    border-radius: 4px;
    background: var(--app-surface-subtle);
    color: var(--app-text-primary);
    font: inherit;
    font-size: 11px;
    text-align: center;
    box-shadow: 0 1px 0 var(--app-border-strong);
  }
  .shortcut-hints {
    flex-wrap: wrap;
    gap: 12px;
    padding: 12px;
    border: 1px solid var(--app-border);
    border-radius: var(--ape-volo-radius);
    color: var(--app-text-secondary);
    font-size: 12px;
  }
  .shortcut-hints span {
    gap: 5px;
  }
</style>
