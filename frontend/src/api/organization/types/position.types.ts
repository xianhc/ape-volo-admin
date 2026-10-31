import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 岗位实体接口
 */
export interface Position extends BaseEntity {
  /** 岗位名称 */
  name: string
  /** 排序 */
  sort: number
  /** 是否启用 */
  enabled: boolean
  /** 岗位描述 */
  description?: string
}

/**
 * 岗位查询参数接口
 */
export interface PositionQueryParams extends BaseQueryParams {
  /** 岗位名称 */
  name?: string
  /** 是否启用 */
  enabled?: boolean
}

/**
 * 岗位分页响应接口
 */
export type PositionPageResponse = BasePageResponse<Position>
