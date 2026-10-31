import type { RequestOptions } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type {
  ScheduledJobLogQueryParams,
  ScheduledJobLogPageResponse
} from './types/scheduledJobLog.types'

/**
 * 分页查询任务日志列表
 * @param {ScheduledJobLogQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<ScheduledJobLogPageResponse>>} 返回任务日志列表的Promise对象
 */
export const get = (
  params: ScheduledJobLogQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<ScheduledJobLogPageResponse>> => {
  return request({
    url: '/scheduled-job-log/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 导出任务日志数据
 * @param {ScheduledJobLogQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: ScheduledJobLogQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/scheduled-job-log/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}
