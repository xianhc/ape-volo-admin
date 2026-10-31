import { createHttpClient } from '@/services/http/client'
import { httpFeedback } from '@/services/http/httpFeedback'
import setting from '@/setting'
import { encrypt } from '@/utils/rsaEncrypt'
import { getApeToken } from '@/services/session/tokenStorage'
import { reportServerError } from '@/services/logging/appLogger'
import { normalizeAuthContext, normalizeTokenResponse } from './auth.types'
import type {
  AuthContextDto,
  LoginResponse,
  RefreshTokenParams,
  TokenResponse
} from './auth.types'

/** 登录、刷新和退出不经过受保护接口的鉴权拦截器。 */
export const authClient = createHttpClient({
  config: { baseURL: import.meta.env.VITE_BASE_API, timeout: setting.timeout },
  feedback: httpFeedback,
  onServerError: reportServerError
})

export async function login(
  username: string,
  password: string,
  captcha: string,
  captchaId: string
) {
  const response = await authClient.post<
    AuthContextDto & { token: TokenResponse }
  >('/auth/login', {
    username,
    password: await encrypt(password),
    captcha,
    captchaId
  })
  return {
    ...response,
    data: {
      ...normalizeAuthContext(response.data),
      token: normalizeTokenResponse(response.data.token)
    } satisfies LoginResponse
  }
}

export const refreshToken = async (data: RefreshTokenParams) => {
  const response = await authClient.post<TokenResponse>('/auth/refresh-token', data, {
    errorFeedback: false
  })
  return { ...response, data: normalizeTokenResponse(response.data) }
}
export const logout = () =>
  authClient.delete<void>('/auth/logout', {
    data: { token: getApeToken().refreshToken },
    errorFeedback: false
  })
