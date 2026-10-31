import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity,
  EntityId
} from '../../types/base'

/**
 * 菜单实体接口
 */
export interface Menu extends BaseEntity {
  menuType: number
  authCode?: string
  keepAlive: boolean
  enabled: boolean
  /** 菜单标题 */
  title: string
  /** 菜单类型 */
  type?: number
  /** 权限标识 */
  permission?: string
  /** 路由路径 */
  path?: string
  /** 组件路径 */
  component?: string
  /** 父级ID（null 表示顶级菜单） */
  parentId?: EntityId | null
  /** 图标 */
  icon?: string
  /** 排序 */
  sort?: number
  /** 是否缓存 */
  cache?: boolean
  /** 是否隐藏 */
  hidden?: boolean
  /** 组件名称 */
  componentName?: string
  /** 是否有子菜单，懒加载树表据此显示展开箭头 */
  hasChildren?: boolean
  /** 子菜单 */
  children?: Menu[]
}

/**
 * 菜单查询参数接口
 */
export interface MenuQueryParams extends BaseQueryParams {
  /** 菜单标题 */
  title?: string
  /** 菜单类型 */
  type?: number
  /** 是否隐藏 */
  hidden?: boolean
  /** 是否启用 */
  enabled?: boolean
  /** 父级ID */
  parentId?: EntityId
  /** 是否只查顶级菜单（父级为 null） */
  isRoot?: boolean
}

/**
 * 菜单分页响应接口
 */
export type MenuPageResponse = BasePageResponse<Menu>
