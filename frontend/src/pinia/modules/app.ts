import { defineStore } from 'pinia'
import { computed, ref, watchEffect, reactive, watch, nextTick } from 'vue'
import { setBodyPrimaryColor, setCornerSize } from '@/services/appearance/themeRuntime'
import { useColorMode, usePreferredDark } from '@vueuse/core'
import { transitionTheme } from '@/services/appearance/themeTransition'
import {
  MenuNavigationType,
  hasDualSidebarNavigation,
  hasSidebarNavigation
} from '@/enums/MenuNavigationType'
import { BorderRadiusType } from '@/enums/BorderRadiusType'
import type { TransitionTypeValue } from '@/enums/TransitionType'
import { ComponentSizeType } from '@/enums/ComponentSizeType'
import { writeStoredText } from '@/services/storage/appStorage'
import {
  createDefaultPreferences,
  normalizePreferences,
  readPreferences
} from '@/services/appearance/preferences'
import type { AppConfig } from '@/services/appearance/preferences.types'

/**
 * 应用程序全局状态管理 Store
 * @description 管理应用的主题、布局、设备类型等全局配置
 * @returns {Object} 应用状态和相关方法
 */
export const useAppStore = defineStore('app', () => {
  // 设备和界面状态
  const device = ref<string>('')
  const sidebarManuallyCollapsed = ref(false)
  const sidebarHovered = ref(false)
  const sidebarKeyboardFocused = ref(false)
  const drawerSize = ref<string>('')
  const operateMinWith = ref<string>('240')
  const workspaceMaximized = ref(false)
  /**
   * 切换工作区的最大化展示状态。
   */
  const toggleWorkspaceMaximized = () => {
    workspaceMaximized.value = !workspaceMaximized.value
  }

  // 默认配置对象
  const defaultConfig = createDefaultPreferences(localStorage)

  // 响应式配置对象
  const config = reactive<AppConfig>(
    readPreferences(localStorage, defaultConfig)
  )
  const sidebarPinned = computed({
    /**
     * 读取当前注入能力持有的数据或上下文。
     * @returns 当前受控属性或配置的值。
     */
    get: () => config.sidebarPinned,
    /**
     * 通过注入的适配器写入当前数据。
     * @param value 本次转换、校验或更新所接收的值。
     */
    set: (value: boolean) => {
      config.sidebarPinned = value
    }
  })

  const sidebarAutoHide = computed(
    () =>
      device.value === 'desktop' &&
      !sidebarPinned.value &&
      hasSidebarNavigation(config.sideMode)
  )
  const sidebarCollapsed = computed(() =>
    sidebarAutoHide.value
      ? !sidebarHovered.value && !sidebarKeyboardFocused.value
      : sidebarManuallyCollapsed.value
  )
  const sidebarReservedWidth = computed(() => {
    const sideWidth =
      sidebarAutoHide.value || sidebarCollapsed.value
        ? config.layoutSideCollapsedWidth
        : config.layoutSideWidth
    const firstColumnWidth =
      device.value !== 'mobile' && hasDualSidebarNavigation(config.sideMode)
        ? config.layoutSideCollapsedWidth
        : 0
    return sideWidth + firstColumnWidth
  })

  // 悬停只临时展开侧栏，正文和顶部区域始终保留收起后的宽度。
  watch(
    [device, () => config.sideMode],
    () => {
      sidebarManuallyCollapsed.value = device.value === 'mobile'
      sidebarHovered.value = false
      sidebarKeyboardFocused.value = false
    },
    { immediate: true }
  )
  /**
   * 切换侧栏折叠或固定展开状态。
   */
  const toggleSidebar = (): void => {
    if (sidebarAutoHide.value) {
      sidebarHovered.value = sidebarCollapsed.value
      sidebarKeyboardFocused.value = false
    } else {
      sidebarManuallyCollapsed.value = !sidebarManuallyCollapsed.value
    }
  }
  /**
   * 切换侧栏折叠或固定展开状态。
   */
  const toggleSidebarPinned = (): void => {
    sidebarManuallyCollapsed.value = sidebarCollapsed.value
    sidebarPinned.value = !sidebarPinned.value
    sidebarHovered.value = false
    sidebarKeyboardFocused.value = false
  }
  /**
   * 更新侧栏的焦点或指针悬浮状态。
   * @param hovered 指针是否位于当前侧栏内。
   */
  const setSidebarHover = (hovered: boolean): void => {
    sidebarHovered.value = hovered
  }
  /**
   * 更新侧栏的焦点或指针悬浮状态。
   * @param focused 焦点是否位于当前侧栏内。
   */
  const setSidebarFocus = (focused: boolean): void => {
    sidebarKeyboardFocused.value = focused
  }

  // 暗黑模式管理
  const colorMode = useColorMode({
    selector: 'html',
    attribute: 'class',
    modes: { dark: 'dark', light: 'light' },
    initialValue: config.darkMode,
    emitAuto: true,
    storageKey: null,
    disableTransition: false
  })

  // 系统偏好的暗黑模式
  const preferredDark = usePreferredDark()
  const isDark = computed(() =>
    config.darkMode === 'auto'
      ? preferredDark.value
      : config.darkMode === 'dark'
  )
  const themeTransitioning = ref(false)

  /**
   * 切换主题模式
   * @param {boolean} darkMode - 是否为暗黑模式
   * @param {Event} [event] - 主题切换的触发事件
   * @param event 浏览器存储变化事件或主题切换的触发事件。
   */
  const toggleTheme = (darkMode: boolean, event?: Event): Promise<void> =>
    toggleDarkMode(darkMode ? 'dark' : 'light', event)

  /**
   * 切换弱视模式
   * @param {boolean} e - 是否启用弱视模式
   */
  const toggleWeakness = (e: boolean): void => {
    config.weakness = e
  }

  /**
   * 切换灰度模式
   * @param {boolean} e - 是否启用灰度模式
   */
  const toggleGrey = (e: boolean): void => {
    config.grey = e
  }

  /**
   * 切换主色调
   * @param {string} e - 主色调颜色值
   */
  const togglePrimaryColor = (e: string): void => {
    config.primaryColor = e
  }

  /**
   * 切换标签页显示
   * @param {boolean} e - 是否显示标签页
   */
  const toggleTabs = (e: boolean): void => {
    config.showTabs = e
  }

  /**
   * 切换设备类型
   * @param {string} e - 设备类型 ('mobile' | 'desktop')
   */
  const toggleDevice = (e: string): void => {
    if (e === 'mobile') {
      drawerSize.value = '100%'
      operateMinWith.value = '60'
    } else {
      drawerSize.value = '800'
      operateMinWith.value = '80'
    }
    device.value = e
  }

  /**
   * 切换暗黑模式配置
   * @param {string} mode - 暗黑模式设置 ('auto' | 'dark' | 'light')
   * @param {Event} [event] - 主题切换的触发事件
   * @param event 浏览器存储变化事件或主题切换的触发事件。
   */
  async function toggleDarkMode(
    mode: AppConfig['darkMode'],
    event?: Event
  ): Promise<void> {
    if (themeTransitioning.value) return
    const dark = mode === 'auto' ? preferredDark.value : mode === 'dark'
    if (dark === isDark.value) {
      config.darkMode = mode
      return
    }
    themeTransitioning.value = true
    try {
      await transitionTheme(
        async () => {
          config.darkMode = mode
          await nextTick()
        },
        dark,
        event
      )
    } finally {
      themeTransitioning.value = false
    }
  }

  // 监听暗黑模式配置变化
  watchEffect(() => {
    writeStoredText(localStorage, 'vueuse-color-scheme', config.darkMode)
    colorMode.value = config.darkMode
  })

  /**
   * 切换侧边栏宽度
   * @param {number} e - 侧边栏宽度值
   */
  const toggleConfigSideWidth = (e: number): void => {
    config.layoutSideWidth = e
  }

  /**
   * 切换侧边栏收缩宽度
   * @param {number} e - 侧边栏收缩宽度值
   */
  const toggleConfigSideCollapsedWidth = (e: number): void => {
    config.layoutSideCollapsedWidth = e
  }

  /**
   * 切换侧边栏项目高度
   * @param {number} e - 侧边栏项目高度值
   */
  const toggleConfigSideItemHeight = (e: number): void => {
    config.layoutSideItemHeight = e
  }

  /**
   * 切换水印显示
   * @param {boolean} e - 是否显示水印
   */
  const toggleConfigWatermark = (e: boolean): void => {
    config.showWatermark = e
  }

  /**
   * 切换侧边栏模式
   * @param {MenuNavigationType} e - 侧边栏模式
   */
  const toggleSideMode = (e: MenuNavigationType): void => {
    config.sideMode = e
  }

  /**
   * 切换过渡动画类型
   * @param {string} e - 过渡动画类型
   */
  const toggleTransition = (e: TransitionTypeValue): void => {
    config.transitionType = e
  }

  /**
   * 切换圆角大小
   * @param {BorderRadiusType} e - 圆角大小类型
   */
  const toggleCornerSize = (e: BorderRadiusType): void => {
    config.cornerSize = e
  }

  /**
   * 切换组件尺寸
   * @param {ComponentSizeTypeValue} e - 组件尺寸类型
   */
  const toggleComponentSize = (e: ComponentSizeType): void => {
    config.componentSize = e
  }

  /**
   * 重置配置到默认值
   */
  const resetConfig = (): void => {
    Object.assign(config, normalizePreferences(defaultConfig, defaultConfig))
  }

  /**
   * 使用校验通过的配置替换当前编辑内容。
   * @param value 本次转换、校验或更新所接收的值。
   */
  const replaceConfig = (value: unknown): void => {
    Object.assign(config, normalizePreferences(value, defaultConfig))
  }

  // 监听弱视和灰度模式变化，应用到DOM
  watchEffect(() => {
    document.documentElement.classList.toggle('html-weakenss', config.weakness)
    document.documentElement.classList.toggle('html-grey', config.grey)
  })

  // 监听主色调变化，应用主题色
  watchEffect(() => {
    setBodyPrimaryColor(config.primaryColor, isDark.value ? 'dark' : 'light')
  })

  // 监听圆角大小变化，应用圆角样式
  watchEffect(() => {
    setCornerSize(config.cornerSize)
  })

  watchEffect(() => {
    document.documentElement.style.setProperty(
      '--app-font-size-base',
      `${config.fontSize}px`
    )
  })

  return {
    isDark,
    workspaceMaximized,
    toggleWorkspaceMaximized,
    themeTransitioning,
    device,
    sidebarCollapsed,
    sidebarPinned,
    sidebarAutoHide,
    sidebarReservedWidth,
    drawerSize,
    operateMinWith,
    config,
    toggleTheme,
    toggleDevice,
    toggleSidebar,
    toggleSidebarPinned,
    setSidebarHover,
    setSidebarFocus,
    toggleWeakness,
    toggleGrey,
    togglePrimaryColor,
    toggleTabs,
    toggleDarkMode,
    toggleConfigSideWidth,
    toggleConfigSideCollapsedWidth,
    toggleConfigSideItemHeight,
    toggleConfigWatermark,
    toggleSideMode,
    toggleTransition,
    toggleCornerSize,
    toggleComponentSize,
    resetConfig,
    replaceConfig
  }
})
