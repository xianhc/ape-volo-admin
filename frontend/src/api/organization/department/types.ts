import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity,
  EntityId
} from '@/api/types/base'

/**
 * 部门实体接口
 */
export interface Department extends BaseEntity {
  /** 部门名称 */
  name: string
  /** 父级ID（null 表示顶级部门） */
  parentId?: EntityId | null
  /** 是否启用 */
  enabled?: boolean
  /** 排序 */
  sort?: number
  /** 是否有子部门，懒加载树表据此显示展开箭头 */
  hasChildren?: boolean
  /** 子部门 */
  children?: Department[]
}

/**
 * 部门查询参数接口
 */
export interface DepartmentQueryParams extends BaseQueryParams {
  /** 部门名称 */
  name?: string
  /** 是否启用 */
  enabled?: boolean
  /** 父级ID */
  parentId?: EntityId
  /** 是否只查顶级部门（父级为 null） */
  isRoot?: boolean
  /** 创建时间范围 */
  startTime?: string | null
  endTime?: string | null
}

/**
 * 部门分页响应接口
 */
export type DepartmentPageResponse = BasePageResponse<Department>
