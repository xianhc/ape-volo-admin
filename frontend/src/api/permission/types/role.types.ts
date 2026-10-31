import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity,
  EntityId
} from '../../types/base'

/**
 * 角色实体接口
 */
export interface Role extends BaseEntity {
  authCode: string
  dataScopeType: number
  deptIdArray?: EntityId[]
  departmentList?: Array<{ id: EntityId; name: string }>
  /** 角色名称 */
  name: string
  /** 角色编码 */
  code?: string
  /** 角色描述 */
  description?: string
  /** 数据权限范围 */
  dataScope?: string
  /** 是否启用 */
  enabled?: boolean
  /** 菜单ID列表 */
  menuList?: Array<{ id: EntityId; title?: string }>
  /** API ID列表 */
  apiList?: EntityId[]
}

/**
 * 角色查询参数接口
 */
export interface RoleQueryParams extends BaseQueryParams {
  /** 角色名称 */
  name?: string
  /** 角色编码 */
  code?: string
  /** 是否启用 */
  enabled?: boolean
  /** 数据权限类型 */
  dataScopeType?: string
}

/**
 * 单个角色查询参数接口
 */
export interface RoleSingleParams extends BaseQueryParams {
  /** 角色ID */
  id: EntityId
}

/**
 * 角色分页响应接口
 */
export type RolePageResponse = BasePageResponse<Role>

/**
 * 角色菜单权限参数接口
 */
export interface RoleMenuParams {
  /** 角色ID */
  id: EntityId
  /** 菜单ID列表 */
  menuIdArray: EntityId[]
}
