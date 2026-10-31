/**
 * 菜单导航类型枚举
 */
export enum MenuNavigationType {
  /** 侧边栏导航 */
  SidebarNav = 1,
  /** 顶部导航 */
  TopNav = 2,
  /** 双侧边栏导航 */
  DualSidebarNav = 3,
  /** 混合导航 */
  CombinationNav = 4,
  /** 顶部与完整侧栏混合导航 */
  MixedNav = 5,
  /** 顶部与双列侧栏混合导航 */
  HeaderMixedNav = 6,
  /** 全内容布局 */
  FullContent = 7
}

/**
 * 判断导航模式是否包含侧栏导航。
 * @param mode 当前界面选用的菜单导航布局。
 * @returns 当前布局包含侧栏导航时返回 true。
 */
export const hasSidebarNavigation = (mode: MenuNavigationType): boolean =>
  [
    MenuNavigationType.SidebarNav,
    MenuNavigationType.DualSidebarNav,
    MenuNavigationType.CombinationNav,
    MenuNavigationType.MixedNav,
    MenuNavigationType.HeaderMixedNav
  ].includes(mode)

/**
 * 判断导航模式是否包含顶部导航。
 * @param mode 当前界面选用的菜单导航布局。
 * @returns 当前布局包含顶部导航时返回 true。
 */
export const hasHeaderNavigation = (mode: MenuNavigationType): boolean =>
  [
    MenuNavigationType.TopNav,
    MenuNavigationType.CombinationNav,
    MenuNavigationType.MixedNav,
    MenuNavigationType.HeaderMixedNav
  ].includes(mode)

/**
 * 判断导航模式是否使用双侧栏导航。
 * @param mode 当前界面选用的菜单导航布局。
 * @returns 当前布局包含双侧栏导航时返回 true。
 */
export const hasDualSidebarNavigation = (mode: MenuNavigationType): boolean =>
  mode === MenuNavigationType.DualSidebarNav ||
  mode === MenuNavigationType.HeaderMixedNav
