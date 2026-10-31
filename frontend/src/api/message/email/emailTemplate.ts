import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'

/**
 * 邮件模板接口
 */
export interface EmailTemplate {
  id: EntityId
  name: string
  bccEmailAddresses?: string
  subject?: string
  body?: string
  isActive?: boolean
  enabled?: boolean
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
 * 分页查询邮件模板列表
 * @param {QueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<PageResponse<EmailTemplate>>>} 返回邮件模板列表的Promise对象
 */
export const get = (
  params: QueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<PageResponse<EmailTemplate>>> => {
  return request({
    url: '/email-message-template/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新邮件模板
 * @param {Partial<EmailTemplate>} data - 邮件模板信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<EmailTemplate>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/email-message-template/create',
    method: 'post',
    data
  })
}

/**
 * 删除邮件模板
 * @param {EntityId[]} ids - 要删除的邮件模板ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/email-message-template/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑邮件模板信息
 * @param {Partial<EmailTemplate>} data - 邮件模板信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (
  data: Partial<EmailTemplate>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/email-message-template/update',
    method: 'put',
    data
  })
}
