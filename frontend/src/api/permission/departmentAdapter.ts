import type { Department } from './types/department.types'

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
