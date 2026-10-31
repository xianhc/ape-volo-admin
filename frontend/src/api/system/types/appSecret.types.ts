import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 应用密钥实体接口
 */
export interface AppSecret extends BaseEntity {
  /** 应用名称 */
  appName: string
  /** 应用ID */
  appId: string
  /** 应用密钥 */
  appSecretKey: string
  /** 备注 */
  remark?: string
  /** 是否启用 */
  enabled?: boolean
}

/**
 * 应用密钥查询参数接口
 */
export interface AppSecretQueryParams extends BaseQueryParams {
  /** 应用名称 */
  appName?: string
  /** 应用ID */
  appId?: string
  /** 备注 */
  remark?: string
  /** 是否启用 */
  enabled?: boolean
}

/**
 * 应用密钥分页响应接口
 */
export type AppSecretPageResponse = BasePageResponse<AppSecret>
