/**
 * 多页签类型声明。
 */
import type { LocationQueryRaw, RouteMeta, RouteParamsRaw } from 'vue-router'

/**
 * 用于恢复标签及组件缓存的导航历史项。
 */
export interface TabHistoryItem {
  /** 名称。 */
  name: string
  /** 路由标题、图标、缓存等导航元数据。 */
  meta: RouteMeta & {
    /** 面向用户显示的名称。 */
    displayName: string
    /** 当前菜单或控件的图标标识。 */
    icon?: string
    /** 当前路由是否允许关闭标签。 */
    closeTab?: boolean
  }
  /** 查询。 */
  query?: LocationQueryRaw
  /** 参数。 */
  params?: RouteParamsRaw
  /** 完整路径。 */
  fullPath?: string
  /** 是否固定。 */
  pinned?: boolean
}

/**
 * 标签切换、关闭、固定或刷新等允许的动作。
 */
export type TabAction = 'close' | 'pin' | 'maximize' | 'reload' | 'openWindow' |
  'closeLeft' | 'closeRight' | 'closeOthers' | 'closeAll'
