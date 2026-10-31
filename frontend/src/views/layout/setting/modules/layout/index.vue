<template>
  <div class="preference-panel">
    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:panels-top-left" />
        <h3>{{ '布局模式' }}</h3>
      </div>
      <LayoutModeCard
        v-model="config.sideMode"
        @update:model-value="appStore.toggleSideMode"
      />
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:panel-left" />
        <h3>{{ '导航设置' }}</h3>
      </div>
      <div class="preference-group">
        <PreferenceItem
          :label="'固定侧栏'"
          :description="'固定展开侧栏；关闭后悬停展开'"
        >
          <el-switch
            v-model="config.sidebarPinned"
            :disabled="sidebarOptionsDisabled"
          />
        </PreferenceItem>
        <PreferenceItem
          :label="'显示面包屑'"
          :description="'在非顶部导航中显示当前路径'"
        >
          <el-switch
            v-model="config.showBreadcrumb"
            :disabled="breadcrumbOptionsDisabled"
          />
        </PreferenceItem>
        <PreferenceItem :label="'显示面包屑图标'">
          <el-switch
            v-model="config.showBreadcrumbIcon"
            :disabled="breadcrumbOptionsDisabled || !config.showBreadcrumb"
          />
        </PreferenceItem>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:container" />
        <h3>{{ '内容布局' }}</h3>
      </div>
      <div class="preference-group">
        <div class="content-mode-grid" role="radiogroup" :aria-label="'内容宽度模式'">
          <button v-for="option in contentModeOptions" :key="option.value" type="button" role="radio" class="content-mode-option preference-choice" :class="{ 'is-selected': config.contentMode === option.value }" :aria-checked="config.contentMode === option.value" @click="config.contentMode = option.value">
            <span class="content-mode-preview preference-preview-frame" :class="'is-' + option.value" aria-hidden="true"><span class="content-mini-header"></span><span class="content-mini-body"><i></i><i></i><i></i></span></span>
            <span>{{ option.label }}</span>
          </button>
        </div>
        <PreferenceItem :label="'紧凑内容宽度'">
          <el-input-number
            v-model="config.contentWidth"
            :min="800"
            :max="1600"
            :step="40"
            :disabled="config.contentMode !== 'compact'"
          />
        </PreferenceItem>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:panel-top" />
        <h3>{{ '标签栏设置' }}</h3>
      </div>
      <div class="preference-group">
        <PreferenceItem
          :label="'显示标签页'"
          :description="'页面标签导航'"
        >
          <el-switch v-model="config.showTabs"
          />
        </PreferenceItem>
        <PreferenceItem :label="'显示标签图标'"
        >
          <el-switch v-model="config.showTabIcon" :disabled="!config.showTabs"
          />
        </PreferenceItem>
        <PreferenceItem :label="'显示更多操作'"
        >
          <el-switch v-model="config.showTabMore" :disabled="!config.showTabs"
          />
        </PreferenceItem>
        <PreferenceItem :label="'显示刷新按钮'"
        >
          <el-switch
            v-model="config.showTabRefresh"
            :disabled="!config.showTabs"
          />
        </PreferenceItem>
        <PreferenceItem :label="'显示最大化按钮'"
        >
          <el-switch
            v-model="config.showTabMaximize"
            :disabled="!config.showTabs"
          />
        </PreferenceItem>
        <PreferenceItem :label="'允许拖拽排序'"
        >
          <el-switch
            v-model="config.tabDraggable"
            :disabled="!config.showTabs"
          />
        </PreferenceItem>
        <PreferenceItem :label="'标签样式'">
          <el-select
            v-model="config.tabStyle"
            class="compact-select"
            :disabled="!config.showTabs"
          >
            <el-option
              v-for="option in tabStyleOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </PreferenceItem>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:ruler" />
        <h3>{{ '尺寸配置' }}</h3>
      </div>
      <div class="preference-group">
        <PreferenceItem
          :label="'侧边栏展开宽度'"
          :description="'侧边栏完全展开时的宽度'"
        >
          <el-input-number
            v-model="config.layoutSideWidth"
            :min="160"
            :max="480"
            :step="8"
          />
        </PreferenceItem>
        <PreferenceItem
          :label="'侧边栏收缩宽度'"
          :description="'侧边栏收缩时的最小宽度'"
        >
          <el-input-number
            v-model="config.layoutSideCollapsedWidth"
            :min="48"
            :max="160"
            :step="8"
          />
        </PreferenceItem>
        <PreferenceItem
          :label="'菜单项高度'"
          :description="'侧边栏菜单项的行高'"
        >
          <el-input-number
            v-model="config.layoutSideItemHeight"
            :min="32"
            :max="80"
            :step="4"
          />
        </PreferenceItem>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:wrench" />
        <h3>{{ '小部件' }}</h3>
      </div>
      <ToolbarOrderEditor v-model="config.toolbarOrder" v-model:placements="config.toolbarPlacements" />
    </section>
  </div>
