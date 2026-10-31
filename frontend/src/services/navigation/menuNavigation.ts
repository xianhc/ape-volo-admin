/**
 * 菜单跳转目标解析与导航工具。
 */
import type { RouteLocationRaw, Router } from 'vue-router'

/**
 * 菜单参数定义（本项目后端契约的最小字段集）。
 */
export interface MenuParameterDto {
  /** 参数键名（写入 query 或 params）。 */
  key: string
  /** 参数值（valueType 为 4/5 时是 JSON 文本）。 */
  value: string
  /** 参数值类型：4/5 表示数组。 */
  valueType: number
  /** 参数用途：2 表示路由 params，其余写入 query。 */
  type: number
}

/**
 * 菜单导航使用的节点或标题信息。
 */
export interface NavigableMenuItem {
  /** 当前对象的名称。 */
  name?: string
  /** 当前路由或控件的组件标识。 */
  component?: string | null
  /** 菜单的节点类型。 */
  menuType?: number
  /** 文本格式化时允许使用的参数名称集合。 */
  parameters?: MenuParameterDto[]
}

/** 菜单跳转仅接受完整的 HTTP/HTTPS 地址，拒绝脚本和数据协议。 */
export const toHttpUrl = (value: string | null | undefined): string | null => {
  if (!value) return null
  try {
    const url = new URL(value.trim())
    return url.protocol === 'http:' || url.protocol === 'https:' ? url.href : null
  } catch {
    return null
  }
}

/**
 * 按参数声明的值类型解析输入内容。
 * @param parameter 当前导航参数定义。
 * @returns 按菜单参数协议解析的单个字符串或字符串数组。
 */
const parseParameterValue = (
  parameter: MenuParameterDto
): string | string[] => {
  if (parameter.valueType !== 4 && parameter.valueType !== 5)
    return parameter.value
  try {
    const parsed: unknown = JSON.parse(parameter.value)
    return Array.isArray(parsed)
      ? parsed
          .filter(
            (value): value is string | number =>
              typeof value === 'string' || typeof value === 'number'
          )
          .map(String)
      : []
  } catch {
    return []
  }
}

/**
 * 判断菜单是否使用外链类型或 HTTP 地址。
 * @param menu 包含菜单类型及目标地址的菜单节点。
 * @returns 菜单类型为外链或组件地址以 HTTP/HTTPS 开头时返回 true。
 */
export const isExternalMenu = (
  menu: Pick<NavigableMenuItem, 'component' | 'menuType'>
): boolean => menu.menuType === 5 || /^https?:\/\//i.test(menu.component ?? '')

/**
 * 根据菜单名及声明的路径、查询参数构建站内路由目标，忽略空参数。
 * @param menu 包含路由名及参数定义的菜单节点。
 * @returns 可交给 Router 的站内目标；外链菜单或没有路由名时为 null。
 */
export const buildMenuNavigationTarget = (
  menu: NavigableMenuItem
): RouteLocationRaw | null => {
  if (isExternalMenu(menu)) return null
  if (!menu.name) return null
  const query: Record<string, string | string[]> = {}
  const params: Record<string, string> = {}
  for (const parameter of menu.parameters ?? []) {
    const value = parseParameterValue(parameter)
    if (parameter.type === 2) {
      if (Array.isArray(value) || value === '') continue
      params[parameter.key] = value
      continue
    }
    if (Array.isArray(value) && value.length === 0) continue
    if (!Array.isArray(value) && value === '') continue
    query[parameter.key] = value
  }
  return {
    name: menu.name,
    ...(Object.keys(query).length ? { query } : {}),
    ...(Object.keys(params).length ? { params } : {})
  }
}

/**
 * 解析菜单导航目标，外链使用地址，站内菜单按声明的参数构造路由。
 * @param menu 待解析的菜单节点。
 * @returns 外链地址或站内路由目标；缺少必要信息时为 null。
 */
export const getMenuNavigationTarget = (
  menu: NavigableMenuItem
): RouteLocationRaw | string | null => {
  if (isExternalMenu(menu)) return toHttpUrl(menu.component || menu.name)
  return buildMenuNavigationTarget(menu)
}

/**
 * 通过 Router 解析站内菜单的完整路径，保留菜单声明的路径及查询参数。
 * @param router 当前应用的路由实例。
 * @param menu 待解析的菜单节点。
 * @returns 站内目标的完整路径；外链、信息不足或解析失败时为 null。
 */
export const resolveMenuFullPath = (
  router: Router,
  menu: NavigableMenuItem
): string | null => {
  const target = buildMenuNavigationTarget(menu)
  if (!target) return null
  try {
    return router.resolve(target).fullPath
  } catch {
    return null
  }
}

/**
 * 打开外链或导航到站内菜单；已经位于同一路径时直接视为完成。
 * @param router 当前应用的路由实例。
 * @param menu 需要打开的菜单节点。
 * @returns 目标可导航并完成打开或路由调用时为 true；目标缺失或导航抛错时为 false。
 */
export const navigateToMenu = async (
  router: Router,
  menu: NavigableMenuItem
): Promise<boolean> => {
  const target = getMenuNavigationTarget(menu)
  if (!target) return false
  if (typeof target === 'string') {
    window.open(target, '_blank', 'noopener,noreferrer')
    return true
  }
  try {
    const resolved = router.resolve(target)
    if (router.currentRoute.value.fullPath === resolved.fullPath) return true
    await router.push(target)
    return true
  } catch {
    return false
  }
}
