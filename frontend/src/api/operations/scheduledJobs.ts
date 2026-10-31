import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import type {
  ScheduledJob,
  ScheduledJobQueryParams,
  ScheduledJobPageResponse
} from './types/scheduledJob.types'

// 重新导出类型供页面使用
export type { ScheduledJob, ScheduledJobQueryParams, ScheduledJobPageResponse }

/**
 * 分页查询定时任务列表
 * @param {ScheduledJobQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<ScheduledJobPageResponse>>} 返回定时任务列表的Promise对象
 */
export const get = (
  params: ScheduledJobQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<ScheduledJobPageResponse>> => {
  return request({
    url: '/scheduled-job/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 获取所有定时任务列表
 * @returns {Promise<AxiosResponse<ScheduledJob[]>>} 返回所有定时任务的Promise对象
 */
export const getAll = (): Promise<AxiosResponse<ScheduledJob[]>> => {
  return request({
    url: '/scheduled-job/get-all',
    method: 'get'
  })
}

/**
 * 创建新的定时任务
 * @param {Partial<ScheduledJob>} data - 定时任务信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export function add(
  data: Partial<ScheduledJob>
): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/scheduled-job-manager/create',
    method: 'post',
    data
  })
}

/**
 * 删除定时任务
 * @param {EntityId[]} ids - 要删除的任务ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export function del(ids: EntityId[]): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/scheduled-job-manager/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑定时任务信息
 * @param {Partial<ScheduledJob>} data - 定时任务信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export function edit(data: Partial<ScheduledJob>): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/scheduled-job-manager/update',
    method: 'put',
    data
  })
}

/**
 * 导出定时任务数据
 * @param {ScheduledJobQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: ScheduledJobQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/scheduled-job/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}

/**
 * 立即执行定时任务
 * @param {EntityId} id - 任务ID
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回执行结果的Promise对象
 */
export function execution(id: EntityId): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/scheduled-job-manager/execute?id=' + id,
    method: 'post'
  })
}

/**
 * 暂停定时任务
 * @param {EntityId} id - 任务ID
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回暂停结果的Promise对象
 */
export function pause(id: EntityId): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/scheduled-job-manager/pause?id=' + id,
    method: 'post'
  })
}

/**
 * 恢复定时任务
 * @param {EntityId} id - 任务ID
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回恢复结果的Promise对象
 */
export function resume(id: EntityId): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/scheduled-job-manager/resume?id=' + id,
    method: 'post'
  })
}
