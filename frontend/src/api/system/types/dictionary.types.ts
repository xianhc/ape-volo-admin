import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'
import type { DictionaryDetail } from './dictionaryDetail.types'

/**
 * 字典实体接口
 */
export interface Dictionary extends BaseEntity {
  dictType: number
  /** 字典名称 */
  name: string
  /** 字典描述 */
  description?: string
  /** 字典详情列表 */
  dictDetails?: DictionaryDetail[]
}

/**
 * 字典查询参数接口
 */
export interface DictionaryQueryParams extends BaseQueryParams {
  /** 字典名称 */
  name?: string
  /** 字典描述 */
  description?: string
  /** 字典类型 */
  dictType?: string
}

/**
 * 字典分页响应接口
 */
export type DictionaryPageResponse = BasePageResponse<Dictionary>
