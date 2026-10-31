import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type {
  Storage,
  StorageQueryParams,
  StoragePageResponse
} from './types/storage.types'
import type { OperationResult, RequestOptions } from '@/api/types/base'

/**
 * 分页查询存储文件列表
 * @param {StorageQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<StoragePageResponse>>} 返回存储文件列表的Promise对象
 */
export const get = (
  params: StorageQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<StoragePageResponse>> => {
  return request({
    url: '/file-record/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 上传文件
 * @param {FormData} data - 包含文件的FormData对象
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回上传结果的Promise对象
 */
export const add = (data: FormData): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/file-record/create',
    method: 'post',
    data
  })
}

/**
 * 删除存储文件
 * @param {EntityId[]} ids - 要删除的文件ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/file-record/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑存储文件信息
 * @param {Partial<Storage>} data - 文件信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (data: Partial<Storage>): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/file-record/update',
    method: 'put',
    data
  })
}

/**
 * 导出存储文件数据
 * @param {StorageQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: StorageQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/file-record/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}
