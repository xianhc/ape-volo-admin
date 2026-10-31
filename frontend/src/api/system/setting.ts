import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import type {
  Setting,
  SettingQueryParams,
  SettingPageResponse
} from './types/setting.types'

/**
 * 分页查询系统配置列表
 * @param {SettingQueryParams} params - 查询配置
 * @returns {Promise<AxiosResponse<SettingPageResponse>>} 返回系统配置列表的Promise对象
 */
export const get = (
  params: SettingQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<SettingPageResponse>> => {
  return request({
    url: '/setting/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新的系统配置
 * @param {Partial<Setting>} data - 系统配置信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<Setting>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/setting/create',
    method: 'post',
    data
  })
}

/**
 * 删除系统配置
 * @param {EntityId[]} ids - 要删除的配置ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/setting/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑系统配置信息
 * @param {Partial<Setting>} data - 系统配置信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (data: Partial<Setting>): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/setting/update',
    method: 'put',
    data
  })
}

/**
 * 导出系统配置数据
 * @param {SettingQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: SettingQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/setting/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}
