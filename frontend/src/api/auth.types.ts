import type { ApeToken } from '@/services/session/tokenStorage'
import type { EntityId } from './types/base'

export interface UserInfo {
  id: EntityId
  userName: string
  nickName: string
  headerImg: string
  genderCode: number
  email: string
  phone: string
  dept: { id?: EntityId; name?: string }
  authority: Record<string, unknown>
  roleCodes: string[]
  authCodes: string[]
}

/** 认证传输模型；别名只在接口适配层处理。 */
export interface UserDto {
  id: string | number
  userName?: string
  username?: string
  nickName?: string
  nickname?: string
  headerImg?: string
  avatar?: string
  genderCode?: number
  email?: string
  phone?: string
  dept?: { id?: string | number; name?: string } | null
  department?: { id?: string | number; name?: string } | null
  authority?: Record<string, unknown>
}

export interface AuthContextDto {
  user: UserDto
  roleCodes?: string[]
  authCodes?: string[]
}

export interface AuthContext {
  user: UserInfo
  roleCodes: string[]
  authCodes: string[]
}

/** 后端 long 字段按契约序列化为字符串，适配后统一使用数字时间戳。 */
export interface TokenResponse extends Omit<ApeToken, 'expires' | 'refreshTokenExpires'> {
  expires: number | string
  refreshTokenExpires: number | string
}

export function normalizeTokenResponse(dto: TokenResponse): ApeToken {
  return {
    ...dto,
    expires: Number(dto.expires),
    refreshTokenExpires: Number(dto.refreshTokenExpires)
  }
}
export interface LoginResponse extends AuthContext { token: ApeToken }
export interface RefreshTokenParams { token: string }

export function normalizeAuthContext(dto: AuthContextDto): AuthContext {
  if (!dto?.user || dto.user.id === null || dto.user.id === undefined) {
    throw new Error('Invalid authentication response')
  }
  const user = dto.user
  const department = user.dept ?? user.department
  const roleCodes = dto.roleCodes ?? []
  const authCodes = dto.authCodes ?? []
  return {
    roleCodes,
    authCodes,
    user: {
      id: String(user.id),
      userName: user.userName ?? user.username ?? '',
      nickName: user.nickName ?? user.nickname ?? '',
      headerImg: user.headerImg ?? user.avatar ?? '',
      genderCode: user.genderCode ?? 0,
      email: user.email ?? '',
      phone: user.phone ?? '',
      dept: department ? {
        ...(department.id == null ? {} : { id: String(department.id) }),
        name: department.name
      } : {},
      authority: user.authority ?? {},
      roleCodes,
      authCodes
    }
  }
}
