import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 存储文件实体接口
 */
export interface Storage extends BaseEntity {
  description?: string
  originalName: string
  newName: string
  filePath: string
  /** 真实文件名 */
  realName: string
  /** 存储文件名 */
  name: string
  /** 文件后缀 */
  suffix?: string
  /** 文件路径 */
  path?: string
  /** 文件类型 */
  type?: string
  /** 文件大小 */
  size?: string
  /** 操作者 */
  operate?: string
}

/**
 * 存储文件查询参数接口
 */
export interface StorageQueryParams extends BaseQueryParams {
  /** 真实文件名 */
  realName?: string
  /** 文件类型 */
  type?: string
  /** 文件描述 */
  description?: string
}

/**
 * 存储文件分页响应接口
 */
export type StoragePageResponse = BasePageResponse<Storage>
