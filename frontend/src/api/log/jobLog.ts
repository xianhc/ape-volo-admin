import type { RequestOptions } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type {
  JobLogQueryParams,
  JobLogPageResponse
} from './types/jobLog.types'

/**
 * 分页查询任务日志列表
 * @param {JobLogQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<JobLogPageResponse>>} 返回任务日志列表的Promise对象
 */
export const get = (
  params: JobLogQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<JobLogPageResponse>> => {
  return request({
    url: '/job-log/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 导出任务日志数据
 * @param {JobLogQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: JobLogQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/job-log/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}
