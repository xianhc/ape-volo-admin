import type { EntityId } from '@/api/types/base'
import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 字典详情实体接口
 */
export interface DictionaryDetail extends BaseEntity {
  dictSort: number
  /** 字典ID */
  dictId: EntityId
  /** 字典详情标签 */
  label: string
  /** 字典详情值 */
  value: string
  /** 排序 */
  sort?: number
}

/**
 * 字典详情查询参数接口
 */
export interface DictionaryDetailQueryParams extends BaseQueryParams {
  /** 字典ID */
  dictId?: EntityId
  /** 字典详情标签 */
  label?: string
}

/**
 * 字典详情分页响应接口
 */
export type DictionaryDetailPageResponse = BasePageResponse<DictionaryDetail>
