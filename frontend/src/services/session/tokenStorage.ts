/**
 * JWT 认证工具模块
 * 提供统一的用户认证令牌管理功能，包括令牌的存储、获取和清除
 *
 * 功能特性：
 * - 安全的令牌本地存储管理
 * - 自动的 JSON 序列化/反序列化
 * - 默认值和错误处理机制
 * - 支持访问令牌和刷新令牌
 * - 令牌过期时间管理
 *
 * 使用场景：
 * - 用户登录后的令牌保存
 * - API 请求的认证头设置
 * - 用户登出时的令牌清理
 * - 令牌刷新机制
 */

/**
 * 令牌对象接口
 * @interface ApeToken
 * @property {string} accessToken - 访问令牌，用于API请求认证
 * @property {number} expires - 访问令牌过期时间戳
 * @property {string} tokenType - 令牌类型，通常为 'Bearer'
 * @property {number} refreshTokenExpires - 刷新令牌过期时间戳
 */
export interface ApeToken {
  sessionId: string
  accessToken: string
  refreshToken: string
  expires: number
  tokenType: string
  refreshTokenExpires: number
}

/**
 * 设置令牌时的数据接口
 * @interface SetTokenData
 * @property {string} [tokenType] - 令牌类型
 * @property {string} [accessToken] - 访问令牌
 * @property {string} [refreshToken] - 独立刷新令牌
 * @property {number} [expires] - 访问令牌过期时间戳
 * @property {number} [refreshTokenExpires] - 刷新令牌过期时间戳
 */
export interface SetTokenData {
  sessionId?: string
  tokenType?: string
  accessToken?: string
  refreshToken?: string
  expires?: number
  refreshTokenExpires?: number
}

/**
 * 本地存储中令牌的键名
 * @constant {string}
 */
const APE_VOLO_ADMIN_TOKEN_KEY = 'ape-volo-admin-token'

/**
 * 默认令牌对象结构
 * @constant {ApeToken}
 */
const defaultApeToken: ApeToken = {
  sessionId: '',
  accessToken: '',
  refreshToken: '',
  expires: 0,
  tokenType: 'Bearer',
  refreshTokenExpires: 0
}

/**
 * 获取存储的认证令牌
 * @function getApeToken
 * @description 从本地存储中读取并解析认证令牌信息
 *
 * 处理逻辑：
 * - 从 localStorage 读取令牌数据
 * - 自动解析 JSON 格式的令牌信息
 * - 合并默认值，确保返回完整的令牌对象
 * - 异常情况下返回默认令牌对象
 *
 * 错误处理：
 * - JSON 解析失败时记录警告并返回默认值
 * - 不存在令牌时返回默认值
 * - 确保函数调用的安全性和可靠性
 *
 * @returns {ApeToken} 令牌对象，包含访问令牌、过期时间等信息
 *
 * @example
 * // 获取当前用户的认证令牌
 * const token = getApeToken()
 * if (token.accessToken) {
 *   console.log('用户已登录，令牌:', token.accessToken)
 *   console.log('令牌过期时间:', new Date(token.expires))
 * } else {
 *   console.log('用户未登录')
 * }
 */
export const getApeToken = (): ApeToken => {
  try {
    const tokenStr = localStorage.getItem(APE_VOLO_ADMIN_TOKEN_KEY)
    if (!tokenStr) {
      return { ...defaultApeToken }
    }

    const token = JSON.parse(tokenStr) as Partial<ApeToken>
    return {
      ...defaultApeToken,
      ...token
    }
  } catch (error) {
    console.warn('解析 token 失败:', error)
    return { ...defaultApeToken }
  }
}

/**
 * 设置认证令牌到本地存储
 * @function setApeToken
 * @description 将认证令牌信息序列化并保存到本地存储
 *
 * 处理逻辑：
 * - 提取并规范化令牌数据结构
 * - 支持访问令牌和刷新令牌的灵活设置
 * - 自动序列化为 JSON 格式存储
 * - 确保数据格式的一致性
 *
 * 令牌结构：
 * - tokenType: 令牌类型标识
 * - accessToken: API 访问令牌
 * - refreshToken: 每次刷新后轮换的独立凭证
 * - expires: 访问令牌过期时间
 * - refreshTokenExpires: 刷新令牌过期时间
 *
 * @param {SetTokenData} data - 令牌数据对象
 *
 * @example
 * // 用户登录成功后保存令牌
 * const loginResponse = await loginApi(credentials)
 * setApeToken({
 *   tokenType: 'Bearer',
 *   accessToken: loginResponse.accessToken,
 *   expires: Date.now() + 3600000, // 1小时后过期
 *   refreshTokenExpires: Date.now() + 604800000 // 7天后过期
 * })
 *
 * // 令牌刷新后更新存储
 * const refreshResponse = await refreshTokenApi()
 * setApeToken(refreshResponse)
 */
export const setApeToken = (data: SetTokenData): void => {
  const apeToken: ApeToken = {
    sessionId: data.sessionId || '',
    tokenType: data.tokenType || 'Bearer',
    accessToken: data.accessToken || '',
    refreshToken: data.refreshToken || '',
    expires: data.expires || 0,
    refreshTokenExpires: data.refreshTokenExpires || 0
  }
  localStorage.setItem(APE_VOLO_ADMIN_TOKEN_KEY, JSON.stringify(apeToken))
}

/**
 * 清除本地存储的认证令牌
 * @function clearToken
 * @description 从本地存储中移除用户的认证令牌信息
 *
 * 使用场景：
 * - 用户主动登出
 * - 令牌过期或无效
 * - 安全要求下的强制登出
 * - 切换用户账户
 *
 * 安全考虑：
 * - 完全移除本地令牌数据
 * - 防止未授权访问
 * - 确保用户状态的准确性
 *
 * @example
 * // 用户登出时清除令牌
 * const logout = async () => {
 *   try {
 *     await logoutApi() // 调用后端登出接口
 *   } finally {
 *     clearToken() // 无论后端调用是否成功都清除本地令牌
 *     router.push('/login') // 重定向到登录页
 *   }
 * }
 *
 * // 令牌过期时自动清除
 * if (token.expires < Date.now()) {
 *   clearToken()
 *   showMessage('登录已过期，请重新登录')
 * }
 */
export const clearToken = (): void => {
  localStorage.removeItem(APE_VOLO_ADMIN_TOKEN_KEY)
}