</template>

<script setup lang="ts">
  /**
   * 布局设置页。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { computed } from 'vue'
  import { storeToRefs } from 'pinia'
  import { useAppStore } from '@/pinia'
  import {
    MenuNavigationType,
    hasHeaderNavigation,
    hasSidebarNavigation
  } from '@/enums/MenuNavigationType'
  import LayoutModeCard from '../../components/LayoutModeCard.vue'
  import PreferenceItem from '../../components/PreferenceItem.vue'
  import ToolbarOrderEditor from '../../components/ToolbarOrderEditor.vue'
  defineOptions({
    name: 'LayoutSettings'
  })

  const appStore = useAppStore()
  const { config } = storeToRefs(appStore)

  const sidebarOptionsDisabled = computed(
    () => !hasSidebarNavigation(config.value.sideMode)
  )
  const breadcrumbOptionsDisabled = computed(
    () =>
      hasHeaderNavigation(config.value.sideMode) ||
      config.value.sideMode === MenuNavigationType.FullContent
  )
  const contentModeOptions = computed(() => [
    { label: '流式', value: 'wide' as const },
    { label: '定宽', value: 'compact' as const }
  ])
  const tabStyleOptions = computed(() => [
    { label: 'Chrome', value: 'chrome' },
    { label: '卡片', value: 'card' },
    { label: '简洁', value: 'plain' },
    { label: '轻快', value: 'brisk' }
  ])
</script>

<style scoped>
  .content-mode-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 18px;
    margin-bottom: 12px;
  }
  .content-mode-option {
    display: grid;
    gap: 10px;
    padding: 0;
    border: 0;
    background: transparent;
    color: var(--app-text-secondary);
    font: inherit;
    font-size: 12px;
    cursor: pointer;
  }
  .content-mode-preview {
    display: grid;
    grid-template-rows: 12px 1fr;
    gap: 5px;
    height: 86px;
    padding: 5px;
    border: 1px solid var(--app-border);
    border-radius: var(--preference-radius, 10px);
    background: var(--app-surface-subtle);
    transition: border-color 180ms, box-shadow 180ms;
  }
  .content-mode-option:hover .content-mode-preview, .content-mode-option.is-selected .content-mode-preview {
    border-color: var(--el-color-primary);
    box-shadow: 0 0 0 1px var(--el-color-primary);
  }
  .content-mode-option:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 4px;
    border-radius: 10px;
  }
  .content-mini-header {
    border-radius: 2px;
    background: var(--el-color-primary);
  }
  .content-mini-body {
    display: grid;
    grid-template-columns: 1fr 1.3fr;
    gap: 4px;
  }
  .content-mini-body i {
    background: var(--app-border);
    border-radius: 3px;
  }
  .content-mini-body i:last-child {
    grid-column: span 2;
  }
  .is-compact .content-mini-body {
    margin-inline: 14px;
  }
  @media (max-width: 400px) {
    .content-mode-grid {
      gap: 12px;
    }
    .content-mode-preview {
      height: 74px;
    }
  }
</style>
