import { defineStore } from 'pinia'
import { computed, ref, shallowRef } from 'vue'
import type { compileMenuRoutes, RouteMenuItem } from '@/services/navigation/menuRoutes'
import { readStoredText, writeStoredText } from '@/services/storage/appStorage'
export type { RouteMenuItem } from '@/services/navigation/menuRoutes'
export type RouteMap = Record<string, RouteMenuItem>

type CompiledMenus = ReturnType<typeof compileMenuRoutes>

/** 导航状态与缓存归属当前会话，不发请求，也不注册路由。 */
export const useRouterStore = defineStore('router', () => {
  const compiled = shallowRef<CompiledMenus>()
  const asyncRouterFlag = ref(0)
  const topActive = ref('')
  const histories = ref<Array<{ name?: string }>>()
  const suspended = ref(new Set<string>())
  const asyncRouters = computed(() => compiled.value?.tree ?? [])
  const routeMap = computed<RouteMap>(() => compiled.value?.byName ?? {})
  const topMenu = computed(() => (asyncRouters.value[0]?.children ?? []).filter(item => !item.hidden).map(item => ({ ...item, children: [] })))
  const leftMenu = computed(() => routeMap.value[topActive.value]?.children ?? [])
  const keepAliveRouters = computed(() => {
    if (!compiled.value) return []
    if (!histories.value) return compiled.value.allowedCacheNames.filter(name => !suspended.value.has(name))
    const names = new Set<string>()
    for (const history of histories.value) {
      let node = history.name ? routeMap.value[history.name] : undefined
      while (node) {
        const name = node.name ? compiled.value.cacheNames[node.name] : undefined
        if (name && compiled.value.allowedCacheNames.includes(name)) names.add(name)
        node = node.parent ?? undefined
      }
    }
    return [...names].filter(name => !suspended.value.has(name))
  })
  const setLeftMenu = (name: string): RouteMenuItem[] | undefined => {
    topActive.value = name
    writeStoredText(sessionStorage, 'topActive', name)
    return routeMap.value[name]?.children
  }
  const syncNavigation = (routeName?: string): void => {
    let node = routeName ? routeMap.value[routeName] : undefined
    while (node?.parent && node.parent.name !== 'layout') node = node.parent
    if (node?.name && node.name !== 'layout') setLeftMenu(node.name)
  }
  const install = (next: CompiledMenus): void => {
    compiled.value = next
    suspended.value.clear()
    histories.value = undefined
    topActive.value = readStoredText(sessionStorage, 'topActive', '')
    if (!routeMap.value[topActive.value]) topActive.value = ''
    asyncRouterFlag.value++
  }
  const reset = (): void => {
    compiled.value = undefined
    suspended.value.clear()
    histories.value = undefined
    topActive.value = ''
    asyncRouterFlag.value = 0
  }
  const setKeepAliveRouters = (history: Array<{ name?: string }>): void => { histories.value = history }
  const getKeepAliveComponentName = (routeName: string): string | undefined => compiled.value?.cacheNames[routeName]
  const suspendCache = (name: string): (() => void) => {
    const owner = compiled.value
    suspended.value.add(name)
    return () => { if (compiled.value === owner) suspended.value.delete(name) }
  }
  return { topActive, topMenu, leftMenu, routeMap, asyncRouters, asyncRouterFlag, keepAliveRouters, setLeftMenu, syncNavigation, install, reset, setKeepAliveRouters, getKeepAliveComponentName, suspendCache }
})
