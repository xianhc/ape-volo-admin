/**
 * 界面偏好读写与默认值。
 */
import { BorderRadiusType } from '@/enums/BorderRadiusType'
import { ComponentSizeType } from '@/enums/ComponentSizeType'
import { MenuNavigationType } from '@/enums/MenuNavigationType'
import { TransitionType } from '@/enums/TransitionType'
import { readStoredJson, readStoredText } from '@/services/storage/appStorage'
import { DEFAULT_PRIMARY_COLOR } from '@/services/appearance/themeRuntime'
import type { AppConfig, ToolbarItemKey } from './preferences.types'

export const PREFERENCES_STORAGE_KEY = 'sysPreferences'

/**
 * 检查未知配置值是否满足目标结构的类型守卫。
 */
type ConfigValueGuard = (value: unknown) => boolean

export const TOOLBAR_ITEM_KEYS: ToolbarItemKey[] = [
  'menuSearch',
  'preferences',
  'theme',
  'fullscreen',
  'refresh',
  'lockScreen',
  'logout'
]

/**
 * 校验工具栏项目顺序是否满足配置约束。
 * @param value 本次转换、校验或更新所接收的值。
 * @returns 输入满足目标类型要求的必要字段与值约束时返回 true。
 */
const isToolbarOrder = (value: unknown): value is ToolbarItemKey[] =>
  Array.isArray(value) &&
  value.length === TOOLBAR_ITEM_KEYS.length &&
  new Set(value).size === TOOLBAR_ITEM_KEYS.length &&
  value.every((key) => TOOLBAR_ITEM_KEYS.includes(key as ToolbarItemKey))

