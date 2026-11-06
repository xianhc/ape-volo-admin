import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import { buildDepartmentTree } from './adapter'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import type {
  Department,
  DepartmentQueryParams,
  DepartmentPageResponse
} from './types'

// 重新导出类型供页面使用
export type { Department, DepartmentQueryParams, DepartmentPageResponse }

/**
 * 分页查询部门列表
 * @param {DepartmentQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<DepartmentPageResponse>>} 返回部门列表的Promise对象
 */
export const get = (
  params: DepartmentQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<DepartmentPageResponse>> => {
  return request({
    url: '/department/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新部门
 * @param {Partial<Department>} data - 部门信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<Department>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/department/create',
    method: 'post',
    data
  })
}

/**
 * 删除部门
 * @param {EntityId[]} ids - 要删除的部门ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/department/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑部门信息
 * @param {Partial<Department>} data - 部门信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (
  data: Partial<Department>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/department/update',
    method: 'put',
    data
  })
}

/**
 * 导出部门数据
 * @param {DepartmentQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: DepartmentQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/department/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}

/**
 * 获取部门树结构
 * @returns {Promise<AxiosResponse<Department[]>>} 返回部门树的Promise对象
 */
export const getDeptTree = async (
  options: RequestOptions = {}
): Promise<AxiosResponse<Department[]>> => {
  const response = await request<Department[]>({
    url: '/department/get-all',
    ...options,
    method: 'get'
  })
  return { ...response, data: buildDepartmentTree(response.data) }
}
