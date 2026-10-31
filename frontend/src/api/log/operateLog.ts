import type { RequestOptions } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type {
  OperateLog,
  OperateCurrentLogQueryParams,
  OperateLogQueryParams,
  OperateLogPageResponse,
  VisitTrendData
} from './types/operateLog.types'

// 重新导出类型供页面使用
export type { OperateLog }

/**
 * 分页查询操作日志列表
 * @param {OperateLogQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<OperateLogPageResponse>>} 返回操作日志列表的Promise对象
 */
export const get = (
  params: OperateLogQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<OperateLogPageResponse>> => {
  return request({
    url: '/operate-log/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 获取当前用户操作日志
 * @param {OperateCurrentLogQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<OperateLogPageResponse>>} 返回当前用户操作日志的Promise对象
 */
export const getCurrent = (
  params: OperateCurrentLogQueryParams
): Promise<AxiosResponse<OperateLogPageResponse>> => {
  return request({
    url: '/operate-log/query-by-current',
    method: 'post',
    data: params
  })
}

/**
 * 获取访问趋势统计数据
 * @param {number} days - 天数
 * @returns {Promise<AxiosResponse<VisitTrendData>>} 返回访问趋势数据的Promise对象
 */
export const getVisitTrend = (
  days: number
): Promise<AxiosResponse<VisitTrendData>> => {
  return request({
    url: '/operate-log/get-operation-number',
    method: 'get',
    params: { days }
  })
}
