import { nextTick, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { editUserPosition, editUserRole } from '@/api/permission/user'
import type { User, UserRelation } from '@/api/permission/types/user.types'

type Field = 'roles' | 'positions'

/** 行内授权拥有自己的提交锁与回滚快照，不与表单草稿共享状态。 */
export function useUserAssignments() {
  const assignmentPending = reactive<Record<string, boolean>>({})
  const snapshots = new Map<string, UserRelation[]>()
  const keyOf = (row: User, field: Field) => `${row.id}:${field}`
  const copy = (items: UserRelation[]) => items.map(item => ({ ...item }))
  const save = async (row: User, field: Field): Promise<void> => {
    await nextTick()
    const key = keyOf(row, field)
    const previous = snapshots.get(key)
    if (assignmentPending[key] || !previous) return
    const ids = row[field].map(item => item.id)
    if (previous.map(item => item.id).sort().join(',') === [...ids].sort().join(',')) { snapshots.delete(key); return }
    assignmentPending[key] = true
    try {
      if (field === 'roles') await editUserRole({ id: row.id, roleIdArray: ids })
      else await editUserPosition({ id: row.id, positionIdArray: ids })
      ElMessage.success(field === 'roles' ? '角色修改成功' : '岗位修改成功')
    } catch {
      row[field] = previous
    } finally {
      snapshots.delete(key)
      assignmentPending[key] = false
    }
  }
  const visible = (row: User, field: Field, open: boolean): void => {
    if (open) snapshots.set(keyOf(row, field), copy(row[field]))
    else void save(row, field)
  }
  const remove = (row: User, field: Field, removed: UserRelation): void => {
    const key = keyOf(row, field)
    if (!snapshots.has(key)) snapshots.set(key, copy([...row[field], removed]))
    void save(row, field)
  }
  return {
    assignmentPending,
    handleRoleVisibleChange: (row: User, open: boolean) => visible(row, 'roles', open),
    handleRoleRemove: (row: User, removed: UserRelation) => remove(row, 'roles', removed),
    handlePositionVisibleChange: (row: User, open: boolean) => visible(row, 'positions', open),
    handlePositionRemove: (row: User, removed: UserRelation) => remove(row, 'positions', removed)
  }
}
