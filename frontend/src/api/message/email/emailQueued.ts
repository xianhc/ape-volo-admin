import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'

/**
 * 邮件队列接口
 */
export interface EmailQueued {
  id: EntityId
  from?: string
  to?: string
  cc?: string
  bcc?: string
  subject?: string
  body?: string
  sentTries?: number
  sentTime?: string
  sendTime?: string
  fromName?: string
  toName?: string
  isSend?: boolean
  priority?: number
  emailAccountId?: EntityId
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
 * 分页查询邮件队列列表
 * @param {QueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<PageResponse<EmailQueued>>>} 返回邮件队列列表的Promise对象
 */
export const get = (
  params: QueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<PageResponse<EmailQueued>>> => {
  return request({
    url: '/queued-email/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新邮件队列
 * @param {Partial<EmailQueued>} data - 邮件队列信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<EmailQueued>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/queued-email/create',
    method: 'post',
    data
  })
}

/**
 * 删除邮件队列
 * @param {EntityId[]} ids - 要删除的邮件队列ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/queued-email/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑邮件队列信息
 * @param {Partial<EmailQueued>} data - 邮件队列信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (
  data: Partial<EmailQueued>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/queued-email/update',
    method: 'put',
    data
  })
}
