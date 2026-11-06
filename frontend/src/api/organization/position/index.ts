import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { Position, PositionQueryParams, PositionPageResponse } from './types'
import type { OperationResult, RequestOptions } from '@/api/types/base'

// 重新导出类型供页面使用
export type { Position, PositionQueryParams, PositionPageResponse }

/**
 * 分页查询岗位列表
 * @param {PositionQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<PositionPageResponse>>} 返回岗位列表的Promise对象
 */
export const get = (
  params: PositionQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<PositionPageResponse>> => {
  return request({
    url: '/position/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 获取所有岗位列表（不分页）
 * @returns {Promise<AxiosResponse<Position[]>>} 返回所有岗位的Promise对象
 */
export const getAll = (): Promise<AxiosResponse<Position[]>> => {
  return request({
    url: '/position/get-all',
    method: 'get'
  })
}

/**
 * 创建新岗位
 * @param {Partial<Position>} data - 岗位信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<Position>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/position/create',
    method: 'post',
    data
  })
}

/**
 * 删除岗位
 * @param {EntityId[]} ids - 要删除的岗位ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/position/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑岗位信息
 * @param {Partial<Position>} data - 岗位信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (data: Partial<Position>): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/position/update',
    method: 'put',
    data
  })
}

/**
 * 导出岗位数据
 * @param {PositionQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: PositionQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/position/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}
