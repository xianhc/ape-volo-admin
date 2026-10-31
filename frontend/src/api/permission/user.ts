import type { EntityId } from '@/api/types/base'
import request from '@/utils/request'
import { encrypt } from '@/utils/rsaEncrypt'
import type { AxiosResponse } from 'axios'
import type { OperationResult, RequestOptions } from '@/api/types/base'
import { normalizeUser, toUserQuery } from './userAdapter'
import type { UserDto } from './userAdapter'
import type {
  User,
  UserDraft,
  UserQueryParams,
  UserPageResponse,
  UserRoleParams,
  UserPositionParams,
  UserCenterData,
  PasswordParams,
  EmailParams
} from './types/user.types'

/**
 * 分页查询用户列表
 * @param {UserQueryParams} params - 查询参数
 * @returns {Promise<AxiosResponse<UserPageResponse>>} 返回用户列表的Promise对象
 */
export const get = async (
  params: UserQueryParams,
  options: RequestOptions = {}
): Promise<AxiosResponse<UserPageResponse>> => {
  const response = await request<{ items: UserDto[]; totalCount: number }>({
    url: '/user/get-list',
    ...options,
    method: 'get',
    params: toUserQuery(params)
  })
  return {
    ...response,
    data: { ...response.data, items: response.data.items.map(normalizeUser) }
  }
}

/**
 * 创建新用户
 * @param {Partial<User>} data - 用户信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回创建结果的Promise对象
 */
export const add = (
  data: Partial<User> | UserDraft
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/user/create',
    method: 'post',
    data
  })
}

/**
 * 删除用户
 * @param {EntityId[]} ids - 要删除的用户ID数组
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回删除结果的Promise对象
 */
export const del = (
  ids: EntityId[]
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/user/delete',
    method: 'delete',
    data: { ids }
  })
}

/**
 * 编辑用户信息
 * @param {Partial<User>} data - 用户信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const edit = (
  data: Partial<User> | UserDraft
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/user/update',
    method: 'put',
    data
  })
}

/**
 * 导出用户数据
 * @param {UserQueryParams} params - 导出参数
 * @returns {Promise<AxiosResponse<Blob>>} 返回文件流的Promise对象
 */
export const download = (
  params: UserQueryParams
): Promise<AxiosResponse<Blob>> => {
  return request({
    url: '/user/download',
    method: 'post',
    responseType: 'blob',
    data: toUserQuery(params)
  })
}

/**
 * 编辑用户角色
 * @param {UserRoleParams} data - 用户角色信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const editUserRole = (
  data: UserRoleParams
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/user/update-role',
    method: 'put',
    data
  })
}

/**
 * 编辑用户岗位
 * @param {UserPositionParams} data - 用户岗位信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export const editUserPosition = (
  data: UserPositionParams
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/user/update-position',
    method: 'put',
    data
  })
}

/**
 * 编辑用户个人中心信息
 * @param {UserCenterData} data - 用户个人信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回编辑结果的Promise对象
 */
export function editUserCenter(
  data: UserCenterData
): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/user/update-center',
    method: 'put',
    data
  })
}

/**
 * 修改用户密码
 * @param {PasswordParams} user - 密码信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回修改结果的Promise对象
 */
export async function editUserPass(
  user: PasswordParams
): Promise<AxiosResponse<OperationResult>> {
  const [oldPassword, newPassword, confirmPassword] = await Promise.all([
    encrypt(user.oldPass),
    encrypt(user.newPass),
    encrypt(user.confirmPass)
  ])
  const data = {
    oldPassword,
    newPassword,
    confirmPassword
  }
  return request({
    url: '/user/update-password',
    method: 'put',
    data
  })
}

/**
 * 修改用户界面偏好配置
 * @param {string} preferencesConfig - 用户界面偏好配置JSON字符串
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回修改结果的Promise对象
 */
export function editPreferencesConfig(
  preferencesConfig: string
): Promise<AxiosResponse<OperationResult>> {
  return request({
    url: '/user/update-preferences-config',
    method: 'put',
    data: { preferencesConfig }
  })
}

/**
 * 修改用户邮箱
 * @param {EmailParams} form - 邮箱信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回修改结果的Promise对象
 */
export async function editUserEmail(
  form: EmailParams
): Promise<AxiosResponse<OperationResult>> {
  const data = {
    password: await encrypt(form.pass),
    email: form.email,
    code: form.code
  }
  return request({
    url: '/user/update-email',
    method: 'put',
    data
  })
}
