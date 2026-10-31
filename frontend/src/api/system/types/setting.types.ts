import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 系统配置实体接口
 */
export interface Setting extends BaseEntity {
  /** 配置名称 */
  name: string
  /** 配置值 */
  value: string
  /** 是否启用 */
  enabled?: boolean
  /** 配置描述 */
  description?: string
}

/**
 * 系统配置查询参数接口
 */
export interface SettingQueryParams extends BaseQueryParams {
  /** 配置名称 */
  name?: string
  /** 配置值 */
  value?: string
  /** 配置描述 */
  description?: string
  /** 是否启用 */
  enabled?: boolean
}

/**
 * 系统配置分页响应接口
 */
export type SettingPageResponse = BasePageResponse<Setting>
