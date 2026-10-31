import request from '@/utils/request'
import qs from 'qs'
import type { AxiosResponse } from 'axios'

/**
 * 查询参数接口
 */
export interface QueryParams {
  [key: string]: any
}

/**
 * 初始化数据请求
 * @param {string} url - 请求URL
 * @param {QueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<any>>} 返回数据的Promise对象
 */
export function initData(
  url: string,
  params: QueryParams
): Promise<AxiosResponse<any>> {
  return request({
    url: url + '?' + qs.stringify(params, { indices: false }),
    method: 'get'
  })
}

/**
 * 以请求体提交数据（适用于 POST/PUT 等写入类动作）
 * @param {string} url - 请求URL
 * @param {QueryParams} payload - 请求体
 * @returns {Promise<AxiosResponse<any>>} 返回数据的Promise对象
 */
export function postData(
  url: string,
  payload: QueryParams = {}
): Promise<AxiosResponse<any>> {
  return request({
    url,
    method: 'post',
    data: payload
  })
}
