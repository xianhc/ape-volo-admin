import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import type {
  DictionaryDetail,
  DictionaryDetailQueryParams,
  DictionaryDetailPageResponse
} from './types/dictionaryDetail.types'

/**
 * 分页查询字典详情列表
 * @param {DictionaryDetailQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<DictionaryDetailPageResponse>>} 返回字典详情列表的Promise对象
 */
export const get = (
  params: DictionaryDetailQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<DictionaryDetailPageResponse>> => {
  return request({
    url: '/dict-detail/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新字典详情
 * @param {Partial<DictionaryDetail>} data - 字典详情信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<DictionaryDetail>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/dict-detail/create',
    method: 'post',
    data
  })
}

/**
 * 删除字典详情
 * @param {EntityId[]} ids - 要删除的字典详情ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/dict-detail/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑字典详情信息
 * @param {Partial<DictionaryDetail>} data - 字典详情信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (
  data: Partial<DictionaryDetail>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/dict-detail/update',
    method: 'put',
    data
  })
}
