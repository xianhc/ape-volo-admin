import type { Component } from 'vue'
import type { RouteRecordRaw } from 'vue-router'
import { MenuType } from '@/enums/MenuType'
import type { MenuParameterDto } from './menuNavigation'
import { toHttpUrl } from './menuNavigation'

export interface RouteMenuItem {
  path: string
  name?: string
  component?: string
  meta?: {
    title?: string
    displayName?: string
    icon?: string
    hidden?: boolean
    keepAlive?: boolean
    closeTab?: boolean
    path?: string
    iframeUrl?: string
  }
  hidden?: boolean
  menuType?: number
  parameters?: MenuParameterDto[]
  children?: RouteMenuItem[]
  parent?: RouteMenuItem | null
}

interface ViewRegistry {
  modules: Record<string, () => Promise<Component>>
  names: Record<string, string>
  iframe: Component
}

export const normalizeViewPath = (value: string): string => {
  let path = value.replace(/^\/?src\//, '').replace(/^\//, '')
  if (!path.startsWith('views/')) path = `views/${path}`
  if (!path.endsWith('.vue')) path += '.vue'
  if (path.split('/').includes('..')) throw new Error('Invalid view path')
  return `/src/${path}`
}

/** 菜单 DTO、导航树和路由记录分别持有，转换不改写服务端数据。 */
export function compileMenuRoutes(source: readonly RouteMenuItem[], registry: ViewRegistry) {
  const byName: Record<string, RouteMenuItem> = {}
  const cacheNames: Record<string, string> = {}
  const allowedCacheNames = new Set<string>()
  const cloneNodes = (items: readonly RouteMenuItem[]): RouteMenuItem[] => items.map(({ parent: _parent, ...item }) => ({
    ...item,
    meta: { ...item.meta },
    parameters: item.parameters?.map(parameter => ({ ...parameter })),
    children: item.children ? cloneNodes(item.children) : undefined
  }))
  const nodes = cloneNodes(source)
  nodes.push({ path: 'reload', name: 'Reload', hidden: true, meta: { closeTab: true }, component: 'views/error/reload.vue' })
  const compile = (node: RouteMenuItem, parent: RouteMenuItem | null): RouteRecordRaw | undefined => {
    node.parent = parent
    node.meta = { ...node.meta, hidden: node.hidden ?? node.meta?.hidden }
    if (node.menuType === MenuType.ExternalLink) {
      const url = toHttpUrl(node.component || node.name)
      if (!url) throw new Error('外链菜单必须使用完整的 HTTP 或 HTTPS 地址')
      node.component = url
      node.name = url
    }
    if (node.name) {
      if (byName[node.name]) throw new Error(`Duplicate route name: ${node.name}`)
      byName[node.name] = node
    }
    const children = node.children?.map(child => compile(child, node)).filter((child): child is RouteRecordRaw => Boolean(child))
    if (node.menuType === MenuType.ExternalLink || node.menuType === MenuType.Button) return undefined
    let component: Component | (() => Promise<Component>) | undefined
    let componentName: string | undefined
    if (node.menuType === MenuType.InternalLink || node.meta.iframeUrl) {
      const url = toHttpUrl(node.meta.iframeUrl || node.component)
      if (!url) throw new Error('内嵌菜单必须使用完整的 HTTP 或 HTTPS 地址')
      node.meta.iframeUrl = url
      component = registry.iframe
      componentName = 'IFrame'
    } else if (node.component) {
      const path = normalizeViewPath(node.component)
      component = registry.modules[path]
      if (!component) throw new Error(`Unknown view: ${node.component}`)
      node.meta.path = path
      componentName = registry.names[path]
    }
    if (node.name && componentName) {
      cacheNames[node.name] = componentName
      if (node.meta.keepAlive || node.children?.some(child => child.meta?.keepAlive)) allowedCacheNames.add(componentName)
    }
    return { path: node.path, name: node.name, meta: { ...node.meta }, component, children } as RouteRecordRaw
  }
  const root: RouteMenuItem = { path: '/layout', name: 'layout', component: 'views/layout/index.vue', children: nodes }
  const record = compile(root, null)
  return { tree: [root], records: record ? [record] : [], byName, cacheNames, allowedCacheNames: [...allowedCacheNames] }
}

/** 注册失败时回滚已添加记录；退出时通过 Router 返回的注销句柄清理。 */
export function createRouteInstaller(router: { addRoute: (route: RouteRecordRaw) => () => void }) {
  let removers: Array<() => void> = []
  const reset = (): void => { for (const remove of removers.reverse()) remove(); removers = [] }
  const replace = (routes: RouteRecordRaw[]): void => {
    reset()
    try { for (const route of routes) removers.push(router.addRoute(route)) }
    catch (error) { reset(); throw error }
  }
  return { replace, reset }
}
