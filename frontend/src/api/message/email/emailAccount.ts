import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'

/**
 * 邮箱账户接口
 */
export interface EmailAccount {
  id: EntityId
  email: string
  displayName?: string
  host?: string
  port?: number
  userName?: string
  password?: string
  enableSsl?: boolean
  useDefaultCredentials?: boolean
  isDefault?: boolean
  creationTime?: string
  lastModificationTime?: string
}

/**
 * 分页查询参数接口
 */
export interface QueryParams {
  page?: number
  size?: number
  sort?: string
  [key: string]: any
}

/**
 * 分页响应接口
 */
export interface PageResponse<T> {
  items: T[]
  totalCount: number
}

/**
 * 分页查询邮箱账户列表
 * @param {QueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<PageResponse<EmailAccount>>>} 返回邮箱账户列表的Promise对象
 */
export const get = (
  params: QueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<PageResponse<EmailAccount>>> => {
  return request({
    url: '/email-account/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新邮箱账户
 * @param {Partial<EmailAccount>} data - 邮箱账户信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<EmailAccount>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/email-account/create',
    method: 'post',
    data
  })
}

/**
 * 删除邮箱账户
 * @param {EntityId[]} ids - 要删除的邮箱账户ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/email-account/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑邮箱账户信息
 * @param {Partial<EmailAccount>} data - 邮箱账户信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (
  data: Partial<EmailAccount>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/email-account/update',
    method: 'put',
    data
  })
}

/**
 * 获取所有邮箱账户列表（不分页）
 * @returns {Promise<AxiosResponse<EmailAccount[]>>} 返回所有邮箱账户的Promise对象
 */
export const getAll = (): Promise<AxiosResponse<EmailAccount[]>> => {
  return request({
    url: '/email-account/get-all',
    method: 'get'
  })
}
