/**
 * 界面偏好类型声明。
 */
import type { BorderRadiusType } from '@/enums/BorderRadiusType'
import type { ComponentSizeType } from '@/enums/ComponentSizeType'
import type { MenuNavigationType } from '@/enums/MenuNavigationType'
import type { TransitionTypeValue } from '@/enums/TransitionType'

/**
 * 界面颜色、主题、布局或动画偏好的取值。
 */
export type ContentLayoutMode = 'wide' | 'compact'
/** 桌面登录表单所在区域；移动端统一采用单列表单。 */
export type LoginLayout = 'left' | 'center' | 'right'
/**
 * 工具栏项目、位置或标签展示样式的取值。
 */
export type TabStyleType = 'card' | 'chrome' | 'plain' | 'brisk'
/**
 * 工具栏项目、位置或标签展示样式的取值。
 */
export type ToolbarPosition = 'header' | 'userDropdown' | 'hidden'
/**
 * 工具栏项目、位置或标签展示样式的取值。
 */
export type ToolbarItemKey =
  | 'menuSearch'
  | 'preferences'
  | 'theme'
  | 'fullscreen'
  | 'refresh'
  | 'lockScreen'
  | 'logout'

/**
 * 应用偏好设置的配置结构或可选布局项。
 */
export interface AppConfig {
  /** 弱项。 */
  weakness: boolean
  /** 灰色。 */
  grey: boolean
  /** 主题色。 */
  primaryColor: string
  /** 登录表单的位置，默认在编码插画右侧。 */
  loginLayout: LoginLayout
  /** 是否显示页签。 */
  showTabs: boolean
  /** 是否深色模式。 */
  darkMode: 'auto' | 'dark' | 'light'
  /** 侧栏宽度。 */
  layoutSideWidth: number
  /** 侧栏折叠宽度。 */
  layoutSideCollapsedWidth: number
  /** 侧栏项高度。 */
  layoutSideItemHeight: number
  /** 是否显示水印。 */
  showWatermark: boolean
  /** 侧栏模式。 */
  sideMode: MenuNavigationType
  /** 过渡类型。 */
  transitionType: TransitionTypeValue
  /** 圆角尺寸。 */
  cornerSize: BorderRadiusType
  /** 组件尺寸。 */
  componentSize: ComponentSizeType
  /** 侧栏是否固定。 */
  sidebarPinned: boolean
  /** 设置页签是否吸顶。 */
  settingTabsSticky: boolean
  /** 是否显示面包屑。 */
  showBreadcrumb: boolean
  /** 是否显示面包屑图标。 */
  showBreadcrumbIcon: boolean
  /** 是否显示页签图标。 */
  showTabIcon: boolean
  /** 是否显示页签更多。 */
  showTabMore: boolean
  /** 是否显示页签刷新。 */
  showTabRefresh: boolean
  /** 是否显示页签最大化。 */
  showTabMaximize: boolean
  /** 是否显示页脚。 */
  showFooter: boolean
  /** 是否启用快捷键。 */
  enableShortcuts: boolean
  /** 半深色头部。 */
  semiDarkHeader: boolean
  /** 半深色侧栏。 */
  semiDarkSidebar: boolean
  /** 当前界面使用的字体大小。 */
  fontSize: number
  /** 内容模式。 */
  contentMode: ContentLayoutMode
  /** 内容宽度。 */
  contentWidth: number
  /** 是否动态标题。 */
  dynamicTitle: boolean
  /** 页签样式。 */
  tabStyle: TabStyleType
  /** 页签是否可拖拽。 */
  tabDraggable: boolean
  /** 工具栏位置。 */
  toolbarPlacements: Record<ToolbarItemKey, ToolbarPosition>
  /** 工具栏顺序。 */
  toolbarOrder: ToolbarItemKey[]
  /** 偏好设置快捷键。 */
  shortcutPreferences: boolean
  /** 锁屏快捷键。 */
  shortcutLockScreen: boolean
  /** 退出登录快捷键。 */
  shortcutLogout: boolean
  /** 退出快捷键。 */
  shortcutEscape: boolean
}

/**
 * 界面偏好异步保存的运行状态。
 */
export type PreferencesSaveStatus = 'idle' | 'saving' | 'saved' | 'error'
