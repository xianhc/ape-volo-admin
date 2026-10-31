import type { EntityId } from '@/api/types/base'
import type {
  BaseQueryParams,
  BasePageResponse,
  BaseEntity
} from '../../types/base'

/**
 * 用户实体接口
 */
export interface User extends BaseEntity {
  userName: string
  nickName: string
  genderCode: number
  phone: string
  email: string
  headerImg: string
  enabled: boolean
  isAdmin?: boolean
  dept: { id: string | null; name?: string }
  positions: UserRelation[]
  roles: UserRelation[]
}

export interface UserRelation { id: string; name: string }
export type UserDraft = Pick<User, 'id' | 'userName' | 'nickName' | 'genderCode' | 'phone' | 'email' | 'enabled' | 'dept' | 'positions' | 'roles'>

/**
 * 用户查询参数接口
 */
export interface UserQueryParams extends BaseQueryParams {
  /** 用户名 */
  username?: string
  /** 昵称 */
  nickname?: string
  /** 性别 */
  gender?: string
  startTime?: string | null
  endTime?: string | null
  /** 手机号 */
  phone?: string
  /** 邮箱 */
  email?: string
  /** 是否启用 */
  enabled?: boolean
  /** 部门ID数组 */
  departmentIdArray?: EntityId[] | string
}

/**
 * 用户分页响应接口
 */
export type UserPageResponse = BasePageResponse<User>

/**
 * 用户角色参数接口
 */
export interface UserRoleParams {
  /** 用户ID */
  id: EntityId
  /** 角色ID列表 */
  roleIdArray: EntityId[]
}

/**
 * 用户岗位参数接口
 */
export interface UserPositionParams {
  /** 用户ID */
  id: EntityId
  /** 岗位ID列表 */
  positionIdArray: EntityId[]
}

/**
 * 用户个人中心信息接口
 */
export interface UserCenterData {
  /** 用户信息 */
  user?: User
  /** 其他数据 */
  [key: string]: any
}

/**
 * 修改密码参数接口
 */
export interface PasswordParams {
  /** 旧密码 */
  oldPass: string
  /** 新密码 */
  newPass: string
  /** 确认密码 */
  confirmPass: string
}

/**
 * 修改邮箱参数接口
 */
export interface EmailParams {
  /** 密码 */
  pass: string
  /** 邮箱 */
  email: string
  /** 验证码 */
  code: string
}
