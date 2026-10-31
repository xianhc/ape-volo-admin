<template>
  <div class="layout-mode-grid" role="radiogroup" :aria-label="'布局模式'">
    <button
      v-for="layout in layoutModes"
      :key="layout.value"
      type="button"
      class="layout-mode-option preference-choice"
      :class="{ 'is-selected': modelValue === layout.value }"
      role="radio"
      :aria-checked="modelValue === layout.value"
      @click="emit('update:modelValue', layout.value)"
    >
      <span class="layout-preview preference-preview-frame" :class="'layout-preview-' + layout.value" aria-hidden="true">
        <span class="mini-sidebar"><i></i><i></i><i></i><i></i></span>
        <span class="mini-secondary"><i></i><i></i><i></i></span>
        <span class="mini-header"><i></i><i></i><i></i><i></i></span>
        <span class="mini-content"><i></i><i></i><i></i></span>
      </span>
      <span class="layout-mode-caption">
        <span>{{ layout.label }}</span>
        <el-tooltip :content="layout.description" placement="top">
          <AppIcon icon="lucide:circle-help" class="layout-help-icon" />
        </el-tooltip>
      </span>
    </button>
  </div>
</template>

<script setup lang="ts">
  /**
   * 布局模式卡片组件。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { computed } from 'vue'
  import { MenuNavigationType } from '@/enums/MenuNavigationType'
  defineOptions({
    name: 'LayoutModeCard'
  })

  withDefaults(
    defineProps<{
      /** 由父组件持有的受控绑定值。 */
      modelValue?: MenuNavigationType
    }>(),
    {
      modelValue: MenuNavigationType.SidebarNav
    }
  )

  const emit = defineEmits<{
    /** 将新的绑定值通知给父组件。 */
    'update:modelValue': [layout: MenuNavigationType]
  }>()

  /**
   * 应用偏好设置的配置结构或可选布局项。
   */
  interface LayoutModeInfo {
    /** 当前菜单或控件的图标标识。 */
    icon: string
    /** 面向用户显示的标签。 */
    label: string
    /** 用于解释用途的描述。 */
    description: string
  }

  const layoutModeMap: Record<MenuNavigationType, LayoutModeInfo> = {
    [MenuNavigationType.SidebarNav]: {
      icon: 'svg:sidebar-nav',
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '垂直'
      },
      /**
       * 用于解释用途的描述。
       * @returns 当前语言下对应条目的显示文案。
       */
      get description() {
        return '侧边垂直菜单模式'
      }
    },
    [MenuNavigationType.TopNav]: {
      icon: 'svg:top-nav',
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '水平'
      },
      /**
       * 用于解释用途的描述。
       * @returns 当前语言下对应条目的显示文案。
       */
      get description() {
        return '水平菜单模式，菜单全部显示在顶部'
      }
    },
    [MenuNavigationType.DualSidebarNav]: {
      icon: 'svg:dual-sidebar-nav',
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '双列菜单'
      },
      /**
       * 用于解释用途的描述。
       * @returns 当前语言下对应条目的显示文案。
       */
      get description() {
        return '垂直双列菜单模式'
      }
    },
    [MenuNavigationType.CombinationNav]: {
      icon: 'svg:combination-nav',
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '顶部+侧栏'
      },
      /**
       * 用于解释用途的描述。
       * @returns 当前语言下对应条目的显示文案。
       */
      get description() {
        return '顶部选择一级菜单，侧栏显示其下级菜单'
      }
    },
    [MenuNavigationType.MixedNav]: {
      icon: 'svg:combination-nav',
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '混合导航'
      },
      /**
       * 用于解释用途的描述。
       * @returns 当前语言下对应条目的显示文案。
       */
      get description() {
        return '顶部完整菜单与完整侧栏同时显示'
      }
    },
    [MenuNavigationType.HeaderMixedNav]: {
      icon: 'svg:dual-sidebar-nav',
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '顶部双列'
      },
      /**
       * 用于解释用途的描述。
       * @returns 当前语言下对应条目的显示文案。
       */
      get description() {
        return '顶部菜单与双列侧栏组合显示'
      }
    },
    [MenuNavigationType.FullContent]: {
      icon: 'lucide:panels-top-left',
      /**
       * 面向用户显示的标签。
       * @returns 当前语言下对应条目的显示文案。
       */
      get label() {
        return '全内容'
      },
      /**
       * 用于解释用途的描述。
       * @returns 当前语言下对应条目的显示文案。
       */
      get description() {
        return '隐藏导航框架，仅保留内容与固定设置入口'
      }
    }
  }

  const layoutModeOrder: MenuNavigationType[] = [
    MenuNavigationType.SidebarNav,
    MenuNavigationType.TopNav,
    MenuNavigationType.DualSidebarNav,
    MenuNavigationType.CombinationNav,
    MenuNavigationType.MixedNav,
    MenuNavigationType.HeaderMixedNav,
    MenuNavigationType.FullContent
  ]

  const layoutModes = computed(() => layoutModeOrder.map((value) => ({
    value,
    ...layoutModeMap[value]
  })))
