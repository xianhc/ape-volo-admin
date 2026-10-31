import type { OperationResult } from '@/api/types/base'
import request from '@/utils/request'
import { authClient } from './authClient'
import type { AxiosResponse } from 'axios'

/**
 * 验证码响应接口
 */
export interface CaptchaResponse {
  id: string
  img: string
  [key: string]: any
}

/**
 * 重置密码邮箱参数接口
 */
export interface ResetEmailCodeParams {
  email: string
  [key: string]: any
}

/**
 * 获取登录验证码
 * @returns {Promise<AxiosResponse<CaptchaResponse>>} 返回验证码信息的Promise对象
 */
export const getLoginCaptcha = (): Promise<AxiosResponse<CaptchaResponse>> => {
  return authClient({
    url: '/auth/captcha',
    method: 'get'
  })
}

/**
 * 获取重置密码邮箱验证码
 * @param {ResetEmailCodeParams} data - 邮箱信息
 * @returns {Promise<AxiosResponse<OperationResult>>} 返回发送结果的Promise对象
 */
export const getResetEmailCode = (
  data: ResetEmailCodeParams
): Promise<AxiosResponse<OperationResult>> => {
  return request({
    url: '/auth/reset-email-code',
    method: 'post',
    data
  })
}
