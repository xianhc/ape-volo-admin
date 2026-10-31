import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import type {
  AppSecret,
  AppSecretQueryParams,
  AppSecretPageResponse
} from './types/appSecret.types'

/**
 * 分页查询应用密钥列表
 * @param {AppSecretQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<AppSecretPageResponse>>} 返回应用密钥列表的Promise对象
 */
export const get = (
  params: AppSecretQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<AppSecretPageResponse>> => {
  return request({
    url: '/app-secret/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新应用密钥
 * @param {Partial<AppSecret>} data - 应用密钥信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<AppSecret>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/app-secret/create',
    method: 'post',
    data
  })
}

/**
 * 删除应用密钥
 * @param {EntityId[]} ids - 要删除的应用密钥ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/app-secret/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑应用密钥信息
 * @param {Partial<AppSecret>} data - 应用密钥信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (
  data: Partial<AppSecret>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/app-secret/update',
    method: 'put',
    data
  })
}

/**
 * 导出应用密钥数据
 * @param {AppSecretQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: AppSecretQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/app-secret/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}
