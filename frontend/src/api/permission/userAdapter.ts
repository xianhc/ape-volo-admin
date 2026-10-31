import type { EntityId } from '@/api/types/base'
import type { User, UserDraft, UserQueryParams } from './types/user.types'

export interface UserDto extends Omit<Partial<User>, 'id' | 'dept' | 'roles' | 'positions'> {
  id: EntityId
  username?: string
  nickname?: string
  avatar?: string
  dept?: { id?: EntityId | null; name?: string } | null
  department?: { id?: EntityId | null; name?: string } | null
  roles?: Array<{ id: EntityId; name: string }>
  positions?: Array<{ id: EntityId; name: string }>
}

export function normalizeUser(dto: UserDto): User {
  const department = dto.dept ?? dto.department
  return {
    id: String(dto.id), userName: dto.userName ?? dto.username ?? '', nickName: dto.nickName ?? dto.nickname ?? '',
    genderCode: dto.genderCode ?? 0, phone: dto.phone ?? '', email: dto.email ?? '', headerImg: dto.headerImg ?? dto.avatar ?? '',
    enabled: dto.enabled ?? true, isAdmin: dto.isAdmin, creationTime: dto.creationTime ?? '', creatorId: dto.creatorId,
    lastModificationTime: dto.lastModificationTime, lastModifierId: dto.lastModifierId,
    dept: { id: department?.id == null ? null : String(department.id), name: department?.name },
    roles: (dto.roles ?? []).map(role => ({ ...role, id: String(role.id) })),
    positions: (dto.positions ?? []).map(position => ({ ...position, id: String(position.id) }))
  }
}

export const toUserQuery = (query: UserQueryParams) => ({
  ...query,
  departmentIdArray: Array.isArray(query.departmentIdArray) ? query.departmentIdArray.join(',') : query.departmentIdArray
})

export function createUserDraft(user?: User): UserDraft {
  return {
    id: user?.id ?? '0', userName: user?.userName ?? '', nickName: user?.nickName ?? '', genderCode: user?.genderCode ?? 1,
    phone: user?.phone ?? '', email: user?.email ?? '', enabled: user?.enabled ?? true,
    dept: { ...(user?.dept ?? { id: null }) },
    roles: user?.roles.map(role => ({ ...role })) ?? [],
    positions: user?.positions.map(position => ({ ...position })) ?? []
  }
}
