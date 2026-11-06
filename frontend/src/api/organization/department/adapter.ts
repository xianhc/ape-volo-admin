import type { Department, DepartmentQueryParams } from './types'

/** 全量部门接口返回平铺列表，按 parentId 组树供选择器展示，不改写接口数据。 */
export function buildDepartmentTree(departments: readonly Department[]): Department[] {
  const nodes = new Map(departments.map(department => [department.id, { ...department, children: [] as Department[] }]))
  const roots: Department[] = []
  for (const department of departments) {
    const node = nodes.get(department.id)!
    const parent = department.parentId ? nodes.get(department.parentId) : undefined
    if (parent && parent !== node) parent.children.push(node)
    else roots.push(node)
  }
  return roots
}

/** 筛选完整部门树，保留匹配后代的祖先；排序仅在同级节点间进行。 */
export function filterDepartmentTree(
  departments: readonly Department[],
  query: DepartmentQueryParams
): Department[] {
  const keyword = query.name?.trim().toLocaleLowerCase()
  const [field = 'sort', direction = 'asc'] = (query.sorting ?? 'sort asc').split(' ')
  const sortField = ['id', 'name', 'sort', 'enabled', 'creationTime'].includes(field)
    ? field as 'id' | 'name' | 'sort' | 'enabled' | 'creationTime'
    : 'sort'
  const filtered: Department[] = []
  for (const department of departments) {
    const children = filterDepartmentTree(department.children ?? [], query)
    const matches = (!keyword || department.name.toLocaleLowerCase().includes(keyword))
      && (query.enabled == null || department.enabled === query.enabled)
      && (!query.startTime || Date.parse(department.creationTime) >= Date.parse(query.startTime))
      && (!query.endTime || Date.parse(department.creationTime) <= Date.parse(query.endTime))
    if (matches || children.length) filtered.push({ ...department, children })
  }
  return filtered.sort((left, right) => {
    const a = left[sortField], b = right[sortField]
    const comparison = typeof a === 'number' && typeof b === 'number'
      ? a - b
      : String(a ?? '').localeCompare(String(b ?? ''), undefined, { numeric: true })
    return direction === 'desc' ? -comparison : comparison
  })
}