</script>

<style scoped>
  .layout-mode-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 22px 18px;
  }
  .layout-mode-option {
    display: grid;
    gap: 10px;
    min-width: 0;
    padding: 0;
    border: 0;
    background: transparent;
    color: var(--app-text-secondary);
    font: inherit;
    cursor: pointer;
  }
  .layout-preview {
    position: relative;
    display: block;
    height: 86px;
    overflow: hidden;
    border: 1px solid var(--app-border);
    border-radius: var(--preference-radius, 10px);
    background: var(--app-surface-subtle);
    transition: border-color 180ms, box-shadow 180ms;
  }
  .layout-mode-option:hover .layout-preview {
    border-color: var(--el-color-primary);
  }
  .layout-mode-option.is-selected .layout-preview {
    border-color: var(--el-color-primary);
    box-shadow: 0 0 0 1px var(--el-color-primary);
  }
  .layout-mode-option:focus-visible {
    outline: 2px solid var(--el-color-primary);
    outline-offset: 4px;
    border-radius: 10px;
  }
  .layout-mode-caption {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 4px;
    font-size: 12px;
    line-height: 1.5;
  }
  .layout-help-icon {
    flex: none;
    width: 12px;
    height: 12px;
    color: var(--app-text-muted);
  }
  .mini-sidebar, .mini-secondary {
    position: absolute;
    display: flex;
    flex-direction: column;
    gap: 7px;
    inset: 5px auto 5px 5px;
    width: 25px;
    padding: 6px 4px;
    background: var(--el-color-primary);
    border-radius: 3px;
  }
  .mini-sidebar i {
    height: 3px;
    background: rgb(255 255 255 / 75%);
    border-radius: 2px;
  }
  .mini-sidebar i:first-child {
    width: 9px;
    height: 9px;
    margin-bottom: 2px;
    background: white;
  }
  .mini-secondary {
    display: none;
    background: var(--app-border-strong);
  }
  .mini-secondary i {
    height: 4px;
    background: var(--app-surface);
    border-radius: 2px;
  }
  .mini-header {
    position: absolute;
    display: flex;
    align-items: center;
    gap: 5px;
    inset: 5px 5px auto 34px;
    height: 12px;
    padding: 3px;
    border-radius: 2px;
    background: var(--app-border-strong);
  }
  .mini-header i {
    width: 9px;
    height: 3px;
    background: var(--app-surface);
    border-radius: 2px;
  }
  .mini-header i:first-child {
    width: 5px;
    height: 5px;
    margin-right: auto;
  }
  .mini-content {
    position: absolute;
    display: grid;
    grid-template-columns: 1fr 1.3fr;
    grid-template-rows: 1fr 1fr;
    gap: 4px;
    inset: 22px 5px 5px 34px;
  }
  .mini-content i {
    background: var(--app-border);
    border-radius: 3px;
  }
  .mini-content i:last-child {
    grid-column: span 2;
  }
  .layout-preview-2 .mini-sidebar, .layout-preview-7 .mini-sidebar {
    display: none;
  }
  .layout-preview-2 .mini-header {
    left: 5px;
    background: var(--el-color-primary);
  }
  .layout-preview-2 .mini-content {
    left: 5px;
  }
  .layout-preview-3 .mini-sidebar, .layout-preview-6 .mini-sidebar {
    width: 13px;
    padding-inline: 3px;
  }
  .layout-preview-3 .mini-sidebar i, .layout-preview-6 .mini-sidebar i {
    max-width: 7px;
  }
  .layout-preview-3 .mini-secondary, .layout-preview-6 .mini-secondary {
    display: flex;
    left: 21px;
    width: 16px;
    padding-inline: 3px;
  }
  .layout-preview-3 .mini-header, .layout-preview-6 .mini-header {
    left: 41px;
  }
  .layout-preview-3 .mini-content, .layout-preview-6 .mini-content {
    left: 41px;
  }
  .layout-preview-4 .mini-header {
    left: 5px;
  }
  .layout-preview-4 .mini-sidebar {
    top: 21px;
  }
  .layout-preview-5 .mini-header, .layout-preview-6 .mini-header {
    background: var(--el-color-primary);
  }
  .layout-preview-5 .mini-sidebar {
    background: var(--app-border-strong);
  }
  .layout-preview-5 .mini-sidebar i {
    background: var(--app-surface);
  }
  .layout-preview-7 .mini-header {
    display: none;
  }
  .layout-preview-7 .mini-content {
    inset: 7px;
  }
  @media (max-width: 400px) {
    .layout-mode-grid {
      gap: 20px 12px;
    }
    .layout-preview {
      height: 74px;
    }
    .layout-mode-caption {
      font-size: 11px;
      gap: 3px;
    }
  }
</style>
