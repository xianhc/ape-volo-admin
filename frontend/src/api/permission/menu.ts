import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import type { RouteMenuItem } from '@/services/navigation/menuRoutes'
import type {
  Menu,
  MenuQueryParams,
  MenuPageResponse
} from './types/menu.types'

// 重新导出类型供页面使用
export type { Menu }

/**
 * 构建用户菜单树
 * @returns {Promise<AxiosResponse<Menu[]>>} 返回用户菜单树的Promise对象
 */
export const build = (): Promise<AxiosResponse<RouteMenuItem[]>> => {
  return request({
    url: '/menu/build-tree',
    method: 'post'
  })
}

/**
 * 分页查询菜单列表
 * @param {MenuQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<MenuPageResponse>>} 返回菜单列表的Promise对象
 */
export const get = (
  params: MenuQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<MenuPageResponse>> => {
  return request({
    url: '/menu/get-list',
    ...options,
    method: 'get',
    params: params
  })
}

/**
 * 创建新菜单
 * @param {Partial<Menu>} data - 菜单信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<Menu>
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/menu/create',
    method: 'post',
    data
  })
}

/**
 * 删除菜单
 * @param {EntityId[]} ids - 要删除的菜单ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/menu/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑菜单信息
 * @param {Partial<Menu>} data - 菜单信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (data: Partial<Menu>): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/menu/update',
    method: 'put',
    data
  })
}

/**
 * 导出菜单数据
 * @param {MenuQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: MenuQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/menu/download',
    method: 'post',
    responseType: 'blob',
    data: params
  })
}

/**
 * 获取所有菜单列表（不分页）
 * @returns {Promise<AxiosResponse<Menu[]>>} 返回所有菜单的Promise对象
 */
export const getAll = (): Promise<AxiosResponse<Menu[]>> => {
  return request({
    url: '/menu/get-all',
    method: 'get'
  })
}
