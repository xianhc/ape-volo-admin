import type { RequestOptions } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type {
  ExceptionLogQueryParams,
  ExceptionLogPageResponse
} from './types/exceptionLog.types'

/**
 * 分页查询异常日志列表
 * @param {ExceptionLogQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<ExceptionLogPageResponse>>} 返回异常日志列表的Promise对象
 */
export const get = (
  params: ExceptionLogQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<ExceptionLogPageResponse>> => {
  return request({
    url: '/exception-log/get-list',
    ...options,
    method: 'get',
    params: params
  })
}
