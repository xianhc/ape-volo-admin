import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import type {
  Role,
  RoleSingleParams,
  RoleQueryParams,
  RolePageResponse,
  RoleMenuParams
} from './types/role.types'

// 重新导出类型供页面使用
export type {
  Role,
  RoleSingleParams,
  RoleQueryParams,
  RolePageResponse,
  RoleMenuParams
}

/**
 * 分页查询角色列表
 * @param {RoleQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<RolePageResponse>>} 返回角色列表的Promise对象
 */
export const get = (
  params: RoleQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<RolePageResponse>> => {
  return request({
    url: '/role/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新角色
 * @param {Partial<Role>} data - 角色信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<Role>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/role/create',
    method: 'post',
    data
  })
}

/**
 * 删除角色
 * @param {EntityId[]} ids - 要删除的角色ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/role/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑角色信息
 * @param {Partial<Role>} data - 角色信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (data: Partial<Role>): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/role/update',
    method: 'put',
    data
  })
}

/**
 * 导出角色数据
 * @param {RoleQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: RoleQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/role/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}

/**
 * 获取单个角色信息
 * @param {RoleSingleParams} params - 查询参数
 * @returns {Promise<AxiosResponse<Role>>} 返回角色详情的Promise对象
 */
export const single = (
  params: RoleSingleParams
): Promise<AxiosResponse<Role>> => {
  return request({
    url: '/role/query-with-details',
    method: 'post',
    params
  })
}

/**
 * 编辑角色菜单权限
 * @param {RoleMenuParams} data - 角色菜单权限信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const editRoleMenu = (
  data: RoleMenuParams
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/role/update-role-menu',
    method: 'put',
    data
  })
}

/**
 * 获取所有角色列表（不分页）
 * @returns {Promise<AxiosResponse<Role[]>>} 返回所有角色的Promise对象
 */
export const getAll = (): Promise<AxiosResponse<Role[]>> => {
  return request({
    url: '/role/get-all',
    method: 'get'
  })
}