const configValueGuards: Record<keyof AppConfig, ConfigValueGuard> = {
  /**
   * 校验色弱模式配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  weakness: (value) => typeof value === 'boolean',
  /**
   * 校验灰度模式配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  grey: (value) => typeof value === 'boolean',
  /**
   * 校验主题主色配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  primaryColor: (value) =>
    typeof value === 'string' && /^#[\da-f]{6}$/i.test(value),
  /**
   * 校验登录表单位置，避免未知布局导致页面区域缺失。
   * @param value 存储或导入配置提供的登录布局。
   * @returns 为 left、center 或 right 时返回 true。
   */
  loginLayout: (value) => ['left', 'center', 'right'].includes(String(value)),
  /**
   * 校验标签栏显示配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showTabs: (value) => typeof value === 'boolean',
  /**
   * 校验深浅色模式配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  darkMode: (value) => ['auto', 'dark', 'light'].includes(String(value)),
  /**
   * 校验展开侧栏宽度配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  layoutSideWidth: (value) =>
    typeof value === 'number' && value >= 160 && value <= 480,
  /**
   * 校验折叠侧栏宽度配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  layoutSideCollapsedWidth: (value) =>
    typeof value === 'number' && value >= 48 && value <= 160,
  /**
   * 校验侧栏项目高度配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  layoutSideItemHeight: (value) =>
    typeof value === 'number' && value >= 32 && value <= 80,
  /**
   * 校验水印显示配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showWatermark: (value) => typeof value === 'boolean',
  /**
   * 校验菜单导航模式配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  sideMode: (value) =>
    Object.values(MenuNavigationType).includes(value as MenuNavigationType),
  /**
   * 取得或应用当前页面切换动画类型。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  transitionType: (value) =>
    Object.values(TransitionType).includes(
      value as AppConfig['transitionType']
    ),
  /**
   * 校验圆角尺寸配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  cornerSize: (value) =>
    Object.values(BorderRadiusType).includes(value as BorderRadiusType),
  /**
   * 校验组件尺寸配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  componentSize: (value) =>
    Object.values(ComponentSizeType).includes(value as ComponentSizeType),
  /**
   * 取得或应用当前侧栏折叠或保持展开的状态。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  sidebarPinned: (value) => typeof value === 'boolean',
  /**
   * 校验设置标签固定配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  settingTabsSticky: (value) => typeof value === 'boolean',
  /**
   * 校验面包屑显示配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showBreadcrumb: (value) => typeof value === 'boolean',
  /**
   * 校验面包屑图标配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showBreadcrumbIcon: (value) => typeof value === 'boolean',
  /**
   * 校验标签图标配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showTabIcon: (value) => typeof value === 'boolean',
  /**
   * 校验标签更多菜单配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showTabMore: (value) => typeof value === 'boolean',
  /**
   * 校验标签刷新入口配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showTabRefresh: (value) => typeof value === 'boolean',
  /**
   * 校验标签最大化入口配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showTabMaximize: (value) => typeof value === 'boolean',
  /**
   * 判断是否显示底部操作区域。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  showFooter: (value) => typeof value === 'boolean',
  /**
   * 校验快捷键开关配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  enableShortcuts: (value) => typeof value === 'boolean',
  /**
   * 校验顶部栏混合深色配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  semiDarkHeader: (value) => typeof value === 'boolean',
  /**
   * 校验侧栏混合深色配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  semiDarkSidebar: (value) => typeof value === 'boolean',
  /**
   * 取得或应用当前验证码绘制使用的字体大小。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  fontSize: (value) => typeof value === 'number' && value >= 12 && value <= 20,
  /**
   * 校验内容布局模式配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  contentMode: (value) => value === 'wide' || value === 'compact',
  /**
   * 校验内容区域宽度配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  contentWidth: (value) =>
    typeof value === 'number' && value >= 800 && value <= 1600,
  /**
   * 校验动态页面标题配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  dynamicTitle: (value) => typeof value === 'boolean',
  /**
   * 校验标签样式配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  tabStyle: (value) =>
    ['card', 'chrome', 'plain', 'brisk'].includes(String(value)),
  /**
   * 校验标签拖动配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  tabDraggable: (value) => typeof value === 'boolean',
  /**
   * 校验工具栏位置配置配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  toolbarPlacements: (value) =>
    isObject(value) &&
    Object.keys(value).length === TOOLBAR_ITEM_KEYS.length &&
    TOOLBAR_ITEM_KEYS.every((key) =>
      ['header', 'userDropdown', 'hidden'].includes(String(value[key]))
    ) &&
    value.preferences !== 'hidden',
  toolbarOrder: isToolbarOrder,
  /**
   * 校验偏好设置快捷键配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  shortcutPreferences: (value) => typeof value === 'boolean',
  /**
   * 校验锁屏快捷键配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  shortcutLockScreen: (value) => typeof value === 'boolean',
  /**
   * 校验退出快捷键配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  shortcutLogout: (value) => typeof value === 'boolean',
  /**
   * 校验退出浮层快捷键配置的类型、允许值及范围。
   * @param value 本次转换、校验或更新所接收的值。
   * @returns 满足该偏好项的类型与范围约束时返回 true。
   */
  shortcutEscape: (value) => typeof value === 'boolean'
}

/**
 * 判断未知值是否为可读取属性的非空对象。
 * @param value 本次转换、校验或更新所接收的值。
 * @returns 未知值是否为可读取属性的非空对象时返回 true，否则返回 false。
 */
const isObject = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null && !Array.isArray(value)

/**
 * 生成完整界面偏好默认值，并沿用浏览器已保存的明暗模式。
 * @param storage 用于读取浏览器颜色模式的存储。
 * @returns 可独立修改的完整默认偏好配置。
 */
