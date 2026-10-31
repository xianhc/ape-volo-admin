import request from '@/utils/request'
import { normalizeAuthContext } from './auth.types'
import type { AuthContextDto } from './auth.types'

export { login, logout, refreshToken } from './authClient'
export type { UserInfo, LoginResponse, TokenResponse, RefreshTokenParams } from './auth.types'

export async function getUserInfo() {
  const response = await request.get<AuthContextDto>('/auth/info')
  return { ...response, data: normalizeAuthContext(response.data) }
}