export function createDefaultPreferences(storage: Storage): AppConfig {
  const storedColorMode = readStoredText(storage, 'vueuse-color-scheme', 'auto')

  return {
    weakness: false,
    grey: false,
    primaryColor: DEFAULT_PRIMARY_COLOR,
    loginLayout: 'right',
    showTabs: true,
    darkMode:
      storedColorMode === 'dark' || storedColorMode === 'light'
        ? storedColorMode
        : 'auto',
    layoutSideWidth: 256,
    layoutSideCollapsedWidth: 80,
    layoutSideItemHeight: 48,
    showWatermark: true,
    sideMode: MenuNavigationType.SidebarNav,
    transitionType: TransitionType.Slide,
    cornerSize: BorderRadiusType.Small,
    componentSize: ComponentSizeType.Default,
    sidebarPinned: true,
    settingTabsSticky: true,
    showBreadcrumb: true,
    showBreadcrumbIcon: true,
    showTabIcon: true,
    showTabMore: true,
    showTabRefresh: true,
    showTabMaximize: true,
    showFooter: true,
    enableShortcuts: true,
    semiDarkHeader: false,
    semiDarkSidebar: false,
    fontSize: 14,
    contentMode: 'wide',
    contentWidth: 1200,
    dynamicTitle: true,
    tabStyle: 'chrome',
    tabDraggable: true,
    toolbarPlacements: {
      menuSearch: 'header',
      preferences: 'header',
      theme: 'header',
      fullscreen: 'header',
      refresh: 'header',
      lockScreen: 'header',
      logout: 'header'
    },
    toolbarOrder: [...TOOLBAR_ITEM_KEYS],
    shortcutPreferences: true,
    shortcutLockScreen: false,
    shortcutLogout: false,
    shortcutEscape: true
  }
}

/**
 * 仅采纳通过逐字段校验的偏好值，对缺失或无效字段使用默认值，并复制数组和对象。
 * @param value 来自存储或服务端的未知偏好数据。
 * @param defaults 缺失或无效字段使用的完整默认配置。
 * @returns 包含全部受支持字段的偏好配置，不直接复用输入的数组或对象。
 */
export function normalizePreferences(
  value: unknown,
  defaults: AppConfig
): AppConfig {
  const source = isObject(value) ? value : {}

  const preferences = {
    ...defaults,
    toolbarOrder: [...defaults.toolbarOrder],
    toolbarPlacements: { ...defaults.toolbarPlacements }
  }
  for (const key of Object.keys(configValueGuards) as (keyof AppConfig)[]) {
    const candidate = source[key]
    if (candidate !== undefined && configValueGuards[key](candidate)) {
      let normalizedValue: unknown
      if (Array.isArray(candidate)) {
        normalizedValue = [...candidate]
      } else {
        if (isObject(candidate)) {
          normalizedValue = { ...candidate }
        } else {
          normalizedValue = candidate
        }
      }
      Object.assign(preferences, {
        [key]: normalizedValue
      })
    }
  }
  return preferences
}

/**
 * 从浏览器存储读取偏好，并按字段校验后合并默认配置。
 * @param storage 保存界面偏好的浏览器存储。
 * @param defaults 字段缺失或无效时使用的完整默认配置。
 * @returns 已经校验并补齐默认字段的界面偏好。
 */
export function readPreferences(
  storage: Storage,
  defaults: AppConfig
): AppConfig {
  const stored = readStoredJson<Record<string, unknown>>(
    storage,
    PREFERENCES_STORAGE_KEY,
    {},
    isObject
  )
  return normalizePreferences(stored, defaults)
}

/**
 * 解析导入的偏好 JSON，拒绝缺少受支持字段或字段值无效的文档。
 * @param serialized 用户导入的偏好 JSON 文本。
 * @param defaults 导入文档未提供字段时使用的完整默认配置。
 * @returns 校验通过并与默认值合并后的偏好。
 */
export function parsePreferences(
  serialized: string,
  defaults: AppConfig
): AppConfig {
  const value: unknown = JSON.parse(serialized)
  if (!isObject(value)) throw new Error('Invalid preferences document')

  const entries = Object.entries(value).filter(
    ([key]) => key in configValueGuards
  )
  if (!entries.length) throw new Error('No supported preferences found')
  if (
    entries.some(
      ([key, candidate]) =>
        !configValueGuards[key as keyof AppConfig](candidate)
    )
  ) {
    throw new Error('Invalid preference value')
  }

  return normalizePreferences(value, defaults)
}

/**
 * 将完整界面偏好序列化为存储或导出使用的 JSON 文本。
 * @param config 需要保存或导出的完整偏好配置。
 * @returns 当前偏好配置的 JSON 文本。
 */
export const serializePreferences = (config: AppConfig): string =>
  JSON.stringify(config)
